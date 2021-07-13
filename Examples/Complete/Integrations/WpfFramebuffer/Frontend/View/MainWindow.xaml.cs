using Fusee.WpfIntegration;
using System;
using System.Windows;
using Window = System.Windows.Window;

namespace Fusee.Examples.Integrations.WpfFramebuffer.Frontend.View
{
    public partial class MainWindow : Window
    {
        public static EventHandler<RoutedEventArgs> ControlInit;
        public static EventHandler<EventArgs> Reload;
        public static EventHandler<EventArgs> Resized;
        public static EventHandler<FbRenderEventArgs> Render;
        public static EventHandler<RoutedEventArgs> ControlUnloaded;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void OtkWpfControl_Initialized(object sender, RoutedEventArgs routedEventArgs)
        {
            ControlInit?.Invoke(sender, routedEventArgs);
        }

        private void OtkWpfControl_Reload(object sender, EventArgs e)
        {
            Reload?.Invoke(sender, e);
        }

        private void OtkWpfControl_Resized(object sender, EventArgs e)
        {
            Resized?.Invoke(sender, e);
        }

        private void OtkWpfControl_Render(object sender, FbRenderEventArgs e)
        {
            Render?.Invoke(sender, e);
        }

        private void OtkWpfControl_Unloaded(object sender, RoutedEventArgs e)
        {
            ControlUnloaded?.Invoke(sender, e);
        }
    }
}