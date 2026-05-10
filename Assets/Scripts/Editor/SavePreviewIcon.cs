using UnityEngine;
using UnityEditor;
using System.IO;

public class SavePreviewIcon
{
    // Adds a context menu item to the Project window
    [MenuItem("Tools/Save Previews as PNG", false, 20)]
    public static void SaveSelectedAssetPreviews()
    {
        // Get all currently selected objects in the project window
        Object[] selectedAssets = Selection.objects;

        if (selectedAssets == null || selectedAssets.Length == 0)
        {
            Debug.LogWarning("No assets selected.");
            return;
        }

        int savedCount = 0;

        foreach (Object selectedAsset in selectedAssets)
        {
            // Get the asset's project path (e.g., "Assets/Models/MyModel.fbx")
            string assetPath = AssetDatabase.GetAssetPath(selectedAsset);
            
            // Skip if it doesn't have a valid path (e.g., a scene object)
            if (string.IsNullOrEmpty(assetPath))
            {
                continue;
            }

            // Determine the folder directory and construct the new PNG path
            string directory = Path.GetDirectoryName(assetPath);
            string fileName = selectedAsset.name + ".png";
            string fullPath = Path.Combine(directory, fileName);

            // Attempt to get the generated preview
            Texture2D previewTexture = AssetPreview.GetAssetPreview(selectedAsset);

            // Fallback to mini thumbnail if the preview hasn't generated yet
            if (previewTexture == null)
            {
                previewTexture = AssetPreview.GetMiniThumbnail(selectedAsset);
            }

            if (previewTexture == null)
            {
                Debug.LogError($"Could not generate a preview texture for {selectedAsset.name}.");
                continue;
            }

            // Make the texture readable so we can extract the pixels
            Texture2D readableTexture = MakeTextureReadable(previewTexture);

            // Encode to PNG format
            byte[] pngData = readableTexture.EncodeToPNG();

            // Save the file automatically to the same folder
            File.WriteAllBytes(fullPath, pngData);
            
            // Clean up the temporary texture from memory immediately to prevent memory leaks during batching
            Object.DestroyImmediate(readableTexture);
            
            savedCount++;
        }

        // Only refresh the Asset Database once all files are written
        if (savedCount > 0)
        {
            AssetDatabase.Refresh();
            Debug.Log($"Successfully saved {savedCount} preview icon(s).");
        }
    }

    // Validation function to ensure the menu item is only clickable when at least one asset is selected
    [MenuItem("Tools/Save Previews as PNG", true)]
    public static bool SaveSelectedAssetPreviewsValidation()
    {
        return Selection.objects != null && Selection.objects.Length > 0;
    }

    // Helper method to bypass the "Texture is not readable" error
    private static Texture2D MakeTextureReadable(Texture2D source)
    {
        RenderTexture renderTex = RenderTexture.GetTemporary(
            source.width,
            source.height,
            0,
            RenderTextureFormat.Default,
            RenderTextureReadWrite.sRGB);

        Graphics.Blit(source, renderTex);
        
        RenderTexture previous = RenderTexture.active;
        RenderTexture.active = renderTex;

        Texture2D readableText = new Texture2D(source.width, source.height, TextureFormat.RGBA32, false);
        readableText.ReadPixels(new Rect(0, 0, renderTex.width, renderTex.height), 0, 0);
        readableText.Apply();

        RenderTexture.active = previous;
        RenderTexture.ReleaseTemporary(renderTex);

        return readableText;
    }
}