namespace DarkAges.Library.Audio;

/// <summary>
/// Represents different audio formats supported by the audio system
/// </summary>
public enum AudioFormat
{
    /// <summary>
    /// Unknown or unsupported format
    /// </summary>
    Unknown = 0,

    /// <summary>
    /// WAV audio format
    /// </summary>
    WAV = 1,
    Wav = 1,

    /// <summary>
    /// MP3 audio format
    /// </summary>
    MP3 = 2,
    Mp3 = 2,

    /// <summary>
    /// OGG Vorbis audio format
    /// </summary>
    Ogg = 3,

    /// <summary>
    /// FLAC audio format
    /// </summary>
    Flac = 4,

    /// <summary>
    /// Raw PCM audio data
    /// </summary>
    Raw = 5,

    /// <summary>
    /// MIDI format
    /// </summary>
    Midi = 6,

    /// <summary>
    /// MOD tracker format
    /// </summary>
    Mod = 7,

    /// <summary>
    /// S3M tracker format
    /// </summary>
    S3M = 8,

    /// <summary>
    /// XM tracker format
    /// </summary>
    XM = 9,

    /// <summary>
    /// IT tracker format
    /// </summary>
    IT = 10
}