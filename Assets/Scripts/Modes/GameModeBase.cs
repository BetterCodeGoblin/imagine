using UnityEngine;
using System;

/// <summary>
/// GameModeBase — Abstract base class for all three Sisyphean modes.
/// 
/// Defines the interface and shared behavior:
/// - Session lifecycle (Start → Input Loop → End)
/// - Progress tracking (output metrics → BoulderSystem)
/// - HR zone integration
/// - Narrator callback hooks
/// </summary>
public abstract class GameModeBase : MonoBehaviour
{
    [System.Serializable]
    public struct ModeConfig
    {
        public string ModeName; // "Climb", "Summit", "Burden"
        public float SessionTimeLimit; // seconds, 0 = unlimited
        public float DriftRate; // units/second at rest
    }

    protected ModeConfig config;
    protected BoulderSystem boulderSystem;
    protected HeartRateManager hrManager;

    protected float sessionElapsedTime = 0f;
    protected bool isSessionActive = false;
    protected float lastProgressTime = 0f;

    public event Action<string> OnNarratorLine; // Narrator should listen to this
    public event Action OnSessionStarted;
    public event Action OnSessionEnded;

    protected virtual void Awake()
    {
        boulderSystem = BoulderSystem.Instance;
        hrManager = HeartRateManager.Instance;
    }

    /// <summary>
    /// Start a new session. Call this to begin gameplay.
    /// </summary>
    public virtual void StartSession()
    {
        if (isSessionActive) return;

        isSessionActive = true;
        sessionElapsedTime = 0f;
        lastProgressTime = 0f;

        boulderSystem.StartSession(config.ModeName);
        OnNarratorLine?.Invoke($"[{config.ModeName}] Starting session...");
        OnSessionStarted?.Invoke();

        Debug.Log($"[{config.ModeName}] Session started.");
    }

    /// <summary>
    /// End the current session. Call this when player quits or natural session end.
    /// </summary>
    public virtual void EndSession(float epocScore = 0f)
    {
        if (!isSessionActive) return;

        isSessionActive = false;
        boulderSystem.EndSession(epocScore);
        OnNarratorLine?.Invoke($"[{config.ModeName}] Session ended. Total altitude: {boulderSystem.GetLifetimeAltitude():F0}m");
        OnSessionEnded?.Invoke();

        Debug.Log($"[{config.ModeName}] Session ended.");
    }

    protected virtual void Update()
    {
        if (!isSessionActive) return;

        sessionElapsedTime += Time.deltaTime;
        lastProgressTime += Time.deltaTime;

        // Check session time limit
        if (config.SessionTimeLimit > 0 && sessionElapsedTime >= config.SessionTimeLimit)
        {
            OnNarratorLine?.Invoke("Time's up.");
            EndSession();
        }
    }

    /// <summary>
    /// Add progress to the session. Called when player produces output.
    /// </summary>
    protected void AddProgress(float amount)
    {
        boulderSystem.AddProgress(amount);
        lastProgressTime = 0f;
    }

    /// <summary>
    /// Get the current HR zone (1-5) from the heart rate manager.
    /// </summary>
    protected int GetCurrentHRZone()
    {
        return hrManager != null ? hrManager.CurrentHRZone : 0;
    }

    /// <summary>
    /// Called by subclasses to trigger narrator voiceover.
    /// </summary>
    protected void TriggerNarrator(string line)
    {
        OnNarratorLine?.Invoke(line);
    }

    public bool IsSessionActive => isSessionActive;
    public float GetSessionElapsedTime() => sessionElapsedTime;
    public float GetTimeSinceLastProgress() => lastProgressTime;
}
