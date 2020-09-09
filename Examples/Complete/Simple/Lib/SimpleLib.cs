using Fusee.Base.Common;
using Fusee.Base.Core;
using Fusee.Base.Imp.Desktop;
using Fusee.Engine.Common;
using Fusee.Engine.Core;
using Fusee.Engine.Core.Scene;
using Fusee.Engine.Imp.Graphics.Desktop;
using Fusee.Serialization;
using System;
using System.IO;
using System.Reflection;
using System.Threading;

using Path = Fusee.Base.Common.Path;

namespace Fusee.Examples.Simple.Lib
{
    //TODO: Make platform independent?
    public static class SimpleLib
    {
        #region Delegates - entry points when hosting a dotnet runtime with host fxr

        public delegate void ExecFusAppDelegate();
        public delegate void AbortFusThreadDelegate();
        public delegate bool IsAppInitializedDelegate();
        public delegate IntPtr GetWindowHandleDelegate();

        #endregion

        private static Thread _fusThread;
        private static Core.Simple _app;
        private static IWindowHandle _windowHandle;

        public static IntPtr GetWindowHandle()
        {
            return ((WindowHandle)_windowHandle).Handle;
        }

        public static void ExecFusAppInNewThread()
        {
            _fusThread = new Thread(() =>
            {
                InitAndRunApp();
            });

            _fusThread.Start();

            SpinWait.SpinUntil(() => _app != null && _app.IsInitialized);
        }

        public static void ExecFusApp()
        {
            InitAndRunApp();
        }

        public static void AbortFusThread()
        {
            if (_fusThread != null)
            {
                _fusThread.Abort();
            }
        }

        public static bool IsAppInitialized()
        {
            return _app.IsInitialized;
        }

        private static void InitAndRunApp()
        {
            // Inject Fusee.Engine.Base InjectMe dependencies
            IO.IOImp = new IOImp();

            var fap = new FileAssetProvider("Assets");
            fap.RegisterTypeHandler(
                new AssetHandler
                {
                    ReturnedType = typeof(Font),
                    Decoder = (string id, object storage) =>
                    {
                        if (!Path.GetExtension(id).Contains("ttf", System.StringComparison.OrdinalIgnoreCase)) return null;
                        return new Font { _fontImp = new FontImp((Stream)storage) };
                    },
                    Checker = id => Path.GetExtension(id).Contains("ttf", System.StringComparison.OrdinalIgnoreCase)
                });
            fap.RegisterTypeHandler(
                new AssetHandler
                {
                    ReturnedType = typeof(SceneContainer),
                    Decoder = (string id, object storage) =>
                    {
                        if (!Path.GetExtension(id).Contains("fus", System.StringComparison.OrdinalIgnoreCase)) return null;
                        return FusSceneConverter.ConvertFrom(ProtoBuf.Serializer.Deserialize<FusFile>((Stream)storage));
                    },
                    Checker = id => Path.GetExtension(id).Contains("fus", System.StringComparison.OrdinalIgnoreCase)
                });

            AssetStorage.RegisterProvider(fap);

            _app = new Core.Simple();

            // Inject Fusee.Engine InjectMe dependencies (hard coded)
            System.Drawing.Icon appIcon = System.Drawing.Icon.ExtractAssociatedIcon(Assembly.GetExecutingAssembly().Location);
            _app.CanvasImplementor = new RenderCanvasImp(appIcon);
            _app.ContextImplementor = new RenderContextImp(_app.CanvasImplementor);
            Input.AddDriverImp(new Fusee.Engine.Imp.Graphics.Desktop.RenderCanvasInputDriverImp(_app.CanvasImplementor));
            Input.AddDriverImp(new WindowsTouchInputDriverImp(_app.CanvasImplementor));
            Input.AddDriverImp(new WindowsSpaceMouseDriverImp(_app.CanvasImplementor));

            _windowHandle = _app.CanvasImplementor.WindowHandle;

            // Start the app
            _app.Run();
        }
    }
}