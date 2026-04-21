# Editor Wiring

## Required Unreal Editor Setup

### Input
Create and assign:
- one `Input Mapping Context`
- one move action
- one look action
- one exert action

Assign them on `AImaginePlayerCharacter` defaults or a Blueprint subclass.

### HUD
Create a Widget Blueprint derived from `UImagineHUDWidget` and implement:
- `UpdateExertion`
- `UpdateProgress`
- `ShowCompletion`

Assign that widget class on `AImagineGameMode`.

### Test Map
- use `AImagineGameMode`
- place one `ABurdenProgressActor`
- place a player start
- trigger the exert action near the actor to validate the loop

## Goal
Do the minimum editor work needed to verify the 2 to 5 minute prototype loop.
