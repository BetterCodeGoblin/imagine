#pragma once

#include "CoreMinimal.h"
#include "GameFramework/GameModeBase.h"

class UImagineHUDWidget;
#include "ImagineGameMode.generated.h"

UCLASS()
class IMAGINE_API AImagineGameMode : public AGameModeBase
{
    GENERATED_BODY()

public:
    AImagineGameMode();
    virtual void BeginPlay() override;

protected:
    UPROPERTY(EditDefaultsOnly, Category="UI")
    TSubclassOf<UImagineHUDWidget> HUDWidgetClass;

private:
    UPROPERTY()
    TObjectPtr<UImagineHUDWidget> ActiveHUDWidget;
};
