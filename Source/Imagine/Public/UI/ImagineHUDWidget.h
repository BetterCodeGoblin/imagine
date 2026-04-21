#pragma once

#include "CoreMinimal.h"
#include "Blueprint/UserWidget.h"
#include "ImagineHUDWidget.generated.h"

UCLASS()
class IMAGINE_API UImagineHUDWidget : public UUserWidget
{
    GENERATED_BODY()

public:
    UFUNCTION(BlueprintImplementableEvent, Category="Imagine HUD")
    void UpdateExertion(float CurrentExertion, float MaxExertion, float NormalizedExertion);

    UFUNCTION(BlueprintImplementableEvent, Category="Imagine HUD")
    void UpdateProgress(float CurrentProgress, float TargetProgress);

    UFUNCTION(BlueprintImplementableEvent, Category="Imagine HUD")
    void ShowCompletion();
};
