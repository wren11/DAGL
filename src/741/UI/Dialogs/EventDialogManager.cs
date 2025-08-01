using System;
using System.Collections.Generic;
using System.Threading;
using DarkAges.Library.Core;
using DarkAges.Library.Graphics;

namespace DarkAges.Library.UI.Dialogs;

/// <summary>
/// Manages event dialog display and user interactions
/// </summary>
public class EventDialogManager : IDisposable
{
    private const int MAX_DIALOGS = 10;
    private const int DIALOG_TIMEOUT = 60000; // 60 seconds
    private const int DIALOG_Z_OFFSET = 100;

    private readonly object syncLock = new object();
    private Dictionary<int, EventInfoDialogPane> activeDialogs = new Dictionary<int, EventInfoDialogPane>();
    private Queue<EventDialogRequest> dialogQueue = new Queue<EventDialogRequest>();
    private EventManager? eventManager;
    private ScreenManager? screenManager;
    private bool isDisposed;
    private bool isEnabled;
    private Thread? dialogThread;
    private AutoResetEvent? dialogEvent;

    // Statistics
    private int totalDialogsShown;
    private int totalDialogsAccepted;
    private int totalDialogsDeclined;
    private int totalDialogsClosed;
    private DateTime lastDialogTime;

    // Events
    public event Action<EventInfo?>? DialogShown;
    public event Action<EventInfo?>? DialogAccepted;
    public event Action<EventInfo?>? DialogDeclined;
    public event Action<EventInfo?>? DialogClosed;
    public event Action<EventDialogError>? DialogError;

    public EventDialogManager()
    {
        InitializeManager();
    }

    private void InitializeManager()
    {
        activeDialogs = new Dictionary<int, EventInfoDialogPane>();
        dialogQueue = new Queue<EventDialogRequest>();
        eventManager = null;
        screenManager = null;
        isDisposed = false;
        isEnabled = true;
        dialogEvent = new AutoResetEvent(false);
        dialogThread = null;
        DialogShown = null;
        DialogAccepted = null;
        DialogDeclined = null;
        DialogClosed = null;
        DialogError = null;

        // Initialize statistics
        totalDialogsShown = 0;
        totalDialogsAccepted = 0;
        totalDialogsDeclined = 0;
        totalDialogsClosed = 0;
        lastDialogTime = DateTime.Now;

        // Start dialog thread
        StartDialogThread();
    }

    public void SetEventManager(EventManager? manager)
    {
        if (isDisposed)
            throw new ObjectDisposedException(nameof(EventDialogManager));

        lock (syncLock)
        {
            eventManager = manager;
            if (manager != null)
            {
                manager.EventReceived += OnEventReceived;
                manager.EventAccepted += OnEventAccepted;
                manager.EventDeclined += OnEventDeclined;
                manager.EventCompleted += OnEventCompleted;
                manager.EventFailed += OnEventFailed;
            }
        }
    }

    public void SetScreenManager(ScreenManager manager)
    {
        if (isDisposed)
            throw new ObjectDisposedException(nameof(EventDialogManager));

        lock (syncLock)
        {
            screenManager = manager;
        }
    }

    public void ShowEventDialog(EventInfo eventInfo)
    {
        if (isDisposed)
            throw new ObjectDisposedException(nameof(EventDialogManager));

        if (eventInfo == null)
            throw new ArgumentNullException(nameof(eventInfo));

        lock (syncLock)
        {
            try
            {
                var request = new EventDialogRequest
                {
                    EventInfo = eventInfo,
                    Timestamp = DateTime.Now,
                    RequestType = DialogRequestType.Show
                };

                if (dialogQueue.Count < MAX_DIALOGS)
                {
                    dialogQueue.Enqueue(request);
                    dialogEvent?.Set();
                }
                else
                {
                    DialogError?.Invoke(new EventDialogError
                    {
                        ErrorCode = EventDialogErrorCode.QueueFull,
                        Message = "Dialog queue is full"
                    });
                }
            }
            catch (Exception ex)
            {
                DialogError?.Invoke(new EventDialogError
                {
                    ErrorCode = EventDialogErrorCode.ShowFailed,
                    Message = $"Failed to show event dialog: {ex.Message}",
                    Exception = ex
                });
            }
        }
    }

    public void CloseEventDialog(int eventId)
    {
        if (isDisposed)
            throw new ObjectDisposedException(nameof(EventDialogManager));

        lock (syncLock)
        {
            try
            {
                if (activeDialogs.TryGetValue(eventId, out var dialog))
                {
                    CloseDialog(dialog);
                    activeDialogs.Remove(eventId);
                    totalDialogsClosed++;
                    DialogClosed?.Invoke(dialog.GetEventInfo());
                }
                else
                {
                    DialogError?.Invoke(new EventDialogError
                    {
                        ErrorCode = EventDialogErrorCode.DialogNotFound,
                        Message = $"Dialog for event {eventId} not found"
                    });
                }
            }
            catch (Exception ex)
            {
                DialogError?.Invoke(new EventDialogError
                {
                    ErrorCode = EventDialogErrorCode.CloseFailed,
                    Message = $"Failed to close event dialog: {ex.Message}",
                    Exception = ex
                });
            }
        }
    }

    public void CloseAllDialogs()
    {
        if (isDisposed)
            throw new ObjectDisposedException(nameof(EventDialogManager));

        lock (syncLock)
        {
            try
            {
                var dialogsToClose = new List<EventInfoDialogPane>(activeDialogs.Values);
                foreach (var dialog in dialogsToClose)
                {
                    CloseDialog(dialog);
                }
                activeDialogs.Clear();
            }
            catch (Exception ex)
            {
                DialogError?.Invoke(new EventDialogError
                {
                    ErrorCode = EventDialogErrorCode.CloseFailed,
                    Message = $"Failed to close all dialogs: {ex.Message}",
                    Exception = ex
                });
            }
        }
    }

    private void StartDialogThread()
    {
        dialogThread = new Thread(DialogLoop)
        {
            Name = "EventDialogManager-Processing",
            IsBackground = true
        };
        dialogThread.Start();
    }

    private void DialogLoop()
    {
        while (!isDisposed)
        {
            try
            {
                // Wait for dialog event or timeout
                if (dialogEvent?.WaitOne(1000) == true)
                {
                    ProcessDialogQueue();
                }
                    
                // Check for timed out dialogs
                CheckDialogTimeouts();
            }
            catch (Exception ex)
            {
                DialogError?.Invoke(new EventDialogError
                {
                    ErrorCode = EventDialogErrorCode.ProcessingError,
                    Message = $"Dialog loop error: {ex.Message}",
                    Exception = ex
                });
            }
        }
    }

    private void ProcessDialogQueue()
    {
        lock (syncLock)
        {
            while (dialogQueue.Count > 0)
            {
                var request = dialogQueue.Dequeue();
                try
                {
                    switch (request.RequestType)
                    {
                    case DialogRequestType.Show:
                        ShowDialog(request.EventInfo);
                        break;
                    case DialogRequestType.Close:
                        CloseEventDialog(request.EventInfo.Id);
                        break;
                    }
                }
                catch (Exception ex)
                {
                    DialogError?.Invoke(new EventDialogError
                    {
                        ErrorCode = EventDialogErrorCode.ProcessingFailed,
                        Message = $"Failed to process dialog request: {ex.Message}",
                        Exception = ex
                    });
                }
            }
        }
    }

    private void ShowDialog(EventInfo eventInfo)
    {
        if (screenManager == null)
            return;

        try
        {
            // Create dialog
            var dialog = new EventInfoDialogPane(eventInfo);
                
            // Set up event handlers
            dialog.EventAccepted += OnDialogAccepted;
            dialog.EventDeclined += OnDialogDeclined;
            dialog.EventClosed += OnDialogClosed;
            dialog.EventError += OnDialogError;

            // Position dialog
            PositionDialog(dialog);

            // Add to screen manager
            screenManager.AddPane(dialog, DIALOG_Z_OFFSET + activeDialogs.Count);

            // Store dialog
            activeDialogs[eventInfo.Id] = dialog;
            totalDialogsShown++;
            lastDialogTime = DateTime.Now;

            DialogShown?.Invoke(eventInfo);
        }
        catch (Exception ex)
        {
            DialogError?.Invoke(new EventDialogError
            {
                ErrorCode = EventDialogErrorCode.ShowFailed,
                Message = $"Failed to show dialog: {ex.Message}",
                Exception = ex
            });
        }
    }

    private void PositionDialog(EventInfoDialogPane dialog)
    {
        try
        {
            // Get screen dimensions
            var screenSize = screenManager.GetScreenSize();
            var dialogSize = new System.Drawing.Size(400, 300);
            var dialogPosition = new System.Drawing.Point(
                (screenSize.Width - dialogSize.Width) / 2,
                (screenSize.Height - dialogSize.Height) / 2
            );
            var dialogBounds = new System.Drawing.Rectangle(dialogPosition, dialogSize);
                
            // Apply offset for multiple dialogs
            var offset = activeDialogs.Count * 20;
            dialogBounds.Offset(offset, offset);
                
            // Ensure dialog stays on screen
            dialogBounds.Offset(Math.Max(0, Math.Min(dialogBounds.X, screenSize.Width - dialogBounds.Width)),
                Math.Max(0, Math.Min(dialogBounds.Y, screenSize.Height - dialogBounds.Height)));
                
            dialog.SetBounds(dialogBounds);
        }
        catch (Exception ex)
        {
            DialogError?.Invoke(new EventDialogError
            {
                ErrorCode = EventDialogErrorCode.PositioningFailed,
                Message = $"Failed to position dialog: {ex.Message}",
                Exception = ex
            });
        }
    }

    private void CloseDialog(EventInfoDialogPane? dialog)
    {
        try
        {
            // Remove from screen manager
            screenManager?.RemovePane(dialog);
                
            // Dispose dialog
            dialog?.Dispose();
        }
        catch (Exception ex)
        {
            DialogError?.Invoke(new EventDialogError
            {
                ErrorCode = EventDialogErrorCode.CloseFailed,
                Message = $"Failed to close dialog: {ex.Message}",
                Exception = ex
            });
        }
    }

    private void CheckDialogTimeouts()
    {
        lock (syncLock)
        {
            var now = DateTime.Now;
            var timedOutDialogs = new List<EventInfoDialogPane>();

            foreach (var kvp in activeDialogs)
            {
                var dialog = kvp.Value;
                var eventInfo = dialog.GetEventInfo();
                    
                if (eventInfo != null && (now - eventInfo.StartTime).TotalMilliseconds > DIALOG_TIMEOUT)
                {
                    timedOutDialogs.Add(dialog);
                }
            }

            foreach (var dialog in timedOutDialogs)
            {
                var eventInfo = dialog.GetEventInfo();
                if (eventInfo != null)
                {
                    activeDialogs.Remove(eventInfo.Id);
                    CloseDialog(dialog);
                    totalDialogsClosed++;
                    DialogClosed?.Invoke(eventInfo);
                }
            }
        }
    }

    private void OnEventReceived(EventInfo eventInfo)
    {
        ShowEventDialog(eventInfo);
    }

    private void OnEventAccepted(EventInfo eventInfo)
    {
        CloseEventDialog(eventInfo.Id);
        totalDialogsAccepted++;
        DialogAccepted?.Invoke(eventInfo);
    }

    private void OnEventDeclined(EventInfo eventInfo)
    {
        CloseEventDialog(eventInfo.Id);
        totalDialogsDeclined++;
        DialogDeclined?.Invoke(eventInfo);
    }

    private void OnEventCompleted(EventInfo eventInfo)
    {
        CloseEventDialog(eventInfo.Id);
    }

    private void OnEventFailed(EventInfo eventInfo)
    {
        CloseEventDialog(eventInfo.Id);
    }

    private void OnDialogAccepted(EventInfo eventInfo)
    {
        eventManager?.AcceptEvent(eventInfo.Id);
    }

    private void OnDialogDeclined(EventInfo eventInfo)
    {
        eventManager?.DeclineEvent(eventInfo.Id);
    }

    private void OnDialogClosed(EventInfo eventInfo)
    {
        CloseEventDialog(eventInfo.Id);
    }

    private void OnDialogError(EventInfo? eventInfo, EventError error)
    {
        DialogError?.Invoke(new EventDialogError
        {
            ErrorCode = EventDialogErrorCode.DialogError,
            Message = error.Message,
            Exception = error.Exception
        });
    }

    public EventInfoDialogPane? GetDialog(int eventId)
    {
        if (isDisposed)
            throw new ObjectDisposedException(nameof(EventDialogManager));

        lock (syncLock)
        {
            return activeDialogs.TryGetValue(eventId, out var dialog) ? dialog : null;
        }
    }

    public List<EventInfoDialogPane> GetActiveDialogs()
    {
        if (isDisposed)
            throw new ObjectDisposedException(nameof(EventDialogManager));

        lock (syncLock)
        {
            return [..activeDialogs.Values];
        }
    }

    public EventDialogStatistics GetStatistics()
    {
        lock (syncLock)
        {
            return new EventDialogStatistics
            {
                TotalDialogsShown = totalDialogsShown,
                TotalDialogsAccepted = totalDialogsAccepted,
                TotalDialogsDeclined = totalDialogsDeclined,
                TotalDialogsClosed = totalDialogsClosed,
                ActiveDialogCount = activeDialogs.Count,
                QueueSize = dialogQueue.Count,
                LastDialogTime = lastDialogTime
            };
        }
    }

    public void SetEnabled(bool enabled)
    {
        if (isDisposed)
            return;

        isEnabled = enabled;
    }

    public bool IsEnabled()
    {
        return isEnabled && !isDisposed;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!isDisposed)
        {
            if (disposing)
            {
                // Stop dialog thread
                dialogEvent?.Set();
                dialogThread?.Join(1000);
                    
                // Dispose resources
                dialogEvent?.Dispose();
                    
                // Close all dialogs
                CloseAllDialogs();
                    
                // Clear collections
                activeDialogs?.Clear();
                dialogQueue?.Clear();
            }

            isDisposed = true;
        }
    }
}