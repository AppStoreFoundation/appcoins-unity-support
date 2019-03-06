using UnityEngine;
using UnityEditor;

class CustomAssetImportHook : AssetPostprocessor
{
    static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
    {
        Debug.Log("Import finished going to do some cleanup!!!");
        //Make sure that older versions of the plugin dont act up
        MigrationHelper.DeleteOlderFiles();
        
    }
}