#include "Characters/ImaginePlayerCharacter.h"
#include "Components/ImagineExertionComponent.h"
#include "Actors/BurdenProgressActor.h"
#include "Camera/CameraComponent.h"
#include "GameFramework/SpringArmComponent.h"
#include "GameFramework/CharacterMovementComponent.h"
#include "EnhancedInputComponent.h"
#include "EnhancedInputSubsystems.h"
#include "InputActionValue.h"
#include "GameFramework/PlayerController.h"
#include "EngineUtils.h"

AImaginePlayerCharacter::AImaginePlayerCharacter()
{
    PrimaryActorTick.bCanEverTick = true;

    CameraBoom = CreateDefaultSubobject<USpringArmComponent>(TEXT("CameraBoom"));
    CameraBoom->SetupAttachment(RootComponent);
    CameraBoom->TargetArmLength = 350.0f;
    CameraBoom->bUsePawnControlRotation = true;

    FollowCamera = CreateDefaultSubobject<UCameraComponent>(TEXT("FollowCamera"));
    FollowCamera->SetupAttachment(CameraBoom, USpringArmComponent::SocketName);
    FollowCamera->bUsePawnControlRotation = false;

    ExertionComponent = CreateDefaultSubobject<UImagineExertionComponent>(TEXT("ExertionComponent"));

    bUseControllerRotationYaw = false;
    GetCharacterMovement()->bOrientRotationToMovement = true;
}

void AImaginePlayerCharacter::BeginPlay()
{
    Super::BeginPlay();

    if (APlayerController* PC = Cast<APlayerController>(GetController()))
    {
        if (ULocalPlayer* LP = PC->GetLocalPlayer())
        {
            if (UEnhancedInputLocalPlayerSubsystem* Subsystem = LP->GetSubsystem<UEnhancedInputLocalPlayerSubsystem>())
            {
                if (DefaultMappingContext)
                {
                    Subsystem->AddMappingContext(DefaultMappingContext, 0);
                }
            }
        }
    }

    if (ExertionComponent)
    {
        ExertionComponent->OnExhausted.AddDynamic(this, &AImaginePlayerCharacter::HandleExhausted);
    }
}

void AImaginePlayerCharacter::Tick(float DeltaSeconds)
{
    Super::Tick(DeltaSeconds);

    if (ExertionComponent)
    {
        ExertionComponent->RecoverExertion(DeltaSeconds);
    }
}

void AImaginePlayerCharacter::SetupPlayerInputComponent(UInputComponent* PlayerInputComponent)
{
    Super::SetupPlayerInputComponent(PlayerInputComponent);

    if (UEnhancedInputComponent* EnhancedInput = Cast<UEnhancedInputComponent>(PlayerInputComponent))
    {
        if (MoveAction)
        {
            EnhancedInput->BindAction(MoveAction, ETriggerEvent::Triggered, this, &AImaginePlayerCharacter::Move);
        }
        if (LookAction)
        {
            EnhancedInput->BindAction(LookAction, ETriggerEvent::Triggered, this, &AImaginePlayerCharacter::Look);
        }
        if (ExertAction)
        {
            EnhancedInput->BindAction(ExertAction, ETriggerEvent::Triggered, this, &AImaginePlayerCharacter::PerformExertAction);
        }
    }
}

void AImaginePlayerCharacter::Move(const FInputActionValue& Value)
{
    const FVector2D MovementVector = Value.Get<FVector2D>();

    if (Controller)
    {
        const FRotator Rotation = Controller->GetControlRotation();
        const FRotator YawRotation(0.0f, Rotation.Yaw, 0.0f);

        const FVector ForwardDirection = FRotationMatrix(YawRotation).GetUnitAxis(EAxis::X);
        const FVector RightDirection = FRotationMatrix(YawRotation).GetUnitAxis(EAxis::Y);

        AddMovementInput(ForwardDirection, MovementVector.Y);
        AddMovementInput(RightDirection, MovementVector.X);
    }
}

void AImaginePlayerCharacter::Look(const FInputActionValue& Value)
{
    const FVector2D LookAxis = Value.Get<FVector2D>();

    AddControllerYawInput(LookAxis.X);
    AddControllerPitchInput(LookAxis.Y);
}

void AImaginePlayerCharacter::PerformExertAction(const FInputActionValue& Value)
{
    if (ExertionComponent)
    {
        ExertionComponent->AddExertion(15.0f);
    }

    if (ABurdenProgressActor* BurdenActor = FindNearestBurdenActor())
    {
        BurdenActor->Interact();
    }
}

ABurdenProgressActor* AImaginePlayerCharacter::FindNearestBurdenActor() const
{
    UWorld* World = GetWorld();
    if (!World)
    {
        return nullptr;
    }

    ABurdenProgressActor* BestActor = nullptr;
    float BestDistanceSq = InteractRange * InteractRange;

    for (TActorIterator<ABurdenProgressActor> It(World); It; ++It)
    {
        ABurdenProgressActor* Candidate = *It;
        if (!Candidate || Candidate->IsComplete())
        {
            continue;
        }

        const float DistanceSq = FVector::DistSquared(GetActorLocation(), Candidate->GetActorLocation());
        if (DistanceSq <= BestDistanceSq)
        {
            BestDistanceSq = DistanceSq;
            BestActor = Candidate;
        }
    }

    return BestActor;
}

void AImaginePlayerCharacter::HandleExhausted()
{
    if (GEngine)
    {
        GEngine->AddOnScreenDebugMessage(-1, 2.0f, FColor::Orange, TEXT("Exhausted!"));
    }
}
