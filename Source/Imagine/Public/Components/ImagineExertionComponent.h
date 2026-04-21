#pragma once

#include "CoreMinimal.h"
#include "Components/ActorComponent.h"
#include "ImagineExertionComponent.generated.h"

DECLARE_DYNAMIC_MULTICAST_DELEGATE_ThreeParams(FOnExertionChangedSignature, float, CurrentExertion, float, MaxExertion, float, Normalized);
DECLARE_DYNAMIC_MULTICAST_DELEGATE(FOnExhaustedSignature);

UCLASS(ClassGroup=(Imagine), meta=(BlueprintSpawnableComponent))
class IMAGINE_API UImagineExertionComponent : public UActorComponent
{
    GENERATED_BODY()

public:
    UImagineExertionComponent();

    UPROPERTY(EditAnywhere, BlueprintReadOnly, Category="Exertion")
    float MaxExertion = 100.0f;

    UPROPERTY(EditAnywhere, BlueprintReadOnly, Category="Exertion")
    float RecoveryPerSecond = 8.0f;

    UPROPERTY(EditAnywhere, BlueprintReadOnly, Category="Exertion")
    float ExhaustedThreshold = 100.0f;

    UPROPERTY(BlueprintAssignable, Category="Exertion")
    FOnExertionChangedSignature OnExertionChanged;

    UPROPERTY(BlueprintAssignable, Category="Exertion")
    FOnExhaustedSignature OnExhausted;

    UFUNCTION(BlueprintCallable, Category="Exertion")
    void AddExertion(float Amount);

    UFUNCTION(BlueprintCallable, Category="Exertion")
    void RecoverExertion(float DeltaSeconds);

    UFUNCTION(BlueprintPure, Category="Exertion")
    float GetCurrentExertion() const { return CurrentExertion; }

    UFUNCTION(BlueprintPure, Category="Exertion")
    float GetExertionNormalized() const;

    UFUNCTION(BlueprintPure, Category="Exertion")
    bool IsExhausted() const { return bIsExhausted; }

protected:
    virtual void BeginPlay() override;

private:
    UPROPERTY(VisibleAnywhere, Category="Exertion")
    float CurrentExertion = 0.0f;

    UPROPERTY(VisibleAnywhere, Category="Exertion")
    bool bIsExhausted = false;

    void BroadcastExertionChanged();
};
