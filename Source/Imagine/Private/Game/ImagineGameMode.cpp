#include "Game/ImagineGameMode.h"
#include "Game/ImagineGameState.h"
#include "UI/ImagineHUDWidget.h"
#include "Blueprint/UserWidget.h"
#include "Characters/ImaginePlayerCharacter.h"

AImagineGameMode::AImagineGameMode()
{
    DefaultPawnClass = AImaginePlayerCharacter::StaticClass();
    GameStateClass = AImagineGameState::StaticClass();
}

void AImagineGameMode::BeginPlay()
{
    Super::BeginPlay();

    if (HUDWidgetClass && GetWorld())
    {
        ActiveHUDWidget = CreateWidget<UImagineHUDWidget>(GetWorld(), HUDWidgetClass);
        if (ActiveHUDWidget)
        {
            ActiveHUDWidget->AddToViewport();
        }
    }
}
