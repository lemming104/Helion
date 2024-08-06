﻿using System.Runtime.InteropServices;
using Helion.Geometry.Vectors;
using Helion.Render.OpenGL.Vertex;

namespace Helion.Render.OpenGL.Renderers.Legacy.World.Entities;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct EntityVertex
{    
    [VertexAttribute]
    public Vec3F Pos;

    [VertexAttribute]
    public float LightLevel;

    [VertexAttribute]
    public float Alpha;

    [VertexAttribute]
    public float Fuzz;

    [VertexAttribute]
    public float FlipU;

    [VertexAttribute]
    public float ColorMapTranslation;

    [VertexAttribute]
    public Vec3F PrevPos;
}
