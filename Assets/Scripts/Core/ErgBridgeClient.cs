using UnityEngine;
using System;
using System.Globalization;
using System.Net.Sockets;
using System.Text;
using System.Threading;

/// <summary>
/// ErgBridgeClient — TCP socket client for ErgBridge data stream.
/// 
/// Establishes TCP connection to ErgBridge.exe on localhost:6789.
/// Runs a background thread polling for stroke data and parsing into rate/pace/power.
/// Thread-safe data handoff to main thread via lock + flag mechanism.
/// 
/// Copied from Strength-ERG-Demo-Wired production code.
/// </summary>
public class ErgBridgeClient : MonoBehaviour
{
    public Action<float, float, float, bool> OnDataReceived;

    private TcpClient _client;
    private NetworkStream _stream;
    private Thread _thread;
    private volatile bool _running = false;
    private volatile bool _isConnecting = false;

    // Thread-safe staging fields
    private float _rate, _pace, _power;
    private bool _connected;
    private bool _hasNewData = false;
    private readonly object _lock = new object();

    private void Start()
    {
        // Delay to let ErgBridge initialize
        Invoke(nameof(Connect), 5f);
    }

    private void Connect()
    {
        if (_isConnecting || _running)
            return;

        _isConnecting = true;

        try
        {
            _client = new TcpClient();
            _client.Connect("127.0.0.1", 6789);
            _stream = _client.GetStream();
            _running = true;

            _thread = new Thread(ReceiveLoop)
            {
                IsBackground = true,
                Name = "ErgBridgeClient.ReceiveLoop"
            };
            _thread.Start();

            Debug.Log("[ErgBridgeClient] Connected to ErgBridge on 127.0.0.1:6789");
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[ErgBridgeClient] Connection attempt failed, retrying in 3s: {e.Message}");
            CleanupSocketResources();
            if (isActiveAndEnabled)
                Invoke(nameof(Connect), 3f);
        }
        finally
        {
            _isConnecting = false;
        }
    }

    private void ReceiveLoop()
    {
        byte[] requestByte = new byte[] { 0x01 };
        byte[] buffer = new byte[256];

        while (_running)
        {
            try
            {
                if (_stream == null || !_stream.CanWrite || !_stream.CanRead)
                    break;

                // Request data from bridge
                _stream.Write(requestByte, 0, 1);
                _stream.Flush();

                // Read response: expected format "rate,pace,power,connected"
                int bytesRead = _stream.Read(buffer, 0, buffer.Length);
                if (bytesRead == 0)
                    break;

                string raw = Encoding.UTF8.GetString(buffer, 0, bytesRead).Trim();
                string[] parts = raw.Split(',');

                if (parts.Length >= 4)
                {
                    lock (_lock)
                    {
                        _rate = ParseFloat(parts[0]);
                        _pace = ParseFloat(parts[1]);
                        _power = ParseFloat(parts[2]);
                        _connected = parts[3].Trim() == "1";
                        _hasNewData = true;
                    }
                }

                Thread.Sleep(100); // Poll at 10Hz
            }
            catch (ThreadInterruptedException)
            {
                break;
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[ErgBridgeClient] Receive error: {e.Message}");
                break;
            }
        }

        _running = false;
    }

    private void Update()
    {
        if (!_hasNewData) return;

        float rate, pace, power;
        bool connected;

        lock (_lock)
        {
            rate = _rate;
            pace = _pace;
            power = _power;
            connected = _connected;
            _hasNewData = false;
        }

        OnDataReceived?.Invoke(rate, pace, power, connected);
    }

    private static float ParseFloat(string value)
    {
        if (float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out float parsed))
            return parsed;

        if (float.TryParse(value, out parsed))
            return parsed;

        return 0f;
    }

    private void OnDisable()
    {
        Shutdown();
    }

    private void OnDestroy()
    {
        Shutdown();
    }

    private void Shutdown()
    {
        _running = false;

        if (IsInvoking(nameof(Connect)))
            CancelInvoke(nameof(Connect));

        try
        {
            _thread?.Interrupt();
        }
        catch
        {
            // Ignore shutdown race conditions.
        }

        if (_thread != null && _thread.IsAlive)
            _thread.Join(500);

        CleanupSocketResources();
        _thread = null;
    }

    private void CleanupSocketResources()
    {
        try
        {
            _stream?.Close();
        }
        catch
        {
            // Ignore socket cleanup errors.
        }

        try
        {
            _client?.Close();
        }
        catch
        {
            // Ignore socket cleanup errors.
        }

        _stream = null;
        _client = null;
    }
}
