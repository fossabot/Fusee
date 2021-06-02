using Fusee.Base.Common;
using Fusee.Base.Core;
using Fusee.Base.Imp.Desktop;
using Fusee.Engine.Core;
using Fusee.Engine.Core.Scene;
using Fusee.Examples.Simple.Wpf.Model;
using Fusee.Math.Core;
using Fusee.Serialization;
using System;
using System.IO;
using System.Reflection;
using System.Windows.Input;
using Path = Fusee.Base.Common.Path;

namespace Fusee.Examples.Simple.Wpf.ViewModel
{
    class SimpleViewModel : IFusViewModel
    {
        public RotationModel RotationModel { get; set; }

        public ColorModel ColorModel { get; set; }
        public virtual ICommand UpdateColorCommand => new RelayCommand(o => ColorModel.ChangeColor());

        public event EventHandler<EventArgs> FusToWpfEvent;

        public RenderCanvas App { get; private set; }

        public SimpleViewModel()
        {
            RotationModel = new RotationModel();
            ColorModel = new ColorModel();
        }

        public void RenderApp()
        {
            App.RenderAFrame();
        }

        public void LoadApp(int width, int height)
        {
            if(App != null)
                CloseApp();
            CreateApp();
            App.InitCanvas();
            App.Init();
            ResizeApp(width, height);
        }

        public void ResizeApp(int width, int height)
        {
            if (App == null) return;

            ((Engine.Imp.Graphics.Desktop.RenderCanvasImp)App.CanvasImplementor).ResizeWindow(width, height);
            App.Resize(new Engine.Common.ResizeEventArgs(width, height));
        }

        public void CloseApp()
        {
            App.CloseGameWindow();
            App = null;
        }

        public void RegisterAssetProvider()
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
                        return FusSceneConverter.ConvertFrom(ProtoBuf.Serializer.Deserialize<FusFile>((Stream)storage), id);
                    },
                    Checker = id => Path.GetExtension(id).Contains("fus", System.StringComparison.OrdinalIgnoreCase)
                });

            AssetStorage.RegisterProvider(fap);
        }

        public void SetColorInApp(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (sender is ColorModel c)
                ((Core.Simple)App).RandomColor = c.Color;
        }

        public void SetRotationInApp(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (sender is RotationModel r)
            {
                var app = (Core.Simple)App;

                if (e.PropertyName == "RotX")
                {
                    app.RotXFromUi = M.DegreesToRadians(r.RotX);
                }
                else if (e.PropertyName == "RotY")
                {
                    app.RotYFromUi = M.DegreesToRadians(r.RotY);
                }
                else if (e.PropertyName == "RotZ")
                {
                    app.RotZFromUi = M.DegreesToRadians(r.RotZ);
                }
            }
        }

        private void CreateApp()
        {
            App = new Core.Simple();
            ((Core.Simple)App).FusToWpfEvents += FusToWpfEvent;

            System.Drawing.Icon appIcon = System.Drawing.Icon.ExtractAssociatedIcon(Assembly.GetExecutingAssembly().Location);
            App.CanvasImplementor = new Fusee.Engine.Imp.Graphics.Desktop.RenderCanvasImp(appIcon);
            App.ContextImplementor = new Fusee.Engine.Imp.Graphics.Desktop.RenderContextImp(App.CanvasImplementor);
            Input.AddDriverImp(new Fusee.Engine.Imp.Graphics.Desktop.RenderCanvasInputDriverImp(App.CanvasImplementor));
            Input.AddDriverImp(new Fusee.Engine.Imp.Graphics.Desktop.WindowsTouchInputDriverImp(App.CanvasImplementor));
        }
    }
}