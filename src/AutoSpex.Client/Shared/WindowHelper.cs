using System;
using System.Runtime.InteropServices;
using Avalonia.Platform;

namespace AutoSpex.Client.Shared;

public static class WindowHelper
{
    [Flags]
    private enum SetWindowPosFlags : uint
    {
        HideWindow = 128,
        ShowWindow = 64
    }

    [DllImport("user32.dll", SetLastError = true)]
    private static extern int SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy,
        uint uFlags);

    public static void FixAfterMaximizing(IntPtr hWnd, Screen screen)
    {
        SetWindowPos(hWnd, IntPtr.Zero, screen.WorkingArea.X, screen.WorkingArea.Y, screen.WorkingArea.Width,
            screen.WorkingArea.Height, (uint) SetWindowPosFlags.ShowWindow);
    }
}