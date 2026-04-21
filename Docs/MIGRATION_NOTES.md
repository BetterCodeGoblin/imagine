# Migration Notes

## Previous Direction
The repository previously contained a Unity project with scripts for:
- progression / burden tracking
- Concept 2 bridge communication
- heart rate scaffolding
- mode-specific gameplay shells

## Current Direction
Those systems are no longer the active implementation path.

## Porting Rule
Any legacy concept must justify itself in the Unreal version by supporting the first playable slice or a later explicitly scheduled milestone.

## Avoid
- carrying dead code forward
- preserving Unity structure out of inertia
- rebuilding broad framework code before core gameplay exists
