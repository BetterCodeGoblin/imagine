#pragma once

#include "CoreMinimal.h"
#include "GameFramework/GameStateBase.h"
#include "ImagineGameState.generated.h"

DECLARE_DYNAMIC_MULTICAST_DELEGATE_TwoParams(FOnImagineProgressChangedSignature, float, CurrentProgress, float, TargetProgress);
DECLARE_DYNAMIC_MULTICAST_DELEGATE(FOnImagineLoopCompletedSignature);

UCLASS()
class IMAGINE_API AImagineGameState : public AGameStateBase
{
    GENERATED_BODY()

public:
    UPROPERTY(BlueprintAssignable, Category="Imagine")
    FOnImagineProgressChangedSignature OnProgressChanged;

    UPROPERTY(BlueprintAssignable, Category="Imagine")
    FOnImagineLoopCompletedSignature OnLoopCompleted;

    UFUNCTION(BlueprintCallable, Category="Imagine")
    void SetLoopProgress(float InCurrentProgress, float InTargetProgress);

    UFUNCTION(BlueprintCallable, Category="Imagine")
    void MarkLoopCompleted();

    UFUNCTION(BlueprintPure, Category="Imagine")
    float GetCurrentProgress() const { return CurrentProgress; }

    UFUNCTION(BlueprintPure, Category="Imagine")
    float GetTargetProgress() const { return TargetProgress; }

    UFUNCTION(BlueprintPure, Category="Imagine")
    bool IsLoopCompleted() const { return bLoopCompleted; }

private:
    UPROPERTY(VisibleAnywhere, Category="Imagine")
    float CurrentProgress = 0.0f;

    UPROPERTY(VisibleAnywhere, Category="Imagine")
    float TargetProgress = 100.0f;

    UPROPERTY(VisibleAnywhere, Category="Imagine")
    bool bLoopCompleted = false;
};
