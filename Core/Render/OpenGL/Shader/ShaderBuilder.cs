namespace Helion.Render.OpenGL.Shader
{
    public class ShaderBuilder
    {
        public string VertexShaderText = "";
        public string FragmentShaderText = "";

        public bool IsValid => VertexShaderText.Length > 0 && FragmentShaderText.Length > 0;
    }
}