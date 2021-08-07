﻿using Helion.Geometry.Boxes;
using Helion.Render.Common;

namespace Helion.Render.OpenGL.Renderers.Hud.Text
{
    public readonly struct RenderableCharacter
    {
        public readonly HudBox Area;
        public readonly Box2F UV;

        public RenderableCharacter(HudBox area, Box2F uv)
        {
            Area = area;
            UV = uv;
        }
    }
}
