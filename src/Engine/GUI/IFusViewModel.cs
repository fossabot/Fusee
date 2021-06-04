using Fusee.Engine.Core;

namespace Fusee.Engine.GUI
{
    /// <summary>
    /// Collection of methods that are needed to render the content of fusee apps in an external UI, eg. using WPF.
    /// Used when implementing the Model View ViewModel pattern.
    /// </summary>
    public interface IFusViewModel
    {
        public RenderCanvas App { get; }

        public void RenderApp();

        public void LoadApp(int width, int height);

        public void ResizeApp(int width, int height);

        public void CloseApp();

        public void RegisterAssetProvider();
    }
}