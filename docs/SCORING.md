# Scoring details

The score is a 0–100 planning aid, not a statement of scientific value.

| Component | Weight | Meaning |
| --- | ---: | --- |
| Visibility | 35 | Sampled hours at or above the selected minimum altitude |
| Moon | 25 | Separation penalty scaled by N.I.N.A.'s illuminated fraction |
| Framing | 25 | Preference for useful occupation of the selected camera/telescope field |
| Altitude | 10 | Maximum sampled altitude above the chosen floor |
| Object interest | 5 | Small broad-category prior; not a substitute for user preference |

`Frame %` estimates the catalog ellipse area as a percentage of the rectangular
camera field. Values above 100% warn that the target's catalog footprint exceeds
the field area. Orientation still matters, so users should confirm every target
in Framing Assistant.

Altitude is sampled every five minutes between astronomical dusk and dawn, with
sunset/sunrise and a ten-hour window as polar/fallback cases. Moon position is
evaluated at the middle of that interval.
