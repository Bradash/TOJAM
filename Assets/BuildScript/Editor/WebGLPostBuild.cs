using UnityEditor;
using UnityEditor.Callbacks;
using System.IO;
using UnityEngine;

public static class WebGLPostBuild
{
    [PostProcessBuild]
    public static void OnPostprocessBuild(BuildTarget target, string pathToBuiltProject)
    {
        if (target != BuildTarget.WebGL) return;

        string activeTemplate = PlayerSettings.WebGL.template;

        if (!activeTemplate.Contains("Discord"))
        {
            return; 
        }

        // Define paths
        string defaultBuildFolder = Path.Combine(pathToBuiltProject, "Build");
        string targetParentFolder = Path.Combine(pathToBuiltProject, "public");
        string targetBuildFolder = Path.Combine(targetParentFolder, "build");

        // 1. Check if the old build directory exists, and delete it if it does
        if (Directory.Exists(targetBuildFolder))
        {
            Debug.Log($"Old build found at {targetBuildFolder}. Overwriting...");
            // The 'true' argument ensures all subfolders and files inside are deleted too
            Directory.Delete(targetBuildFolder, true); 
        }

        // 2. Proceed with moving the fresh build folder
        if (!Directory.Exists(defaultBuildFolder)) return;
        if (!Directory.Exists(targetParentFolder))
        {
            Directory.CreateDirectory(targetParentFolder);
        }

        // Move the fresh build folder into place
        Directory.Move(defaultBuildFolder, targetBuildFolder);
        Debug.Log($"Successfully replaced build files at: {targetBuildFolder}");
    }
}