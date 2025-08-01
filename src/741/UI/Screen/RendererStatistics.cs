namespace DarkAges.Library.UI.Screen;

/// <summary>
/// Renderer statistics
/// </summary>
public struct RendererStatistics
{
    public int DrawCalls;
    public int TrianglesRendered;
    public TimeSpan LastFrameTime;
    public int RenderWidth;
    public int RenderHeight;
    public int RenderMode;
    public bool IsRendering;
    public bool UseDoubleBuffering;
    public bool UseVsync;
    public bool UseAntiAliasing;
}