
using Fusee.Examples.Integrations.WpfFramebuffer.Frontend.CustomComponents;
using Fusee.Examples.Integrations.WpfFramebuffer.Frontend.ViewModel;
using Fusee.Math.Core;
using System;
using System.Windows;
using Window = System.Windows.Window;

namespace Fusee.Examples.Integrations.WpfFramebuffer.Frontend.View
{
    public partial class MainWindow : Window
    {
        private readonly SimpleViewModel _viewModel = new SimpleViewModel();

        public MainWindow()
        {
            DataContext = _viewModel;

            InitializeComponent();
            _viewModel.RegisterAssetProvider();
            _viewModel.RotationModel.PropertyChanged += Rotation_PropertyChanged;
            _viewModel.ColorModel.PropertyChanged += Color_PropertyChanged;
            _viewModel.FusToWpfEvent += OnFusToWpf;
        }

        private void OtkWpfControl_Initialized(object sender, RoutedEventArgs routedEventArgs)
        {
            _viewModel.LoadApp((int)FbComp.ActualWidth, (int)FbComp.ActualHeight);
            FbComp.Renderer.GameWindow = ((Engine.Imp.Graphics.Desktop.RenderCanvasImp)_viewModel.App.CanvasImplementor).GameWindow;
        }

        private void OtkWpfControl_Reload(object sender, EventArgs e)
        {
            _viewModel.LoadApp((int)FbComp.ActualWidth, (int)FbComp.ActualHeight);
            FbComp.Renderer.GameWindow = ((Engine.Imp.Graphics.Desktop.RenderCanvasImp)_viewModel.App.CanvasImplementor).GameWindow;

            rotXSlider.Value = 0;
            rotYSlider.Value = 0;
            rotZSlider.Value = 0;

            FbComp.ReloadRequested = false;
        }

        private void OtkWpfControl_Resized(object sender, EventArgs e)
        {
            var ctrl = sender as FbComponent;
            if (ctrl != null)
            {
                _viewModel.ResizeApp((int)ctrl.ActualWidth, (int)ctrl.ActualHeight);
            }
        }

        private void OtkWpfControl_Render(object sender, FbRenderEventArgs e)
        {
            var ctrl = sender as FbComponent;
            if (ctrl != null)
            {
                _viewModel.RenderApp();
                e.Redrawn = true;
            }
        }

        private void OtkWpfControl_Unloaded(object sender, RoutedEventArgs e)
        {
            _viewModel.CloseApp();
        }

        private void Rotation_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            _viewModel.SetRotationInApp(sender, e);
        }

        private void Color_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            _viewModel.SetColorInApp(sender, e);
        }

        private void LoadButton_Click(object sender, RoutedEventArgs e)
        {
            //FbComponent.Reload will be invoked after the actual frame has been rendered
            FbComp.ReloadRequested = true;
        }

        private void OnFusToWpf(object sender, EventArgs e)
        {
            EventHandler<EventArgs> handler = FusToWpf;
            handler?.Invoke(this, e);
        }

        private void FusToWpf(object sender, EventArgs e)
        {
            if (e is Core.RotationXChangedEventArgs rotXEvent)
            {
                var angle = ClampAngle((float)rotXSlider.Value + M.RadiansToDegrees(rotXEvent.Rad));
                rotXSlider.Value = angle;
            }
            else if (e is Core.RotationYChangedEventArgs rotYEvent)
            {
                var angle = ClampAngle((float)rotYSlider.Value + M.RadiansToDegrees(rotYEvent.Rad));
                rotYSlider.Value = angle;
            }
            else if (e is Core.RotationYChangedEventArgs rotZEvent)
            {
                var angle = ClampAngle((float)rotZSlider.Value + M.RadiansToDegrees(rotZEvent.Rad));
                rotZSlider.Value = angle;
            }
        }

        private float ClampAngle(float angle)
        {
            if (angle > 360f)
                return angle - 360f;
            if (angle < -360f)
                return angle + 360f;
            return angle;
        }
    }
}