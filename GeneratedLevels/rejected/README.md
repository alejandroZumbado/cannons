# Rejected generated levels

Bot drops that were reviewed and NOT imported. Kept (not deleted) so
CannonsLevelGen's level_registry never reuses their levelNumber/password/shape.

- `Level_508_20260923.json` (2026-09-24): last pirate has hp 12 (game supports 1-10),
  27-round single-pirate ramp tagged `trivial_for_baseline`. daily_generator now
  rejects out-of-range fields (`Level.structure_errors`).
- `Level_594_20261006.json` (2026-10-07): 26 pirates, all HP 1 — the naive
  baseline (never moves a cannon) wins it; no decision for the player.
  daily_generator now rejects levels the naive baseline wins.
