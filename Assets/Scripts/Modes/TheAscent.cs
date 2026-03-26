using UnityEngine;

/// <summary>
/// TheAscent (placeholder name) — Cycling mode for smart trainers / power meters.
/// 
/// Future implementation (v1.0):
/// - FTP test protocol
/// - Power zone tracking (56-120% FTP)
/// - ANT+/BLE power meter integration
/// - Smart trainer resistance control (FTMS)
/// - Day/night cycle visual
/// - Gradient events (30/30, 40/20, Tabata climbs)
/// 
/// MVP: Stub with comments showing full architecture.
/// </summary>
public class TheAscent : GameModeBase
{
    [Header("Cycling-Specific Settings")]
    [SerializeField] private float ftpWatts = 250f; // Functional Threshold Power (will be field-tested)
    [SerializeField] private float wattsToPowerMultiplier = 0.08f; // Watts → altitude conversion

    private float sessionEPOCScore = 0f;
    private int sampleCount = 0;

    protected override void Awake()
    {
        base.Awake();

        config = new ModeConfig
        {
            ModeName = "TheAscent",
            SessionTimeLimit = 0f, // Unlimited
            DriftRate = 1f
        };
    }

    private void Start()
    {
        // TODO: v1.0 — Wire up ANT+/BLE power meter and smart trainer
        // For now, this is a stub awaiting hardware integration.
        TriggerNarrator("The Summit awaits. [v1.0 cycling integration pending]");
    }

    protected override void Update()
    {
        base.Update();

        if (!isSessionActive) return;

        // TODO: Power meter polling and altitude update
        // Placeholder: slow auto-progress for testing
        if (sampleCount % 60 == 0) // Every ~1 second
        {
            float mockPower = 180f + Random.Range(-20f, 20f);
            float mockProgress = mockPower * wattsToPowerMultiplier * Time.deltaTime;
            AddProgress(mockProgress);
            sessionEPOCScore += (mockPower / ftpWatts) * Time.deltaTime;
            sampleCount = 0;
        }
        sampleCount++;
    }

    /// <summary>
    /// FTP Test Protocol (v1.0).
    /// Guides player through 20-minute sustained effort.
    /// Records average power, sets FTP to 95% of that.
    /// </summary>
    public void StartFTPTest()
    {
        Debug.Log("[TheAscent] FTP test protocol (stub). Full implementation in v1.0.");
        TriggerNarrator("FTP test is coming in v1.0. For now, manually set your FTP in settings.");
    }

    public override void EndSession(float epocScore = 0f)
    {
        base.EndSession(sessionEPOCScore);
        Debug.Log($"[TheAscent] Session ended. EPOC score: {sessionEPOCScore:F1}");
    }
}
