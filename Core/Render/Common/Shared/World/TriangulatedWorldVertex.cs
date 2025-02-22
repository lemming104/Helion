using Helion.Geometry.Vectors;

namespace Helion.Render.OpenGL.Shared.World;

public struct TriangulatedWorldVertex
{
    public float X;
    public float Y;
    public float Z;
    public float U;
    public float V;
    public float PrevZ;
    public float PrevU;
    public float PrevV;

    public TriangulatedWorldVertex(float x, float y, float z, float prevZ, float u, float v, float prevU, float prevV)
    {
        X = x;
        Y = y;
        Z = z;
        U = u;
        V = v;
        PrevZ = prevZ;
        PrevU = prevU;
        PrevV = prevV;
    }

    public TriangulatedWorldVertex(double x, double y, double z, double prevZ, double u, double v, double prevU, double prevV) :
        this((float)x, (float)y, (float)z, (float)prevZ, (float)u, (float)v, (float)prevU, (float)prevV)
    {
    }

    public TriangulatedWorldVertex(Vec3F position, float prevZ, Vec2F uv, Vec2F prevUV) :
        this(position.X, position.Y, position.Z, prevZ, uv.X, uv.Y, prevUV.X, prevUV.Y)
    {
    }

    public override string ToString() => $"{X}, {Y}, {Z} [{U}, {V}]";
}
