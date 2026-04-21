#include "Game/ImagineGameState.h"

void AImagineGameState::SetLoopProgress(float InCurrentProgress, float InTargetProgress)
{
    CurrentProgress = InCurrentProgress;
    TargetProgress = InTargetProgress;
    OnProgressChanged.Broadcast(CurrentProgress, TargetProgress);
}

void AImagineGameState::MarkLoopCompleted()
{
    if (bLoopCompleted)
    {
        return;
    }

    bLoopCompleted = true;
    OnLoopCompleted.Broadcast();
}
