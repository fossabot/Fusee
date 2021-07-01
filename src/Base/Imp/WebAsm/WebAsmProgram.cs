using Fusee.Math.Core;
using Microsoft.JSInterop;
using System;


namespace Fusee.Base.Imp.WebAsm
{
    /// <summary>
    /// A WebAsmProgram contains some runtime variables, like canvasName, the canvas clear color as well as the render loop action
    /// </summary>
    public static class WebAsmProgram
    {
        private static readonly float4 CanvasColor = new float4(255, 0, 255, 255);
        private static readonly Action<double> loop = new Action<double>(Loop);
        private static double previousMilliseconds;
        private static IJSObjectReference window;
        private static IJSRuntime Runtime;

        private static string divCanvasName;
        private static string canvasName;

        private static WebAsmBase mainExecutable;

        /// <summary>
        /// Starts the WASM program
        /// </summary>
        /// <param name="runtime"></param>
        /// <param name="wasm"></param>
        public static void Start(WebAsmBase wasm, IJSRuntime runtime)
        {
            Runtime = runtime;

            // Let's first check if we can continue with WebGL2 instead of crashing.
            if (!IsBrowserSupportsWebGL2())
            {
                HtmlHelper.AddParagraph(wasm.Runtime, "We are sorry, but your browser does not seem to support WebGL2.");
                return;
            }

            // Create our sample
            mainExecutable = wasm;

            divCanvasName = "div_canvas";
            canvasName = "canvas";

            using (var window = Runtime.GetGlobalObject<IJSInProcessObjectReference>("window"))
            {
                var windowWidth = window.GetObjectProperty<int>("innerWidth");
                var windowHeight = window.GetObjectProperty<int>("innerHeight");

                using var canvas = (IJSInProcessObjectReference)HtmlHelper.AddCanvas(wasm.Runtime, divCanvasName, canvasName, windowWidth, windowHeight);
                mainExecutable.Init(canvas, Runtime, CanvasColor);
                mainExecutable.Run();
            }

            AddEnterFullScreenHandler();
            AddResizeHandler();

            RequestAnimationFrame();
        }

        private static void AddResizeHandler()
        {
            using var window = Runtime.GetGlobalObject<IJSInProcessObjectReference>("window");
            window.InvokeVoid("addEventListener", "resize", new Action<IJSInProcessObjectReference>((o) =>
            {
                using (var d = Runtime.GetGlobalObject<IJSInProcessObjectReference>("document"))
                using (var w = Runtime.GetGlobalObject<IJSInProcessObjectReference>("window"))
                {
                    using var canvasObject = d.Invoke<IJSInProcessObjectReference>("getElementById", canvasName);

                    var windowWidth = w.GetObjectProperty<int>("innerWidth");
                    var windowHeight = w.GetObjectProperty<int>("innerHeight");

                    var cobj = canvasObject.GetObjectProperty<string>("id");

                    canvasObject.SetObjectProperty("width", windowWidth);
                    canvasObject.SetObjectProperty("height", windowHeight);
                    // call fusee resize
                    mainExecutable.Resize(windowWidth, windowHeight);
                }

                o.Dispose();
            }), false);
        }

        private static void RequestFullscreen(IJSInProcessObjectReference canvas)
        {
            if (canvas.GetObjectProperty<IJSInProcessObjectReference>("requestFullscreen") != null)
                canvas.InvokeVoid("requestFullscreen");
            if (canvas.GetObjectProperty<IJSInProcessObjectReference>("webkitRequestFullscreen") != null)
                canvas.InvokeVoid("webkitRequestFullscreen");
            if (canvas.GetObjectProperty<IJSInProcessObjectReference>("msRequestFullscreen") != null)
                canvas.InvokeVoid("msRequestFullscreen");

        }

        private static void AddEnterFullScreenHandler()
        {
            using var canvas = Runtime.GetGlobalObject<IJSInProcessObjectReference>(canvasName);
            canvas.InvokeVoid("addEventListener", "dblclick", new Action<IJSInProcessObjectReference>((o) =>
           {
               using (var d = Runtime.GetGlobalObject<IJSInProcessObjectReference>("document"))
               {
                   var canvasObject = d.Invoke<IJSInProcessObjectReference>("getElementById", canvasName);

                   RequestFullscreen(canvasObject);

                   var width = canvasObject.GetObjectProperty<int>("clientWidth");
                   var height = canvasObject.GetObjectProperty<int>("clientHeight");

                   SetNewCanvasSize(canvasObject, width, height);

                        // call fusee resize
                        mainExecutable.Resize(width, height);
               }

               o.Dispose();
           }), false);
        }

        private static void SetNewCanvasSize(IJSInProcessObjectReference canvasObject, int newWidth, int newHeight)
        {
            canvasObject.SetObjectProperty("width", newWidth);
            canvasObject.SetObjectProperty("height", newHeight);
        }


        private static void Loop(double milliseconds)
        {
            var elapsedMilliseconds = milliseconds - previousMilliseconds;
            previousMilliseconds = milliseconds;

            mainExecutable.Update(elapsedMilliseconds);
            mainExecutable.Draw();

            RequestAnimationFrame();
        }

        private static void RequestAnimationFrame()
        {
            if (window == null)
            {
                window = Runtime.GetGlobalObject<IJSObjectReference>("window");
            }

            ((IJSInProcessObjectReference)window).InvokeVoid("requestAnimationFrame", loop);
        }

        private static bool IsBrowserSupportsWebGL2()
        {
            if (window == null)
            {
                window = Runtime.GetGlobalObject<IJSObjectReference>("window");
            }

            // This is a very simple check for WebGL2 support.
            return window.GetObjectProperty<IJSObjectReference>("WebGL2RenderingContext") != null;
        }
    }

    /// <summary>
    /// Helper class for often used functions
    /// </summary>
    public static class HtmlHelper
    {
        /// <summary>
        /// Creates an attaches a canvas to the HTML page
        /// </summary>
        /// <param name="runtime"></param>
        /// <param name="divId"></param>
        /// <param name="canvasId"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <returns></returns>
        public static IJSObjectReference AddCanvas(IJSRuntime runtime, string divId, string canvasId, int width = 800, int height = 600)
        {
            using var document = runtime.GetGlobalObject<IJSInProcessObjectReference>("document");
            using var body = document.GetObjectProperty<IJSInProcessObjectReference>("body");
            var canvas = document.Invoke<IJSInProcessObjectReference>("createElement", "canvas");
            canvas.SetObjectProperty("width", width);
            canvas.SetObjectProperty("height", height);
            canvas.SetObjectProperty("id", canvasId);

            using (var canvasDiv = document.Invoke<IJSInProcessObjectReference>("createElement", "div"))
            {
                canvasDiv.SetObjectProperty("id", divId);
                canvasDiv.InvokeVoid("appendChild", canvas);

                body.InvokeVoid("appendChild", canvasDiv);
            }

            return canvas;
        }

        /// <summary>
        /// Adds a paragraph to the current HTML page
        /// </summary>
        /// <param name="runtime"
        /// <param name="text"></param>
        public static void AddParagraph(IJSRuntime runtime, string text)
        {
            using var document = runtime.GetGlobalObject<IJSInProcessObjectReference>("document");
            using var body = document.GetObjectProperty<IJSInProcessObjectReference>("body");
            using var paragraph = document.Invoke<IJSInProcessObjectReference>("createElement", "p");
            paragraph.SetObjectProperty("innerHTML", text);
            body.InvokeVoid("appendChild", paragraph);
        }
    }
}
