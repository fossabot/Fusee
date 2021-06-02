using System;

namespace Fusee.Examples.Simple.Wpf.CustomComponents
{
    /// <summary>
    /// event args for draw timing
    /// </summary>
    public class FbRenderEventArgs : EventArgs
    {
        /// <summary>
        /// true if handler requires drawing (refresh image)
        /// initially false , means skip refreshing the image
        /// </summary>
        public bool Redrawn { get; set; }

        /// <summary>
        /// time from CompositionTarget_Rendering has attached
        /// this is not the interval
        /// </summary>
        public TimeSpan RenderingTime { get; set; }
    }
}
