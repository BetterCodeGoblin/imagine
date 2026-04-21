# IMAGINE Unreal Setup and Engine Buildout Guide

This document explains how to take the current repository scaffold and turn it into a working first playable slice in Unreal Engine 5.

---

# 1. Current Repository State

The repository already includes:

- `Imagine.uproject`
- Unreal C++ module scaffold
- `AImaginePlayerCharacter`
- `UImagineExertionComponent`
- `ABurdenProgressActor`
- `AImagineGameMode`
- `AImagineGameState`
- `UImagineHUDWidget`
- design and wiring docs

What is **not** complete yet:

- no real input assets
- no Blueprint subclass for the player
- no HUD widget implementation
- no playable map
- no test environment art
- no sound, VFX, or narration
- no hardware integration

So the repo is now in the correct state to move into Unreal Editor and build the first slice.

---

# 2. Goal of the First Slice

The first slice should prove this loop:

1. player moves through a small test space
2. player performs an exert action
3. exertion rises
4. a nearby burden objective gains progress
5. the player can stop and recover
6. completing the burden objective clearly signals success

That is enough to answer the important question:

**Does IMAGINE feel like effort, burden, and progress in play?**

Do not expand beyond that until the answer is yes.

---

# 3. Open the Project in Unreal

## Step 1: Open the repo

Open `Imagine.uproject` in Unreal Engine 5.4 or later.

## Step 2: Let Unreal generate project files

If prompted:
- generate project files
- rebuild the C++ project
- let Unreal finish indexing and compiling

## Step 3: Confirm the module loads

You should be able to see the project open without fatal module errors.

If there are compile issues:
- regenerate project files
- open the C++ solution in Visual Studio
- build the Editor target once
- reopen Unreal

---

# 4. What Needs To Be Built In-Engine

You should build the following in Unreal Editor.

## Required assets

### A. Input assets
Build:
- `IMC_ImagineDefault`
- `IA_Move`
- `IA_Look`
- `IA_Exert`

Purpose:
- wire movement
- wire camera look
- wire the burden/exert action

### B. Player Blueprint
Build:
- `BP_ImaginePlayerCharacter`

Parent class:
- `AImaginePlayerCharacter`

Purpose:
- assign input assets
- tune movement
- become the pawn used in the first test map

### C. HUD Widget Blueprint
Build:
- `WBP_ImagineHUD`

Parent class:
- `UImagineHUDWidget`

Purpose:
- visualize exertion
- visualize burden progress
- show completion state

### D. GameMode Blueprint
Build:
- `BP_ImagineGameMode`

Parent class:
- `AImagineGameMode`

Purpose:
- assign the HUD widget class
- make it easy to swap project defaults in-editor

### E. Burden Objective Blueprint
Build:
- `BP_BurdenProgressActor`

Parent class:
- `ABurdenProgressActor`

Purpose:
- place burden progress targets in the world
- expose tuning values in-editor

### F. Test level
Build:
- `L_Prototype_Burden`

Purpose:
- one tiny, clear test map for the first playable loop

---

# 5. Recommended Folder Structure In Unreal

Inside the Unreal Content Browser, create:

- `Content/Blueprints/Characters/`
- `Content/Blueprints/Game/`
- `Content/Blueprints/Actors/`
- `Content/UI/`
- `Content/Input/`
- `Content/Maps/`
- `Content/Materials/Prototype/`
- `Content/Meshes/Prototype/`
- `Content/Audio/Prototype/`

Keep the first slice clean and obvious.

---

# 6. Exact First Build Steps In Unreal Editor

## Step 1: Create input assets

Create an Input Mapping Context and three Input Actions.

### `IA_Move`
- value type: Axis2D

### `IA_Look`
- value type: Axis2D

### `IA_Exert`
- value type: Bool or Digital

### `IMC_ImagineDefault`
Bind:
- move to WASD / left stick
- look to mouse / right stick
- exert to E, Space, or Left Mouse Button

Then assign these assets to `BP_ImaginePlayerCharacter`.

---

## Step 2: Create `BP_ImaginePlayerCharacter`

Parent:
- `AImaginePlayerCharacter`

Assign:
- `DefaultMappingContext`
- `MoveAction`
- `LookAction`
- `ExertAction`

Tune if needed:
- camera boom length
- movement speed
- interact range
- exertion recovery values in the component

Do not over-polish this yet.

---

## Step 3: Create the HUD widget

Create `WBP_ImagineHUD` derived from `UImagineHUDWidget`.

Implement:
- exertion bar
- progress bar
- simple completion text or panel

Suggested UI:
- top left: exertion bar
- top center or bottom center: burden progress bar
- center screen: “Burden Complete” when finished

You only need primitive bars and text for now.

---

## Step 4: Create `BP_ImagineGameMode`

Parent:
- `AImagineGameMode`

Assign:
- `HUDWidgetClass = WBP_ImagineHUD`

This gives the map an editor-friendly game mode.

---

## Step 5: Create `BP_BurdenProgressActor`

Parent:
- `ABurdenProgressActor`

Add:
- simple static mesh placeholder, rock, altar, pillar, crate, burden object, etc.
- collision if needed
- optional text render or icon for visibility

Tune exposed values:
- `TargetProgress`
- `ProgressPerInteract`

For the first test:
- `TargetProgress = 100`
- `ProgressPerInteract = 10` or `20`

That gives fast iteration.

---

## Step 6: Build the map `L_Prototype_Burden`

Map contents:
- player start
- one burden actor
- simple terrain or ramp
- light environmental framing that suggests uphill struggle or ritual effort

The map should be tiny.

Good first layout:
- player spawns at foot of a hill or platform
- burden actor is placed ahead in a clear focal spot
- some simple geometry creates a sense of space and effort

Use cubes, ramps, and placeholder materials first.

---

## Step 7: Set world defaults

In the level:
- use `BP_ImagineGameMode`
- confirm player pawn is `BP_ImaginePlayerCharacter`

Test Play-In-Editor.

---

# 7. What “Done” Looks Like For The First Slice

The first slice is successful if all of this works:

- player can move and look
- player can trigger exert action
- exertion rises on action
- burden actor progress increases when nearby
- exertion recovers over time
- progress bar updates
- completion state appears when burden fills

If that works, the slice is real.

---

# 8. What Not To Build Yet

Do **not** do these yet:

- Concept 2 integration
- heart rate monitor support
- BLE stack
- data persistence systems
- narrator voice system
- multiple modes
- fancy menus
- polished art pipeline
- broad abstractions for future modes

Those are all second-wave problems.

---

# 9. Recommended Next Steps After The Slice Works

Once the prototype loop works, build in this order:

1. improve feedback
   - exertion VFX
   - sound cues
   - completion feedback

2. improve burden fantasy
   - stronger world object identity
   - heavier interaction feel
   - progress pacing tuning

3. add failure / recovery texture
   - overexertion penalties
   - temporary slowdown
   - risk/recovery rhythm

4. decide the direction of the next milestone
   - traversal-heavy
   - rowing-inspired
   - embodied effort sim
   - combat-adjacent exertion loop

---

# 10. Suggested Naming Conventions

Use:

- `BP_` for Blueprints
- `WBP_` for widgets
- `IMC_` for input mapping contexts
- `IA_` for input actions
- `L_` for levels
- `M_` / `MI_` for materials
- `SM_` for static meshes

This will keep the project readable as it grows.

---

# 11. Practical First Session Checklist

If you want the shortest path in-editor, do this in order:

1. open project
2. compile successfully
3. create input actions + mapping context
4. create `BP_ImaginePlayerCharacter`
5. create `WBP_ImagineHUD`
6. create `BP_ImagineGameMode`
7. create `BP_BurdenProgressActor`
8. create `L_Prototype_Burden`
9. place player start + burden actor
10. press Play and verify the loop

That is the build order.

---

# 12. Core Principle

The next milestone is **not** “port all of Imagine.”

The next milestone is:

**make one tiny Unreal slice feel like effort, burden, and progress.**

Everything else can wait.
