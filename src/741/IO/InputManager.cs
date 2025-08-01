using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using DarkAges.Library.UI;
using System.Text;

namespace DarkAges.Library.IO;

public class InputManager(IntPtr hWnd, IMECandidatePane candidatePane)
{
    private IntPtr _hImc = ImmGetContext(hWnd);
    private bool _isComposing;

    [DllImport("imm32.dll")]
    private static extern IntPtr ImmGetContext(IntPtr hWnd);

    [DllImport("imm32.dll")]
    private static extern bool ImmReleaseContext(IntPtr hWnd, IntPtr hImc);

    [DllImport("imm32.dll")]
    private static extern bool ImmSetOpenStatus(IntPtr hImc, bool b);

    [DllImport("imm32.dll", CharSet = CharSet.Ansi)]
    private static extern int ImmGetCompositionString(IntPtr hImc, uint dwIndex, byte[] lpBuf, uint dwBufLen);

    [DllImport("imm32.dll")]
    private static extern int ImmGetCandidateList(IntPtr hImc, uint deIndex, byte[] lpCandList, uint dwBufLen);

    public void Cleanup()
    {
        if (_hImc != IntPtr.Zero)
        {
            ImmReleaseContext(hWnd, _hImc);
            _hImc = IntPtr.Zero;
        }
    }

    public bool WndProc(int msg, IntPtr wParam, IntPtr lParam)
    {
        const int WM_IME_COMPOSITION = 0x010F;
        const int WM_IME_NOTIFY = 0x0282;
        const int GCS_RESULTSTR = 0x800;
        const int GCS_COMPSTR = 8;
        const int IMN_OPENCANDIDATE = 2;
        const int IMN_CLOSECANDIDATE = 3;

        switch (msg)
        {
        case WM_IME_COMPOSITION:
            if (((int)lParam & GCS_COMPSTR) != 0)
            {
                var len = ImmGetCompositionString(_hImc, GCS_COMPSTR, null, 0);
                if (len > 0)
                {
                    var buffer = new byte[len];
                    ImmGetCompositionString(_hImc, GCS_COMPSTR, buffer, (uint)len);
                    // A real implementation would display this composition string in the text input area.
                    _isComposing = true;
                }
                else
                {
                    _isComposing = false;
                }
            }
            if (((int)lParam & GCS_RESULTSTR) != 0)
            {
                var len = ImmGetCompositionString(_hImc, GCS_RESULTSTR, null, 0);
                if (len > 0)
                {
                    var buffer = new byte[len];
                    ImmGetCompositionString(_hImc, GCS_RESULTSTR, buffer, (uint)len);
                    // A real implementation would commit this result string to the text input area.
                    _isComposing = false;
                }
            }
            return true;

        case WM_IME_NOTIFY:
            if ((int)wParam == IMN_OPENCANDIDATE)
            {
                var size = ImmGetCandidateList(_hImc, 0, null, 0);
                if (size > 0)
                {
                    var buffer = new byte[size];
                    ImmGetCandidateList(_hImc, 0, buffer, (uint)size);
                            
                    var candList = MemoryMarshal.Read<CANDIDATELIST>(buffer);
                            
                    var candidates = new List<string>();
                    var offsets = new int[candList.dwCount];
                            
                    // Copy offsets from the buffer
                    Buffer.BlockCopy(buffer, (int)candList.dwOffset, offsets, 0, (int)candList.dwCount * sizeof(int));

                    for (var i = 0; i < candList.dwCount; i++)
                    {
                        var offset = offsets[i];
                        // Find null terminator
                        var end = Array.IndexOf(buffer, (byte)0, offset);
                        if(end == -1) end = size;
                        candidates.Add(Encoding.Default.GetString(buffer, offset, end - offset));
                    }
                    candidatePane.ShowCandidates(candidates, (int)candList.dwSelection);
                }
            }
            else if ((int)wParam == IMN_CLOSECANDIDATE)
            {
                candidatePane.Hide();
            }
            return true;
        }

        return false;
    }
}