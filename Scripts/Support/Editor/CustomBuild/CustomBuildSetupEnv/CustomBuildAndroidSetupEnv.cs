using UnityEditor;
using UnityEngine;

using System;

public class CustomBuildAndroidSetupEnv : CustomBuildSetupEnv
{
    private const string SAMPLE_BUNDLE_ID = "com.appcoins.sample";
    private const string defaultUnityPackage = "com.Company.ProductName";
    private static string appcoinsMainTemplate = Application.dataPath +
                                                            "/AppcoinsUnity/" +
                                                            "Plugins/Android/" +
                                                            "mainTemplate." +
                                                            "gradle";

    private static string currentMainTemplate = Application.dataPath +
                                                           "/Plugins/Android/" +
                                                           "mainTemplate." +
                                                           "gradle";

    public CustomBuildAndroidSetupEnv(AppcoinsGameObject a) : base(a) {}

    public override void Setup()
    {
        // Merge main templates
        Tools.MergeMainTemplates(currentMainTemplate, appcoinsMainTemplate);

        // Update Assets folder (otherwise mainTemplate file icon will not show 
        // up)
        AssetDatabase.Refresh();

        try
        {
            base.Setup();
        }

        catch (Exception e)
        {
            throw e;
        }

        // Check if the active platform is Android. If it isn't change it
        if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.Android)
        {
            EditorUserBuildSettings.SwitchActiveBuildTarget(
                BuildTargetGroup.Android, BuildTarget.Android);
        }

        // Check if min sdk version is lower than 21. If it is, set it to 21
        if (PlayerSettings.Android.minSdkVersion < 
            AndroidSdkVersions.AndroidApiLevel21
           )
        {
            PlayerSettings.Android.minSdkVersion = 
                AndroidSdkVersions.AndroidApiLevel21;
        }

        // Check if the bunde id is the default one and change it if it to 
        // avoid that error
        if (PlayerSettings.applicationIdentifier.Equals(defaultUnityPackage))
        {
            PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, 
                                                    SAMPLE_BUNDLE_ID);
        }

        // Export Project with gradle format (template)
        EditorUserBuildSettings.androidBuildSystem = AndroidBuildSystem.Gradle;

        //Make sure that older versions of the plugin dont act up
        MigrationHelper.DeleteOlderFiles();

        //Make sure all non relevant errors go away
        UnityEngine.Debug.ClearDeveloperConsole();
        UnityEngine.Debug.Log("Successfully integrated Appcoins Unity plugin! :)");
    }
}