using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media;

namespace Fusee.Examples.Integrations.WpfFramebuffer.Frontend.CustomComponents
{
    public class FbComponent : FrameworkElement
    {
        private TimeSpan _lastRenderTime = TimeSpan.Zero;
        public FbRenderer Renderer { get; private set; }

        public bool ReloadRequested
        {
            get => _reloadRequsted;
            set
            {
                _reloadRequsted = value;
                Renderer.ReloadRequested = value;
            }
        }
        private bool _reloadRequsted;

        /// <summary>
        /// Measured drawing time for a frame in milliseconds.
        /// </summary>
        internal static readonly DependencyProperty DrawTimeProperty = DependencyProperty.Register
        (
            "DrawTime",
            typeof(double),
            typeof(FbComponent),
            new PropertyMetadata(0.0)
        );

        public double DrawTime
        {
            get { return (double)GetValue(DrawTimeProperty); }
            set { SetValue(DrawTimeProperty, value); }
        }

        /// <summary>
        /// Occurs when the control is resized. This can be used to perform custom projections.
        /// </summary>
        [Description("Called when the control is resized - you can use this to do custom viewport projections."), Category("SharpGL")]
        public event EventHandler Resized;

        /// <summary>
        /// Occurs when OpenGL drawing should occur.
        /// </summary>
        [Description("Called whenever OpenGL drawing can should occur."), Category("SharpGL")]
        public event EventHandler<FbRenderEventArgs> Render;

        /// <summary>
        /// Occurs when the rendered scene is reloaded.
        /// </summary>
        [Description("Called whenever the rendered Scene should be reloaded."), Category("SharpGL")]
        public event EventHandler Reload;

        /// <summary>
        /// Initializes a new instance
        /// </summary>
        public FbComponent()
        {
            Renderer = new FbRenderer();

            Renderer.Render += HandleRender;
            Renderer.Reload += HandleReload;

            Unloaded += OpenGLControl_Unloaded;
            Loaded += OpenGLControl_Loaded;
        }

        internal void HandleRender(object sender, FbRenderEventArgs e)
        {
            Render?.Invoke(this, e);
        }

        internal void HandleReload(object sender, EventArgs e)
        {
            Reload?.Invoke(this, e);
        }

        /// <summary>
        /// When overridden in a derived class, is invoked whenever application code or 
        /// internal processes call <see cref="M:System.Windows.FrameworkElement.ApplyTemplate"/>.
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            //Initialize framebuffer handler for OpenTK
            Renderer.IsLoaded = false;
            Renderer.BufferSize = Size.Empty;
            Renderer.FramebufferId = -1;
            Renderer.ColorbufferId = -1;
            Renderer.DepthbufferId = -1;
        }

        /// <summary>
        /// Handles the Loaded event of the OpenGLControl control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> Instance containing the event data.</param>
        private void OpenGLControl_Loaded(object sender, RoutedEventArgs routedEventArgs)
        {
            Resized?.Invoke(this, EventArgs.Empty);

            // start rendering to be on WPF rendering timing
            CompositionTarget.Rendering += CompositionTarget_Rendering;
        }

        /// <summary>
        /// Handles the Unloaded event of the OpenGLControl control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> Instance containing the event data.</param>
        private void OpenGLControl_Unloaded(object sender, RoutedEventArgs routedEventArgs)
        {
            CompositionTarget.Rendering -= CompositionTarget_Rendering;
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo info)
        {
            var isInDesignMode = DesignerProperties.GetIsInDesignMode(this);
            if (isInDesignMode)
                return;

            if ((info.WidthChanged || info.HeightChanged) && info.NewSize.Width > 0 && info.NewSize.Height > 0)
            {
                Resized?.Invoke(this, EventArgs.Empty);
            }

            base.OnRenderSizeChanged(info);
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            var isDesignMode = DesignerProperties.GetIsInDesignMode(this);
            if (isDesignMode)
            {
                //DesignTimeHelper.DrawDesignTimeHelper(this, drawingContext);
            }
            else if (Renderer != null && IsLoaded)
            {
                Renderer.DoRender(drawingContext, (int)ActualWidth, (int)ActualHeight);
            }
            //else
            //{
            //    UnstartedControlHelper.DrawUnstartedControlHelper(this, drawingContext);
            //}

            base.OnRender(drawingContext);
            DrawTime = Renderer.DrawTime;
        }


        /// <summary>
        /// Handles the WPF rendering timing
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">cast RenderingEventArgs to know RenderginTime</param>
        private void CompositionTarget_Rendering(object sender, EventArgs e)
        {
            //https://evanl.wordpress.com/2009/12/06/efficient-optimal-per-frame-eventing-in-wpf/
            var args = (RenderingEventArgs)e;
            if (args.RenderingTime == _lastRenderTime)
            {
                return;
            }
            _lastRenderTime = args.RenderingTime;

            InvalidateVisual();
        }
    }
}