using OpenTK.Windowing.Desktop;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using FramebufferAttachment = OpenTK.Graphics.OpenGL.FramebufferAttachment;
using FramebufferErrorCode = OpenTK.Graphics.OpenGL.FramebufferErrorCode;
using FramebufferTarget = OpenTK.Graphics.OpenGL.FramebufferTarget;
using GL = OpenTK.Graphics.OpenGL.GL;
using PixelFormat = OpenTK.Graphics.OpenGL.PixelFormat;
using PixelType = OpenTK.Graphics.OpenGL.PixelType;
using RenderbufferStorage = OpenTK.Graphics.OpenGL.RenderbufferStorage;
using RenderbufferTarget = OpenTK.Graphics.OpenGL.RenderbufferTarget;

namespace Fusee.Examples.Simple.Wpf.CustomComponents
{
    public class FbRenderer
    {
        internal event EventHandler Reload;
        internal event EventHandler<FbRenderEventArgs> Render;
        internal bool ReloadRequested;

        public GameWindow GameWindow
        {
            get { return _gameWindow; }
            set
            {
                IsLoaded = false;
                _gameWindow = value;
                _gameWindow.IsVisible = false;
                _gameWindow.MakeCurrent();
            }
        }
        private GameWindow _gameWindow;

        public double DrawTime { get; private set; }
        private Stopwatch _watch = new Stopwatch();

        internal int FramebufferId;
        internal int ColorbufferId;
        internal int DepthbufferId;

        internal Size BufferSize;
        internal bool IsLoaded;

        private static byte[] _backbuffer;          // FBO pixels read buffer , create statically to avoid GC
        private WriteableBitmap _writableBitmap;    // ImageSource for drawingContext.DrawImage

        internal void DoRender(DrawingContext drawingContext, int widht, int height)
        {
            //  Start the stopwatch so that we can time the rendering.
            _watch.Restart();

            // import from FrameBufferHandler
            if (!GameWindow.Context.IsCurrent)
            {
                GameWindow.MakeCurrent();
            }

            if (widht != BufferSize.Width || height != BufferSize.Height || !IsLoaded)
            {
                BufferSize = new Size(widht, height);
                CreateFramebuffer();
                GL.Viewport(0, 0, widht, height);
            }

            GL.BindFramebuffer(FramebufferTarget.Framebuffer, FramebufferId);

            //	If there is a draw handler, then call it.
            var handler = Render;
            if (handler != null)
            {
                var ev = new FbRenderEventArgs() { Redrawn = false };
                handler(this, ev);
                if (!ev.Redrawn)
                {
                    // handler doesn't draw anything then skip refresh the image
                    _watch.Stop();
                    return;
                }
            }
            else
            {
                GL.Clear(OpenTK.Graphics.OpenGL.ClearBufferMask.ColorBufferBit);
            }

            //wait until FBO has completed drawing
            GL.Finish();

            if (_writableBitmap == null || _writableBitmap.Width != (int)BufferSize.Width || _writableBitmap.Height != (int)BufferSize.Height)
            {
                _writableBitmap = new WriteableBitmap((int)BufferSize.Width, (int)BufferSize.Height, 96, 96, PixelFormats.Bgr32, BitmapPalettes.WebPalette);
                _backbuffer = new byte[(int)BufferSize.Width * (int)BufferSize.Height * 4];
            }

            GL.ReadPixels(0, 0, (int)BufferSize.Width, (int)BufferSize.Height, PixelFormat.Bgra, PixelType.UnsignedByte, _backbuffer);

            // WriteableBitmap should be locked as short as possible
            _writableBitmap.Lock();

            // copy pixels upside down
            var src = new Int32Rect(0, 0, (int)_writableBitmap.Width, 1);
            for (int y = 0; y < (int)_writableBitmap.Height; y++)
            {
                src.Y = (int)_writableBitmap.Height - y - 1;
                _writableBitmap.WritePixels(src, _backbuffer, _writableBitmap.BackBufferStride, 0, y);
            }

            _writableBitmap.AddDirtyRect(new Int32Rect(0, 0, (int)_writableBitmap.Width, (int)_writableBitmap.Height));
            _writableBitmap.Unlock();

            if (_backbuffer != null)
            {
                drawingContext.DrawImage(_writableBitmap, new Rect(0, 0, BufferSize.Width, BufferSize.Height));
            }

            _watch.Stop();

            //Store the frame drawing time
            DrawTime = _watch.Elapsed.TotalMilliseconds;

            if (ReloadRequested)
            {
                Reload?.Invoke(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Create FBO for off-screen rendering using a render buffer
        /// </summary>
        private void CreateFramebuffer()
        {
            GameWindow.MakeCurrent();

            Deletebuffers();

            FramebufferId = GL.GenFramebuffer();
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, FramebufferId);

            ColorbufferId = GL.GenRenderbuffer();
            GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, ColorbufferId);
            GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, RenderbufferStorage.Rgba8, (int)BufferSize.Width, (int)BufferSize.Height);

            DepthbufferId = GL.GenRenderbuffer();
            GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, DepthbufferId);
            GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, RenderbufferStorage.DepthComponent24, (int)BufferSize.Width, (int)BufferSize.Height);

            GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, RenderbufferTarget.Renderbuffer, ColorbufferId);
            GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, RenderbufferTarget.Renderbuffer, DepthbufferId);

            if (GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer) != FramebufferErrorCode.FramebufferComplete)
                throw new Exception("Failed to create FrameBuffer for OpenGLControl");

            IsLoaded = true;
        }

        private void Deletebuffers()
        {
            if (FramebufferId > 0)
                GL.DeleteFramebuffer(FramebufferId);

            if (ColorbufferId > 0)
                GL.DeleteRenderbuffer(ColorbufferId);

            if (DepthbufferId > 0)
                GL.DeleteRenderbuffer(DepthbufferId);
        }
    }
}
