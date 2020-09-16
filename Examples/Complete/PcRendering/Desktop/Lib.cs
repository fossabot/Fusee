using Fusee.Base.Common;
using Fusee.Base.Core;
using Fusee.Base.Imp.Desktop;
using Fusee.Engine.Common;
using Fusee.Engine.Core;
using Fusee.Engine.Core.Scene;
using Fusee.Engine.Imp.Graphics.Desktop;
using Fusee.Examples.PcRendering.Core;
using Fusee.Pointcloud.PointAccessorCollections;
using Fusee.Serialization;
using System;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Path = Fusee.Base.Common.Path;

namespace Fusee.Examples.PcRendering.Desktop
{
    /// <summary>
    /// The purpose of this class is to expose functionality of this application so it can be called from external environments e.g. some GUI.
    /// </summary>
    public static class Lib
    {
        #region Delegates - entry points when hosting a dotnet runtime with host fxr

        public delegate void ExecFusAppInNewThreadDelegate();
        public delegate void ExecFusAppDelegate();
        public delegate bool IsAppInitializedDelegate();
        public delegate IntPtr GetWindowHandleDelegate();
        public delegate void CloseGameWindowDelegate();
        public delegate void SetRenderPauseDelegate(bool isRenderPauseRequested);

        #endregion

        private static Pointcloud.Common.IPcRendering _app;
        private static IWindowHandle _windowHandle;
        public static Task FusTask { get; private set; }

        private static CancellationTokenSource _cts;

        public static IntPtr GetWindowHandle()
        {
            return ((WindowHandle)_windowHandle).Handle;
        }

        /// <summary>
        /// Will create and run a new instance of Core.Simple.
        /// Note that we can only call this from the main thread at the moment, due to glfw limitations.
        /// </summary>
        public static void ExecFusAppInNewThread()
        {
            _cts = new CancellationTokenSource();
            CancellationToken ct = _cts.Token;
            FusTask = Task.Run(() =>
            {
                InitAndRunApp();

            }, ct);

            SpinWait.SpinUntil(() => _app != null && _app.IsInitialized);
        }

        public static void ExecFusApp()
        {
            InitAndRunApp();
        }

        public static bool IsAppInitialized()
        {
            if (_app != null)
                return _app.IsInitialized;
            return false;
        }

        public static void CloseGameWindow()
        {
            _app.IsClosingRequested = true;
            _cts.Cancel();
            FusTask.Wait();
            _cts.Dispose();
        }

        public static void SetRenderPause(bool isRenderPauseRequested)
        {
            if (_app != null)
                _app.IsRenderPauseRequested = isRenderPauseRequested;
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

            var ptType = AppSetupHelper.GetPtType(PtRenderingParams.PathToOocFile);
            var ptEnumName = Enum.GetName(typeof(PointType), ptType);

            var genericType = Type.GetType("Fusee.Pointcloud.PointAccessorCollections." + ptEnumName + ", " + "Fusee.Pointcloud.PointAccessorCollections");

            var objectType = typeof(PcRendering<>);
            var objWithGenType = objectType.MakeGenericType(genericType);

            _app = (Pointcloud.Common.IPcRendering)Activator.CreateInstance(objWithGenType);
            AppSetup.DoSetup(_app, ptType, PtRenderingParams.MaxNoOfVisiblePoints, PtRenderingParams.PathToOocFile);

            // Inject Fusee.Engine InjectMe dependencies (hard coded)
            System.Drawing.Icon appIcon = System.Drawing.Icon.ExtractAssociatedIcon(Assembly.GetExecutingAssembly().Location);
            _app.CanvasImplementor = new Engine.Imp.Graphics.Desktop.RenderCanvasImp(appIcon);
            _app.ContextImplementor = new Engine.Imp.Graphics.Desktop.RenderContextImp(_app.CanvasImplementor);
            Input.AddDriverImp(new Engine.Imp.Graphics.Desktop.RenderCanvasInputDriverImp(_app.CanvasImplementor));
            Input.AddDriverImp(new Engine.Imp.Graphics.Desktop.WindowsTouchInputDriverImp(_app.CanvasImplementor));

            _windowHandle = _app.CanvasImplementor.WindowHandle;

            // Start the app
            _app.Run();
        }

    }
}