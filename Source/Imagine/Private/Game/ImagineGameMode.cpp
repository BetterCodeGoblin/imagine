#include "Game/ImagineGameMode.h"
#include "Characters/ImaginePlayerCharacter.h"

AImagineGameMode::AImagineGameMode()
{
    DefaultPawnClass = AImaginePlayerCharacter::StaticClass();
}
