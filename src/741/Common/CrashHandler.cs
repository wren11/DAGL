using System;
using System.IO;
using System.Text;

namespace DarkAges.Library.Common;

public static class CrashHandler
{
    private static readonly string LogFilePath = "crash.log";
    private static readonly object LogLock = new object();

    public static void Initialize()
    {
        // Set up global exception handlers
        AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
    }

    public static void LogException(Exception ex, string context = "")
    {
        try
        {
            var logEntry = new StringBuilder();
            logEntry.AppendLine($"=== CRASH REPORT ===");
            logEntry.AppendLine($"Timestamp: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            logEntry.AppendLine($"Context: {context}");
            logEntry.AppendLine($"Exception Type: {ex.GetType().Name}");
            logEntry.AppendLine($"Message: {ex.Message}");
            logEntry.AppendLine($"Stack Trace:");
            logEntry.AppendLine(ex.StackTrace);

            if (ex.InnerException != null)
            {
                logEntry.AppendLine($"Inner Exception: {ex.InnerException.Message}");
                logEntry.AppendLine($"Inner Stack Trace:");
                logEntry.AppendLine(ex.InnerException.StackTrace);
            }

            lock (LogLock)
            {
                File.AppendAllText(LogFilePath, logEntry.ToString());
            }

            Console.WriteLine($"Exception logged to {LogFilePath}");
        }
        catch (Exception logEx)
        {
            Console.WriteLine($"Failed to log exception: {logEx.Message}");
        }
    }

    private static void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        if (e.ExceptionObject is Exception ex)
        {
            LogException(ex, "Unhandled Exception");
        }
    }

    public static void Cleanup()
    {
        // Cleanup resources if needed
    }
}