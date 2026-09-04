using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Windows;
using CommunityToolkit.Mvvm.Input;
using NINA.Astrometry.Interfaces;
using NINA.Core.Utility;
using NINA.Equipment.Equipment.MyCamera;
using NINA.Equipment.Equipment.MyTelescope;
using NINA.Equipment.Interfaces.Mediator;
using NINA.Equipment.Interfaces.ViewModel;
using NINA.Profile.Interfaces;
using NINA.WPF.Base.Interfaces.Mediator;
using NINA.WPF.Base.Interfaces.ViewModel;
using NINA.WPF.Base.ViewModel;
using TonightsBest.Core;

namespace TonightsBest.NINA.Plugin;

[Export(typeof(IDockableVM))]
public sealed class TonightsBestDockableVM : DockableVM, ICameraConsumer, ITelescopeConsumer, IDisposable {
    private readonly ICameraMediator cameraMediator;
    private readonly ITelescopeMediator telescopeMediator;
    private readonly IFramingAssistantGateway framingGateway;
    private readonly TonightBestService service;
    private CancellationTokenSource? refreshCancellation;
    private CameraInfo? cameraInfo;
    private TelescopeInfo? telescopeInfo;
    private RankedTarget? selectedTarget;
    private string status = "Connect/select equipment, then refresh.";
    private bool isBusy;
    private double minimumAltitude = 30;

    [ImportingConstructor]
    public TonightsBestDockableVM(
        IProfileService profileService,
        ICameraMediator cameraMediator,
        ITelescopeMediator telescopeMediator,
        INighttimeCalculator nighttimeCalculator,
        IFramingAssistantVM framingAssistant,
        IApplicationMediator applicationMediator) : base(profileService) {
        this.cameraMediator = cameraMediator;
        this.telescopeMediator = telescopeMediator;
        cameraMediator.RegisterConsumer(this);
        telescopeMediator.RegisterConsumer(this);
        profileService.ProfileChanged += ProfileService_ProfileChanged;

        var resources = new ResourceDictionary {
            Source = new Uri("TonightsBest.NINA.Plugin;component/TonightsBestDockableTemplates.xaml", UriKind.RelativeOrAbsolute)
        };
        ImageGeometry = (System.Windows.Media.GeometryGroup)resources["TonightsBestIcon"];
        ImageGeometry.Freeze();
        Title = "Tonight's Best";

        var catalog = new NinaSkyAtlasCatalog(profileService);
        var context = new NinaObservingContextProvider(profileService, nighttimeCalculator, () => cameraInfo, () => telescopeInfo, () => MinimumAltitude);
        service = new TonightBestService(catalog, context, new TargetScorer());
        framingGateway = new NinaFramingAssistantGateway(profileService, framingAssistant, applicationMediator);
        RefreshCommand = new AsyncRelayCommand(async () => { await RefreshAsync(); });
        OpenInFramingAssistantCommand = new AsyncRelayCommand(async () => { await OpenInFramingAssistantAsync(); });
    }

    public ObservableCollection<RankedTarget> Targets { get; } = [];
    public IAsyncRelayCommand RefreshCommand { get; }
    public IAsyncRelayCommand OpenInFramingAssistantCommand { get; }

    public RankedTarget? SelectedTarget {
        get => selectedTarget;
        set { selectedTarget = value; RaisePropertyChanged(); }
    }

    public string Status {
        get => status;
        private set { status = value; RaisePropertyChanged(); }
    }

    public bool IsBusy {
        get => isBusy;
        private set { isBusy = value; RaisePropertyChanged(); }
    }

    public double MinimumAltitude {
        get => minimumAltitude;
        set { minimumAltitude = Math.Clamp(value, 0, 85); RaisePropertyChanged(); }
    }

    public string EquipmentSummary {
        get {
            var telescopeName = telescopeInfo?.Name ?? profileService.ActiveProfile.TelescopeSettings.LastDeviceName;
            var focalLength = profileService.ActiveProfile.TelescopeSettings.FocalLength;
            if (cameraInfo is null || cameraInfo.XSize <= 0 || cameraInfo.YSize <= 0)
                return $"Telescope: {telescopeName}; camera dimensions unavailable—connect the selected camera.";
            var pixelSize = cameraInfo.PixelSize > 0 ? cameraInfo.PixelSize : profileService.ActiveProfile.CameraSettings.PixelSize;
            try {
                var fov = new EquipmentProfile(focalLength, cameraInfo.XSize, cameraInfo.YSize, pixelSize).FieldOfView;
                return $"{telescopeName} ({focalLength:0.#} mm) + {cameraInfo.Name}: {fov.WidthArcMinutes:0.#}′ × {fov.HeightArcMinutes:0.#}′";
            } catch (ArgumentOutOfRangeException) {
                return "Set valid telescope focal length, camera dimensions, and pixel size in the active profile.";
            }
        }
    }

    private async Task<bool> RefreshAsync() {
        refreshCancellation?.Cancel();
        refreshCancellation?.Dispose();
        refreshCancellation = new CancellationTokenSource();
        IsBusy = true;
        Status = "Searching N.I.N.A. Sky Atlas and scoring targets…";
        try {
            var ranked = await service.GetTopAsync(15, refreshCancellation.Token);
            Targets.Clear();
            foreach (var target in ranked) Targets.Add(target);
            SelectedTarget = Targets.FirstOrDefault();
            Status = Targets.Count == 0 ? "No targets meet the current altitude requirement." : $"Top {Targets.Count} targets for tonight.";
            return true;
        } catch (OperationCanceledException) {
            Status = "Refresh canceled.";
            return false;
        } catch (Exception ex) {
            Status = ex.Message;
            return false;
        } finally {
            IsBusy = false;
        }
    }

    private async Task<bool> OpenInFramingAssistantAsync() {
        if (SelectedTarget is null) { Status = "Select a target first."; return false; }
        try {
            await framingGateway.OpenAsync(SelectedTarget.Target, CancellationToken.None);
            Status = $"Opened {SelectedTarget.Target.Name} in Framing Assistant.";
            return true;
        } catch (Exception ex) {
            Status = ex.Message;
            return false;
        }
    }

    public void UpdateDeviceInfo(CameraInfo deviceInfo) {
        cameraInfo = deviceInfo;
        RaisePropertyChanged(nameof(EquipmentSummary));
    }

    public void UpdateDeviceInfo(TelescopeInfo deviceInfo) {
        telescopeInfo = deviceInfo;
        RaisePropertyChanged(nameof(EquipmentSummary));
    }

    private void ProfileService_ProfileChanged(object? sender, EventArgs e) {
        Targets.Clear();
        SelectedTarget = null;
        Status = "Profile changed. Refresh to calculate tonight's targets.";
        RaisePropertyChanged(nameof(EquipmentSummary));
    }

    public void Dispose() {
        refreshCancellation?.Cancel();
        refreshCancellation?.Dispose();
        cameraMediator.RemoveConsumer(this);
        telescopeMediator.RemoveConsumer(this);
        profileService.ProfileChanged -= ProfileService_ProfileChanged;
    }
}
