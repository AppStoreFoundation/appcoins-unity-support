using UnityEngine;

using System;
using System.IO;

public static class MigrationHelper {
    public static void DeleteOlderFiles() {

        Debug.Log("Cleaning up possible old plugin files");

        //v0.3 Files
        string scriptsPath = Application.dataPath + "/AppcoinsUnity";
        string editorScriptsPath = scriptsPath + "/Editor";

        string[] olderUnusedFiles03 = { 
            "unity-2018.pdb"
             };

        DeleteFiles(scriptsPath, olderUnusedFiles03);

        string[] olderUnusedFiles = {
            "unity-editor-2018.pdb",
            "unity-editor-2018.dll",
            "unity-editor-2018.dll.mdb"
             };

        DeleteFiles(editorScriptsPath, olderUnusedFiles);

        //string[] olderScriptFiles = { "AppcoinsPurchaser", "AppcoinsSku",
            //"AppcoinsUnity", "BashUtils", 
            //"AppCoinsUnityPluginTests2018", 
            //"AppCoinsUnityPluginTests2017", 
            //"AppCoinsUnityPluginTests5_6",
            //"AppCoinsUnityPluginEditorMode2018", 
            //"AppCoinsUnityPluginEditorMode5_6",
            //"AppCoinsUnityPluginEditorMode2017" };

        editorScriptsPath = scriptsPath + "/Scripts/Editor";

        //NOTE:works with partial file names but be careful to avoid unwanted deletions!
        string[] olderEditorScriptFiles = { 
            "appcoins-unity-support-2018.dll",
            "appcoins-unity-support-5_6.dll",
            "appcoins-unity-support-2017.dll",
            "AppCoinsUnitySupport2018.dll",
            "AppCoinsUnitySupport2017.dll",
            "AppCoinsUnitySupport5_6.dll" };

        //DeleteFiles(scriptsPath, olderScriptFiles);
        DeleteFiles(editorScriptsPath, olderEditorScriptFiles);
    }

    private static void DeleteFiles(string dirPath, string[] filesToDelete) {
        //If folder doesnt exist return immediately
        if (!Directory.Exists(dirPath))
            return;

        string[] files = Directory.GetFiles(dirPath);
        foreach(string filePath in files)
        {
            string fName = Path.GetFileName(filePath);

            if (Array.Exists(filesToDelete, dF => fName.Contains(dF)))
            {
                File.Delete(filePath);
            }
        }
    }
}