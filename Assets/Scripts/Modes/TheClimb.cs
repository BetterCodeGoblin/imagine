using UnityEngine;

/// <summary>
/// TheClimb — Rowing mode for Concept 2 RowErg / BikeErg.
/// 
/// Mechanics:
/// - SPM + drive power = boat velocity
/// - Drive ratio (form bonus) for smooth strokes
/// - Drift on rest, regression between sessions
/// - HR zone feedback for training zones (Z2 base, Z4 intervals)
/// - Interval demands (Tabata, 2:1, pyramid) in later zones
/// </summary>
public class TheClimb : GameModeBase
{
    [Header("Rowing-Specific Settings")]
    [SerializeField] private float metersPerWatt = 0.05f; // Tuning constant: watts → altitude conversion
    [SerializeField] private float formBonusMultiplier = 1.3f; // Drive ratio bonus multiplier
    [SerializeField] private float minSPMForProgress = 14f; // Below this, no forward movement

    private Concept2Manager concept2Manager;
    private float accumulatedProgress = 0f;
    private int strokeCount = 0;
    private float sessionEPOCScore = 0f;

    protected override void Awake()
    {
        base.Awake();

        config = new ModeConfig
        {
            ModeName = "TheClimb",
            SessionTimeLimit = 0f, // Unlimited
            DriftRate = 1f
        };

        concept2Manager = Concept2Manager.Instance;
    }

    private void Start()
    {
        // Subscribe to stroke events
        if (concept2Manager != null)
        {
            concept2Manager.OnStrokeDataReceived += OnStrokeReceived;
            concept2Manager.OnConnectionStatusChanged += OnPM5ConnectionChanged;
            Debug.Log("[TheClimb] Subscribed to Concept2Manager events.");
        }
    }

    private void OnPM5ConnectionChanged(string status)
    {
        TriggerNarrator($"PM5: {status}");
    }

    /// <summary>
    /// Called whenever a new rowing stroke is detected.
    /// </summary>
    private void OnStrokeReceived(Concept2Manager.RowingStrokeData stroke)
    {
        if (!isSessionActive) return;

        strokeCount++;

        // Base progress from power output
        float baseProgress = stroke.PowerWatts * metersPerWatt;

        // SPM gate: below minimum SPM, no progress
        if (stroke.StrokesPerMinute < minSPMForProgress)
        {
            TriggerNarrator("Your stroke rate is too low. The current is too strong.");
            return;
        }

        // Form bonus: drive ratio quality
        float formBonus = concept2Manager.CalculateFormBonus(stroke);
        if (formBonus > 0f)
        {
            baseProgress *= (1f + (formBonus - 1f) * formBonusMultiplier);
        }

        // HR zone multiplier (Z5 is fastest, Z1 is slowest but stable)
        int hrZone = GetCurrentHRZone();
        float hrMultiplier = GetZoneMultiplier(hrZone);
        baseProgress *= hrMultiplier;

        // Add to session progress
        AddProgress(baseProgress);
        accumulatedProgress += baseProgress;

        // EPOC contribution (intensity-weighted)
        sessionEPOCScore += CalculateEPOCContribution(stroke, hrZone);

        // Narrator feedback (sparse — only at milestones)
        if (strokeCount % 100 == 0) // Every ~2-3 minutes at 22 SPM
        {
            float currentAltitude = boulderSystem.GetCurrentSessionAltitude();
            TriggerNarrator($"You've climbed {currentAltitude:F0} meters. Keep the rhythm steady.");
        }

        Debug.Log($"[TheClimb] Stroke #{strokeCount}: power={stroke.PowerWatts}W, SPM={stroke.StrokesPerMinute}, formBonus={formBonus:F2}, progress=+{baseProgress:F1}m");
    }

    /// <summary>
    /// Get progress multiplier based on HR zone.
    /// Zone 2 (base) = 1.0x
    /// Zone 4 (threshold) = 1.3x
    /// Zone 5 (VO2 Max) = 1.5x
    /// </summary>
    private float GetZoneMultiplier(int zone)
    {
        return zone switch
        {
            1 => 0.7f, // Recovery — slow, struggling
            2 => 1.0f, // Aerobic base — standard
            3 => 1.15f, // Aerobic power — working
            4 => 1.3f, // Threshold — pushing
            5 => 1.5f, // VO2 Max — red zone
            _ => 1.0f
        };
    }

    /// <summary>
    /// Contribute to EPOC (afterburn) score based on intensity.
    /// </summary>
    private float CalculateEPOCContribution(Concept2Manager.RowingStrokeData stroke, int hrZone)
    {
        // Base: time in zone (seconds per stroke at this SPM)
        float secPerStroke = 60f / Mathf.Max(stroke.StrokesPerMinute, 10f);

        // Zone weight
        float zoneWeight = hrZone switch
        {
            1 => 0.2f,
            2 => 1.0f,
            3 => 1.5f,
            4 => 2.5f,
            5 => 5.0f,
            _ => 1.0f
        };

        return secPerStroke * zoneWeight;
    }

    public override void EndSession(float epocScore = 0f)
    {
        base.EndSession(sessionEPOCScore);
        Debug.Log($"[TheClimb] Session stats: {strokeCount} strokes, {accumulatedProgress:F0}m total progress, EPOC score {sessionEPOCScore:F1}");
    }

    private void OnDestroy()
    {
        if (concept2Manager != null)
        {
            concept2Manager.OnStrokeDataReceived -= OnStrokeReceived;
            concept2Manager.OnConnectionStatusChanged -= OnPM5ConnectionChanged;
        }
    }
}
