# IMAGINE — Unreal Reboot

IMAGINE is being rebooted as an **Unreal Engine project**.

The original Unity-first experiment explored a Sisyphean exercise game suite tied to real exercise hardware, motion data, and philosophical framing. That direction still stands, but the repository is now being reset around an Unreal-first foundation so future development can happen in a single engine and a cleaner gameplay architecture.

## Current Status

**Status:** Unreal reboot in progress  
**Engine Direction:** Unreal Engine 5  
**Repository State:** Foundation reset, design intent preserved, implementation restarting

This repository should now be treated as a design and migration scaffold, not an actively usable Unity project.

## Why the Reset

The Unity prototype established some useful design ideas, but the project direction has changed.
The new goals are:

- rebuild the project around Unreal Engine 5
- consolidate gameplay development into one engine stack
- support stronger 3D gameplay prototyping and interaction systems
- preserve the strongest concepts from the previous prototype without carrying over brittle engine-specific scaffolding

## What IMAGINE Still Is

IMAGINE remains an exercise-driven action / training experience built around struggle, repetition, physical effort, and progress through resistance.

Core concepts being preserved:

- exercise-informed gameplay loops
- progression through sustained effort
- motion, form, and training data as design inputs
- philosophical framing inspired by effort, burden, ascent, and repetition
- modular mode structure for different movement / machine contexts

## Unreal Reboot Direction

The Unreal version is expected to focus on:

- a strong third-person or embodied player experience
- clean C++ gameplay foundations with optional Blueprint-facing extension points
- modular systems for health, progression, exercise input, and encounter logic
- support for future hardware integration, motion capture pipelines, and training telemetry
- vertical-slice-first development instead of broad unfinished scaffolding

## Repository Structure Going Forward

Planned top-level direction:

- `Docs/` for game design, architecture, migration notes, and hardware plans
- `Source/` for Unreal C++ gameplay code once the UE project is initialized
- `Content/` for Unreal assets once production begins
- `Config/` for Unreal project configuration

## Preserved Design Themes

The old sub-modes still matter conceptually, even though their implementation is being restarted:

- **The Climb** , ascent / endurance / rowing-inspired effort
- **The Ascent** , sustained output / cycling / pace and power
- **The Burden** , strength / resistance / deliberate load management

These are now design pillars, not implementation promises.

## Immediate Next Steps

1. initialize the Unreal project for IMAGINE
2. define the first vertical slice in UE5
3. rebuild only the systems required for that slice
4. reintroduce hardware and motion-data integrations only when the gameplay core is working

## Migration Notes

The previous Unity scripts and project folders were removed from the active repository structure as part of this reset. If old Unity-specific logic is worth preserving, it should be ported intentionally into Unreal design docs or rewritten in Unreal-native form instead of carried forward as dead scaffold.

## Recommendation

Treat this repo as the canonical home for the **Unreal version of IMAGINE** from this point onward.


## Unreal Scaffold Added

The repository now includes a lightweight Unreal C++ project scaffold:

- `Imagine.uproject`
- `Source/Imagine/`
- `Config/`
- `Content/`
- `Docs/FIRST_VERTICAL_SLICE.md`

This is a starter foundation only. It is intentionally minimal and should be expanded around the first playable slice rather than broad framework code.


## First Gameplay Scaffold

A first-pass C++ gameplay scaffold is now present for the Unreal reboot:

- `AImaginePlayerCharacter` for third-person player control
- `UImagineExertionComponent` for effort / recovery state
- `AImagineGameMode` as the minimal project game mode

The immediate next implementation step is input assets plus a tiny playable test map that uses exertion as the core progress tension.


## Push the Burden Prototype Loop

The current first-loop scaffold assumes a simple interaction model:

- the player performs an exert action
- exertion increases
- a nearby burden actor gains progress
- recovery happens over time when the player stops

See `Docs/PUSH_THE_BURDEN_LOOP.md` for the intended first playable loop.


## Editor Wiring Status

The repo now includes code-side scaffolding for HUD and loop completion, but Unreal Editor setup is still required for:

- input actions and mapping context
- widget blueprint implementation
- test map placement

See `Docs/EDITOR_WIRING.md`.
