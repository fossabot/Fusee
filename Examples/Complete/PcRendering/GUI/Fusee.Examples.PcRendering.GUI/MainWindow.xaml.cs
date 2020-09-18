using Fusee.Examples.PcRendering.Core;
using Fusee.Examples.PcRendering.Desktop;
using Fusee.Math.Core;
using Fusee.Pointcloud.Common;
using System;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace Fusee.Examples.PcRendering.WPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private bool _areOctantsShown;

        private bool _ptSizeDragStarted;
        private bool _projSizeModDragStarted;
        private bool _edlStrengthDragStarted;
        private bool _edlNeighbourPxDragStarted;
        private bool _ssaoStrengthDragStarted;
        private bool _specularStrengthPxDragStarted;

        private Thread _fusThread;

        private static readonly Regex numRegex = new Regex("[^0-9.-]+");

        public MainWindow()
        {
            InitializeComponent();

            Lighting.SelectedValue = PtRenderingParams.Lighting;
            PtShape.SelectedValue = PtRenderingParams.Shape;
            PtSizeMode.SelectedValue = PtRenderingParams.PtMode;
            ColorMode.SelectedValue = PtRenderingParams.ColorMode;

            PtSize.Value = PtRenderingParams.Size;

            SSAOCheckbox.IsChecked = PtRenderingParams.CalcSSAO;
            SSAOStrength.Value = PtRenderingParams.SSAOStrength;

            EDLStrengthVal.Content = EDLStrength.Value;
            EDLStrength.Value = PtRenderingParams.EdlStrength;
            EDLNeighbourPxVal.Content = EDLNeighbourPx.Value;
            EDLNeighbourPx.Value = PtRenderingParams.EdlNoOfNeighbourPx;

            ShininessVal.Text = PtRenderingParams.Shininess.ToString();
            SpecStrength.Value = PtRenderingParams.SpecularStrength;

            var col = PtRenderingParams.SingleColor;
            SingleColor.SelectedColor = System.Windows.Media.Color.FromScRgb(col.a, col.r, col.g, col.b);
            SSAOStrength.IsEnabled = PtRenderingParams.CalcSSAO;

            if (PtRenderingParams.ColorMode != Pointcloud.Common.ColorMode.Single)
                SingleColor.IsEnabled = false;
            else
                SingleColor.IsEnabled = true;

            InnerGrid.IsEnabled = false;
        }

        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll")]
        static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);

        #region UI Handler

        private void SSAOCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            PtRenderingParams.CalcSSAO = !PtRenderingParams.CalcSSAO;
            SSAOStrength.IsEnabled = PtRenderingParams.CalcSSAO;
        }

        #region ssao strength
        private void SSAOStrength_DragStarted(object sender, System.Windows.Controls.Primitives.DragStartedEventArgs e)
        {
            if (Lib.App == null || !Lib.App.IsInitialized || !Lib.App.IsSceneLoaded) return;
            _ssaoStrengthDragStarted = true;
        }

        private void SSAOStrength_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            if (Lib.App == null || !Lib.App.IsInitialized || !Lib.App.IsSceneLoaded) return;
            PtRenderingParams.SSAOStrength = (float)((Slider)sender).Value;
            _ssaoStrengthDragStarted = false;
        }

        private void SSAOStrength_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (Lib.App == null || !Lib.App.IsInitialized || !Lib.App.IsSceneLoaded) return;
            if (_ssaoStrengthDragStarted) return;
            PtRenderingParams.SSAOStrength = (float)e.NewValue;
        }
        #endregion

        #region edl strength
        private void EDLStrength_DragStarted(object sender, System.Windows.Controls.Primitives.DragStartedEventArgs e)
        {
            if (Lib.App == null || !Lib.App.IsInitialized || !Lib.App.IsSceneLoaded) return;
            _edlStrengthDragStarted = true;
        }

        private void EDLStrength_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            if (Lib.App == null || !Lib.App.IsInitialized || !Lib.App.IsSceneLoaded) return;
            PtRenderingParams.EdlStrength = (float)((Slider)sender).Value;
            _edlStrengthDragStarted = false;
        }

        private void EDLStrengthSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (Lib.App == null || !Lib.App.IsInitialized || !Lib.App.IsSceneLoaded) return;
            EDLStrengthVal.Content = e.NewValue.ToString("0.000");

            if (_edlStrengthDragStarted) return;
            PtRenderingParams.EdlStrength = (float)e.NewValue;
        }
        #endregion

        #region edl neighbor px

        private void EDLNeighbourPx_DragStarted(object sender, System.Windows.Controls.Primitives.DragStartedEventArgs e)
        {
            if (Lib.App == null || !Lib.App.IsInitialized || !Lib.App.IsSceneLoaded) return;
            _edlNeighbourPxDragStarted = true;
        }

        private void EDLNeighbourPx_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            if (Lib.App == null || !Lib.App.IsInitialized || !Lib.App.IsSceneLoaded) return;
            if (EDLNeighbourPxVal == null) return;

            EDLNeighbourPxVal.Content = ((Slider)sender).Value.ToString("0");
            PtRenderingParams.EdlNoOfNeighbourPx = (int)((Slider)sender).Value;

            _edlNeighbourPxDragStarted = false;
        }

        private void EDLNeighbourPxSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (Lib.App == null || !Lib.App.IsInitialized || !Lib.App.IsSceneLoaded) return;
            if (EDLNeighbourPxVal == null) return;

            EDLNeighbourPxVal.Content = e.NewValue.ToString("0");

            if (_edlNeighbourPxDragStarted) return;
            PtRenderingParams.EdlNoOfNeighbourPx = (int)e.NewValue;
        }

        #endregion       

        #region point size
        private void PtSize_DragStarted(object sender, System.Windows.Controls.Primitives.DragStartedEventArgs e)
        {
            _ptSizeDragStarted = true;
        }

        private void PtSize_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            if (PtSizeVal == null) return;

            PtSizeVal.Content = ((Slider)sender).Value.ToString("0");
            PtRenderingParams.Size = (int)((Slider)sender).Value;
        }

        private void PtSize_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (Lib.App == null || !Lib.App.IsInitialized || !Lib.App.IsSceneLoaded) return;
            if (PtSizeVal == null) return;
            PtSizeVal.Content = e.NewValue.ToString("0");

            if (_ptSizeDragStarted) return;
            PtRenderingParams.Size = (int)e.NewValue;
        }

        #endregion

        #region specular strength
        private void SpecStrength_DragStarted(object sender, System.Windows.Controls.Primitives.DragStartedEventArgs e)
        {
            if (Lib.App == null || !Lib.App.IsInitialized || !Lib.App.IsSceneLoaded) return;
            _specularStrengthPxDragStarted = true;
        }

        private void SpecStrength_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            if (Lib.App == null || !Lib.App.IsInitialized || !Lib.App.IsSceneLoaded) return;
            PtRenderingParams.SpecularStrength = (float)((Slider)sender).Value;
            _specularStrengthPxDragStarted = false;
        }

        private void SpecStrength_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (Lib.App == null || !Lib.App.IsInitialized || !Lib.App.IsSceneLoaded) return;
            if (_specularStrengthPxDragStarted) return;
            PtRenderingParams.SpecularStrength = (float)e.NewValue;
        }
        #endregion

        #region min. proj. size modifier

        private void MinProjSize_DragStarted(object sender, System.Windows.Controls.Primitives.DragStartedEventArgs e)
        {
            _projSizeModDragStarted = true;
        }

        private void MinProjSize_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            if (Lib.App == null || Lib.App.IsInitialized)
                MinProjSizeVal.Content = MinProjSize.Value.ToString("0.00");
            Lib.App?.SetOocLoaderMinProjSizeMod((float)MinProjSize.Value);

            _projSizeModDragStarted = false;
        }

        private void MinProjSize_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (Lib.App == null || Lib.App.IsInitialized)
                return;
            //MinProjSizeVal.Content = MinProjSize.Value.ToString("0.00");

            if (_projSizeModDragStarted) return;
            Lib.App?.SetOocLoaderMinProjSizeMod((float)MinProjSize.Value);
        }
        #endregion

        private void SingleColor_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<System.Windows.Media.Color?> e)
        {
            if (Lib.App == null || !Lib.App.IsInitialized || !Lib.App.IsSceneLoaded) return;
            var col = e.NewValue.Value;

            PtRenderingParams.SingleColor = new float4(col.ScR, col.ScG, col.ScB, col.ScA);
        }

        private void Lighting_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (Lib.App == null || !Lib.App.IsInitialized || !Lib.App.IsSceneLoaded) return;

            PtRenderingParams.Lighting = (Lighting)e.AddedItems[0];

            if (PtRenderingParams.Lighting == Pointcloud.Common.Lighting.SsaoOnly || PtRenderingParams.Lighting == Pointcloud.Common.Lighting.Unlit)
            {
                SSAOCheckbox.IsEnabled = false;
                SSAOStrengthLabel.IsEnabled = false;
                SSAOStrength.IsEnabled = false;
            }
            else
            {
                SSAOCheckbox.IsEnabled = true;
                SSAOStrengthLabel.IsEnabled = true;
                SSAOStrength.IsEnabled = PtRenderingParams.CalcSSAO;
            }

            if (PtRenderingParams.Lighting == Pointcloud.Common.Lighting.SsaoOnly)
                SSAOCheckbox.IsChecked = PtRenderingParams.CalcSSAO = true;
            if (PtRenderingParams.Lighting == Pointcloud.Common.Lighting.Unlit)
                SSAOCheckbox.IsChecked = PtRenderingParams.CalcSSAO = false;

            if (PtRenderingParams.Lighting != Pointcloud.Common.Lighting.BlinnPhong)
            {
                SpecStrength.IsEnabled = false;
                SpecStrengthLabel.IsEnabled = false;
                Shininess.IsEnabled = false;
                ShininessVal.IsEnabled = false;
            }
            else
            {
                SpecStrength.IsEnabled = true;
                SpecStrengthLabel.IsEnabled = true;
                Shininess.IsEnabled = true;
                ShininessVal.IsEnabled = true;
            }

            if (PtRenderingParams.Lighting != Pointcloud.Common.Lighting.Edl)
            {
                EDLNeighbourPx.IsEnabled = false;
                EDLNeighbourPxLabel.IsEnabled = false;
                EDLStrength.IsEnabled = false;
                EDLStrengthLabel.IsEnabled = false;
            }
            else
            {
                EDLNeighbourPx.IsEnabled = true;
                EDLNeighbourPxLabel.IsEnabled = true;
                EDLStrength.IsEnabled = true;
                EDLStrengthLabel.IsEnabled = true;
            }
        }

        private void PtShape_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (Lib.App == null || !Lib.App.IsInitialized || !Lib.App.IsSceneLoaded) return;
            PtRenderingParams.Shape = (PointShape)e.AddedItems[0];
        }

        private void PtSizeMode_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (Lib.App == null || !Lib.App.IsInitialized || !Lib.App.IsSceneLoaded) return;
            PtRenderingParams.PtMode = (PointSizeMode)e.AddedItems[0];
        }

        private void ColorMode_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (Lib.App == null || !Lib.App.IsInitialized || !Lib.App.IsSceneLoaded) return;
            PtRenderingParams.ColorMode = (ColorMode)e.AddedItems[0];

            if (PtRenderingParams.ColorMode != Pointcloud.Common.ColorMode.Single)
                SingleColor.IsEnabled = false;
            else
                SingleColor.IsEnabled = true;
        }

        private async void LoadFile_Button_Click(object sender, RoutedEventArgs e)
        {
            string fullPath;
            string path;
            var ofd = new System.Windows.Forms.OpenFileDialog
            {
                Filter = "Meta json (*.json)|*.json"
            };

            var dialogResult = ofd.ShowDialog();

            Console.WriteLine(dialogResult);

            if (dialogResult == System.Windows.Forms.DialogResult.OK)
            {
                if (!ofd.SafeFileName.Contains("meta.json"))
                {
                    MessageBox.Show("Invalid file selected", "Alert", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    return;
                }

                fullPath = ofd.FileName;
                path = fullPath.Replace(ofd.SafeFileName, "");

                PtRenderingParams.PathToOocFile = path;

                InnerGrid.IsEnabled = false;

                if (Lib.App != null && !Lib.App.IsAlive)
                    Lib.CloseGameWindow();

                if (Lib.FusTask != null && !Lib.FusTask.IsCompleted)
                {
                    try
                    {
                        Lib.CloseGameWindow();

                    }
                    catch (NullReferenceException) { }
                }

                Lib.ExecFusAppInNewThread(true);

                Closed += (s, e) => Lib.CloseGameWindow();
                InnerGrid.IsEnabled = true;

                SpinWait.SpinUntil(() => Lib.App.ReadyToLoadNewFile);

                MinProjSize.Value = Lib.App.GetOocLoaderMinProjSizeMod();
                MinProjSizeVal.Content = MinProjSize.Value.ToString("0.00");

                if (Lib.App.GetOocLoaderRootNode() != null) //if RootNode == null no scene was ever initialized
                {
                    Lib.App.DeletePointCloud();

                    SpinWait.SpinUntil(() => Lib.App.ReadyToLoadNewFile && Lib.App.GetOocLoaderWasSceneUpdated() && Lib.App.IsInitialized);
                }

                Lib.App.ResetCamera();
                Lib.App.LoadPointCloudFromFile();
                InnerGrid.IsEnabled = true;
                ShowOctants_Button.IsEnabled = true;
                ShowOctants_Img.Source = new BitmapImage(new Uri("Assets/octants.png", UriKind.Relative));
                inactiveBorder.Visibility = Visibility.Collapsed;
            }

        }

        private void DeleteFile_Button_Click(object sender, RoutedEventArgs e)
        {
            Lib.App.DeletePointCloud();
        }

        private void ResetCam_Button_Click(object sender, RoutedEventArgs e)
        {
            Lib.App?.ResetCamera();
        }

        private void VisPoints_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (Lib.App == null || !Lib.App.IsInitialized) return;
            e.Handled = !IsTextAllowed(PtThreshold.Text);

            if (!e.Handled)
            {
                if (!int.TryParse(PtThreshold.Text, out var ptThreshold)) return;
                if (ptThreshold < 0)
                {
                    PtThreshold.Text = Lib.App.GetOocLoaderPointThreshold().ToString();
                    return;
                }
                Lib.App.SetOocLoaderPointThreshold(ptThreshold);
            }
            else
            {
                PtThreshold.Text = Lib.App.GetOocLoaderPointThreshold().ToString();
            }
        }

        private void ShininessVal_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (Lib.App == null || !Lib.App.IsInitialized) return;
            e.Handled = !IsTextAllowed(ShininessVal.Text);

            if (!e.Handled)
            {
                if (!int.TryParse(ShininessVal.Text, out var shininess)) return;
                if (shininess < 0)
                {
                    ShininessVal.Text = Core.PtRenderingParams.Shininess.ToString();
                    return;
                }

                PtRenderingParams.Shininess = shininess;
            }
            else
                ShininessVal.Text = PtRenderingParams.Shininess.ToString();
        }

        private void ShininessVal_LostFocus(object sender, RoutedEventArgs e)
        {
            ShininessVal.Text = PtRenderingParams.Shininess.ToString();
        }

        private void VisPoints_LostFocus(object sender, RoutedEventArgs e)
        {
            PtThreshold.Text = Lib.App.GetOocLoaderPointThreshold().ToString();
        }

        private void ShowOctants_Button_Click(object sender, RoutedEventArgs e)
        {
            while (!Lib.App.ReadyToLoadNewFile || !Lib.App.GetOocLoaderWasSceneUpdated() || !Lib.App.IsSceneLoaded)
                continue;

            if (!_areOctantsShown)
            {
                Lib.App.DoShowOctants = true;
                _areOctantsShown = true;
                ShowOctants_Img.Source = new BitmapImage(new Uri("Assets/octants_on.png", UriKind.Relative));
            }
            else
            {
                Lib.App.DeleteOctants();
                _areOctantsShown = false;
                ShowOctants_Img.Source = new BitmapImage(new Uri("Assets/octants.png", UriKind.Relative));
            }
        }

        #endregion

        private static bool IsTextAllowed(string text)
        {
            return !numRegex.IsMatch(text);
        }
    }
}