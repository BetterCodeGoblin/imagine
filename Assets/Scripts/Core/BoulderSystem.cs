using UnityEngine;
using System;

/// <summary>
/// BoulderSystem — Tracks player progress, handles regression, and manages persistent state.
/// 
/// Core mechanics:
/// - Lifetime progress (permanent)
/// - Session progress (resets each session)
/// - Regression system (between-session detraining)
/// - Mid-session drift (rest causes backward movement)
/// - EPOC (afterburn) suspension of regression
/// </summary>
public class BoulderSystem : MonoBehaviour
{
    [System.Serializable]
    public class SessionData
    {
        public DateTime StartTime;
        public DateTime EndTime;
        public string GameMode; // "Climb", "Summit", "Burden"
        public float SessionAltitude; // distance/TUT this session
        public float LifetimeAltitude; // persistent total
        public float AverageHRZone;
        public float EPOCScore;
        public float AfterburnDurationHours;
        public bool IsActive;
    }

    [System.Serializable]
    public class BoulderState
    {
        public float LifetimeAltitude; // primary persistent stat
        public float LifetimeTUT; // total time under tension (Burden mode)
        public float LifetimeSessionHours;
        public DateTime LastSessionEnd;
        public DateTime AfterburnExpiryTime; // when EPOC suspension ends
        public string BoulderCustomization; // JSON: stone type, markings, name
    }

    public static BoulderSystem Instance { get; private set; }

    private BoulderState boulderState = new BoulderState();
    private SessionData currentSession = new SessionData();

    private const float BASE_DRIFT_RATE = 1f; // units/second at rest
    private const float DRIFT_ACCELERATION_FACTOR = 1.5f;
    private const float MAX_DRIFT_MULTIPLIER = 3f;
    private const float DRIFT_REST_THRESHOLD_SECONDS = 2f;

    private float timeSinceLastInput = 0f;
    private float currentDriftMultiplier = 1f;

    public event Action<float> OnDriftOccurred;
    public event Action<string> OnProgressChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        LoadPersistedState();
    }

    private void Update()
    {
        if (!currentSession.IsActive) return;

        timeSinceLastInput += Time.deltaTime;

        // Check if resting
        if (timeSinceLastInput > DRIFT_REST_THRESHOLD_SECONDS)
        {
            ApplyDrift();
        }
    }

    /// <summary>
    /// Starts a new session. Sets up regression offset based on time since last session.
    /// </summary>
    public void StartSession(string gameMode)
    {
        currentSession = new SessionData
        {
            StartTime = DateTime.Now,
            GameMode = gameMode,
            SessionAltitude = 0f,
            LifetimeAltitude = boulderState.LifetimeAltitude - CalculateRegressionOffset(),
            IsActive = true
        };

        // Clamp regression — never drop below zero
        currentSession.LifetimeAltitude = Mathf.Max(0, currentSession.LifetimeAltitude);

        OnProgressChanged?.Invoke($"Session started: {gameMode}. Altitude: {currentSession.LifetimeAltitude:F0}m");
    }

    /// <summary>
    /// Registers a workout output (rowing distance, cycling altitude, TUT, etc.)
    /// </summary>
    public void AddProgress(float amount)
    {
        if (!currentSession.IsActive) return;

        currentSession.SessionAltitude += amount;
        currentSession.LifetimeAltitude += amount;
        boulderState.LifetimeAltitude = currentSession.LifetimeAltitude;

        timeSinceLastInput = 0f; // Reset rest timer
        currentDriftMultiplier = 1f; // Reset drift acceleration

        OnProgressChanged?.Invoke($"Progress: +{amount:F1} (Session: {currentSession.SessionAltitude:F0}, Lifetime: {boulderState.LifetimeAltitude:F0})");
    }

    /// <summary>
    /// Applies drift when player is resting mid-session.
    /// </summary>
    private void ApplyDrift()
    {
        // Accelerate drift the longer the rest persists
        currentDriftMultiplier = Mathf.Min(MAX_DRIFT_MULTIPLIER, currentDriftMultiplier + DRIFT_ACCELERATION_FACTOR * Time.deltaTime);

        float drift = BASE_DRIFT_RATE * currentDriftMultiplier * Time.deltaTime;

        // HR-modified drift: if HR monitor is connected and HR is still elevated, reduce drift
        float hrModifier = 1f;
        if (HeartRateManager.Instance != null && HeartRateManager.Instance.CurrentHRZone >= 3)
        {
            hrModifier = 0.7f; // 30% reduction in drift if still elevated
        }

        drift *= hrModifier;

        // Apply drift to current progress only, not lifetime
        currentSession.LifetimeAltitude -= drift;
        currentSession.LifetimeAltitude = Mathf.Max(0, currentSession.LifetimeAltitude);

        OnDriftOccurred?.Invoke(drift);
    }

    /// <summary>
    /// Calculates regression offset based on days offline.
    /// Formula: base_rate * log(days_offline + 1)
    /// Recovery day (1 day) gets minimal penalty to reflect real physiology.
    /// </summary>
    private float CalculateRegressionOffset()
    {
        if (boulderState.LastSessionEnd == DateTime.MinValue)
            return 0f; // First session, no regression

        TimeSpan timeSinceLastSession = DateTime.Now - boulderState.LastSessionEnd;
        float daysOffline = (float)timeSinceLastSession.TotalDays;

        if (daysOffline < 1f)
            return 0f; // Same day or next day, minimal regression

        // Check if in EPOC/afterburn window
        if (DateTime.Now < boulderState.AfterburnExpiryTime)
        {
            return 0f; // Regression suspended during EPOC
        }

        // Recovery day (1 day) gets minimal regression
        if (daysOffline <= 1f)
            return currentSession.LifetimeAltitude * 0.02f; // 2% penalty only

        // Standard detraining curve
        float baseRegressionRate = 0.05f; // 5% per day baseline
        float regression = currentSession.LifetimeAltitude * baseRegressionRate * Mathf.Log(daysOffline + 1f);

        return Mathf.Min(regression, currentSession.LifetimeAltitude * 0.3f); // Cap at 30%
    }

    /// <summary>
    /// Ends the current session. Calculates EPOC score and saves progress.
    /// </summary>
    public void EndSession(float epocScore = 0f)
    {
        if (!currentSession.IsActive) return;

        currentSession.EndTime = DateTime.Now;
        currentSession.EPOCScore = epocScore;

        // EPOC afterburn window calculation
        // Zone 2 base: 4 hours, Threshold intervals: 8 hours, VO2 Max: 12-16 hours, HIT: 24-48 hours
        currentSession.AfterburnDurationHours = CalculateAfterburnDuration(epocScore);
        boulderState.AfterburnExpiryTime = DateTime.Now.AddHours(currentSession.AfterburnDurationHours);

        currentSession.IsActive = false;

        // Persist to disk
        SavePersistedState();

        OnProgressChanged?.Invoke($"Session ended. Total altitude: {currentSession.LifetimeAltitude:F0}m. EPOC duration: {currentSession.AfterburnDurationHours:F1}h");
    }

    /// <summary>
    /// Calculates EPOC afterburn duration based on session intensity.
    /// </summary>
    private float CalculateAfterburnDuration(float epocScore)
    {
        // Simplified: epocScore represents total session intensity
        if (epocScore < 20f) return 4f; // Zone 2 base
        if (epocScore < 60f) return 8f; // Threshold intervals
        if (epocScore < 120f) return 12f; // VO2 Max
        return 24f; // HIT / very high intensity
    }

    /// <summary>
    /// Records TUT for strength mode.
    /// </summary>
    public void AddTUT(float tut)
    {
        boulderState.LifetimeTUT += tut;
        OnProgressChanged?.Invoke($"TUT added: +{tut:F1}s (Lifetime TUT: {boulderState.LifetimeTUT:F0}s)");
    }

    public float GetCurrentSessionAltitude() => currentSession.SessionAltitude;
    public float GetLifetimeAltitude() => boulderState.LifetimeAltitude;
    public float GetLifetimeTUT() => boulderState.LifetimeTUT;
    public BoulderState GetBoulderState() => boulderState;
    public SessionData GetCurrentSession() => currentSession;
    public float GetAfterburnRemainingHours()
    {
        var remaining = boulderState.AfterburnExpiryTime - DateTime.Now;
        return (float)remaining.TotalHours;
    }

    private void SavePersistedState()
    {
        // TODO: Save to SQLite or PlayerPrefs
        string json = JsonUtility.ToJson(boulderState);
        PlayerPrefs.SetString("BoulderState", json);
        PlayerPrefs.Save();
    }

    private void LoadPersistedState()
    {
        if (PlayerPrefs.HasKey("BoulderState"))
        {
            string json = PlayerPrefs.GetString("BoulderState");
            JsonUtility.FromJsonOverwrite(json, boulderState);
        }
    }
}
