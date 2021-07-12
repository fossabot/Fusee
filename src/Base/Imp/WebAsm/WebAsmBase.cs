using Fusee.Engine.Imp.Graphics.WebAsm;
using Fusee.Math.Core;
using Microsoft.JSInterop;

namespace Fusee.Base.Imp.WebAsm
{
    /// <summary>
    /// Base class for generation a Fusee WASM project
    /// </summary>
    public abstract class WebAsmBase
    {
        /// <summary>
        /// The WebGL2 context retrieved from javascript
        /// </summary>
        protected WebGL2RenderingContextBase gl;

        /// <summary>
        /// The clear color of the canvas
        /// </summary>
        protected float4 clearColor;

        /// <summary>
        /// The canvas itself, retrieved / created
        /// </summary>
        protected IJSObjectReference canvas;

        /// <summary>
        /// The current canvas width
        /// </summary>
        protected int canvasWidth;

        /// <summary>
        /// The current canvas height
        /// </summary>
        protected int canvasHeight;

        /// <summary>
        /// Should full screen be possible?
        /// </summary>
        public virtual bool EnableFullScreen => true;

        /// <summary>
        /// The Javascript runtime instance
        /// </summary>
        public static IJSRuntime Runtime { get; private set; }

        /// <summary>
        /// This method generates the WebGL2 context
        /// </summary>
        /// <param name="canvas"></param>
        /// <param name="runtime"></param>
        /// <param name="clearColor"></param>
        public virtual void Init(IJSObjectReference canvas, IJSRuntime runtime, float4 clearColor)
        {
            this.clearColor = clearColor;
            this.canvas = canvas;
            Runtime = runtime;

            canvasWidth = canvas.GetObjectProperty<int>(runtime, "width");
            canvasHeight = canvas.GetObjectProperty<int>(runtime, "height");
            var ctxAttr = new WebGLContextAttributes(canvas)
            {
                Alpha = false,
                Antialias = true,
                PremultipliedAlpha = false,
                Depth = true,
                PowerPreference = "high-performance",
                Desynchronized = true
            };

            gl = new WebGL2RenderingContext(canvas, runtime, ctxAttr);
        }

        /// <summary>
        /// Start the core project
        /// </summary>
        public virtual void Run()
        {
        }

        /// <summary>
        /// Call update inside the core project
        /// </summary>
        /// <param name="elapsedMilliseconds"></param>
        public virtual void Update(double elapsedMilliseconds)
        {

        }

        /// <summary>
        /// Draw elements on canvas
        /// </summary>
        public virtual void Draw()
        {
            gl.Enable(WebGLRenderingContextBase.DEPTH_TEST);

            gl.Viewport(0, 0, canvasWidth, canvasHeight);
            Resize(canvasWidth, canvasHeight);

            gl.ClearColor(clearColor.x, clearColor.y, clearColor.z, clearColor.w);
            gl.Clear(WebGLRenderingContextBase.COLOR_BUFFER_BIT);
        }

        /// <summary>
        /// This method is called when a resize occurs
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        public virtual void Resize(int width, int height)
        {
            gl.Viewport(0, 0, width, height);
            canvasWidth = width;
            canvasHeight = height;
        }
    }
}
