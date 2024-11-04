using System;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;

namespace ThemeEditor
{
    public static class Extensions
    {
        public static Rect ToDevicePixels(this Screen screen, Rect rect)
        {
            double scaleX, scaleY;

            if (OperatingSystem.IsWindowsVersionAtLeast(10, 0, build: 14393, 0))
            {
                POINT pt = new()
                {
                    x = (int)(screen.Bounds.Left + screen.Bounds.Width / 2),
                    y = (int)(screen.Bounds.Top + screen.Bounds.Height / 2)
                };

                var hMonitor = NativeMethods.MonitorFromPoint(pt, MonitorOptions.MONITOR_DEFAULTTONEAREST);
                _ = NativeMethods.GetDpiForMonitor(hMonitor, MONITOR_DPI_TYPE.MDT_EFFECTIVE_DPI, out int dpiX, out int dpiY);
                scaleX = (double)dpiX / 96;
                scaleY = (double)dpiY / 96;
            }
            else
            {
                // an old version of Windows (7 or 8)
                // Get scale of main window and assume scale is the same for all monitors
                var dpiScale = VisualTreeHelper.GetDpi(Application.Current.MainWindow);
                scaleX = dpiScale.DpiScaleX;
                scaleY = dpiScale.DpiScaleY;
            }

            Rect result = new(
                rect.X * scaleX,
                rect.Y * scaleY,
                rect.Width * scaleX,
                rect.Height * scaleY);
            return result;
        }

        public static Rect FromDevicePixels(this Window window, Rect rect)
        {
            var t = PresentationSource.FromVisual(window).CompositionTarget.TransformFromDevice;
            var topLeft = t.Transform(rect.TopLeft);
            var botRight = t.Transform(rect.BottomRight);
            return new Rect(topLeft, botRight);
        }

        public static bool IsOnScreen(this Rect windowBounds)
        {
            // test to see if the center of the title bar is on a screen
            // this will allow the user to easily move the window if partially off screen
            // 44 is the width of a title bar button, 30 is the height
            Rect bounds = new(
                windowBounds.Left + 5 + 44,
                windowBounds.Top + 5,
                Math.Max(windowBounds.Width - 3 * 44, 44),  // can't be negative!
                30);

            foreach (Screen screen in Screen.AllScreens)
            {
                Rect deviceRect = screen.ToDevicePixels(bounds);
                if (screen.WorkingArea.IntersectsWith(deviceRect))
                {
                    return true;
                }
            }
            return false;
        }

        public static Screen? ScreenFromWpfPoint(this Point pt)
        {
            Rect bounds = new(pt, new Size(2, 2));
            foreach (Screen screen in Screen.AllScreens)
            {
                Rect deviceRect = screen.ToDevicePixels(bounds);
                if (screen.WorkingArea.Contains(deviceRect))
                {
                    return screen;
                }
            }
            return null;
        }

        public static bool MoveWindow(this Window window, double x, double y)
        {
            double width = window.Width;
            double height = window.Height;
            Point pt = new(x, y);
            Screen? screen = pt.ScreenFromWpfPoint();
            if (screen != null)
            {
                Rect bounds = new(x, y, window.ActualWidth, window.ActualHeight);
                Rect r = screen.ToDevicePixels(bounds);
                if (NativeMethods.MoveWindow(new(new WindowInteropHelper(window).Handle), (int)r.Left, (int)r.Top, (int)r.Width, (int)r.Height, true))
                {
                    window.Dispatcher.Invoke(() =>
                    {
                        window.Width = width;
                        window.Height = height;

                        //var w = window.ActualWidth;
                        //var h = window.ActualHeight;
                    });
                }
            }
            return false;
        }

        public static void ConstrainToScreen(this Window window)
        {
            // don't let the window grow beyond the right edge of the screen
            var screen = Screen.FromHandle(new WindowInteropHelper(window).Handle);
            var bounds = window.FromDevicePixels(screen.WorkingArea);
            if (window.Left + window.ActualWidth > bounds.Right)
            {
                window.Width = Math.Min(20, bounds.Right - window.Left);  // can't be negative!
            }
        }

        public static void CenterWindow(this Window window)
        {
            var screen = Screen.FromHandle(new WindowInteropHelper(window).Handle);
            var rect = window.FromDevicePixels(screen.WorkingArea);

            double screenWidth = rect.Width;
            double screenHeight = rect.Height;

            if (window.ActualHeight > screenHeight)
                window.Height = screenHeight - 20;
            if (window.ActualWidth > screenWidth)
                window.Width = screenWidth - 20;

            window.Left = (screenWidth / 2) - (window.ActualWidth / 2);
            window.Top = (screenHeight / 2) - (window.ActualHeight / 2);
        }
    }
}
