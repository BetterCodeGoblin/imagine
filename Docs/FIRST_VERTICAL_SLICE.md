# First Vertical Slice

## Goal
Prove IMAGINE works as a game in Unreal before adding hardware complexity.

## Slice Theme
A compact effort-driven traversal / action loop that communicates burden, momentum, and recovery.

## Minimum Playable Loop
- third-person player character
- movement and camera
- one stamina / exertion or health-like resource
- one interaction that converts effort into progress
- one small environment with a clear goal
- one feedback layer (UI, VFX, or narrator text)

## Suggested First Slice
"Push the Burden"
- player moves through a small uphill arena
- repeated exertion input or interactable object advances progress
- overexertion slows or punishes the player
- reaching the top ends the slice cleanly

## Systems To Build First
1. player character base
2. input mapping
3. exertion / health component
4. simple progress tracker
5. one UI widget
6. one test map

## Explicitly Not In Slice 1
- live Concept 2 integration
- BLE heart rate support
- broad mode support
- full narration system
- cloud sync
- multiple machine types

## Success Condition
Someone can play for 2 to 5 minutes and immediately understand the fantasy of effort, burden, and progress.


## Current Code Scaffold

The repo now includes:
- `AImaginePlayerCharacter`
- `UImagineExertionComponent`
- `AImagineGameMode`

This is still intentionally minimal. The current code proves project shape and first-slice system boundaries, not a finished gameplay loop.
