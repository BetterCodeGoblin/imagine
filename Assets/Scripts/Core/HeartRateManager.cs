using UnityEngine;
using System;
using System.Collections.Generic;

/// <summary>
/// HeartRateManager — BLE Heart Rate Monitor integration.
/// 
/// MVP: Stub implementation. Detects BLE HRM (0x180D) and parses BPM + RR intervals.
/// v1.0: Full BLE integration with Karvonen formula zone calculation, HRV recovery scoring.
/// 
/// Singleton that runs across all modes. Provides:
/// - Real-time HR zone detection (1-5)
/// - HR data for EPOC calculation
/// - RR interval → HRV baseline tracking
/// - Recovery recommendations
/// </summary>
public class HeartRateManager : MonoBehaviour
{
    [System.Serializable]
    public struct HRData
    {
        public int BPM;
        public int[] RRIntervals; // milliseconds between beats
        public int HRZone; // 1-5
        public DateTime Timestamp;
    }

    public static HeartRateManager Instance { get; private set; }

    [Header("HR Zone Configuration")]
    [SerializeField] private int _age = 30;
    [SerializeField] private int _restingHR = 60;
    [SerializeField] private int _maxHR = 190; // auto-calculated as 220 - age, but can be field-tested

    [Header("Debug/Simulation")]
    public bool simulateHeartRate = false;
    public int simulatedBPM = 120;

    // Current state
    private int _currentBPM = 0;
    private int _currentHRZone = 0;
    private bool _isConnected = false;
    private HRData _lastReading;

    // HRV tracking (rolling 7-day baseline)
    private List<float> _rmssdBaseline = new List<float>();
    private const int HRV_BASELINE_SAMPLES = 7;

    public event Action<HRData> OnHRDataReceived;
    public event Action<string> OnConnectionStatusChanged;

    public int CurrentHRZone => _currentHRZone;
    public int CurrentBPM => _currentBPM;
    public bool IsConnected => _isConnected;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Calculate max HR if not field-tested
        if (_maxHR == 190)
            _maxHR = 220 - _age;
    }

    private void Start()
    {
        if (simulateHeartRate)
        {
            _isConnected = true;
            OnConnectionStatusChanged?.Invoke("HR Simulation enabled");
            Debug.Log("[HeartRateManager] Heart rate simulation mode.");
            return;
        }

        // TODO: Implement BLE HRM scanning and connection
        // For now, this is a stub that waits for external data injection
        OnConnectionStatusChanged?.Invoke("Waiting for BLE HRM connection...");
    }

    private void Update()
    {
        if (simulateHeartRate)
        {
            _currentBPM = simulatedBPM + Random.Range(-3, 4); // Add noise
            _currentHRZone = CalculateHRZone(_currentBPM);
        }
    }

    /// <summary>
    /// Injects HR data (called by BLE scanning layer once implemented).
    /// </summary>
    public void InjectHRData(int bpm, int[] rrIntervals = null)
    {
        _currentBPM = bpm;
        _currentHRZone = CalculateHRZone(bpm);

        _lastReading = new HRData
        {
            BPM = bpm,
            RRIntervals = rrIntervals,
            HRZone = _currentHRZone,
            Timestamp = DateTime.Now
        };

        // Calculate HRV if RR intervals available
        if (rrIntervals != null && rrIntervals.Length > 1)
        {
            float rmssd = CalculateRMSSD(rrIntervals);
            UpdateHRVBaseline(rmssd);
        }

        OnHRDataReceived?.Invoke(_lastReading);
    }

    /// <summary>
    /// Calculates HR zone using Karvonen formula.
    /// Zone 1: <60% HRR (recovery)
    /// Zone 2: 60-70% HRR (aerobic base)
    /// Zone 3: 71-80% HRR (aerobic power)
    /// Zone 4: 81-90% HRR (threshold)
    /// Zone 5: 91-100% HRR (VO2 max)
    /// </summary>
    private int CalculateHRZone(int bpm)
    {
        float hrr = _maxHR - _restingHR;
        float percentHRR = (_currentBPM - _restingHR) / hrr;

        if (percentHRR < 0.60f) return 1;
        if (percentHRR < 0.70f) return 2;
        if (percentHRR < 0.80f) return 3;
        if (percentHRR < 0.90f) return 4;
        return 5;
    }

    /// <summary>
    /// Calculates RMSSD (Root Mean Square of Successive Differences).
    /// Lower RMSSD = higher sympathetic tone = less recovered.
    /// Higher RMSSD = higher parasympathetic tone = better recovered.
    /// </summary>
    private float CalculateRMSSD(int[] rrIntervals)
    {
        if (rrIntervals.Length < 2) return 0f;

        float sumSquares = 0f;
        for (int i = 0; i < rrIntervals.Length - 1; i++)
        {
            float diff = rrIntervals[i + 1] - rrIntervals[i];
            sumSquares += diff * diff;
        }

        float meanSquare = sumSquares / (rrIntervals.Length - 1);
        return Mathf.Sqrt(meanSquare);
    }

    /// <summary>
    /// Maintains rolling 7-day HRV baseline. Used to detect recovery state.
    /// </summary>
    private void UpdateHRVBaseline(float rmssd)
    {
        _rmssdBaseline.Add(rmssd);
        if (_rmssdBaseline.Count > HRV_BASELINE_SAMPLES)
            _rmssdBaseline.RemoveAt(0);
    }

    /// <summary>
    /// Returns recovery status based on current RMSSD vs baseline.
    /// </summary>
    public string GetRecoveryStatus()
    {
        if (_rmssdBaseline.Count < 3)
            return "insufficient data";

        float avgBaseline = 0f;
        foreach (var val in _rmssdBaseline)
            avgBaseline += val;
        avgBaseline /= _rmssdBaseline.Count;

        if (_lastReading.RRIntervals == null || _lastReading.RRIntervals.Length < 2)
            return "no RR data";

        float currentRMSSD = CalculateRMSSD(_lastReading.RRIntervals);
        float suppression = (avgBaseline - currentRMSSD) / avgBaseline;

        if (suppression > 0.20f) return "poor"; // >20% suppression
        if (suppression > 0.10f) return "moderate";
        return "good";
    }

    public HRData GetLastReading() => _lastReading;

    // Stub: Will be implemented in v1.0
    public bool TryConnectBLE(string deviceName = null)
    {
        Debug.Log("[HeartRateManager] BLE HRM connection is a v1.0 feature. Using simulation or manual injection.");
        return false;
    }
}
