# SurvivorMaster Arena Prototype (Unity 2022.3 LTS, URP 3D)

This project contains a mobile-first survivor-style prototype using pooled entities, spatial partitioning, and centralized ticking.

## Controls
- Mobile: drag `VirtualJoystick`.
- Desktop/editor fallback: `WASD` / arrow keys.
- Combat: automatic targeting and firing.

## Required Folder Layout
- `Assets/Scenes/DemoScene.unity`
- `Assets/Scripts/Core`
- `Assets/Scripts/Player`
- `Assets/Scripts/Enemy`
- `Assets/Scripts/Systems`
- `Assets/Scripts/UI`
- `Assets/Prefabs`
- `Assets/ScriptableObjects`

## Scene Setup (`DemoScene`)
1. Create/open `Assets/Scenes/DemoScene.unity`.
2. Arena:
- Add Plane at `(0,0,0)`, scale `(5,1,5)` for 50x50 world space.
- Create URP Unlit material (`M_Arena_Unlit`) and assign to plane.
3. Lighting:
- Keep one Directional Light, no post-processing volume.
4. Camera:
- Position `(0,20,-14)`, rotation `(45,0,0)`.
- Add `CameraFollow`.
5. Add `GameRoot` object and attach:
- `GameTimer`
- `EnemyManager`
- `ProjectileManager`
- `XPOrbManager`
- `XPManager`
- `EnemySpawner`
- `DemoSceneBootstrap`

## Prefab Setup

### `Player.prefab` (`Assets/Prefabs/Player.prefab`)
1. Create Capsule named `Player`.
2. Add components:
- `CharacterController`
- `PlayerStats`
- `PlayerController`
- `AutoAttack`
3. Tuning defaults:
- CharacterController: Radius `0.45`, Height `1.8`.
4. Save as prefab.

### `Enemy.prefab` (`Assets/Prefabs/Enemy.prefab`)
1. Create Capsule named `Enemy`.
2. Add components:
- `Enemy`
- `EnemyAI`
- `Health`
3. Assign one shared instancing-enabled material (`M_Enemy`) to all enemies.
4. Save as prefab.

### `Projectile.prefab` (`Assets/Prefabs/Projectile.prefab`)
1. Create Sphere named `Projectile` and scale to `(0.25,0.25,0.25)`.
2. Add component `Projectile`.
3. Use shared material (`M_Projectile`).
4. Save as prefab.

### `XPOrb.prefab` (`Assets/Prefabs/XPOrb.prefab`)
1. Create Sphere named `XPOrb` and scale to `(0.3,0.3,0.3)`.
2. Add component `XPOrb`.
3. Use shared material (`M_XPOrb`).
4. Save as prefab.

## Upgrade ScriptableObjects
Create 4 assets in `Assets/ScriptableObjects/` from `Create > SurvivorMaster > Upgrade Definition`:
1. `U_Damage`: type `Damage`, value `+3`
2. `U_AttackSpeed`: type `AttackSpeed`, value `+0.2`
3. `U_MoveSpeed`: type `MoveSpeed`, value `+0.35`
4. `U_MaxHP`: type `MaxHp`, value `+12`

## UI Setup
1. Create `Canvas` (Screen Space - Overlay).
2. Add HUD elements:
- HP bar (Image fill) + `SimpleBar`
- XP bar (Image fill) + `SimpleBar`
- `Text` for Level, Enemy Count, Timer
3. Add `HUDController` to a HUD object and wire refs (`PlayerStats`, `XPManager`, bars, texts).
4. Add a joystick panel:
- Background image + child handle image
- Attach `VirtualJoystick` to background, assign handle.
- Assign joystick into `PlayerController.virtualJoystick`.
5. Add level-up panel:
- Root panel (disabled by default)
- 3 Buttons each with title/description Texts
- Attach `LevelUpUI` and assign root/buttons/text arrays.
6. Add `PerformanceDebugOverlay` with a Text target.

## System Wiring
Assign in inspector:
- `EnemyManager.enemyPrefab` -> `Enemy.prefab`
- `ProjectileManager.projectilePrefab` -> `Projectile.prefab`
- `XPOrbManager.xpOrbPrefab` -> `XPOrb.prefab`
- `EnemySpawner.player` -> Player instance
- `EnemySpawner.enemyManager` -> `EnemyManager`
- `XPManager.playerStats` -> Player `PlayerStats`
- `XPManager.levelUpUI` -> `LevelUpUI`
- `XPManager.availableUpgrades` -> 4 upgrade assets
- `DemoSceneBootstrap.player` -> Player
- `DemoSceneBootstrap.cameraFollow` -> Main Camera `CameraFollow`
- `DemoSceneBootstrap.enemyManager` -> `EnemyManager`
- `DemoSceneBootstrap.xpOrbManager` -> `XPOrbManager`

## Architecture
- `Player`:
- `PlayerController` handles joystick/keyboard movement with `CharacterController`.
- `AutoAttack` queries nearest enemy and fires pooled projectiles.
- `PlayerStats` is the stat/HP source of truth.
- `Enemies`:
- `EnemyManager` handles pooling, active list, spatial grid registration, and batch ticking.
- `Enemy` contains runtime state and delegates move/combat to `EnemyAI`.
- `Health` handles death event and HP.
- `Combat`:
- `ProjectileManager` moves pooled projectiles and resolves hits without physics bodies.
- `Progression`:
- `XPOrbManager` updates pooled XP orbs and attraction/pickup behavior.
- `XPManager` drives level progression and pause-based level-up choices.
- `LevelUpUI` presents 3 random upgrades.
- `UI`:
- `HUDController` updates bars/text.
- `PerformanceDebugOverlay` shows active counts, pool sizes, frame time.

## Performance Strategy (Mobile-First)
- Object pooling for enemies, projectiles, and XP orbs.
- No runtime `Instantiate/Destroy` during gameplay loops.
- Spatial hash grid (`SpatialHashGrid2D`) for nearest and radius queries.
- Centralized enemy ticking in `EnemyManager` batches (no per-enemy `Update`).
- Manual movement for enemies/projectiles/orbs (no rigidbody simulation needed).
- Shared materials with GPU instancing compatibility.
- Pre-allocated query buffers and reusable `StringBuilder` for debug text.

## Why Spatial Grid
A grid on XZ is a low-overhead broad-phase for dense crowds. Nearest-target and hit checks only scan nearby cells instead of all enemies, reducing CPU cost as active enemy count scales toward 500.

## Add a New Weapon
1. Duplicate/extend `AutoAttack` into a new weapon behavior (e.g. burst, cone, orbit).
2. Call `ProjectileManager.SpawnProjectile(...)` with new timing/spread logic.
3. Optionally create a new projectile prefab variant for visuals.
4. Attach the new weapon script to player and tune cooldown/range/speed.

## Add a New Upgrade
1. Create a new `UpgradeDefinition` asset.
2. Add/extend `UpgradeType` if needed.
3. Update `PlayerStats.ApplyUpgrade` to handle the new type.
4. Add the asset to `XPManager.availableUpgrades`.

## Target Load
This implementation is designed for:
- 500 active enemies
- 1000 XP orbs
- 200 projectiles

Adjust pool prewarm sizes in manager inspectors to match expected peak load.
