using Fusee.Engine.Core;

namespace Fusee.Examples.Integrations.WpfFramebuffer.Frontend.ViewModel
{
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
