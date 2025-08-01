using System;
using System.Collections.Generic;
using System.Text;
using DarkAges.Library.Core;

namespace DarkAges.Library.UI.Dialogs;

/// <summary>
/// Processes event messages and extracts event information
/// </summary>
public class EventProcessor : IDisposable
{
    private const int MAX_EVENT_NAME_LENGTH = 256;
    private const int MAX_EVENT_DESC_LENGTH = 1024;
    private const int MAX_REWARD_COUNT = 32;
    private const int EVENT_PACKET_HEADER_SIZE = 8;

    private readonly object syncLock = new object();
    private bool isDisposed;
    private Dictionary<int, EventInfo> processedEvents;
    private int nextEventId;

    // Events
    public event Action<EventInfo> EventProcessed;
    public event Action<EventError> EventError;

    public EventProcessor()
    {
        InitializeProcessor();
    }

    private void InitializeProcessor()
    {
        isDisposed = false;
        processedEvents = new Dictionary<int, EventInfo>();
        nextEventId = 1;
    }

    public EventInfo ProcessEventMessage(EventMessage message)
    {
        if (isDisposed)
            throw new ObjectDisposedException(nameof(EventProcessor));

        if (message == null)
            throw new ArgumentNullException(nameof(message));

        lock (syncLock)
        {
            try
            {
                // Validate packet structure
                if (!ValidateEventPacket(message.Data))
                {
                    throw new ArgumentException("Invalid event packet structure");
                }

                // Parse event information based on disassembly patterns
                var eventInfo = ParseEventInfo(message.Data);
                    
                if (eventInfo != null)
                {
                    // Store processed event
                    processedEvents[eventInfo.Id] = eventInfo;
                        
                    // Notify listeners
                    EventProcessed?.Invoke(eventInfo);
                }

                return eventInfo;
            }
            catch (Exception ex)
            {
                EventError?.Invoke(new EventError
                {
                    ErrorCode = EventErrorCode.ParsingFailed,
                    Message = $"Failed to process event message: {ex.Message}",
                    Exception = ex
                });
                return null;
            }
        }
    }

    private bool ValidateEventPacket(byte[] data)
    {
        if (data == null || data.Length < EVENT_PACKET_HEADER_SIZE)
            return false;

        // Check packet type
        if (data[0] != 0x23) // Event packet type
            return false;

        // Check packet length
        int expectedLength = BitConverter.ToUInt16(data, 1);
        if (data.Length != expectedLength)
            return false;

        return true;
    }

    private EventInfo ParseEventInfo(byte[] data)
    {
        var eventInfo = new EventInfo();
        var offset = EVENT_PACKET_HEADER_SIZE;

        try
        {
            // Parse event ID
            eventInfo.Id = BitConverter.ToInt32(data, offset);
            offset += 4;

            // Parse event type
            int eventType = data[offset++];
            eventInfo.Status = MapEventTypeToStatus(eventType);

            // Parse event name length
            int nameLength = BitConverter.ToUInt16(data, offset);
            offset += 2;

            // Parse event name
            if (nameLength > 0 && nameLength <= MAX_EVENT_NAME_LENGTH)
            {
                eventInfo.Name = Encoding.UTF8.GetString(data, offset, nameLength);
                offset += nameLength;
            }

            // Parse event description length
            int descLength = BitConverter.ToUInt16(data, offset);
            offset += 2;

            // Parse event description
            if (descLength > 0 && descLength <= MAX_EVENT_DESC_LENGTH)
            {
                eventInfo.Description = Encoding.UTF8.GetString(data, offset, descLength);
                offset += descLength;
            }

            // Parse event level
            eventInfo.Level = BitConverter.ToInt32(data, offset);
            offset += 4;

            // Parse icon ID
            eventInfo.IconId = BitConverter.ToInt32(data, offset);
            offset += 4;

            // Parse requirements length
            int reqLength = BitConverter.ToUInt16(data, offset);
            offset += 2;

            // Parse requirements
            if (reqLength > 0)
            {
                eventInfo.Requirements = Encoding.UTF8.GetString(data, offset, reqLength);
                offset += reqLength;
            }

            // Parse reward count
            int rewardCount = BitConverter.ToUInt16(data, offset);
            offset += 2;

            // Parse rewards
            if (rewardCount > 0 && rewardCount <= MAX_REWARD_COUNT)
            {
                eventInfo.Rewards = [];
                for (var i = 0; i < rewardCount; i++)
                {
                    var reward = ParseEventReward(data, ref offset);
                    if (reward != null)
                    {
                        eventInfo.Rewards.Add(reward);
                    }
                }
            }

            // Parse timestamps
            eventInfo.StartTime = ParseDateTime(data, ref offset);
            eventInfo.EndTime = ParseDateTime(data, ref offset);

            // Parse flags
            var flags = data[offset++];
            eventInfo.IsRepeatable = (flags & 0x01) != 0;

            // Parse participant info
            eventInfo.MaxParticipants = BitConverter.ToInt32(data, offset);
            offset += 4;
            eventInfo.CurrentParticipants = BitConverter.ToInt32(data, offset);
            offset += 4;

            return eventInfo;
        }
        catch (Exception ex)
        {
            EventError?.Invoke(new EventError
            {
                ErrorCode = EventErrorCode.ParsingFailed,
                Message = $"Failed to parse event info: {ex.Message}",
                Exception = ex
            });
            return null;
        }
    }

    private EventReward ParseEventReward(byte[] data, ref int offset)
    {
        try
        {
            var reward = new EventReward();

            // Parse reward ID
            reward.Id = BitConverter.ToInt32(data, offset);
            offset += 4;

            // Parse reward type
            reward.Type = (RewardType)data[offset++];

            // Parse reward name length
            int nameLength = BitConverter.ToUInt16(data, offset);
            offset += 2;

            // Parse reward name
            if (nameLength > 0)
            {
                reward.Name = Encoding.UTF8.GetString(data, offset, nameLength);
                offset += nameLength;
            }

            // Parse quantity
            reward.Quantity = BitConverter.ToInt32(data, offset);
            offset += 4;

            // Parse type-specific data
            switch (reward.Type)
            {
            case RewardType.Item:
                reward.ItemId = BitConverter.ToInt32(data, offset);
                offset += 4;
                break;
            case RewardType.Experience:
                reward.Experience = BitConverter.ToInt32(data, offset);
                offset += 4;
                break;
            case RewardType.Gold:
                reward.Gold = BitConverter.ToInt32(data, offset);
                offset += 4;
                break;
            }

            return reward;
        }
        catch (Exception ex)
        {
            EventError?.Invoke(new EventError
            {
                ErrorCode = EventErrorCode.RewardParsingFailed,
                Message = $"Failed to parse event reward: {ex.Message}",
                Exception = ex
            });
            return null;
        }
    }

    private DateTime ParseDateTime(byte[] data, ref int offset)
    {
        try
        {
            var ticks = BitConverter.ToInt64(data, offset);
            offset += 8;
            return new DateTime(ticks);
        }
        catch
        {
            offset += 8;
            return DateTime.MinValue;
        }
    }

    private EventStatus MapEventTypeToStatus(int eventType)
    {
        return eventType switch
        {
            1 => EventStatus.Available,
            2 => EventStatus.InProgress,
            3 => EventStatus.Completed,
            4 => EventStatus.Failed,
            5 => EventStatus.Expired,
            _ => EventStatus.Unknown
        };
    }

    public EventInfo GetProcessedEvent(int eventId)
    {
        if (isDisposed)
            throw new ObjectDisposedException(nameof(EventProcessor));

        lock (syncLock)
        {
            return processedEvents.TryGetValue(eventId, out var eventInfo) ? eventInfo : null;
        }
    }

    public List<EventInfo> GetProcessedEvents()
    {
        if (isDisposed)
            throw new ObjectDisposedException(nameof(EventProcessor));

        lock (syncLock)
        {
            return [..processedEvents.Values];
        }
    }

    public void ClearProcessedEvents()
    {
        if (isDisposed)
            throw new ObjectDisposedException(nameof(EventProcessor));

        lock (syncLock)
        {
            processedEvents.Clear();
        }
    }

    public bool IsDisposed()
    {
        return isDisposed;
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
                lock (syncLock)
                {
                    processedEvents?.Clear();
                }
            }

            isDisposed = true;
        }
    }
}