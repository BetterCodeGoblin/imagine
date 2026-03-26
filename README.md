# IMAGINE — Sisyphean Exercise Game Suite

A philosophical game about eternal struggle, powered by real exercise machines and evidence-based training science.

**Status:** MVP in development. TheClimb (rowing) functional scaffold with Concept 2 integration.

---

## Quick Start

### Prerequisites
- **Unity 6.0 LTS** or later
- **Windows 10+** or **macOS 12+** (Linux support via Proton)
- A Concept 2 RowErg/BikeErg or strength machine with BLE/USB output

### Clone & Open
```bash
git clone https://github.com/BetterCodeGoblin/imagine.git
cd imagine
```

Open the project folder in Unity Editor (version 6000+).

### Concept 2 SDK Setup
The project uses **ErgBridge** — a compiled C# bridge DLL for PM5 communication. 

1. Copy the `ErgBridge/` folder (binaries) to your project root:
   ```
   imagine/ErgBridge/
   ├── ErgBridge.exe
   ├── ErgBridge.dll
   ├── PM3CsafeCP.dll
   ├── PM3DDICP.dll
   └── PM3USBCP.dll
   ```

2. In Unity, assign the bridge path in any `Concept2Manager` inspector:
   - Set `bridgeExePath = "ErgBridge/ErgBridge.exe"`
   - The bridge auto-launches on play

3. **Hardware Connection:**
   - Connect your Concept 2 rowing machine via USB
   - Launch a scene with Concept 2Manager active
   - Bridge discovers PM5 and streams data via TCP/IP (localhost:6789)

### Heart Rate Monitor (Optional)
Connect any BLE Heart Rate Monitor (Polar H10, Wahoo TICKR, Garmin HRM-Pro, etc.):
- **v1.0 feature:** Full BLE scanning and zone-aware training
- **MVP:** Manual HR injection or simulation mode

---

## Architecture

### Single Project, Three Modes (Scenes)

| Mode | Machine | File | Status |
|------|---------|------|--------|
| **TheClimb** | Concept 2 RowErg | `Assets/Scenes/TheClimb.unity` | MVP (functional scaffold) |
| **TheAscent** | Smart Bike / Power Meter | `Assets/Scenes/TheAscent.unity` | v1.0 (stub) |
| **TheBurden** | Strength Machines (HIT) | `Assets/Scenes/TheBurden.unity` | v1.0 (stub) |

### Core Systems

**BoulderSystem** (`Assets/Scripts/Core/BoulderSystem.cs`)
- Persistent progress tracking (lifetime altitude)
- Session management (progress + regression)
- Mid-session drift on rest
- EPOC afterburn calculation
- SQLite persistence (PlayerPrefs in MVP)

**Concept2Manager** (`Assets/Scripts/Core/Concept2Manager.cs`)
- Launches ErgBridge.exe (PM5 bridge)
- TCP polling loop (10 Hz)
- Stroke data parsing (SPM, watts, pace)
- Form bonus calculation (drive ratio)

**HeartRateManager** (`Assets/Scripts/Core/HeartRateManager.cs`)
- BLE HRM scanning (v1.0)
- Karvonen HR zone calculation (1-5)
- RMSSD HRV tracking for recovery scoring
- Simulation mode for testing

**GameModeBase** (`Assets/Scripts/Modes/GameModeBase.cs`)
- Abstract class for all three modes
- Session lifecycle hooks
- Narrator event callbacks
- Progress → BoulderSystem wiring

**TheClimb** (`Assets/Scripts/Modes/TheClimb.cs`)
- Rowing-specific mechanics
- Form bonus multiplier from drive ratio
- SPM gate (min 14 SPM for progress)
- HR zone-aware power multipliers
- Narrator integration

---

## Gameplay Flow

### TheClimb (Rowing)

1. **Start Session**
   - BoulderSystem loads regression offset (time since last session)
   - Concept2Manager connects to PM5
   - Narrator greets you

2. **Row**
   - Each stroke fires `OnStrokeDataReceived` event
   - Concept2Manager parses SPM, watts, pace, drive ratio
   - TheClimb calculates progress:
     ```
     progress = watts × metersPerWatt × formBonus × hrZoneMultiplier
     ```
   - BoulderSystem adds altitude
   - Narrator speaks at milestones (every 100 strokes)

3. **Rest (>2 seconds)**
   - BoulderSystem applies drift (backward movement)
   - Drift accelerates if rest continues
   - HR monitor can reduce drift if HR still elevated (EPOC effect)

4. **End Session**
   - BoulderSystem calculates EPOC (afterburn) score
   - Sets regression suspension timer
   - Saves to persistent storage
   - Narrator acknowledges effort

---

## Narrator System

The Narrator is the game's philosophical voice. Integrate via:

```csharp
// In any GameModeBase subclass:
TriggerNarrator("The mountain recognizes effort, not ego.");
```

All three modes publish `OnNarratorLine` events. Wire a NarratorController to subscribe and play audio.

Sample narrator lines are in the GDD: `/home/jsypherd/.openclaw/workspace/projects/unity/IMAGINE_GDD.md`

---

## Data Persistence

**MVP:** PlayerPrefs + local JSON
```csharp
boulderState.LifetimeAltitude        // persistent altitude
boulderState.LastSessionEnd          // for regression calc
boulderState.AfterburnExpiryTime     // EPOC suspension
```

**v1.0:** SQLite via `SQLite4Unity`
- Full session history
- FTP test results (TheAscent)
- HRV baseline (HeartRateManager)
- Periodization blocks (meta-game)

---

## Development Roadmap

### MVP (In Progress)
- [x] BoulderSystem (progress, regression, drift)
- [x] Concept2Manager (PM5 USB bridge)
- [x] TheClimb (rowing mechanics)
- [ ] Basic UI (altitude display, HR zone color)
- [ ] Narrator voiceover (~50 lines)
- [ ] MainMenu scene

### v1.0 (Target: ~10–12 months)
- All three modes functional
- FTP test protocol (TheAscent)
- Manual TUT input (TheBurden)
- Full narrator script (200+ lines)
- Cloud sync (Firebase)
- Leaderboards
- Co-op (2-player shared boulder)

### v2.0 (Target: +12 months)
- Quest 3 VR (TheClimb VR)
- Smart trainer FTMS control
- Smart barbell/collar sensor
- HRV recovery recommendations
- DLC: Tantalus, Ixion, Prometheus modes

---

## Hardware Support

### Concept 2 Rowing / Cycling

| Model | Protocol | Support |
|-------|----------|---------|
| RowErg PM5 | USB + ErgData BLE | ✅ MVP (USB) |
| BikeErg PM5 | USB + ErgData BLE | ✅ MVP (USB) |
| SkiErg PM5 | USB + ErgData BLE | 📋 v1.0 |
| PM3 / PM4 | Serial | 📋 v1.0 |

### Power Meters & Smart Trainers

| Hardware | Protocol | Support |
|----------|----------|---------|
| Wahoo KICKR | ANT+ / BLE FTMS | 📋 v1.0 |
| Tacx NEO | BLE FTMS | 📋 v1.0 |
| Generic ANT+ | ANT+ Cycling Power | 📋 v1.0 |
| Generic BLE | BLE Cycling Power (0x1818) | 📋 v1.0 |

### Heart Rate Monitors

| Hardware | Protocol | Support |
|----------|----------|---------|
| Polar H10 | BLE HRS (0x180D) | 📋 v1.0 |
| Wahoo TICKR | BLE HRS | 📋 v1.0 |
| Garmin HRM-Pro | BLE HRS | 📋 v1.0 |
| Chest Straps | BLE HRS | 📋 v1.0 |

### Strength Machines

| Hardware | Support |
|----------|---------|
| Keiser M-series (BLE) | 📋 v1.0 |
| Vmaxpro collar (BLE) | 📋 v1.0 |
| Phone camera (ML pose) | 📋 v2.0 |
| Manual rep logging | ✅ MVP |

---

## References

- **Game Design Document:** See `IMAGINE_GDD.md` in the workspace
- **Exercise Science:**
  - Karvonen formula (HR zones)
  - Progressive overload (strength)
  - EPOC (afterburn mechanism)
  - HIT protocol (High Intensity Training)
- **Literature:**
  - Albert Camus — *The Myth of Sisyphus* (1942)
  - Bennett Foddy — *Getting Over It* (game design inspiration)
  - Arthur Jones — *Nautilus Training Principles*
  - Doug McGuff & John Little — *Body by Science*

---

## Contributing

This is a personal project by **James Sypherd**, authored with **Kato** (AI assistant).

Hardware integration, testing, and scene design contributions welcome. Fork and submit a PR.

---

## License

Proprietary (all rights reserved). Contact `sypherdj1@gmail.com` for inquiries.

---

*One must imagine Sisyphus happy. One must also imagine him well-trained.*
