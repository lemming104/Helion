using Helion.Graphics;
using Helion.Render.Common.Renderers;
using Helion.Window;

namespace Helion.Layer.Images;

public class ImageLayer : IGameLayer
{
    public string Image { get; protected set; }

    public ImageLayer(string image)
    {
        Image = image;
    }

    public virtual void HandleInput(IConsumableInput input)
    {
        // Not used.
    }

    public virtual void RunLogic()
    {
        // Not used.
    }

    public virtual void Render(IHudRenderContext hud)
    {
        hud.Clear(Color.Black);

        hud.DoomVirtualResolution(() =>
        {
            hud.Image(Image, (0, 0));
        });
    }

    public void Dispose()
    {
        // Nothing to dispose.
    }
}
