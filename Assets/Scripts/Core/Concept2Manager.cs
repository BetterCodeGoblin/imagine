using UnityEngine;
using System;
using System.Diagnostics;
using System.IO;
using Debug = UnityEngine.Debug;
using Process = System.Diagnostics.Process;

/// <summary>
/// Concept2Manager — Manages Concept 2 PM5 rowing machine integration.
/// 
/// Production implementation: Uses ErgBridge (compiled C# bridge DLL).
/// ErgBridge launches as a separate process and streams data via TCP/IP.
/// 
/// Data flow:
/// 1. Concept2Manager launches ErgBridge.exe process
/// 2. ErgBridgeClient connects via TCP to localhost:6789
/// 3. Polling loop requests data, receives rate/pace/power updates
/// 4. Parsed into session-aware stroke tracking
/// 
/// This architecture avoids platform-specific serial/HID complexity and 
/// leverages existing proven PM5 integration from Strength-ERG-Demo-Wired.
/// </summary>
public class Concept2Manager : MonoBehaviour
{
    [System.Serializable]
    public struct RowingStrokeData
    {
        public float StrokesPerMinute;       // SPM
        public float PaceSecondsPer500m;     // /500m split (seconds per 500m)
        public float PowerWatts;             // instantaneous power
        public float DistanceMeters;         // elapsed distance (cumulative)
        public float DriveRatio;             // DriveTime / (DriveTime + RecoveryTime) — estimated from timing
        public DateTime Timestamp;
    }

    public static Concept2Manager Instance { get; private set; }

    [Header("Bridge Settings")]
    public string bridgeExePath = "ErgBridge/ErgBridge.exe";

    [Header("Debug / Simulation")]
    public bool simulateInput = false;
    public float simulatedStrokeRate = 22f;
    public float simulatedPower = 150f;
    public float simulatedPace = 160f;

    // Live data from bridge
    private float _currentStrokeRate = 0f;
    private float _currentPace = 160f;
    private float _currentPower = 0f;
    private bool _isConnected = false;

    private Process _bridgeProcess;
    private float _connectionStatusLogCooldown = 0f;
    private ErgBridgeClient _bridgeClient;
    private RowingStrokeData _lastStroke;
    private float _timeSinceLastStroke = 0f;
    private const float STROKE_TIMEOUT_SECONDS = 2f;

    public event Action<RowingStrokeData> OnStrokeDataReceived;
    public event Action<string> OnConnectionStatusChanged;
    public event Action OnDisconnected;

    // Public properties
    public bool IsConnected => _isConnected;
    public float CurrentStrokeRate => _currentStrokeRate;
    public float CurrentPower => _currentPower;
    public float CurrentPace => _currentPace;
    public RowingStrokeData LastStroke => _lastStroke;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        if (simulateInput)
        {
            _isConnected = true;
            OnConnectionStatusChanged?.Invoke("Simulation mode enabled");
            Debug.Log("[Concept2Manager] Simulation mode.");
            return;
        }

        LaunchBridge();
        EnsureBridgeClient();
    }

    private void Update()
    {
        if (_connectionStatusLogCooldown > 0f)
            _connectionStatusLogCooldown -= Time.deltaTime;

        if (simulateInput)
        {
            _currentStrokeRate = simulatedStrokeRate;
            _currentPower = simulatedPower;
            _currentPace = simulatedPace;
            _timeSinceLastStroke += Time.deltaTime;

            float simulatedStrokeInterval = Mathf.Max(0.25f, 60f / Mathf.Max(1f, simulatedStrokeRate));
            if (_timeSinceLastStroke >= simulatedStrokeInterval)
            {
                GenerateSimulatedStroke();
                _timeSinceLastStroke = 0f;
            }
            return;
        }

        _timeSinceLastStroke += Time.deltaTime;

        if (_timeSinceLastStroke > STROKE_TIMEOUT_SECONDS && _isConnected)
        {
            _isConnected = false;
            if (_connectionStatusLogCooldown <= 0f)
            {
                OnConnectionStatusChanged?.Invoke("PM5 connected, waiting for next stroke...");
                _connectionStatusLogCooldown = 2f;
            }
        }
    }

    /// <summary>
    /// Launches ErgBridge.exe (the PM5 communication bridge).
    /// </summary>
    private void LaunchBridge()
    {
        try
        {
            string fullPath = ResolveBridgeExecutablePath();

            if (string.IsNullOrWhiteSpace(fullPath) || !File.Exists(fullPath))
            {
                Debug.LogError($"[Concept2Manager] Bridge not found at: {fullPath}");
                OnConnectionStatusChanged?.Invoke($"ErgBridge not found at {fullPath}");
                return;
            }

            if (_bridgeProcess != null && !_bridgeProcess.HasExited)
            {
                Debug.Log("[Concept2Manager] ErgBridge already running.");
                OnConnectionStatusChanged?.Invoke("ErgBridge already running. Waiting for connection...");
                return;
            }

            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = fullPath,
                WorkingDirectory = Path.GetDirectoryName(fullPath),
                UseShellExecute = false,
                CreateNoWindow = true
            };

            _bridgeProcess = Process.Start(psi);
            Debug.Log($"[Concept2Manager] ErgBridge launched (PID: {_bridgeProcess?.Id})");
            OnConnectionStatusChanged?.Invoke("ErgBridge process started. Waiting for connection...");
        }
        catch (Exception ex)
        {
            Debug.LogError($"[Concept2Manager] Failed to launch ErgBridge: {ex.Message}");
            OnConnectionStatusChanged?.Invoke($"Failed to launch bridge: {ex.Message}");
        }
    }

    /// <summary>
    /// Called by ErgBridgeClient when new data arrives from PM5.
    /// </summary>
    private void OnErgDataReceived(float rate, float pace, float power, bool connected)
    {
        _currentStrokeRate = rate;
        _currentPace = pace;
        _currentPower = power;

        if (_isConnected != connected)
        {
            _isConnected = connected;
            OnConnectionStatusChanged?.Invoke(connected ? "PM5 connected" : "PM5 disconnected");
            if (!connected)
                OnDisconnected?.Invoke();
        }

        if (!connected)
            return;

        if (rate > 0f && _timeSinceLastStroke > 0.5f)
        {
            _lastStroke = new RowingStrokeData
            {
                StrokesPerMinute = rate,
                PaceSecondsPer500m = pace,
                PowerWatts = power,
                DriveRatio = EstimateDriveRatio(rate, power),
                Timestamp = DateTime.UtcNow
            };

            _timeSinceLastStroke = 0f;
            OnStrokeDataReceived?.Invoke(_lastStroke);
        }
    }

    /// <summary>
    /// Generates a simulated stroke event for testing without hardware.
    /// </summary>
    private void GenerateSimulatedStroke()
    {
        _lastStroke = new RowingStrokeData
        {
            StrokesPerMinute = simulatedStrokeRate,
            PaceSecondsPer500m = simulatedPace,
            PowerWatts = simulatedPower,
            DriveRatio = EstimateDriveRatio(simulatedStrokeRate, simulatedPower),
            DistanceMeters = Mathf.Max(0f, _lastStroke.DistanceMeters + (500f / Mathf.Max(1f, simulatedPace)) * (60f / Mathf.Max(1f, simulatedStrokeRate))),
            Timestamp = DateTime.UtcNow
        };

        OnStrokeDataReceived?.Invoke(_lastStroke);
    }

    /// <summary>
    /// Estimates drive ratio from stroke rate and power.
    /// Healthy ratio: 0.33–0.40 (1:2 to 1:2.5 drive:recovery).
    /// This is a heuristic — real data comes from PM5 directly in advanced versions.
    /// </summary>
    private float EstimateDriveRatio(float spm, float power)
    {
        // Baseline: moderate effort, good form
        float baseRatio = 0.36f;

        // Very high power can indicate explosive drive (shorter drive phase)
        if (power > 200f)
            baseRatio = 0.33f;
        // Low power suggests slower, less crisp drive
        else if (power < 80f)
            baseRatio = 0.38f;

        // Add slight variation based on SPM
        // Lower SPM tends to mean longer drive phase
        if (spm < 18f)
            baseRatio += 0.02f;
        else if (spm > 26f)
            baseRatio -= 0.02f;

        return Mathf.Clamp(baseRatio, 0.30f, 0.42f);
    }

    /// <summary>
    /// Calculate form bonus based on drive ratio.
    /// Healthy ratio: 0.33–0.40 (1:2 to 1:2.5 drive:recovery).
    /// Returns multiplier: 0.0 to 1.5
    /// </summary>
    public float CalculateFormBonus(RowingStrokeData stroke)
    {
        const float MIN_HEALTHY_RATIO = 0.33f;
        const float MAX_HEALTHY_RATIO = 0.40f;

        if (stroke.DriveRatio < MIN_HEALTHY_RATIO || stroke.DriveRatio > MAX_HEALTHY_RATIO)
            return 0f; // Form penalty — sloppy stroke

        // Smooth bonus curve: 1.0 at perfect midpoint, 0.5 at edges
        float midpoint = (MIN_HEALTHY_RATIO + MAX_HEALTHY_RATIO) / 2f;
        float tolerance = (MAX_HEALTHY_RATIO - MIN_HEALTHY_RATIO) / 2f;
        float deviation = Mathf.Abs(stroke.DriveRatio - midpoint);

        float bonus = Mathf.Max(0.5f, 1f - (deviation / tolerance) * 0.5f);
        return bonus;
    }

    public void Disconnect()
    {
        if (_bridgeClient != null)
            _bridgeClient.OnDataReceived -= OnErgDataReceived;

        if (_bridgeProcess != null)
        {
            try
            {
                if (!_bridgeProcess.HasExited)
                {
                    _bridgeProcess.Kill();
                    _bridgeProcess.WaitForExit(1000);
                    Debug.Log("[Concept2Manager] ErgBridge killed.");
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[Concept2Manager] Failed to stop ErgBridge cleanly: {ex.Message}");
            }
            finally
            {
                _bridgeProcess.Dispose();
                _bridgeProcess = null;
            }
        }

        _isConnected = false;
        OnConnectionStatusChanged?.Invoke("PM5 disconnected");
        OnDisconnected?.Invoke();
    }

    private void EnsureBridgeClient()
    {
        if (_bridgeClient == null)
            _bridgeClient = GetComponent<ErgBridgeClient>();

        if (_bridgeClient == null)
            _bridgeClient = gameObject.AddComponent<ErgBridgeClient>();

        _bridgeClient.OnDataReceived -= OnErgDataReceived;
        _bridgeClient.OnDataReceived += OnErgDataReceived;
    }

    private string ResolveBridgeExecutablePath()
    {
        if (string.IsNullOrWhiteSpace(bridgeExePath))
            return string.Empty;

        if (Path.IsPathRooted(bridgeExePath))
            return Path.GetFullPath(bridgeExePath);

        string projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
        return Path.GetFullPath(Path.Combine(projectRoot, bridgeExePath));
    }

    private void OnDestroy()
    {
        Disconnect();
    }
}
