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

        public static Pointcloud.Common.IPcRendering App { get; private set; }
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
        public static void ExecFusAppInNewThread(bool useExtUi)
        {
            _cts = new CancellationTokenSource();
            CancellationToken ct = _cts.Token;
            FusTask = Task.Run(() =>
            {
                Diagnostics.Debug(FusTask.Id);
                InitAndRunApp(useExtUi);

            }, ct);

            SpinWait.SpinUntil(() => App != null && App.IsInitialized);
        }

        public static void ExecFusApp(bool useExtUi)
        {
            InitAndRunApp(useExtUi);
        }

        public static bool IsAppInitialized()
        {
            if (App != null)
                return App.IsInitialized;
            return false;
        }

        public static void CloseGameWindow()
        {
            if(App != null)
                App.IsClosingRequested = true;
            
            FusTask.Wait();
            _cts.Cancel();
            _cts.Dispose();
            App = null;   
        }

        public static void SetRenderPause(bool isRenderPauseRequested)
        {
            if (App != null)
                App.IsRenderPauseRequested = isRenderPauseRequested;
        }

        private static void InitAndRunApp(bool useExtUi)
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

            App = (Pointcloud.Common.IPcRendering)Activator.CreateInstance(objWithGenType);
            App.UseExtUi = useExtUi;
            AppSetup.DoSetup(App, ptType, PtRenderingParams.MaxNoOfVisiblePoints, PtRenderingParams.PathToOocFile);

            // Inject Fusee.Engine InjectMe dependencies (hard coded)
            System.Drawing.Icon appIcon = System.Drawing.Icon.ExtractAssociatedIcon(Assembly.GetExecutingAssembly().Location);
            App.CanvasImplementor = new Engine.Imp.Graphics.Desktop.RenderCanvasImp(appIcon);
            App.ContextImplementor = new Engine.Imp.Graphics.Desktop.RenderContextImp(App.CanvasImplementor);
            Input.AddDriverImp(new Engine.Imp.Graphics.Desktop.RenderCanvasInputDriverImp(App.CanvasImplementor));
            Input.AddDriverImp(new Engine.Imp.Graphics.Desktop.WindowsTouchInputDriverImp(App.CanvasImplementor));

            _windowHandle = App.CanvasImplementor.WindowHandle;

            // Start the app
            App.Run();
        }

    }
}