namespace TonightsBest.Core;

public interface ISkyAtlasCatalog { Task<IReadOnlyList<SkyTarget>> SearchAsync(CancellationToken cancellationToken); }
public interface IObservingContextProvider { Task<ObservingContext> GetAsync(CancellationToken cancellationToken); }
public interface IFramingAssistantGateway { Task OpenAsync(SkyTarget target, CancellationToken cancellationToken); }

public sealed class TonightBestService(ISkyAtlasCatalog catalog, IObservingContextProvider contextProvider, TargetScorer scorer) {
    public async Task<IReadOnlyList<RankedTarget>> GetTopAsync(int count, CancellationToken cancellationToken) {
        var contextTask = contextProvider.GetAsync(cancellationToken);
        var targetsTask = catalog.SearchAsync(cancellationToken);
        await Task.WhenAll(contextTask, targetsTask);
        return scorer.Rank(await targetsTask, await contextTask, count);
    }
}

