#pragma once

#include "CoreMinimal.h"
#include "GameFramework/Actor.h"
#include "BurdenProgressActor.generated.h"

DECLARE_DYNAMIC_MULTICAST_DELEGATE_TwoParams(FOnProgressChangedSignature, float, CurrentProgress, float, TargetProgress);
DECLARE_DYNAMIC_MULTICAST_DELEGATE(FOnProgressCompletedSignature);

UCLASS()
class IMAGINE_API ABurdenProgressActor : public AActor
{
    GENERATED_BODY()

public:
    ABurdenProgressActor();

    UPROPERTY(EditAnywhere, BlueprintReadOnly, Category="Progress")
    float TargetProgress = 100.0f;

    UPROPERTY(EditAnywhere, BlueprintReadOnly, Category="Progress")
    float ProgressPerInteract = 10.0f;

    UPROPERTY(BlueprintAssignable, Category="Progress")
    FOnProgressChangedSignature OnProgressChanged;

    UPROPERTY(BlueprintAssignable, Category="Progress")
    FOnProgressCompletedSignature OnProgressCompleted;

    UFUNCTION(BlueprintCallable, Category="Progress")
    void AddProgress(float Amount);

    UFUNCTION(BlueprintCallable, Category="Progress")
    void Interact();

    UFUNCTION(BlueprintPure, Category="Progress")
    float GetCurrentProgress() const { return CurrentProgress; }

    UFUNCTION(BlueprintPure, Category="Progress")
    bool IsComplete() const { return CurrentProgress >= TargetProgress; }

protected:
    virtual void BeginPlay() override;

private:
    UPROPERTY(VisibleAnywhere, Category="Progress")
    float CurrentProgress = 0.0f;

    bool bCompletionBroadcast = false;
};
