using System;
using UnityEngine;

/// <summary>
/// Lightweight transport-agnostic PM5 event payloads.
///
/// The Strength-ERG BLE bridge streams newline-delimited JSON over TCP. These
/// serializable types give Imagine a place to deserialize those messages later
/// without coupling the rest of the game code to a specific bridge executable.
/// </summary>
public static class Concept2BleModels
{
    [Serializable]
    public class BleBridgeMessage
    {
        public string type;
    }

    [Serializable]
    public class BleStatusMessage : BleBridgeMessage
    {
        public bool connected;
        public int heartRate;
        public float elapsedSec;
        public string statusText;
    }

    [Serializable]
    public class BleRepMessage : BleBridgeMessage
    {
        public int repCount;
        public float driveTimeSec;
        public float pullDistance;
        public int heartRate;
        public float elapsedSec;
    }

    public static bool TryParseType(string json, out string messageType)
    {
        messageType = null;
        if (string.IsNullOrWhiteSpace(json))
            return false;

        try
        {
            BleBridgeMessage envelope = JsonUtility.FromJson<BleBridgeMessage>(json);
            if (envelope == null || string.IsNullOrWhiteSpace(envelope.type))
                return false;

            messageType = envelope.type;
            return true;
        }
        catch
        {
            return false;
        }
    }
}
