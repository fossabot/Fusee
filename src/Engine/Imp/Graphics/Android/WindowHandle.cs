using Fusee.Engine.Common;
using Android.Views;

namespace Fusee.Engine.Imp.Graphics.Android
{
    /// <summary>
    /// Implementation of the cross-platform abstraction of the window handle.
    /// </summary>
    public class WindowHandle : IWindowHandle
    {
        public WindowId WinId { get; internal set; }
    }
}
