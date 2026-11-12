#nullable enable
using UnityEditor;

namespace Expedientia.Cliente.Editor
{
    /// <summary>El arte de Assets/Resources/arte entra como sprite, sin mipmaps y sin recortar a potencia de 2.</summary>
    public sealed class ImportarArte : AssetPostprocessor
    {
        private void OnPreprocessTexture()
        {
            if (!assetPath.Replace('\\', '/').Contains("/Resources/arte/")) return;
            var t = (TextureImporter)assetImporter;
            t.textureType = TextureImporterType.Sprite;
            t.spriteImportMode = SpriteImportMode.Single;
            t.alphaIsTransparency = true;
            t.mipmapEnabled = false;
            t.npotScale = TextureImporterNPOTScale.None;
            t.wrapMode = UnityEngine.TextureWrapMode.Clamp;
            t.filterMode = UnityEngine.FilterMode.Bilinear;
            t.textureCompression = TextureImporterCompression.CompressedHQ;
            t.maxTextureSize = 1024;
        }
    }
}
