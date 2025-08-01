using System;
using System.Collections.Generic;
using System.Threading;
using DarkAges.Library.Core;
using DarkAges.Library.GameLogic;
using DarkAges.Library.Network;

namespace DarkAges.Library.UI.Dialogs;

/// <summary>
/// Manages game events, their processing, and user interactions
/// </summary>
public class EventManager : IDisposable
{
    private const int MAX_EVENTS = 1000;
    private const int MAX_EVENT_QUEUE = 100;
    private const int EVENT_TIMEOUT = 30000; // 30 seconds

    private readonly object syncLock = new object();
    private Dictionary<int, EventInfo> events;
    private Queue<EventMessage> eventQueue;
    private List<EventInfo> activeEvents;
    private EventProcessor eventProcessor;
    private NetworkConnection networkConnection;
    private bool isDisposed;
    private bool isEnabled;
    private Thread processingThread;
    private AutoResetEvent processingEvent;

    // Statistics
    private int totalEventsProcessed;
    private int totalEventsAccepted;
    private int totalEventsDeclined;
    private int totalEventsFailed;
    private DateTime lastEventTime;

    // Events
    public event Action<EventInfo> EventReceived;
    public event Action<EventInfo> EventAccepted;
    public event Action<EventInfo> EventDeclined;
    public event Action<EventInfo> EventCompleted;
    public event Action<EventInfo> EventFailed;
    public event Action<EventError> EventError;

    public EventManager()
    {
        InitializeManager();
    }

    private void InitializeManager()
    {
        events = new Dictionary<int, EventInfo>();
        eventQueue = new Queue<EventMessage>();
        activeEvents = [];
        eventProcessor = new EventProcessor();
        networkConnection = null;
        isDisposed = false;
        isEnabled = true;
        processingEvent = new AutoResetEvent(false);

        // Initialize statistics
        totalEventsProcessed = 0;
        totalEventsAccepted = 0;
        totalEventsDeclined = 0;
        totalEventsFailed = 0;
        lastEventTime = DateTime.Now;

        // Subscribe to event processor events
        eventProcessor.EventProcessed += OnEventProcessed;
        eventProcessor.EventError += OnProcessorError;

        // Start processing thread
        StartProcessingThread();
    }

    public void SetNetworkConnection(NetworkConnection connection)
    {
        if (isDisposed)
            throw new ObjectDisposedException(nameof(EventManager));

        lock (syncLock)
        {
            networkConnection = connection;
            if (connection != null)
            {
                connection.DataReceived += OnNetworkDataReceived;
            }
        }
    }

    public void ProcessEventMessage(EventMessage message)
    {
        if (isDisposed)
            throw new ObjectDisposedException(nameof(EventManager));

        if (message == null)
            throw new ArgumentNullException(nameof(message));

        lock (syncLock)
        {
            try
            {
                // Add to processing queue
                if (eventQueue.Count < MAX_EVENT_QUEUE)
                {
                    eventQueue.Enqueue(message);
                    processingEvent.Set();
                }
                else
                {
                    EventError?.Invoke(new EventError
                    {
                        ErrorCode = EventErrorCode.QueueFull,
                        Message = "Event queue is full"
                    });
                }
            }
            catch (Exception ex)
            {
                EventError?.Invoke(new EventError
                {
                    ErrorCode = EventErrorCode.ProcessingFailed,
                    Message = $"Failed to process event message: {ex.Message}",
                    Exception = ex
                });
            }
        }
    }

    public void AcceptEvent(int eventId)
    {
        if (isDisposed)
            throw new ObjectDisposedException(nameof(EventManager));

        lock (syncLock)
        {
            try
            {
                if (events.TryGetValue(eventId, out var eventInfo))
                {
                    // Send accept packet
                    SendEventResponse(eventId, EventResponseType.Accept);
                        
                    // Update event status
                    eventInfo.Status = EventStatus.InProgress;
                    activeEvents.Add(eventInfo);
                        
                    totalEventsAccepted++;
                    EventAccepted?.Invoke(eventInfo);
                }
                else
                {
                    EventError?.Invoke(new EventError
                    {
                        ErrorCode = EventErrorCode.EventNotFound,
                        Message = $"Event with ID {eventId} not found"
                    });
                }
            }
            catch (Exception ex)
            {
                EventError?.Invoke(new EventError
                {
                    ErrorCode = EventErrorCode.AcceptFailed,
                    Message = $"Failed to accept event: {ex.Message}",
                    Exception = ex
                });
            }
        }
    }

    public void DeclineEvent(int eventId)
    {
        if (isDisposed)
            throw new ObjectDisposedException(nameof(EventManager));

        lock (syncLock)
        {
            try
            {
                if (events.TryGetValue(eventId, out var eventInfo))
                {
                    // Send decline packet
                    SendEventResponse(eventId, EventResponseType.Decline);
                        
                    // Remove from active events
                    activeEvents.Remove(eventInfo);
                    events.Remove(eventId);
                        
                    totalEventsDeclined++;
                    EventDeclined?.Invoke(eventInfo);
                }
                else
                {
                    EventError?.Invoke(new EventError
                    {
                        ErrorCode = EventErrorCode.EventNotFound,
                        Message = $"Event with ID {eventId} not found"
                    });
                }
            }
            catch (Exception ex)
            {
                EventError?.Invoke(new EventError
                {
                    ErrorCode = EventErrorCode.DeclineFailed,
                    Message = $"Failed to decline event: {ex.Message}",
                    Exception = ex
                });
            }
        }
    }

    public void CompleteEvent(int eventId)
    {
        if (isDisposed)
            throw new ObjectDisposedException(nameof(EventManager));

        lock (syncLock)
        {
            try
            {
                if (events.TryGetValue(eventId, out var eventInfo))
                {
                    // Send completion packet
                    SendEventResponse(eventId, EventResponseType.Complete);
                        
                    // Update event status
                    eventInfo.Status = EventStatus.Completed;
                    activeEvents.Remove(eventInfo);
                        
                    EventCompleted?.Invoke(eventInfo);
                }
                else
                {
                    EventError?.Invoke(new EventError
                    {
                        ErrorCode = EventErrorCode.EventNotFound,
                        Message = $"Event with ID {eventId} not found"
                    });
                }
            }
            catch (Exception ex)
            {
                EventError?.Invoke(new EventError
                {
                    ErrorCode = EventErrorCode.CompleteFailed,
                    Message = $"Failed to complete event: {ex.Message}",
                    Exception = ex
                });
            }
        }
    }

    public void FailEvent(int eventId)
    {
        if (isDisposed)
            throw new ObjectDisposedException(nameof(EventManager));

        lock (syncLock)
        {
            try
            {
                if (events.TryGetValue(eventId, out var eventInfo))
                {
                    // Send failure packet
                    SendEventResponse(eventId, EventResponseType.Fail);
                        
                    // Update event status
                    eventInfo.Status = EventStatus.Failed;
                    activeEvents.Remove(eventInfo);
                        
                    totalEventsFailed++;
                    EventFailed?.Invoke(eventInfo);
                }
                else
                {
                    EventError?.Invoke(new EventError
                    {
                        ErrorCode = EventErrorCode.EventNotFound,
                        Message = $"Event with ID {eventId} not found"
                    });
                }
            }
            catch (Exception ex)
            {
                EventError?.Invoke(new EventError
                {
                    ErrorCode = EventErrorCode.FailFailed,
                    Message = $"Failed to mark event as failed: {ex.Message}",
                    Exception = ex
                });
            }
        }
    }

    private void SendEventResponse(int eventId, EventResponseType responseType)
    {
        if (networkConnection == null || !networkConnection.IsConnected)
            return;

        try
        {
            var packet = CreateEventResponsePacket(eventId, responseType);
            _ = networkConnection.SendAsync(packet);
        }
        catch (Exception ex)
        {
            EventError?.Invoke(new EventError
            {
                ErrorCode = EventErrorCode.NetworkError,
                Message = $"Failed to send event response: {ex.Message}",
                Exception = ex
            });
        }
    }

    private byte[] CreateEventResponsePacket(int eventId, EventResponseType responseType)
    {
        // Create packet structure based on disassembly
        var packet = new byte[8];
            
        // Packet header
        packet[0] = 0x24; // Event response packet type
        packet[1] = (byte)responseType;
            
        // Event ID (little endian)
        packet[2] = (byte)(eventId & 0xFF);
        packet[3] = (byte)((eventId >> 8) & 0xFF);
        packet[4] = (byte)((eventId >> 16) & 0xFF);
        packet[5] = (byte)((eventId >> 24) & 0xFF);
            
        // Reserved bytes
        packet[6] = 0;
        packet[7] = 0;
            
        return packet;
    }

    private void StartProcessingThread()
    {
        processingThread = new Thread(ProcessingLoop)
        {
            Name = "EventManager-Processing",
            IsBackground = true
        };
        processingThread.Start();
    }

    private void ProcessingLoop()
    {
        while (!isDisposed)
        {
            try
            {
                // Wait for processing event or timeout
                if (processingEvent.WaitOne(1000))
                {
                    ProcessQueuedEvents();
                }
                    
                // Check for timed out events
                CheckEventTimeouts();
            }
            catch (Exception ex)
            {
                EventError?.Invoke(new EventError
                {
                    ErrorCode = EventErrorCode.ProcessingError,
                    Message = $"Processing loop error: {ex.Message}",
                    Exception = ex
                });
            }
        }
    }

    private void ProcessQueuedEvents()
    {
        lock (syncLock)
        {
            while (eventQueue.Count > 0)
            {
                var message = eventQueue.Dequeue();
                try
                {
                    var eventInfo = eventProcessor.ProcessEventMessage(message);
                    if (eventInfo != null)
                    {
                        events[eventInfo.Id] = eventInfo;
                        totalEventsProcessed++;
                        lastEventTime = DateTime.Now;
                        EventReceived?.Invoke(eventInfo);
                    }
                }
                catch (Exception ex)
                {
                    EventError?.Invoke(new EventError
                    {
                        ErrorCode = EventErrorCode.MessageProcessingFailed,
                        Message = $"Failed to process event message: {ex.Message}",
                        Exception = ex
                    });
                }
            }
        }
    }

    private void CheckEventTimeouts()
    {
        lock (syncLock)
        {
            var now = DateTime.Now;
            var timedOutEvents = new List<EventInfo>();

            foreach (var eventInfo in activeEvents)
            {
                if ((now - eventInfo.StartTime).TotalMilliseconds > EVENT_TIMEOUT)
                {
                    timedOutEvents.Add(eventInfo);
                }
            }

            foreach (var eventInfo in timedOutEvents)
            {
                eventInfo.Status = EventStatus.Failed;
                activeEvents.Remove(eventInfo);
                totalEventsFailed++;
                EventFailed?.Invoke(eventInfo);
            }
        }
    }

    private void OnNetworkDataReceived(object sender, SocketDataEventArgs e)
    {
        if (isDisposed || !isEnabled)
            return;

        try
        {
            // Check if this is an event packet
            if (e.Data.Length >= 2 && e.Data[0] == 0x23) // Event packet type
            {
                var message = new EventMessage
                {
                    Data = e.Data,
                    Timestamp = DateTime.Now
                };
                ProcessEventMessage(message);
            }
        }
        catch (Exception ex)
        {
            EventError?.Invoke(new EventError
            {
                ErrorCode = EventErrorCode.NetworkError,
                Message = $"Failed to process network data: {ex.Message}",
                Exception = ex
            });
        }
    }

    private void OnEventProcessed(EventInfo eventInfo)
    {
        EventReceived?.Invoke(eventInfo);
    }

    private void OnProcessorError(EventError error)
    {
        EventError?.Invoke(error);
    }

    public EventInfo GetEvent(int eventId)
    {
        if (isDisposed)
            throw new ObjectDisposedException(nameof(EventManager));

        lock (syncLock)
        {
            return events.TryGetValue(eventId, out var eventInfo) ? eventInfo : null;
        }
    }

    public List<EventInfo> GetActiveEvents()
    {
        if (isDisposed)
            throw new ObjectDisposedException(nameof(EventManager));

        lock (syncLock)
        {
            return [..activeEvents];
        }
    }

    public List<EventInfo> GetAllEvents()
    {
        if (isDisposed)
            throw new ObjectDisposedException(nameof(EventManager));

        lock (syncLock)
        {
            return [..events.Values];
        }
    }

    public EventManagerStatistics GetStatistics()
    {
        lock (syncLock)
        {
            return new EventManagerStatistics
            {
                TotalEventsProcessed = totalEventsProcessed,
                TotalEventsAccepted = totalEventsAccepted,
                TotalEventsDeclined = totalEventsDeclined,
                TotalEventsFailed = totalEventsFailed,
                ActiveEventCount = activeEvents.Count,
                TotalEventCount = events.Count,
                QueueSize = eventQueue.Count,
                LastEventTime = lastEventTime
            };
        }
    }

    public void ClearEvents()
    {
        if (isDisposed)
            throw new ObjectDisposedException(nameof(EventManager));

        lock (syncLock)
        {
            events.Clear();
            activeEvents.Clear();
            eventQueue.Clear();
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
                // Stop processing thread
                processingEvent?.Set();
                processingThread?.Join(1000);
                    
                // Dispose resources
                processingEvent?.Dispose();
                eventProcessor?.Dispose();
                    
                // Clear collections
                events?.Clear();
                eventQueue?.Clear();
                activeEvents?.Clear();
            }

            isDisposed = true;
        }
    }
}