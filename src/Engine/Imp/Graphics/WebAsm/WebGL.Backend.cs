using Fusee.Base.Core;
using Fusee.Base.Imp.WebAsm;
using Microsoft.JSInterop;
using System;
using System.Text;


namespace Fusee.Engine.Imp.Graphics.WebAsm
{
#pragma warning disable 1591
    public abstract class JSHandler : IDisposable
    {
        internal IJSObjectReference Handle { get; set; }

        public bool IsDisposed { get; private set; }

        ~JSHandler()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual async void Dispose(bool disposing)
        {
            if (IsDisposed)
            {
                return;
            }

            IsDisposed = true;

            await Handle.DisposeAsync();
        }
    }

    public partial class WebGLActiveInfo : JSHandler
    {
    }

    public partial class WebGLContextAttributes : JSHandler
    {
        // TODO(MR): Check for runtime error, fix it with initialization of a Handle object
        //public WebGLContextAttributes()
        //{
        //Handle = new IJSObjectReference();
        //}

        public WebGLContextAttributes()
        {
        }
    }

    public partial class WebGLObject : JSHandler
    {
    }

    public partial class WebGLRenderingContext : WebGLRenderingContextBase
    {
        public WebGLRenderingContext(IJSObjectReference canvas, IJSRuntime runtime)
            : base(canvas, runtime, "webgl")
        {
        }

        public WebGLRenderingContext(IJSObjectReference canvas, IJSRuntime runtime, WebGLContextAttributes contextAttributes)
            : base(canvas, runtime, "webgl", contextAttributes)
        {
        }
    }

    public abstract partial class WebGLRenderingContextBase : JSHandler
    {
        private const string WindowPropertyName = "WebGLRenderingContext";

        protected readonly IJSObjectReference gl;
        protected readonly IJSRuntime runtime;

        protected WebGLRenderingContextBase(
            IJSObjectReference canvas,
            IJSRuntime runtime,
            string contextType,
            string windowPropertyName = WindowPropertyName)
            : this(canvas, runtime, contextType, null, windowPropertyName)
        {
        }

        protected WebGLRenderingContextBase(
            IJSObjectReference canvas,
            IJSRuntime runtime,
            string contextType,
            WebGLContextAttributes contextAttributes,
            string windowPropertyName = WindowPropertyName)
        {
            if (!CheckWindowPropertyExists(windowPropertyName))
            {
                throw new PlatformNotSupportedException(
                    $"The context '{contextType}' is not supported in this browser");
            }
            this.runtime = runtime;
            gl = ((IJSInProcessObjectReference)canvas).Invoke<IJSObjectReference>("getContext", contextType, contextAttributes?.Handle);
        }

        public bool IsSupported => CheckWindowPropertyExists(WindowPropertyName);

        public static bool IsVerbosityEnabled { get; set; } = false;

        //public ITypedArray CastNativeArray(object managedArray)
        //{
        //    var arrayType = managedArray.GetType();

        //    // Here are listed some JavaScript array types:
        //    // https://github.com/mono/mono/blob/a7f5952c69ae76015ccaefd4dfa8be2274498a21/sdks/wasm/bindings-test.cs
        //    if (arrayType == typeof(byte[]))
        //    {
        //        return Uint8Array.From((byte[])managedArray);
        //    }
        //    else if (arrayType == typeof(float[]))
        //    {
        //        return Float32Array.From((float[])managedArray);
        //    }
        //    else if (arrayType == typeof(ushort[]))
        //    {
        //        return Uint16Array.From((ushort[])managedArray);
        //    }
        //    else if (arrayType == typeof(uint[]))
        //    {
        //        return Uint32Array.From((uint[])managedArray);
        //    }
        //
        //    var ex = new ArgumentException("Type {managedArray} not convertible!");
        //    Diagnostics.Error("Error converting managed array to javascript array!", ex);
        //    throw ex;
        //}

        protected bool CheckWindowPropertyExists(string property)
        {
            var window = runtime.GetGlobalObject<IJSObjectReference>("window");
            var exists = window.GetObjectProperty<bool>(property);

            return exists;
        }

        private void DisposeArrayTypes(object[] args)
        {
            for (var i = 0; i < args.Length; i++)
            {
                var arg = args[i];

                //if (arg is ITypedArray typedArray && typedArray != null)
                //{
                    //var disposable = (IDisposable)typedArray;
                    //disposable.Dispose();
                //}
                //if (arg is WebAssembly.Core.Array jsArray && jsArray != null)
                //{
                    //var disposable = (IDisposable)jsArray;
                    //disposable.Dispose();

                //}
            }
        }

        protected object Invoke(string method, params object[] args)
        {
            var actualArgs = Translate(args);
            var result = ((IJSInProcessObjectReference)gl).Invoke<IJSObjectReference>(method, actualArgs);
            //DisposeArrayTypes(actualArgs); TODO(MR): Later

            //if (IsVerbosityEnabled)
            //{
                //var dump = new StringBuilder();
                //dump.Append(method).Append('(');

                //for (var i = 0; i < args.Length; i++)
                //{
                    //var item = args[i];
                    //dump.Append(Dump(item));

                    //if (i < (args.Length - 1))
                    //{
                        //dump.Append(", ");
                    //}
                //}

                //dump.Append(") = ").Append(Dump(result));
            //}

            return result;
        }

        protected T Invoke<T>(string method, params object[] args)
            where T : JSHandler, new()
        {
            var rawResult = Invoke(method, args);

            return new T
            {
                Handle = (IJSObjectReference)rawResult
            };
        }

        protected uint[] InvokeForIntToUintArray(string method, params object[] args)
        {
            var temp = InvokeForArray<int>(method, args);

            if (temp == null)
            {
                return null;
            }

            var result = new uint[temp.Length];

            for (var i = 0; i < temp.Length; i++)
            {
                result[i] = (uint)temp[i];
            }

            return result;
        }

        protected T[] InvokeForArray<T>(string method, params object[] args)
        {
            using (var rawResult = (WebAssembly.Core.Array)Invoke(method, args))
            {
                return rawResult.ToArray(item => (T)item);
            }
        }

        protected T[] InvokeForJavaScriptArray<T>(string method, params object[] args)
            where T : JSHandler, new()
        {
            using var rawResult = (WebAssembly.Core.Array)Invoke(method, args);
            return rawResult.ToArray(item => new T { Handle = (IJSObjectReference)item });
        }

        protected T InvokeForBasicType<T>(string method, params object[] args)
            where T : IConvertible
        {
            var result = Invoke(method, args);

            return (T)result;
        }

        private string Dump(object @object)
        {
            return $"{@object ?? "null"} ({@object?.GetType()})";
        }

        private object[] Translate(object[] args)
        {
            var actualArgs = new object[args.Length];

            for (var i = 0; i < actualArgs.Length; i++)
            {
                var arg = args[i];

                if (arg == null)
                {
                    actualArgs[i] = null;
                    continue;
                }

                if (arg is JSHandler jsHandler)
                {
                    arg = jsHandler.Handle;
                }
                else if (arg is System.Array array)
                {
                    if (((System.Array)arg).GetType().GetElementType().IsPrimitive)
                    {
                        arg = array;
                    }
                    else
                    {
                        var argArray = new WebAssembly.Core.Array();
                        foreach (var item in (System.Array)arg)
                        {
                            argArray.Push(item);
                        }
                        arg = argArray;
                    }
                }

                actualArgs[i] = arg;
            }

            return actualArgs;
        }
    }

    public partial class WebGLShaderPrecisionFormat : JSHandler
    {
    }

    public partial class WebGLUniformLocation : JSHandler, IDisposable
    {
    }

    public partial class WebGL2RenderingContext : WebGL2RenderingContextBase
    {
        public WebGL2RenderingContext(IJSObjectReference canvas, IJSRuntime runtime)
            : base(canvas, runtime, "webgl2")
        {
        }

        public WebGL2RenderingContext(IJSObjectReference canvas, IJSRuntime runtime, WebGLContextAttributes contextAttributes)
            : base(canvas, runtime, "webgl2", contextAttributes)
        {
        }
    }

    public abstract partial class WebGL2RenderingContextBase : WebGLRenderingContextBase
    {
        private const string WindowPropertyName = "WebGL2RenderingContext";

        protected WebGL2RenderingContextBase(
            IJSObjectReference canvas,
            IJSRuntime runtime,
            string contextType,
            string windowPropertyName = WindowPropertyName)
            : this(canvas, runtime, contextType, null, windowPropertyName)
        {
        }

        protected WebGL2RenderingContextBase(
            IJSObjectReference canvas,
            IJSRuntime runtime,
            string contextType,
            WebGLContextAttributes contextAttributes,
            string windowPropertyName = WindowPropertyName)
            : base(canvas, runtime, contextType, contextAttributes, windowPropertyName)
        {
        }

        public new bool IsSupported => CheckWindowPropertyExists(WindowPropertyName);

        public void TexImage2D(
            uint target,
            int level,
            int internalformat,
            int width,
            int height,
            int border,
            uint format,
            uint type,
            ReadOnlySpan<byte> source)
        {
            // TODO(MR): managed to native via javscript (implement & test)
            //using var nativeArray = Uint8Array.From(source);
            TexImage2D(target, level, internalformat, width, height, border, format, type, source);
        }

        public void TexImage3D(
            uint target,
            int level,
            int internalformat,
            int width,
            int height,
            int depth,
            int border,
            uint format,
            uint type,
            ReadOnlySpan<byte> source)
        {
            // TODO(MR): managed to native via javscript (implement & test)
            //using var nativeArray = Uint8Array.From(source);
            TexImage3D(target, level, internalformat, width, height, depth, border, format, type, source);
        }

        public void TexSubImage3D(
            uint target,
            int level,
            int xoffset,
            int yoffset,
            int zoffset,
            int width,
            int height,
            int depth,
            uint format,
            uint type,
            ReadOnlySpan<byte> source)
        {
            // TODO(MR): managed to native via javscript (implement & test)
            //using var nativeArray = Uint8Array.From(source);
            TexSubImage3D(
                target,
                level,
                xoffset,
                yoffset,
                zoffset,
                width,
                height,
                depth,
                format,
                type,
                source);
        }

        public void TexSubImage2D(
            uint target,
            int level,
            int xoffset,
            int yoffset,
            int width,
            int height,
            uint format,
            uint type,
            ReadOnlySpan<byte> source)
        {
            // TODO(MR): managed to native via javscript (implement & test)
            //using var nativeArray = Uint8Array.From(source);
            TexSubImage2D(
                target,
                level,
                xoffset,
                yoffset,
                width,
                height,
                format,
                type,
                source);
        }
    }

#pragma warning restore 1591
}
