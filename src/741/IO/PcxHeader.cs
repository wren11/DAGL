using System.Runtime.InteropServices;

namespace DarkAges.Library.IO;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct PcxHeader
{
    public byte Manufacturer;
    public byte Version;
    public byte Encoding;
    public byte BitsPerPixel;
    public ushort XMin, YMin, XMax, YMax;
    public ushort Hdpi, Vdpi;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 48)]
    public byte[] Colormap;
    public byte Reserved;
    public byte NPlanes;
    public ushort BytesPerLine;
    public ushort PaletteInfo;
    public ushort HScreenSize, VScreenSize;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 54)]
    public byte[] Filler;
}