#include "Actors/BurdenProgressActor.h"

ABurdenProgressActor::ABurdenProgressActor()
{
    PrimaryActorTick.bCanEverTick = false;
}

void ABurdenProgressActor::BeginPlay()
{
    Super::BeginPlay();
    OnProgressChanged.Broadcast(CurrentProgress, TargetProgress);
}

void ABurdenProgressActor::AddProgress(float Amount)
{
    if (Amount <= 0.0f || IsComplete())
    {
        return;
    }

    CurrentProgress = FMath::Clamp(CurrentProgress + Amount, 0.0f, TargetProgress);
    OnProgressChanged.Broadcast(CurrentProgress, TargetProgress);

    if (!bCompletionBroadcast && IsComplete())
    {
        bCompletionBroadcast = true;
        OnProgressCompleted.Broadcast();
        if (GEngine)
        {
            GEngine->AddOnScreenDebugMessage(-1, 3.0f, FColor::Green, TEXT("Burden progress complete."));
        }
    }
}

void ABurdenProgressActor::Interact()
{
    AddProgress(ProgressPerInteract);
}
