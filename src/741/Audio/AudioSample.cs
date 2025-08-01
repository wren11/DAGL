using System;
using System.IO;
using System.Runtime.InteropServices;

namespace DarkAges.Library.Audio;

/// <summary>
/// Represents an individual audio sample for playback
/// </summary>
public class AudioSample : IDisposable
{
    private IntPtr sampleHandle;
    private string filePath;
    private AudioFormat format;
    private bool isPlaying;
    private bool isPaused;
    private bool isDisposed;
    private byte volume;
    private int sampleRate;
    private int channels;
    private int bitsPerSample;
    private long dataSize;
    private IntPtr audioData;
    private AudioDevice audioDevice;

    public AudioSample(string filePath, AudioFormat format)
    {
        this.filePath = filePath;
        this.format = format;
        this.isPlaying = false;
        this.isPaused = false;
        this.isDisposed = false;
        this.volume = 100;
        this.sampleHandle = IntPtr.Zero;
        this.audioData = IntPtr.Zero;
        this.audioDevice = null;

        LoadAudioFile();
    }

    private void LoadAudioFile()
    {
        try
        {
            // Get audio device
            audioDevice = GetAudioDevice();
            if (audioDevice == null)
            {
                throw new AudioException("No audio device available");
            }

            // Load audio file data
            LoadAudioData();

            // Allocate sample handle
            sampleHandle = audioDevice.AllocateSampleHandle();
            if (sampleHandle == IntPtr.Zero)
            {
                throw new AudioException("Failed to allocate sample handle");
            }

            // Initialize sample
            if (!audioDevice.InitSample(sampleHandle))
            {
                throw new AudioException("Failed to initialize sample");
            }

            // Set sample file
            if (!audioDevice.SetNamedSampleFile(sampleHandle, filePath, audioData, (int)dataSize, 0))
            {
                throw new AudioException("Failed to set sample file");
            }

            // Set initial volume
            SetVolume(volume);
        }
        catch (Exception ex)
        {
            Cleanup();
            throw new AudioException($"Failed to load audio file: {ex.Message}", ex);
        }
    }

    private void LoadAudioData()
    {
        try
        {
            // Read audio file
            var fileData = File.ReadAllBytes(filePath);
            dataSize = fileData.Length;

            // Allocate unmanaged memory for audio data
            audioData = Marshal.AllocHGlobal((int)dataSize);
            Marshal.Copy(fileData, 0, audioData, (int)dataSize);

            // Parse audio format information
            ParseAudioFormat(fileData);
        }
        catch (Exception ex)
        {
            throw new AudioException($"Failed to load audio data: {ex.Message}", ex);
        }
    }

    private void ParseAudioFormat(byte[] fileData)
    {
        switch (format)
        {
        case AudioFormat.WAV:
            ParseWavFormat(fileData);
            break;
        case AudioFormat.MP3:
            ParseMp3Format(fileData);
            break;
        default:
            // Use default values
            sampleRate = 44100;
            channels = 2;
            bitsPerSample = 16;
            break;
        }
    }

    private void ParseWavFormat(byte[] fileData)
    {
        if (fileData.Length < 44)
            throw new AudioException("Invalid WAV file format");

        // Parse WAV header
        sampleRate = BitConverter.ToInt32(fileData, 24);
        channels = BitConverter.ToInt16(fileData, 22);
        bitsPerSample = BitConverter.ToInt16(fileData, 34);
    }

    private void ParseMp3Format(byte[] fileData)
    {
        // MP3 format parsing would require a more complex implementation
        // For now, use default values
        sampleRate = 44100;
        channels = 2;
        bitsPerSample = 16;
    }

    private AudioDevice GetAudioDevice()
    {
        // Get the global audio device instance
        // This would typically be managed by the SoundManager
        return new AudioDevice();
    }

    public bool Play()
    {
        if (isDisposed || sampleHandle == IntPtr.Zero || audioDevice == null)
            return false;

        try
        {
            if (audioDevice.StartSample(sampleHandle))
            {
                isPlaying = true;
                isPaused = false;
                return true;
            }
            return false;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public bool Stop()
    {
        if (isDisposed || sampleHandle == IntPtr.Zero || audioDevice == null)
            return false;

        try
        {
            if (audioDevice.StopSample(sampleHandle))
            {
                isPlaying = false;
                isPaused = false;
                return true;
            }
            return false;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public bool Pause()
    {
        if (isDisposed || !isPlaying)
            return false;

        // Note: The original implementation doesn't seem to have pause functionality
        // This is a simplified implementation
        isPaused = true;
        return true;
    }

    public bool Resume()
    {
        if (isDisposed || !isPaused)
            return false;

        // Note: The original implementation doesn't seem to have resume functionality
        // This is a simplified implementation
        isPaused = false;
        return true;
    }

    public void SetVolume(byte volume)
    {
        this.volume = volume;
            
        if (sampleHandle != IntPtr.Zero && audioDevice != null)
        {
            try
            {
                audioDevice.SetSampleVolume(sampleHandle, volume);
            }
            catch
            {
                // Ignore volume setting errors
            }
        }
    }

    public byte GetVolume()
    {
        return volume;
    }

    public bool IsPlaying()
    {
        return isPlaying && !isPaused;
    }

    public bool IsPaused()
    {
        return isPaused;
    }

    public void Update()
    {
        // Update sample state
        // In a real implementation, this would check if the sample has finished playing
        if (isPlaying && !isPaused)
        {
            // Check if sample has finished (this would require additional native calls)
            // For now, we'll assume it's still playing
        }
    }

    public int GetSampleRate()
    {
        return sampleRate;
    }

    public int GetChannels()
    {
        return channels;
    }

    public int GetBitsPerSample()
    {
        return bitsPerSample;
    }

    public long GetDataSize()
    {
        return dataSize;
    }

    public string GetFilePath()
    {
        return filePath;
    }

    public AudioFormat GetFormat()
    {
        return format;
    }

    private void Cleanup()
    {
        if (sampleHandle != IntPtr.Zero)
        {
            try
            {
                audioDevice?.ReleaseSampleHandle(sampleHandle);
            }
            catch
            {
                // Ignore cleanup errors
            }
            sampleHandle = IntPtr.Zero;
        }

        if (audioData != IntPtr.Zero)
        {
            try
            {
                Marshal.FreeHGlobal(audioData);
            }
            catch
            {
                // Ignore cleanup errors
            }
            audioData = IntPtr.Zero;
        }

        audioDevice?.Dispose();
        audioDevice = null;
    }

    public void Dispose()
    {
        if (!isDisposed)
        {
            Cleanup();
            isDisposed = true;
        }
    }
}