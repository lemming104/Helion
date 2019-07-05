using Helion.Util.Geometry;

namespace Helion.Render.Commands.Types
{
    public struct ViewportCommand : IRenderCommand
    {
        public readonly Dimension Dimension;
        public readonly Vec2I Offset;

        public ViewportCommand(Dimension dimension) : this(dimension, new Vec2I(0, 0))
        {
        }
        
        public ViewportCommand(Dimension dimension, Vec2I offset)
        {
            Dimension = dimension;
            Offset = offset;
        }
    }
}