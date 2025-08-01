using System.Runtime.InteropServices;

namespace DarkAges.Library.IO;

[StructLayout(LayoutKind.Sequential)]
public struct CANDIDATELIST
{
    public uint dwSize;
    public uint dwStyle;
    public uint dwCount;
    public uint dwSelection;
    public uint dwPageStart;
    public uint dwPageSize;
    public uint dwOffset; 
}