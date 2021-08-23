using Fusee.Base.Common;
using Fusee.Base.Core;
using Fusee.Base.Imp.Desktop;
using Fusee.Engine.Core;
using Fusee.Engine.Core.Scene;
using Fusee.Engine.GUI;
using Fusee.Examples.Integrations.WpfFramebuffer.Frontend.Model;
using Fusee.Math.Core;
using Fusee.Serialization;
using Fusee.WpfIntegration;
using OpenTK.Windowing.Desktop;
using System;
using System.IO;
using System.Reflection;
using System.Windows.Input;

namespace Fusee.Examples.Integrations.WpfFramebuffer.Frontend.ViewModel
{
    class SimpleViewModel : IFusViewModel
    {
        public RotationModel RotationModel { get; set; }

        public ColorModel ColorModel { get; set; }

        public virtual ICommand UpdateColorCommand => new RelayCommand(o => ColorModel.ChangeColor());

        public event EventHandler<EventArgs> FusToWpfEvent;

        public RenderCanvas App { get; private set; }

        public FbComponent GlobalFBComp;

        public SimpleViewModel()
        {
            RotationModel = new RotationModel();
            ColorModel = new ColorModel();

            RegisterAssetProvider();
            RotationModel.PropertyChanged += Rotation_PropertyChanged;
            ColorModel.PropertyChanged += Color_PropertyChanged;

            View.MainWindow.ControlInit += (s, e) =>
            {
                var FbComp = s as FbComponent;
                GlobalFBComp = FbComp;
                LoadApp((int)FbComp.ActualWidth, (int)FbComp.ActualHeight);

                FbComp.Renderer.GameWindow = (GameWindow)((Engine.Imp.Graphics.Desktop.RenderCanvasImp)App.CanvasImplementor).GameWindow;

                FbComp.Renderer.GameWindow.Closing += (s) =>
                {
                    global::System.Console.WriteLine("Hans");
                };

                FbComp.Renderer.GameWindow.Closed += () =>
                {
                    global::System.Console.WriteLine("Close Hans");
                };

            };

            View.MainWindow.Reload += (s, e) =>
            {
                var FbComp = s as FbComponent;

                LoadApp((int)FbComp.ActualWidth, (int)FbComp.ActualHeight);

                FbComp.Renderer.GameWindow = ((Engine.Imp.Graphics.Desktop.RenderCanvasImp)App.CanvasImplementor).GameWindow;

                FbComp.Renderer.GameWindow.Closing += (s) =>
                {
                    global::System.Console.WriteLine("Hans");
                };

                FbComp.Renderer.GameWindow.Closed += () =>
                {
                    global::System.Console.WriteLine("Close Hans");
                };


                RotationModel.RotX = 0;
                RotationModel.RotY = 0;
                RotationModel.RotZ = 0;

                FbComp.ReloadRequested = false;
            };

            View.MainWindow.Resized += (o, e) =>
            {
                var ctrl = o as FbComponent;
                if (ctrl != null)
                {
                    ResizeApp((int)ctrl.ActualWidth, (int)ctrl.ActualHeight);
                }
            };

            View.MainWindow.Render += (o, e) =>
            {
                var ctrl = o as FbComponent;
                if (ctrl != null)
                {
                    RenderApp();
                    e.Redrawn = true;
                }
            };

            FusToWpfEvent += (s, e) =>
            {
                if (e is Core.RotationXChangedEventArgs rotXEvent)
                {
                    var angle = ClampAngle((float)RotationModel.RotX + M.RadiansToDegrees(rotXEvent.Rad));
                    RotationModel.RotX = angle;
                }
                else if (e is Core.RotationYChangedEventArgs rotYEvent)
                {
                    var angle = ClampAngle((float)RotationModel.RotY + M.RadiansToDegrees(rotYEvent.Rad));
                    RotationModel.RotY = angle;
                }
                else if (e is Core.RotationYChangedEventArgs rotZEvent)
                {
                    var angle = ClampAngle((float)RotationModel.RotZ + M.RadiansToDegrees(rotZEvent.Rad));
                    RotationModel.RotZ = angle;
                }
            };
        }


        public void RenderApp()
        {
            App.RenderAFrame();
        }

        public void LoadApp(int width, int height)
        {
            if (App != null)
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
            GC.Collect();
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

        public void SetRotationInApp(RotationModel sender, System.ComponentModel.PropertyChangedEventArgs e)
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

        public ICommand LoadButton_Click => new RelayCommand(o => GlobalFBComp.ReloadRequested = true);

        private float ClampAngle(float angle)
        {
            if (angle > 360f)
                return angle - 360f;
            if (angle < -360f)
                return angle + 360f;
            return angle;
        }

        private void Rotation_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            SetRotationInApp(sender as RotationModel, e);
        }

        private void Color_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            SetColorInApp(sender, e);
        }

    }
}