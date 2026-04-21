#include "Components/ImagineExertionComponent.h"

UImagineExertionComponent::UImagineExertionComponent()
{
    PrimaryComponentTick.bCanEverTick = false;
}

void UImagineExertionComponent::BeginPlay()
{
    Super::BeginPlay();
    CurrentExertion = 0.0f;
    bIsExhausted = false;
    BroadcastExertionChanged();
}

void UImagineExertionComponent::AddExertion(float Amount)
{
    if (Amount <= 0.0f)
    {
        return;
    }

    CurrentExertion = FMath::Clamp(CurrentExertion + Amount, 0.0f, MaxExertion);

    const bool bWasExhausted = bIsExhausted;
    bIsExhausted = CurrentExertion >= ExhaustedThreshold;

    BroadcastExertionChanged();

    if (!bWasExhausted && bIsExhausted)
    {
        OnExhausted.Broadcast();
    }
}

void UImagineExertionComponent::RecoverExertion(float DeltaSeconds)
{
    if (DeltaSeconds <= 0.0f || CurrentExertion <= 0.0f)
    {
        return;
    }

    CurrentExertion = FMath::Clamp(CurrentExertion - (RecoveryPerSecond * DeltaSeconds), 0.0f, MaxExertion);
    bIsExhausted = CurrentExertion >= ExhaustedThreshold;
    BroadcastExertionChanged();
}

float UImagineExertionComponent::GetExertionNormalized() const
{
    return MaxExertion > 0.0f ? CurrentExertion / MaxExertion : 0.0f;
}

void UImagineExertionComponent::BroadcastExertionChanged()
{
    OnExertionChanged.Broadcast(CurrentExertion, MaxExertion, GetExertionNormalized());
}
