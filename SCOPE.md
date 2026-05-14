# Swapping Sellout — Project Scope

A TOJAM jam project (given one extra week). This document captures what's
planned, what's done, and what's deliberately out of scope so we can ship.

Contributors: Brandon, Eevee059, Swash984, Takato.

For the game design / pitch, see `GAME.md`.

## Goal

A short, replayable stealth round where the player tries to deliver a
quota of stolen store items to the getaway car before getting caught
too many times. Target run length: 2–5 minutes.

## In Scope (Must-Have)

These need to work end-to-end for the game to be playable.

- **Scene flow** — Boot → MainMenu → MainGame.
  - **Boot** — splash + "press any key to continue". *(scene exists,
    `BootLoader.cs` ready; UI text needs to be added)*
  - **MainMenu** — title, PLAY/Settings/Quit, 3D background.
    *(scene exists with PLAY wired; Settings panel and Quit need
    assembly)*
  - **MainGame** — gameplay. *(done)*
- **Player controller** — FPS movement + mouse look + slot-based
  inventory + weight system. *(done)*
- **Item system**
  - ScriptableObject `ItemData` definitions. *(done)*
  - Store displays and player displays via `ItemDisplay`. *(done)*
  - Singleton `ItemSystem` distributing random destinations. *(done,
    minor bug — see Open Questions)*
- **Item interaction**
  - Click → swap fake-for-real at correct shelf. *(done, with rejection
    feedback via `PromptDisplay`)*
  - Click → deliver store-item at the Car. *(done — fires
    `ItemInteraction.OnStoreSwapCompleted`)*
- **Enemy AI** — Wander/Chase/Search state machine driven by vision,
  sound, and aggro zones. *(done)*
- **Throwables** — enemy lobs projectiles during chase. *(done)*
- **Respawn system** — `PlayerRespawn` + `RespawnPoint` markers; enemy
  catch triggers respawn. *(done)*
- **Win condition** — quota of N store-items delivered to the Car.
  *(done, `GameManager`)*
- **Lose condition** — N catches. *(done, `GameManager`)*
- **Settings** — mouse sensitivity, master volume; PlayerPrefs
  persistence; applied at startup via
  `[RuntimeInitializeOnLoadMethod]`. *(scripts done; settings panel
  UI to be assembled in MainMenu)*
- **HUD** — selected slot + selected item destination, Quota counter,
  catch counter. *(partial — counter scripts exist, in-scene UI
  needs wiring)*

## Stretch (Nice-to-Have, only if there's time)

- Multiple AI patrols or smarter pathing.
- More item variety / themed knock-offs.
- Difficulty modes (catches allowed, quota size, AI speed).
- Music + ambient store soundscape.
- Item highlight / outline shader when looking at it.
- Run-end screen with stats (swaps, catches, time).
- Save high scores locally.
- Pause menu inside MainGame (re-uses the same Settings panel).

## Out of Scope (cut to ship)

- Multiplayer / networking of any kind.
- Save game / persistent campaign progress.
- Procedurally generated stores.
- Voice acting beyond the existing voicelines.
- Multiple levels — one store, one round, one quota.
- Steam integration / achievements.
- Localization.

## Open Questions / Risks

- **Wall LOS reliability** — `EnemyVision`'s obstacle-mask raycast was
  unreliable when wall colliders weren't on expected layers. Aggro
  zones now act as the primary gate. Long term either zones become
  *the* system, or wall layer assignments get a cleanup pass.
- **`Timer.cs` semantics** — currently reloads the scene at zero with
  no win/lose meaning. Decision needed: remove it, or convert
  timer-out into a lose.
- **`ItemSystem.GetRandomItem` bug** — in the `store` branch it
  indexes `storeItems[Random.Range(0, swappedItems.Count)]` instead
  of `storeItems.Count`. One-line fix, not yet patched.
- **Dead event** — `ItemDisplay.OnStoreSwapCompleted` still fires on
  correct-shelf swaps but nothing listens. The win counter migrated
  to `ItemInteraction.OnStoreSwapCompleted` (Car delivery). Decide
  whether to remove the unused event or repurpose it (e.g., a
  per-swap audio cue at the shelf).
- **`ItemInteraction.Update` doesn't early-out after Car delivery** —
  after a successful Car drop it still runs the ItemDisplay/Item
  branches in the same click. Cosmetic; worth a small refactor.

## Current Status Snapshot

| Area              | Status                                         |
|-------------------|------------------------------------------------|
| Boot scene        | Empty scene exists; `BootLoader` ready; splash UI to add |
| Main Menu scene   | Exists with title, PLAY wired; needs Settings/Quit/Credits assembly |
| Player controller | Done (movement + look + weight)                |
| Inventory         | Done (slot-based)                              |
| Item system       | Done (one indexing bug, see Open Questions)    |
| Item interaction  | Done (shelf swap + Car delivery)               |
| Enemy AI          | Done (state machine + vision + zones)          |
| Respawn           | Done                                           |
| Win / Lose        | Logic done; in-scene panels need assembly      |
| Settings          | Scripts done; UI not yet assembled             |
| Audio             | Footsteps + enemy voicelines + door SFX done   |
| Art               | Placeholder + red-variant swap art in progress |

## File-Level Map (for new contributors)

- `Assets/Scripts/GameManager.cs` — win/lose state, UI panels, restart.
- `Assets/Scripts/GameSettings.cs` — PlayerPrefs settings (mouse + vol).
- `Assets/Scripts/SettingsMenu.cs` — settings panel UI binding.
- `Assets/Scripts/PanelToggleButton.cs` — generic show/hide button for
  menu nav.
- `Assets/Scripts/BootLoader.cs` — splash + press-any-key → load
  MainMenu.
- `Assets/Scripts/PromptDisplay.cs` — fading "you can't do that" prompt
  used by `ItemInteraction`.
- `Assets/Scripts/Buttons/` — `StartGameButton`, `QuitButton`,
  `RetryButton`, `ReturnMainMenu`. Each auto-wires its onClick on
  Awake.
- `Assets/Scripts/FPS Controller.cs` — player controller. Pulls mouse
  sensitivity from `GameSettings` on Start.
- `Assets/Scripts/Item*.cs` — item system (Data / Display / Inventory /
  Interaction / System).
- `Assets/Scripts/Enemy/` — AI, vision, sound, throwing, aggro zones.
- `Assets/Scripts/Enemy/EnemyStateMachine/` — state classes.
- `Assets/Scripts/RespawnLogic/` — respawn point + player respawn.
- `Assets/Scripts/Timer.cs` — round timer (currently unwired to game
  state).
- `Assets/Scenes/Boot.unity` — splash scene.
- `Assets/Scenes/MainMenu.unity` — title / menu scene.
- `Assets/Scenes/MainGame.unity` — gameplay scene.

## Build Settings Reminder

Scenes must be in **File → Build Settings…** in this order for
StartGameButton / ReturnMainMenu / BootLoader to all find their
targets:

```
0  Boot
1  MainMenu
2  MainGame
```
