using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Interop;

namespace ThemeEditor
{
    public class Screen
    {
        private readonly IntPtr hMonitor;

        unsafe private Screen(IntPtr monitor)
        {
            //var info = new PInvoke.
            MONITORINFOEX info = new();
            NativeMethods.GetMonitorInfo(monitor, info);

            int dpiX = 96, dpiY = 96;
            if (OperatingSystem.IsWindowsVersionAtLeast(10, 0, build: 14393, 0))
            {
                _ = NativeMethods.GetDpiForMonitor(monitor, dpiType: MONITOR_DPI_TYPE.MDT_EFFECTIVE_DPI, out dpiX, out dpiY);
            }

            ScaleX = dpiX / 96.0;
            ScaleY = dpiY / 96.0;

            Bounds = new Rect(
                info.rcMonitor.left, info.rcMonitor.top,
                info.rcMonitor.right - info.rcMonitor.left,
                info.rcMonitor.bottom - info.rcMonitor.top);

            BoundsDip = new(
                Bounds.Left / ScaleX, Bounds.Top / ScaleY,
                Bounds.Width / ScaleX, Bounds.Height / ScaleY);

            WorkingArea = new Rect(
                info.rcWork.left, info.rcWork.top,
                info.rcWork.right - info.rcWork.left,
                info.rcWork.bottom - info.rcWork.top);

            WorkingAreaDip = new(
                WorkingArea.Left / ScaleX, WorkingArea.Top / ScaleY,
                WorkingArea.Width / ScaleX, WorkingArea.Height / ScaleY);

            Primary = (info.dwFlags & (uint)MonitorOptions.MONITOR_DEFAULTTOPRIMARY) != 0;

            DeviceName = new string(info.szDevice).TrimEnd((char)0);

            hMonitor = monitor;
        }

        public override bool Equals(object? obj)
        {
            return obj is Screen screen &&
                   EqualityComparer<IntPtr>.Default.Equals(hMonitor, screen.hMonitor);
        }

        public override int GetHashCode()
        {
            return -1250308577 + hMonitor.GetHashCode();
        }

        public override string ToString()
        {
            return $"{DeviceName} [{WorkingAreaDip}]";
        }

        public double ScaleX { get; } = 1.0;
        public double ScaleY { get; } = 1.0;

        public Rect Bounds { get; private set; }

        public Rect BoundsDip { get; }

        public Rect WorkingArea { get; private set; }

        public Rect WorkingAreaDip { get; }

        public string DeviceName { get; }

        public bool Primary { get; }

        internal static Screen FromHandle(IntPtr handle)
        {
            return new Screen(NativeMethods.MonitorFromWindow(handle,
                MonitorOptions.MONITOR_DEFAULTTONEAREST));
        }

        public static Screen FromPoint(Point point)
        {
            return new(NativeMethods.MonitorFromPoint(point,
                MonitorOptions.MONITOR_DEFAULTTONEAREST));
        }

        public static Screen FromWindow(Window window)
        {
            return new Screen(NativeMethods.MonitorFromWindow(new(new WindowInteropHelper(window).Handle),
                MonitorOptions.MONITOR_DEFAULTTONEAREST));
        }

        unsafe public static IEnumerable<Screen> AllScreens
        {
            get
            {
                List<Screen> screens = [];

                NativeMethods.EnumDisplayMonitors(new IntPtr(), (RECT*)null,
                    delegate (IntPtr hMonitor, IntPtr IntPtrMonitor, RECT* lprcMonitor, void* dwData)
                    {
                        screens.Add(new Screen(hMonitor));
                        return true;

                    },
                    null);

                return screens;
            }
        }
    }
}
