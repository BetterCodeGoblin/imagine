using System;

/// <summary>
/// CSAFE and PM5-specific command constants used by Concept2 integrations.
///
/// These values are carried over from the working Strength-ERG reference so the
/// Unity-side code has a single source of truth for PM monitor commands.
/// This file does not execute CSAFE traffic by itself. It documents and centralizes
/// command IDs for whichever transport layer is active (USB bridge, BLE bridge, etc.).
/// </summary>
public static class CSAFECommands
{
    // Standard short commands
    public const byte GetStatus = 0x80;
    public const byte Reset = 0x81;
    public const byte GoIdle = 0x85;
    public const byte GoReady = 0x86;

    // Standard/public PM data commands
    public const byte GetWorkTime = 0xA0;
    public const byte GetHorizontal = 0xA1;
    public const byte GetCalories = 0xA3;
    public const byte GetHeartRate = 0xB0;

    // PM proprietary command wrapper
    public const byte ProprietaryWrapper = 0x1A;
    public const byte LongCommandPrefix = 0x76;

    // PM-specific proprietary subcommands
    public const byte GetWorkDistance = 0x01;
    public const byte GetPace = 0x06;
    public const byte GetCadence = 0x07;
    public const byte GetPower = 0x1A;
    public const byte GetCaloricBurnRate = 0x1B;
    public const byte GetStrokeStats = 0x6E;

    public static byte CalculateChecksum(ReadOnlySpan<byte> payload)
    {
        byte checksum = 0;
        for (int i = 0; i < payload.Length; i++)
            checksum ^= payload[i];
        return checksum;
    }

    public static byte[] WrapFrame(params byte[] payload)
    {
        byte[] frame = new byte[payload.Length + 3];
        frame[0] = 0xF1;
        Array.Copy(payload, 0, frame, 1, payload.Length);
        frame[frame.Length - 2] = CalculateChecksum(payload);
        frame[frame.Length - 1] = 0xF2;
        return frame;
    }
}
