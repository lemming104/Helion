using System;
using System.Collections.Generic;
using Helion.Geometry.Vectors;
using Helion.Util.Bytes;
using NLog;

namespace Helion.Resources.Definitions.Texture;

/// <summary>
/// The data structure for a Texture1/2/3 entry.
/// </summary>
public class TextureX
{
    private static readonly Logger Log = LogManager.GetCurrentClassLogger();

    /// <summary>
    /// All the different textures that make up this data structure.
    /// </summary>
    public readonly List<TextureXImage> Definitions;

    private TextureX(List<TextureXImage> definitions)
    {
        Definitions = definitions;
    }

    /// <summary>
    /// Attempts to read the Texture1/2/3 data.
    /// </summary>
    /// <param name="data">The data to read.</param>
    /// <returns>The Texture1/2/3 data, or an empty value if the data is
    /// corrupt.</returns>
    public static TextureX? From(byte[] data)
    {
        try
        {
            using ByteReader reader = new(data);
            int numTextures = reader.ReadInt32();

            List<TextureXImage> definitions = new(Math.Max(numTextures, 0));
            List<int> dataOffsets = new(numTextures);
            for (int offsetIndex = 0; offsetIndex < numTextures; offsetIndex++)
                dataOffsets.Add(reader.ReadInt32());

            foreach (int dataOffset in dataOffsets)
            {
                reader.Offset(dataOffset);

                string name = reader.ReadEightByteString();
                reader.Advance(4); // Skip flags/scalex/scaley.
                int width = reader.ReadInt16();
                int height = reader.ReadInt16();
                reader.Advance(4); // Skip columndirectory, so no Strife.
                int numPatches = reader.ReadInt16();

                List<TextureXPatch> patches = new(Math.Max(numPatches, 0));
                for (int patchIndex = 0; patchIndex < numPatches; patchIndex++)
                {
                    Vec2I patchOffset = (reader.ReadInt16(), reader.ReadInt16());
                    short index = reader.ReadInt16();
                    reader.Advance(4); // Skip stepdir/colormap

                    patches.Add(new TextureXPatch(index, patchOffset));
                }

                definitions.Add(new TextureXImage(name, width, height, patches));
            }

            return new TextureX(definitions);
        }
        catch
        {
            Log.Warn("Corrupt TextureX entry, textures will likely be missing");
            return null;
        }
    }

    /// <summary>
    /// Creates a series of texture definitions from the pnames provided.
    /// </summary>
    /// <param name="pnames">The pnames to make the texture definitions
    /// with.</param>
    /// <returns>A list of all the texture definitions.</returns>
    public List<TextureDefinition> ToTextureDefinitions(Pnames pnames)
    {
        List<TextureDefinition> definitions = new(Definitions.Count);
        foreach (TextureXImage image in Definitions)
        {
            List<TextureDefinitionComponent> components = CreateComponents(image, pnames);
            definitions.Add(new TextureDefinition(image.Name, image.Dimension, ResourceNamespace.Textures, components));
        }

        return definitions;
    }

    private static List<TextureDefinitionComponent> CreateComponents(TextureXImage image, Pnames pnames)
    {
        List<TextureDefinitionComponent> components = new(image.Patches.Count);
        foreach (TextureXPatch patch in image.Patches)
        {
            if (patch.PnamesIndex < 0 || patch.PnamesIndex >= pnames.Names.Count)
            {
                Log.Warn("Corrupt pnames index in texture {0}, texture will be corrupt", image.Name);
                continue;
            }

            string name = pnames.Names[patch.PnamesIndex];
            TextureDefinitionComponent component = new(name, patch.Offset);
            components.Add(component);
        }

        return components;
    }
}
