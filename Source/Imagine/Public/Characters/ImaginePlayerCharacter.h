#pragma once

#include "CoreMinimal.h"
#include "GameFramework/Character.h"
#include "ImaginePlayerCharacter.generated.h"

class UImagineExertionComponent;
class USpringArmComponent;
class UCameraComponent;
class UInputMappingContext;
class UInputAction;
class ABurdenProgressActor;
struct FInputActionValue;

UCLASS()
class IMAGINE_API AImaginePlayerCharacter : public ACharacter
{
    GENERATED_BODY()

public:
    AImaginePlayerCharacter();

    virtual void Tick(float DeltaSeconds) override;
    virtual void SetupPlayerInputComponent(class UInputComponent* PlayerInputComponent) override;
    virtual void BeginPlay() override;

protected:
    UPROPERTY(VisibleAnywhere, BlueprintReadOnly, Category="Components")
    TObjectPtr<USpringArmComponent> CameraBoom;

    UPROPERTY(VisibleAnywhere, BlueprintReadOnly, Category="Components")
    TObjectPtr<UCameraComponent> FollowCamera;

    UPROPERTY(VisibleAnywhere, BlueprintReadOnly, Category="Components")
    TObjectPtr<UImagineExertionComponent> ExertionComponent;

    UPROPERTY(EditDefaultsOnly, Category="Input")
    TObjectPtr<UInputMappingContext> DefaultMappingContext;

    UPROPERTY(EditDefaultsOnly, Category="Input")
    TObjectPtr<UInputAction> MoveAction;

    UPROPERTY(EditDefaultsOnly, Category="Input")
    TObjectPtr<UInputAction> LookAction;

    UPROPERTY(EditDefaultsOnly, Category="Input")
    TObjectPtr<UInputAction> ExertAction;


    UPROPERTY(EditAnywhere, BlueprintReadOnly, Category="Imagine")
    float InteractRange = 250.0f;

private:
    void Move(const FInputActionValue& Value);
    void Look(const FInputActionValue& Value);
    void PerformExertAction(const FInputActionValue& Value);
    ABurdenProgressActor* FindNearestBurdenActor() const;
    UFUNCTION()
    void HandleExhausted();
};
