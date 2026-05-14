# Swapping Sellout

A first-person stealth/heist game built for TOJAM. You're a low-budget
saboteur sneaking through a convenience store: swap the store's real
merchandise with cheap fakes, smuggle the originals out to your getaway
car, and don't let the clerk catch you too many times.

## Pitch

> The store thinks it's still selling premium goods. You'll have hauled
> the real stuff to your car before they ever notice. Hit your quota
> before the clerk catches you three times.

## Core Fantasy

You're a stealthy prankster doing inventory swaps in plain sight, then
running each prize out the back. Speed, weight, and a patrolling clerk
all push back against your quota.

## Game Loop

1. **Grab a fake item** from one of the drop-off shelves (player
   displays). Each fake has a hidden destination — a specific real
   shelf where it can be swapped in.
2. **Sneak to the correct shelf** while watching the clerk's patrol
   zones (`EnemyAggroZone`) and line of sight.
3. **Swap** the fake onto the shelf at its matching destination. The
   shelf rejects swaps that don't match the destination, so the
   destination text in your HUD matters. The real store item now
   sits in your inventory.
4. **Run the stolen item out to the Car** (any GameObject tagged
   `Car`). Click on it while holding a store item — the inventory
   removes the item and the quota counter ticks up.
5. **Repeat** until you hit the quota (win), or get caught too many
   times (lose).

Inventory has multiple slots; mouse-wheel switches the active slot,
so you can be carrying fakes and stolen goods at the same time.
Carried weight (`FPSController.weightCarried`) slows movement and
raises footstep pitch.

## Win Condition

Deliver **N store items to the Car** (default 5, tunable on
`GameManager.swapsToWin`). Each delivery is detected by
`ItemInteraction` when the player clicks an object tagged `Car`
while holding a store item — it fires
`ItemInteraction.OnStoreSwapCompleted`, which `GameManager`
listens to. The HUD reads `Quota X/Y`.

## Lose Condition

Get caught **N times** (default 3, tunable on
`GameManager.maxCatches`). Each catch triggers `PlayerRespawn` at
the active `RespawnPoint` and increments the counter via
`PlayerRespawn.OnRespawn`. When the counter hits the maximum, the
lose panel appears and the game pauses.

## The Enemy

A single patrolling clerk driven by a state machine:

- **Wander** — roams random NavMesh points within a radius. Switches
  to Chase if the player enters line of sight.
- **Chase** — pursues directly. Ramps speed up while the player is
  visible. Throws projectiles via `EnemyThrow`. Catches the player at
  close range, triggering respawn.
- **Search** — heads to the last known player position and scouts
  nearby waypoints before giving up.

Detection layers:

- **Field-of-view** cone (horizontal + vertical) with a configurable
  view distance.
- **Sound radius** — short-range omnidirectional "hearing" that skips
  the FOV cone (but still respects walls/zones).
- **Aggro zones** (`EnemyAggroZone`) — optional collider volumes; if
  any are assigned, the player must be inside at least one to be
  detected at all. Added because wall-LOS raycasts proved unreliable
  with the current scene's layer setup.

## Player Capabilities

- First-person movement (CharacterController, walk only).
- Mouse-look — sensitivity is configurable in Settings, saved to
  PlayerPrefs.
- Slot-based inventory with mouse-wheel slot switching.
- Click to pick up / swap items at displays, or deliver to the Car.
- Carried weight scales movement and pitches up footsteps.

## Setting / Mood

A grocery-store / convenience-store interior at night. Spotlights,
signage, racks of items. Patrolled by a clerk who absolutely should
not see you. A getaway car waits somewhere outside.

## Audio

- Footsteps that pitch with weight.
- Enemy voicelines: `foundSound` on chase start, `suspiciousSound` on
  search start, `pickupSound` reserved for item events.
- Sliding-door whooshes as the player approaches store doors.

## Scene Flow

```
Boot   →   MainMenu   →   MainGame
splash     PLAY/Settings/Quit
press      (3D environment background, "Swapping Sellout" title)
any key
```

- **Boot** (`Assets/Scenes/Boot.unity`) — splash + "press any key to
  continue"; initialises persistent state. Driven by `BootLoader.cs`.
- **MainMenu** (`Assets/Scenes/MainMenu.unity`) — the main menu, with
  a 3D environment background, "Swapping Sellout" title, PLAY button
  (loads MainGame), Settings panel, Quit, Credits.
- **MainGame** (`Assets/Scenes/MainGame.unity`) — the playable round.
