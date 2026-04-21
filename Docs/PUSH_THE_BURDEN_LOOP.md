# Push the Burden Loop

## Intent
Create the first tiny playable loop for IMAGINE with as little surface area as possible.

## Loop
1. player enters a small uphill or effort-themed space
2. player uses an exert action near a burden progress actor
3. exertion rises as progress advances
4. recovery happens over time when the player stops pushing
5. completing the burden actor ends the loop or triggers a success state

## Current Code Pieces
- `UImagineExertionComponent` tracks effort and exhaustion
- `AImaginePlayerCharacter` can trigger exertion and attempt nearby burden interaction
- `ABurdenProgressActor` tracks progress toward a single objective

## Next Implementation Step
In Unreal Editor:
- create a test map
- place one `ABurdenProgressActor`
- bind an input action to the exert action
- print progress and exertion to screen or a minimal widget
- confirm the 2 to 5 minute loop works before adding hardware integration
