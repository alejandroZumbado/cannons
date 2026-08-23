# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project

Tower-defense game in Unity **6000.3.13f1** ("Cannons"). The player drags cannons onto a 5-column grid to shoot pirates advancing up the columns. Levels 1-500 progress via `PlayerPrefs`, with a password shortcut to jump to any level. Target platforms: PC Windows (mouse) and Android landscape (single-touch, auto-mapped to mouse by Unity — no touch-specific code needed).

There is no README; this file and the code are the source of truth.

## Working in this repo

This is a Unity project, not a CLI-buildable one — there is no `npm`/`make`/test-runner workflow. Builds, Play Mode testing, and asset (re)imports must be done from the Unity Editor by the user. When making script changes:
- Edit `.cs` files under `Assets/Scripts/` (and `Assets/Editor/` for editor tooling) directly.
- You cannot run or verify Play Mode behavior — describe what to test in-editor rather than claiming it works.
- `com.unity.test-framework` is a dependency but no test assemblies exist yet in `Assets/`.
- **Active Input Handling must stay `Input Manager (Old)` or `Both`.** All drag/drop uses `OnMouseDown/Drag/Up`, which breaks entirely under `New Input System` only.

### Editor menu commands (`Assets/Editor/`)
- `Levels > Generate Intro Level` (`LevelGenerator.cs`) — creates `Level_01.asset` in `Assets/Levels/`, inserts into `LevelDatabase` sorted by `levelNumber`.
- `Levels > Generate All Levels` (`LevelGeneratorMass.cs`) — bulk level generation.
- `Dev > Clear PlayerPrefs` (`DevTools.cs`) — resets save progress (`MaxLevel`).
- `Dev > Verify Android` / `Dev > Setup Android` (`AndroidSetup.cs`) — checks/applies Android player settings.

## Scene flow

Build order: `Menu` (index 0) → `Game` (index 1). Both are the only real gameplay scenes under `Assets/Scenes/`; everything else under `Assets/` (DavePixel, Hovl Studio, Stylish Cannon Pack, PolyAngel, etc.) is third-party asset-pack demo content, not part of the game.

```
MainMenu → LevelManager.PlayCurrent() → Game
Game Win     → LevelManager.LevelCompleted() → WinUI.Show()
Game Lose    → GameOverUI.Show()
WinUI "Siguiente"    → LevelManager.PlayCurrent() (already points at new MaxLevel)
GameOverUI "Reiniciar" → LevelManager.ReloadCurrentLevel()
Any "Menu" button → LevelManager.LoadMenu()
```

`SceneLoader` (singleton, `DontDestroyOnLoad`) wraps scene transitions with a loading screen (`minDisplayTime = 1.2f`), used automatically by `LevelManager.LoadLevel`/`LoadMenu` when present; falls back to plain `SceneManager.LoadScene` otherwise.

`LevelManager` (singleton, `DontDestroyOnLoad`, lives in `MainMenu`) owns progression: `PlayerPrefs["MaxLevel"]` is the unlocked-level index, `CurrentLevel` is the level about to be played, `TryPassword` jumps to any level whose `Level.password` matches (case-insensitive), `LevelCompleted` advances `MaxLevel` only if the completed level *was* the max.

## Level data model (`Assets/Scripts/Terrain/Level.cs`)

`Level` is a `ScriptableObject` (`Assets/Levels/*.asset`, indexed by `LevelDatabase.asset`, ordered by `levelNumber`). `Fila` and `Cuadro` are `[Serializable]` nested classes, not separate SOs:
- `Fila.cuadros: List<Cuadro>` — one wave/round.
- `Cuadro`: `index` (0-4, column: 0=right, 4=left), `tipo` (0=none, 1-3=normal pirate skin variants, 4=last-pirate-of-level skin, 5=reserved/unused), `hp` (1-10).

`GameManager` reads `_level = LevelManager.Instance?.CurrentLevel`; if `LevelManager` is absent (e.g. testing `Game` scene directly in-editor), `_level` stays null and no pirates spawn.

## Core gameplay loop (`GameManager.cs`)

Grid: `Matriz` is a 4-row × 5-column `GameObject[,]` built by walking child transforms of a scene hierarchy (`SetDimensions(4, 5)` in `GameManager.Start()`). Row 0 (nearest camera) holds the 5 cannon slots; rows 1-3 are pirate lanes. A central barrel outside the grid is where new cannons spawn each round.

Per-round flow:
1. **`GameFlow1`** (grab phase): `canDrag = true`; ensures a fresh cannon exists at the spawn barrel (`CannonCreator.SpawnCannon()`) if the previous spawn was placed into a slot.
2. Player drags the spawned cannon (or a previously placed one) onto a slot. Not moving the spawn cannon sacrifices that round's new cannon.
3. **`GameFlow2`** (shoot phase, triggered by a real placement/merge — see below): `canDrag = false`; every `CannonManagement` in `cannons` fires at the most-advanced pirate in its column; after a fixed 5s delay, `GameFlow1` and `nextRound` both fire.
4. **`nextRound`**: all pirates `Advance()` one row, then that round's wave (`Level.Fila`) spawns.

Win: `pirates.Count == 0 && round >= _level.filas.Count` (checked both when waves run out and when the last pirate dies). Lose: a pirate tries to advance past the front row (`PirateManager.Advance()` calls `GameManager.GameOver()`). Both `Win()`/`GameOver()` set a `_gameEnded` latch and show their UI panel after a 1.5s `Invoke` delay (workaround: the panel must stay active in the hierarchy so `Awake` doesn't disable it before the delayed `Show()` call needs it).

Music: `AudioManager.PlayNormalLvl()`/`PlayHardLvl()` at level start (`Level.isHard`); switches to `PlayFinalLvl(isHard)` the round the last wave spawns (`round == _level.filas.Count`); `PlayWinLvl()`/`PlayLoseLvl()` on end.

### Cannon drag/place/merge (`CannonManagement.cs` + `ReceiptCannon.cs`)

Single source of truth for placement is the mutual pointer pair `CannonManagement.currentSlot` ↔ `ReceiptCannon.cannon`. No `OnTriggerEnter/Exit` is used — the target slot is resolved only on drop, via `GameManager.FindSlotAt(Bounds)` comparing collider bounds against the 5 slots cached in `GameManager.slots` (`FindObjectsByType<ReceiptCannon>` at `Start()`).

- `ReceiptCannon.PlaceCannon(newCannon)` returns `true` only on a real change (place into an empty slot, move to a different slot, or merge) — `GameManager.GameFlow2()` (ending the turn) is called only then. Dropping a cannon back onto its own current slot is a no-op and does not consume a turn.
- **Merge**: dropping onto an occupied slot makes the dropped cannon absorb the resident's damage (`ShootUp`) and destroys the resident. Base damage is 1; each merge adds the absorbed cannon's damage.
- Visual states are mutually exclusive between two child GameObjects on the cannon prefab: `ShowIcon()` (small `createCannon` model, used while dragging or freshly spawned) vs. `PlaceInSlot()` (large `deployCannon` model, used once settled) vs. a live drag-time `UpdatePreview()` ghost of `deployCannon` over the hovered slot.
- **Collider gotcha**: any `OnMouse*`-driven object that can end up exactly coincident with a "zone" trigger collider (like a slot) can have its clicks swallowed by that collider. `ReceiptCannon.Awake()` disables its own `hitbox` for this reason (`.bounds` still works with the collider disabled, so slot-detection is unaffected) — check for this class of bug first if something stops responding to clicks.

### Pirates (`Assets/Scripts/Enemys/`)

`PirateSpawnManager.SpawnPirate(Cuadro)` instantiates at the column's back barrel (`Matriz.GetFirstElementOfColumn`). `PirateManager.Advance()` moves one barrel forward via `Matriz.GetObjectInFrontOf`, or calls `GameManager.GameOver()` if there's no further barrel ahead. `TakeDamage` reduces `hp`, updates a per-pirate HP sprite (`spritesHp[hp]`), and on death calls `GameManager.RemovePirate`, plays a death animation, and self-destructs after a 5s delay. Only the most-advanced pirate per column is targeted by cannon fire (enforced in `GameManager.GameFlow2` via `Matriz` lookups through `CannonManagement.Shoot`).

## Level design constraints

These aren't enforced in code — they're invariants a hand-authored or generated `Level` asset must satisfy to be winnable, given cannon base damage 1 and the fixed shoot timing above:

- A pirate spawned at the end of round N gets shot at in rounds N+1, N+2, N+3 — 3 shots max before it must reach the front. With damage 1, max beatable HP without a merge is **3**; HP 4+ requires a prior merge (damage 2+).
- **Blocking**: two pirates in the same column on consecutive rows (N and N+1) — the row-N pirate blocks shots meant for row N+1. If row N has HP `h`, row N+1 effectively only receives `4-h` shots.
- Merge budget: with `N` rows (rounds) in a level, at most `N - 5` merges are possible (1 round is needed per base column).
- Passwords: 1 uppercase letter + 4 digits (e.g. `B2341`); compared case-sensitively against the input after `.ToUpper()`.
- `isHard` should be true only for the two hardest tiers (columns fully active, HP4, 2+ forced merges, zero margin for error).

## Build configuration (PC + Android)

- Canvas Scaler on both `Menu` and `Game`: **Scale With Screen Size**, reference **1920×1080**, Match Width Or Height, **Match = 1**.
- Android Player Settings: min API **23**, target API **34**, **IL2CPP**, **ARM64**, landscape-left orientation.
- Two separate Build Profiles (Menu=0, Game=1 in both): one targeting Windows/Mac/Linux, one targeting Android.
- Distribution: `.apk` for direct testing, `.aab` + signed keystore for Play Store.
