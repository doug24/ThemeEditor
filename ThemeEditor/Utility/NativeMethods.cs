using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace ThemeEditor
{
    [ComVisible(false)]
    internal sealed partial class NativeMethods
    {

#pragma warning disable SYSLIB1054
        // cannot get GetMonitorInfo to work using LibraryImport,
        // so keeping this as-is
        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        internal static extern bool GetMonitorInfo(IntPtr hMonitor, [In, Out] MONITORINFOEX info);
#pragma warning restore SYSLIB1054

        [LibraryImport("User32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static unsafe partial bool EnumDisplayMonitors(IntPtr hdc, RECT* lprcClip, MONITORENUMPROC lpfnEnum, void* dwData);

        [LibraryImport("User32.dll", SetLastError = true)]
        public static partial IntPtr MonitorFromWindow(IntPtr hwnd, MonitorOptions dwFlags);

        [LibraryImport("User32.dll", SetLastError = true)]
        public static partial IntPtr MonitorFromPoint(POINT pt, MonitorOptions dwFlags);

        [LibraryImport("User32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static partial bool MoveWindow(
            IntPtr hWnd,
            int X,
            int Y,
            int nWidth,
            int nHeight,
            [MarshalAs(UnmanagedType.Bool)] bool bRepaint);


        [LibraryImport("SHCore.dll")]
        public static partial HResult GetDpiForMonitor(IntPtr hMonitor, MONITOR_DPI_TYPE dpiType, out int dpiX, out int dpiY);
    }

    public struct POINT
    {
        /// <summary>
        ///  The x-coordinate of the point.
        /// </summary>
        public int x;

        /// <summary>
        /// The x-coordinate of the point.
        /// </summary>
        public int y;

        public static implicit operator System.Windows.Point(POINT point) => new(point.x, point.y);

        public static implicit operator POINT(System.Windows.Point point) => new() { x = (int)point.X, y = (int)point.Y };
    }

    public struct RECT
    {
        /// <summary>
        /// The x-coordinate of the upper-left corner of the rectangle.
        /// </summary>
        public int left;

        /// <summary>
        /// The y-coordinate of the upper-left corner of the rectangle.
        /// </summary>
        public int top;

        /// <summary>
        /// The x-coordinate of the lower-right corner of the rectangle.
        /// </summary>
        public int right;

        /// <summary>
        /// The y-coordinate of the lower-right corner of the rectangle.
        /// </summary>
        public int bottom;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto, Pack = 4)]
    internal class MONITORINFOEX
    {
        internal uint cbSize = (uint)Marshal.SizeOf(typeof(MONITORINFOEX));
        internal RECT rcMonitor = new();
        internal RECT rcWork = new();
        internal uint dwFlags = 0;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)] internal char[] szDevice = new char[32];
    }
    public enum MonitorOptions : uint
    {
        MONITOR_DEFAULTTONULL = 0x00000000,
        MONITOR_DEFAULTTOPRIMARY = 0x00000001,
        MONITOR_DEFAULTTONEAREST = 0x00000002,
    }
    public enum MONITOR_DPI_TYPE
    {
        /// <summary>
        /// The effective DPI. This value should be used when determining the correct scale factor for scaling UI elements.
        /// This incorporates the scale factor set by the user for this specific display
        /// </summary>
        MDT_EFFECTIVE_DPI = 0,

        /// <summary>
        /// The angular DPI. This DPI ensures rendering at a compliant angular resolution on the screen.
        /// This does not include the scale factor set by the user for this specific display
        /// </summary>
        MDT_ANGULAR_DPI = 1,

        /// <summary>
        /// The raw DPI. This value is the linear DPI of the screen as measured on the screen itself.
        /// Use this value when you want to read the pixel density and not the recommended scaling setting.
        /// This does not include the scale factor set by the user for this specific display and is not guaranteed to be a supported DPI value
        /// </summary>
        MDT_RAW_DPI = 2,

        /// <summary>
        /// The default is the same as <see cref="MDT_EFFECTIVE_DPI"/>
        /// </summary>
        MDT_DEFAULT = MDT_EFFECTIVE_DPI,
    }

    public unsafe delegate bool MONITORENUMPROC(IntPtr hMonitor, IntPtr hdcMonitor, RECT* lprcMonitor, void* dwData);


    /// <summary>
    /// Describes an HRESULT error or success condition.
    /// </summary>
    /// <remarks>
    ///  HRESULTs are 32 bit values layed out as follows:
    /// <code>
    ///   3 3 2 2 2 2 2 2 2 2 2 2 1 1 1 1 1 1 1 1 1 1
    ///   1 0 9 8 7 6 5 4 3 2 1 0 9 8 7 6 5 4 3 2 1 0 9 8 7 6 5 4 3 2 1 0
    ///  +-+-+-+-+-+---------------------+-------------------------------+
    ///  |S|R|C|N|r|    Facility         |               Code            |
    ///  +-+-+-+-+-+---------------------+-------------------------------+
    ///
    ///  where
    ///
    ///      S - Severity - indicates success/fail
    ///
    ///          0 - Success
    ///          1 - Fail (COERROR)
    ///
    ///      R - reserved portion of the facility code, corresponds to NT's
    ///              second severity bit.
    ///
    ///      C - reserved portion of the facility code, corresponds to NT's
    ///              C field.
    ///
    ///      N - reserved portion of the facility code. Used to indicate a
    ///              mapped NT status value.
    ///
    ///      r - reserved portion of the facility code. Reserved for internal
    ///              use. Used to indicate HRESULT values that are not status
    ///              values, but are instead message ids for display strings.
    ///
    ///      Facility - is the facility code
    ///
    ///      Code - is the facility's status code
    /// </code>
    /// </remarks>
    [DebuggerDisplay("{Value}")]
    public partial struct HResult : IComparable, IComparable<HResult>, IEquatable<HResult>, IFormattable
    {
        /// <summary>
        /// The mask of the bits that describe the <see cref="Severity"/>.
        /// </summary>
        private const uint SeverityMask = 0x80000000;

        /// <summary>
        /// The number of bits that <see cref="Severity"/> values are shifted
        /// in order to fit within <see cref="SeverityMask"/>.
        /// </summary>
        private const int SeverityShift = 31;

        /// <summary>
        /// The mask of the bits that describe the <see cref="Facility"/>.
        /// </summary>
        private const int FacilityMask = 0x7ff0000;

        /// <summary>
        /// The number of bits that <see cref="Facility"/> values are shifted
        /// in order to fit within <see cref="FacilityMask"/>.
        /// </summary>
        private const int FacilityShift = 16;

        /// <summary>
        /// The mask of the bits that describe the <see cref="FacilityStatus"/>.
        /// </summary>
        private const int FacilityStatusMask = 0xffff;

        /// <summary>
        /// The number of bits that <see cref="FacilityStatus"/> values are shifted
        /// in order to fit within <see cref="FacilityStatusMask"/>.
        /// </summary>
        private const int FacilityStatusShift = 0;

        /// <summary>
        /// Initializes a new instance of the <see cref="HResult"/> struct.
        /// </summary>
        /// <param name="value">The value of the HRESULT.</param>
        public HResult(Code value)
        {
            this.Value = value;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HResult"/> struct.
        /// </summary>
        /// <param name="value">The value of the HRESULT.</param>
        public HResult(int value)
            : this((Code)value)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HResult"/> struct.
        /// </summary>
        /// <param name="value">The value of the HRESULT.</param>
        public HResult(uint value)
            : this((Code)value)
        {
        }

        /// <summary>
        /// Gets the full HRESULT value, as a <see cref="Code"/> enum.
        /// </summary>
        public Code Value { get; }

        /// <summary>
        /// Gets the HRESULT as a 32-bit signed integer.
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        public int AsInt32 => (int)this.Value;

        /// <summary>
        /// Gets the HRESULT as a 32-bit unsigned integer.
        /// </summary>
        public uint AsUInt32 => (uint)this.Value;

        /// <summary>
        /// Gets a value indicating whether this HRESULT represents a successful operation.
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        public bool Succeeded => this.Severity == SeverityCode.Success;

        /// <summary>
        /// Gets a value indicating whether this HRESULT represents a failed operation.
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        public bool Failed => this.Severity == SeverityCode.Fail;

        ///// <summary>
        ///// Gets the facility code of the HRESULT.
        ///// </summary>
        //public FacilityCode Facility => (FacilityCode)((this.AsUInt32 & FacilityMask) >> FacilityShift);

        /// <summary>
        /// Gets the severity of the HRESULT.
        /// </summary>
        public SeverityCode Severity => (SeverityCode)((this.AsUInt32 & SeverityMask) >> SeverityShift);

        /// <summary>
        /// Gets the facility's status code bits from the HRESULT.
        /// </summary>
        public uint FacilityStatus => this.AsUInt32 & FacilityStatusMask;

        /// <summary>
        /// Converts an <see cref="int"/> into an <see cref="HResult"/>.
        /// </summary>
        /// <param name="hr">The value of the HRESULT.</param>
        public static implicit operator HResult(int hr) => new HResult(hr);

        /// <summary>
        /// Converts an <see cref="HResult"/> into an <see cref="int"/>.
        /// </summary>
        /// <param name="hr">The value of the HRESULT.</param>
        public static implicit operator int(HResult hr) => hr.AsInt32;

        /// <summary>
        /// Converts an <see cref="uint"/> into an <see cref="HResult"/>.
        /// </summary>
        /// <param name="hr">The value of the HRESULT.</param>
        public static implicit operator HResult(uint hr) => new HResult(hr);

        /// <summary>
        /// Converts an <see cref="HResult"/> into an <see cref="uint"/>.
        /// </summary>
        /// <param name="hr">The value of the HRESULT.</param>
        public static explicit operator uint(HResult hr) => hr.AsUInt32;

        /// <summary>
        /// Converts a <see cref="Code"/> enum to its structural <see cref="HResult"/> representation.
        /// </summary>
        /// <param name="hr">The value to convert.</param>
        public static implicit operator HResult(Code hr) => new HResult(hr);

        /// <summary>
        /// Converts an <see cref="HResult"/> to its <see cref="Code"/> enum representation.
        /// </summary>
        /// <param name="hr">The value to convert.</param>
        public static implicit operator Code(HResult hr) => hr.Value;

        /// <summary>
        /// Checks equality between this HResult and a <see cref="uint"/> value.
        /// </summary>
        /// <param name="hr">An <see cref="HResult"/>.</param>
        /// <param name="value">Some <see cref="uint"/> value.</param>
        /// <returns><c>true</c> if they equal; <c>false</c> otherwise.</returns>
        /// <remarks>
        /// This operator overload is useful because HResult-uint conversion must be explicit,
        /// and without this overload, it makes comparing HResults to 0x8xxxxxxx values require casts.
        /// </remarks>
        public static bool operator ==(HResult hr, uint value) => hr.AsUInt32 == value;

        /// <summary>
        /// Checks inequality between this HResult and a <see cref="uint"/> value.
        /// </summary>
        /// <param name="hr">An <see cref="HResult"/>.</param>
        /// <param name="value">Some <see cref="uint"/> value.</param>
        /// <returns><c>true</c> if they unequal; <c>false</c> otherwise.</returns>
        /// <remarks>
        /// This operator overload is useful because HResult-uint conversion must be explicit,
        /// and without this overload, it makes comparing HResults to 0x8xxxxxxx values require casts.
        /// </remarks>
        public static bool operator !=(HResult hr, uint value) => hr.AsUInt32 != value;

        /// <summary>
        /// Throws an exception if this HRESULT <see cref="Failed"/>, based on the failure value.
        /// </summary>
        public void ThrowOnFailure()
        {
            Marshal.ThrowExceptionForHR(this.AsInt32);
        }

        /// <summary>
        /// Throws an exception if this HRESULT <see cref="Failed"/>, based on the failure value and the specified IErrorInfo interface.
        /// </summary>
        /// <param name="errorInfo">
        /// A pointer to the IErrorInfo interface that provides more information about the
        /// error. You can specify IntPtr(0) to use the current IErrorInfo interface, or
        /// IntPtr(-1) to ignore the current IErrorInfo interface and construct the exception
        /// just from the error code.
        /// </param>
        public void ThrowOnFailure(IntPtr errorInfo)
        {
            Marshal.ThrowExceptionForHR(this.AsInt32, errorInfo);
        }

        /// <summary>
        /// Gets an exception that represents this <see cref="HResult" />
        /// if it represents a failure.
        /// </summary>
        /// <returns>
        /// The exception, if applicable; otherwise null.
        /// </returns>
        public Exception GetException() => Marshal.GetExceptionForHR(this) ?? new Exception();

        /// <summary>
        /// Gets an exception that represents this <see cref="HResult" />
        /// if it represents a failure.
        /// </summary>
        /// <param name="errorInfo">
        /// A pointer to additional error information that may be used to populate the Exception.
        /// </param>
        /// <returns>
        /// The exception, if applicable; otherwise null.
        /// </returns>
        public Exception GetException(IntPtr errorInfo) => Marshal.GetExceptionForHR(this, errorInfo) ?? new Exception();

        /// <inheritdoc />
        public override int GetHashCode() => this.AsInt32;

        /// <inheritdoc />
        public bool Equals(HResult other) => this.Value == other.Value;

        /// <inheritdoc />
        public override bool Equals(object? obj) => obj is HResult && this.Equals((HResult)obj);

        /// <inheritdoc />
        public int CompareTo(object? obj) => ((IComparable)this.Value).CompareTo(obj);

        /// <inheritdoc />
        public int CompareTo(HResult other) => this.Value.CompareTo(other.Value);

        /// <inheritdoc />
        public override string ToString() => this.Value.ToString();

        /// <inheritdoc />
        public string ToString(string? format, IFormatProvider? formatProvider) => this.AsUInt32.ToString(format, formatProvider);

        /// <summary>
        /// HRESULT severity codes defined by winerror.h.
        /// </summary>
        public enum SeverityCode : uint
        {
            Success = 0,
            Fail = 1,
        }

        /// <summary>
        /// Common HRESULT constants.
        /// </summary>
        public enum Code : uint
        {
            /// <summary>
            /// Operation successful, and returned a false result.
            /// </summary>
            S_FALSE = 1,

            /// <summary>
            /// Operation successful
            /// </summary>
            S_OK = 0,

            /// <summary>
            /// Unspecified failure
            /// </summary>
            E_FAIL = 0x80004005,

            /// <summary>
            /// Operation aborted
            /// </summary>
            E_ABORT = 0x80004004,

            /// <summary>
            /// General access denied error
            /// </summary>
            E_ACCESSDENIED = 0x80070005,

            /// <summary>
            /// Handle that is not valid
            /// </summary>
            E_HANDLE = 0x80070006,

            /// <summary>
            /// One or more arguments are not valid
            /// </summary>
            E_INVALIDARG = 0x80070057,

            /// <summary>
            /// No such interface supported
            /// </summary>
            E_NOINTERFACE = 0x80004002,

            /// <summary>
            /// Not implemented
            /// </summary>
            E_NOTIMPL = 0x80004001,

            /// <summary>
            /// Failed to allocate necessary memory
            /// </summary>
            E_OUTOFMEMORY = 0x8007000E,

            /// <summary>
            /// Pointer that is not valid
            /// </summary>
            E_POINTER = 0x80004003,

            /// <summary>
            /// Unexpected failure
            /// </summary>
            E_UNEXPECTED = 0x8000FFFF,

            /// <summary>
            /// The call was already canceled
            /// </summary>
            RPC_E_CALL_CANCELED = 0x80010002,

            /// <summary>
            /// The call was completed during the timeout interval
            /// </summary>
            RPC_E_CALL_COMPLETE = 0x80010117,

            /// <summary>
            /// Call cancellation is not enabled on the specified thread
            /// </summary>
            CO_E_CANCEL_DISABLED = 0x80010140,
        }
    }
}
