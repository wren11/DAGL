namespace DarkAges.Library.Audio;

/// <summary>
/// Audio device information
/// </summary>
public struct AudioDeviceInfo
{
    public int SampleRate;
    public int Channels;
    public int BitsPerSample;
    public int BufferSize;
    public string DeviceName;
    public bool IsDefault;
}