using UnityEngine;

/// <summary>
/// TheBurden — Strength machine mode (HIT protocol).
/// 
/// Mechanics:
/// - Time Under Tension (TUT) tracking
/// - Tempo validation (4-6 seconds concentric, 4-6 seconds eccentric)
/// - One working set to momentary muscular failure
/// - Rest timer enforcement (2-3 minutes between sets)
/// - Muscle group unlock system
/// - Progressive overload prompts
/// 
/// MVP: Manual rep logging. v1.0: Smart barbell/machine sensor integration.
/// </summary>
public class TheBurden : GameModeBase
{
    [System.Serializable]
    public struct SetData
    {
        public int RepsCompleted;
        public float LoadKg;
        public float TempoSecondsUp;
        public float TempoSecondsDown;
        public bool ReachedMuscularFailure;
        public string MuscleGroupName; // "Chest", "Legs", "Back", etc.
    }

    [Header("Strength-Specific Settings")]
    [SerializeField] private float tutToAltitudeMultiplier = 0.1f; // TUT seconds → altitude meters
    [SerializeField] private float minRestTimeSec = 120f; // 2 minutes minimum
    [SerializeField] private float recommendedRestTimeSec = 180f; // 3 minutes ideal

    private float sessionEPOCScore = 0f;
    private float currentSetTUT = 0f;
    private float restTimerRemaining = 0f;
    private bool isRestingBetweenSets = false;
    private SetData lastSetData;
    private int setCount = 0;

    protected override void Awake()
    {
        base.Awake();

        config = new ModeConfig
        {
            ModeName = "TheBurden",
            SessionTimeLimit = 0f, // Unlimited
            DriftRate = 1f
        };
    }

    private void Start()
    {
        TriggerNarrator("Pick up the boulder. One set to failure. You know what to do.");
    }

    protected override void Update()
    {
        base.Update();

        if (!isSessionActive) return;

        // Rest timer countdown
        if (isRestingBetweenSets)
        {
            restTimerRemaining -= Time.deltaTime;
            if (restTimerRemaining <= 0f)
            {
                isRestingBetweenSets = false;
                TriggerNarrator("You're ready. Pick up the boulder.");
            }
        }
    }

    /// <summary>
    /// Logs a completed set.
    /// </summary>
    public void LogSet(SetData setData)
    {
        if (!isSessionActive)
        {
            StartSession();
        }

        setCount++;

        // Calculate TUT (Time Under Tension)
        float rawTUT = setData.RepsCompleted * (setData.TempoSecondsUp + setData.TempoSecondsDown);

        // Penalty for explosive (sub-3s concentric)
        if (setData.TempoSecondsUp < 3f)
        {
            rawTUT *= 0.5f;
            TriggerNarrator("The weight moved too fast. That's momentum, not muscle.");
        }

        // Bonus for reaching muscular failure
        if (setData.ReachedMuscularFailure)
        {
            rawTUT *= 1.2f;
            TriggerNarrator("That last rep you couldn't finish — that's the one that mattered.");
        }

        // Convert TUT to altitude progress
        float altitude = rawTUT * tutToAltitudeMultiplier;
        AddProgress(altitude);

        // Log to BoulderSystem TUT tracker
        boulderSystem.AddTUT(rawTUT);

        lastSetData = setData;
        currentSetTUT = rawTUT;

        // EPOC contribution (hard set to failure = high EPOC)
        if (setData.ReachedMuscularFailure)
            sessionEPOCScore += rawTUT * 0.5f; // High intensity

        Debug.Log($"[TheBurden] Set #{setCount}: {setData.RepsCompleted} reps @ {setData.LoadKg}kg, TUT={rawTUT:F1}s, failure={setData.ReachedMuscularFailure}");

        // Start rest timer
        StartRest();
    }

    /// <summary>
    /// Starts the mandatory rest timer between sets.
    /// </summary>
    private void StartRest()
    {
        isRestingBetweenSets = true;
        restTimerRemaining = recommendedRestTimeSec;

        TriggerNarrator($"Rest. {recommendedRestTimeSec:F0} seconds. The muscle needs it.");
    }

    /// <summary>
    /// Progressive overload prompt: check if load hasn't increased in 3 sessions.
    /// </summary>
    public void CheckProgressiveOverload()
    {
        // TODO: Query history from BoulderSystem, check last 3 sessions at same exercise
        TriggerNarrator("The boulder hasn't changed weight in three sessions. It's time to make it heavier.");
    }

    public override void EndSession(float epocScore = 0f)
    {
        base.EndSession(sessionEPOCScore);
        Debug.Log($"[TheBurden] Session ended. {setCount} sets completed, total TUT: {boulderSystem.GetLifetimeTUT():F0}s");
    }

    // Public accessors
    public bool IsResting => isRestingBetweenSets;
    public float GetRestTimeRemaining() => restTimerRemaining;
    public float GetRestTimeRecommended() => recommendedRestTimeSec;
    public int GetSetCount() => setCount;
}
