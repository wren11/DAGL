using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Runtime.InteropServices;

namespace DarkAges.Library.Network;

/// <summary>
/// Handles network errors and provides recovery mechanisms
/// Based on disassembly analysis from chunks 024 and 025
/// </summary>
public class NetworkErrorHandler
{
    public NetworkErrorResult HandleError(NetworkError error)
    {
        var severity = GetErrorSeverity(error.Code);
        return new NetworkErrorResult(error, severity);
    }

    private static NetworkErrorSeverity GetErrorSeverity(NetworkErrorCode code)
    {
        return code switch
        {
            NetworkErrorCode.ConnectionFailed => NetworkErrorSeverity.Fatal,
            NetworkErrorCode.DisconnectionFailed => NetworkErrorSeverity.Warning,
            NetworkErrorCode.SendFailed => NetworkErrorSeverity.Warning,
            NetworkErrorCode.ReceiveFailed => NetworkErrorSeverity.Warning,
            NetworkErrorCode.DecryptionFailed => NetworkErrorSeverity.Warning,
            NetworkErrorCode.EncryptionFailed => NetworkErrorSeverity.Warning,
            NetworkErrorCode.InvalidData => NetworkErrorSeverity.Warning,
            NetworkErrorCode.InvalidState => NetworkErrorSeverity.Fatal,
            NetworkErrorCode.Timeout => NetworkErrorSeverity.Warning,
            _ => NetworkErrorSeverity.Warning
        };
    }
}