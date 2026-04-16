# IMAGINE — Sisyphean Exercise Game Suite

A philosophical exercise game scaffold built in Unity around rowing, cycling, and strength training loops.

**Current state:** code-first prototype. The repository is ready for C# iteration and Unity assembly, but it does **not** yet include committed scenes, prefabs, UI, or polished hardware plugins.

---

## What is in this repo right now

Included:
- Core gameplay/system scripts under `Assets/Scripts/Core`
- Mode scripts under `Assets/Scripts/Modes`
- Unity project/package settings
- Simulation-friendly scaffolding for early development

Not included yet:
- Production scenes (`.unity`)
- Prefabs, UI, and art setup
- Full BLE device integration
- Bundled ErgBridge binaries
- Final persistence/database layer

That split is intentional for now. This repo is meant to give you a solid script layer to assemble in-editor.

---

## Unity version

- **Editor version:** Unity `6000.0.0f1`
- Recommended: use Unity 6 to avoid package/version drift

---

## Quick start

```bash
git clone https://github.com/BetterCodeGoblin/imagine.git
cd imagine
```

Open the folder in Unity 6.

### First assembly in Unity

Because scenes are not committed yet, expect to wire things together manually in-editor.

Recommended first scene setup:
1. Create a new scene, for example `Assets/Scenes/TheClimbPrototype.unity`
2. Add empty GameObjects for:
   - `BoulderSystem`
   - `Concept2Manager`
   - `HeartRateManager`
   - `TheClimb`
3. Attach the matching scripts
4. Enable simulation on hardware managers if you do not have devices connected yet:
   - `Concept2Manager.simulateInput = true`
   - `HeartRateManager.simulateHeartRate = true`
5. Hook UI and narrator systems on top of the emitted events

If you only need the C# layer, the repo is already structured for that workflow.

---

## Hardware integration status

### Concept 2 / PM5

`Concept2Manager` expects an external ErgBridge executable that speaks to the PM5 and streams rowing data over localhost.

Expected default path:

```text
ErgBridge/ErgBridge.exe
```

The bridge is **not bundled** in this repository. You will need to provide it separately.

Current behavior:
- launches ErgBridge if present
- connects to `127.0.0.1:6789`
- parses `rate,pace,power,connected`
- emits stroke events to gameplay systems
- supports simulation mode for editor-side work

### Heart rate monitor

`HeartRateManager` is currently scaffolded for:
- simulated heart rate
- manual HR injection via code
- zone calculation
- RR interval storage and RMSSD calculation

Actual BLE discovery/connection is **not implemented yet**.

### Cycling and strength modes

- `TheAscent` is still a stub for future cycling hardware integration
- `TheBurden` supports manual set logging and progression logic, not sensor-driven strength hardware yet

---

## Repository architecture

### Core scripts

- `Assets/Scripts/Core/BoulderSystem.cs`
  - session progress
  - lifetime progress
  - regression / drift
  - lightweight PlayerPrefs persistence

- `Assets/Scripts/Core/Concept2Manager.cs`
  - ErgBridge process launch
  - PM5 connection state
  - simulated strokes
  - stroke event generation

- `Assets/Scripts/Core/ErgBridgeClient.cs`
  - TCP client for the local bridge
  - background polling loop
  - thread-safe handoff to Unity main thread

- `Assets/Scripts/Core/HeartRateManager.cs`
  - HR zone logic
  - simulated/manual HR injection
  - RMSSD support for future recovery features

### Mode scripts

- `Assets/Scripts/Modes/GameModeBase.cs`
- `Assets/Scripts/Modes/TheClimb.cs`
- `Assets/Scripts/Modes/TheAscent.cs`
- `Assets/Scripts/Modes/TheBurden.cs`

These are meant to be attached to scene objects once you assemble the Unity side.

---

## Recommended next steps

1. Create prototype scenes in Unity
2. Add a minimal HUD for altitude, HR zone, and connection status
3. Drop in ErgBridge locally if testing against a real PM5
4. Reuse proven Strength-ERG code for:
   - CSAFE command handling
   - PM5 parsing details
   - Bluetooth connectivity where it is already battle-tested
5. Promote persistence from PlayerPrefs to a more durable session/history layer when needed

---

## Reality check

This repo is now best thought of as:
- **ready for code work**
- **ready for Unity-side assembly**
- **not yet a frictionless clone-and-play project**

That is okay. If your immediate goal is to get the script layer solid and then assemble in engine, this repo is in the right shape for that.

---

## License

Proprietary (all rights reserved). Contact `sypherdj1@gmail.com` for inquiries.
