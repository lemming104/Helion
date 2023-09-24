using Helion.Render.OpenGL.Vertex;
using System.Runtime.InteropServices;

namespace Helion.Render.OpenGL.Renderers.Legacy.World;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct LegacyVertex
{
    [VertexAttribute("pos", size: 3)]
    public float X;
    public float Y;
    public float Z;

    [VertexAttribute("uv", size: 2)]
    public float U;
    public float V;

    [VertexAttribute]
    public float LightLevel;

    [VertexAttribute]
    public float Alpha;

    [VertexAttribute]
    public float ClearAlpha;
    
    [VertexAttribute]
    public float LightLevelBufferIndex;

    [VertexAttribute("prevPos", size: 3)]
    public float PrevX;
    public float PrevY;
    public float PrevZ;

    [VertexAttribute("prevUV", size: 2)]
    public float PrevU;
    public float PrevV;

    [VertexAttribute]
    public float Fuzz;

    public LegacyVertex(float x, float y, float z, float prevX, float prevY, float prevZ, float u, float v, 
        short lightLevelAdd = 0, float alpha = 1.0f, float fuzz = 0.0f, float clearAlpha = 0.0f,
        int lightLevelBufferIndex = 0)
    {
        X = x;
        Y = y;
        Z = z;
        PrevX = prevX;
        PrevY = prevY;
        PrevZ = prevZ;
        U = u;
        V = v;
        PrevU = u;
        PrevV = v;
        LightLevel = lightLevelAdd;
        Alpha = alpha;
        Fuzz = fuzz;
        ClearAlpha = clearAlpha;
        LightLevelBufferIndex = lightLevelBufferIndex;
    }
}
