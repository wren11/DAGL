using System;
using System.Runtime.InteropServices;

namespace DarkAges.Library.Common;

public static class ExceptionHandler
{
    public delegate int UnhandledExceptionFilter(IntPtr exceptionInfo);

    [DllImport("kernel32.dll")]
    private static extern IntPtr SetUnhandledExceptionFilter(UnhandledExceptionFilter filter);

    public static void Initialize()
    {
        try
        {
            // Set up unhandled exception filter for Windows
            if (OperatingSystem.IsWindows())
            {
                var filter = new UnhandledExceptionFilter(OnUnhandledException);
                SetUnhandledExceptionFilter(filter);
            }

            // Set up global exception handlers
            AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to initialize exception handler: {ex.Message}");
        }
    }

    private static int OnUnhandledException(IntPtr exceptionInfo)
    {
        try
        {
            Console.WriteLine("Unhandled exception occurred");
            // Log the exception details
            return 1; // EXCEPTION_EXECUTE_HANDLER
        }
        catch
        {
            return 0; // EXCEPTION_CONTINUE_SEARCH
        }
    }

    private static void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        if (e.ExceptionObject is Exception ex)
        {
            Console.WriteLine($"Unhandled exception: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
        }
    }

    public static void ShowErrorDialog(string title, string message, string details = "")
    {
        try
        {
            Console.WriteLine($"ERROR: {title}");
            Console.WriteLine($"Message: {message}");
            if (!string.IsNullOrEmpty(details))
            {
                Console.WriteLine($"Details: {details}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to show error dialog: {ex.Message}");
        }
    }

    public static void LogException(Exception ex, string context = "")
    {
        try
        {
            Console.WriteLine($"Exception in {context}: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
        }
        catch (Exception logEx)
        {
            Console.WriteLine($"Failed to log exception: {logEx.Message}");
        }
    }
}