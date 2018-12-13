using UnityEditor;
using UnityEngine;
using System;
using System.IO;
using System.Collections;
using System.Timers;

// Draw the window for the user select what scenes he wants to export
// and configure player settings.
public class AndroidCustomBuildWindow : CustomBuildWindow
{
    // Default values:
    private string packageName;
    private string gradlePath;
    private bool buildRelease;
    private bool debugMode;
    private string gradleMem;
    private string dexMem;
    private string adbPath;
    private bool runAdbInstall;
    private string mainActivityPath;
    private bool runAdbRun;
    private BuildStage lastBuildSatage;
    private string windowsPath = "C:\\Program Files\\Android\\";     private string macPath = "/Applications/";
    private string androidVersion;
    private string devPath;
    bool correctFoundGradle;

    private string defaultGradleMem = "1536";
    private string defaultDexMem = "1024";
    protected override void IdleGUI()
    {

        int xEnd = (int)instance.position.width;
        int xDelta = 10;

        int yEnd = (int)instance.position.height;
        int yDelta = 10;

        // GRADLE
        float gradlePartHeight = 5;
        GUI.Label(new Rect(5, gradlePartHeight, xEnd - xDelta, 40),
                  "Select the gradle path:");

        gradlePartHeight += 20;
        gradlePath = GUI.TextField(
            new Rect(5, gradlePartHeight, xEnd - xDelta, 20),
            gradlePath);

        gradlePath =
            HandleCopyPaste(GUIUtility.keyboardControl) ??
            gradlePath;

        gradlePartHeight += 20;
        buildRelease = GUI.Toggle(
                new Rect(5, gradlePartHeight, xEnd - xDelta, 20),
                buildRelease,
                "Build a Release version? (Uncheck it if you want to build a " +
                "Debug Version)."
        );

        gradlePartHeight += 20;
        debugMode = GUI.Toggle(
            new Rect(5, gradlePartHeight, xEnd - xDelta, 20),
            debugMode,
            "Run gradle in debug mode? This will not end gradle terminal " +
            "automatically."
        );

        gradlePartHeight += 20;
        GUI.Label(new Rect(5, gradlePartHeight, 105, 20), "Gradle heap size:");
        gradleMem = GUI.TextField(
            new Rect(105, gradlePartHeight, 60, 20),
            gradleMem
        );
        GUI.Label(new Rect(165, gradlePartHeight, 70, 20), "MB");

        gradlePartHeight += 25;
        GUI.Label(new Rect(5, gradlePartHeight, 150, 20), "Dex heap size:");
        dexMem = GUI.TextField(
            new Rect(105, gradlePartHeight, 60, 20),
            dexMem
        );
        GUI.Label(new Rect(165, gradlePartHeight, xEnd - xDelta, 20),
                  "MB  (Gradle heap size has to be grater than Dex heap size)");

        // ADB
        float adbPartHeight = gradlePartHeight + 50;
        GUI.Label(new Rect(5, adbPartHeight, xEnd - xDelta, 40), "Select the adb path:");

        adbPartHeight += 20;
        adbPath = GUI.TextField(new Rect(5, adbPartHeight, xEnd - xDelta, 20),
                                            adbPath);
        adbPath =
            HandleCopyPaste(GUIUtility.keyboardControl) ??
            adbPath;

        adbPartHeight += 20;
        runAdbInstall = GUI.Toggle(
            new Rect(5, adbPartHeight, xEnd - xDelta, 20),
            runAdbInstall,
            "Install build when done?"
        );

        float adbRunPartHeight = adbPartHeight + 20;
        GUI.Label(new Rect(5, adbRunPartHeight, xEnd - xDelta, 40),
                  "Path to the main activity name " +
                  "({package name}/.UnityPlayerActivity by default)"
                 );

        adbRunPartHeight += 20;
        mainActivityPath = GUI.TextField(
            new Rect(5, adbRunPartHeight, xEnd - xDelta, 20),
            mainActivityPath
        );
        mainActivityPath =
            HandleCopyPaste(GUIUtility.keyboardControl) ??
            mainActivityPath;

        adbRunPartHeight += 20;
        runAdbRun = GUI.Toggle(
            new Rect(5, adbRunPartHeight, xEnd - xDelta, 20),
            runAdbRun,
            "Run build when done?");

        // SCENES
        float scenesPartHeight = adbRunPartHeight + 40;
        GUI.Label(new Rect(5, scenesPartHeight, xEnd - xDelta, 40),
                  "Select what scenes you want to export:\n(Only scenes that " +
                  "are in build settings are true by default)");

        int scenesLength = EditorBuildSettings.scenes.Length;


        // Add open scenes in the hierarchy window if build settings scenes list
        // have none
        if (instance.buildScenesEnabled.Length == 0)
        {
            instance.selector.AddAllOpenScenesToBuildSettings();
        }

 
        instance.selector.CheckFirstScene();

        // Get enabled scenes at build settings scenes
        instance.buildScenesEnabled =
                    instance.selector.GetBuildSettingsScenesEnabled();

        float scrollViewLength = scenesLength * 25f;
        scenesPartHeight += 30;
        scrollViewVector = GUI.BeginScrollView(
            new Rect(5, scenesPartHeight, xEnd - xDelta, 215),
            scrollViewVector,
            new Rect(0, 0, xEnd - xEnd / 10, scrollViewLength)
        );
        for (int i = 0; i < scenesLength; i++)
        {
            instance.buildScenesEnabled[i] = GUI.Toggle(
                new Rect(10, 10 + i * 20, xEnd - xEnd / 10, 20),
                instance.buildScenesEnabled[i],
                EditorBuildSettings.scenes[i].path
            );
        }
        GUI.EndScrollView();

        // Pass enabled scenes to SelectScenes class 
        instance.selector.UpdatedBuildScenes(instance.buildScenesEnabled);

        // BUTTONS
        int buttonHeight = 30;
        int buttonWidth = 120;

        if (GUI.Button(
            new Rect(
                xDelta,
                yEnd - buttonHeight - yDelta,
                buttonWidth,
                buttonHeight),
            "Player Settings")
           )
        {
            EditorApplication.ExecuteMenuItem("Edit/Project Settings/Player");
        }

        if (GUI.Button(
            new Rect(
                2 * xDelta + buttonWidth,
                yEnd - buttonHeight - yDelta,
                buttonWidth,
                buttonHeight),
            "Add Open Scenes")
           )
        {
            instance.selector.AddAllOpenScenesToBuildSettings();
            instance.selector.buildScenesEnabled =
                instance.selector.GetBuildSettingsScenesEnabled();
        }

        if (GUI.Button(
            new Rect(
                xEnd - 2 * buttonWidth - 2 * xDelta,
                yEnd - buttonHeight - yDelta,
                buttonWidth,
                buttonHeight),
            "Cancel")
           )
        {
            instance.Close();
        }

        bool invalidReleaseBuild = (buildRelease && (PlayerSettings.keyaliasPass == "" || PlayerSettings.keystorePass == ""));
        if (!buildRelease)
            invalidReleaseBuild = false;

        if (gradlePath != "" && !invalidReleaseBuild && correctFoundGradle &&
            GUI.Button(
                new Rect(
                    xEnd - buttonWidth - xDelta,
                    yEnd - buttonHeight - yDelta,
                    buttonWidth,
                    buttonHeight),
                "Confirm"
               )
           )
        {
            // Check what is the last build stage
            if (runAdbInstall == true && runAdbRun == true)
            {
                lastBuildSatage = BuildStage.PROJECT_RUN;
            }

            else if (runAdbInstall == true)
            {
                lastBuildSatage = BuildStage.PROJECT_INSTALL;
            }

            else
            {
                lastBuildSatage = BuildStage.PROJECT_BUILD;
            }

            SetCustomBuildPrefs();
            instance.Close();
            instance.unityEvent.Invoke(lastBuildSatage);

        }

        //KEYSIGN CHECK
        //If release mode is enabled and password not provided display a warning
        if (invalidReleaseBuild)
        {
            GUIStyle style = GUIStyle.none;
            style.normal.textColor = Color.red;
            GUI.Label(new Rect(5, yEnd - buttonHeight - yDelta * 2 - 20, xEnd - xDelta, 20),
                      "WARNING: Keystore password and/or keyalias password are empty!",
                      style);
        }

        //Gradle Path Check
        //If gradle path isn't found successfully the confirm button is disabled
        if (!correctFoundGradle)         {             GUIStyle style = GUIStyle.none;             style.normal.textColor = Color.red;             GUI.Label(new Rect(5, yEnd - buttonHeight - yDelta * 2 - 20, xEnd - xDelta, 20),                       "WARNING: Gradle Version is incorrect!",                       style);             if (gradlePath != "Gradle Path not found.Please fill it manually!")             {                 correctFoundGradle = true;
            }         }      }

    protected override void UnityExportGUI()
    {
        GUI.Label(new Rect(5, 30, 590, 40), "building gradle project...\n" +
                  "Please be patient as Unity might stop responding...\nThis " +
                  "process will launch external windows so that you can keep " +
                  "tracking the build progress");
    }

    protected override void ProjectBuildGUI()
    {
        GUI.Label(new Rect(5, 30, 590, 40), "Running gradle to generate " +
                  "APK...\nPlease be patient...");
    }

    protected override void ProjectInstallGUI()
    {
        GUI.Label(new Rect(5, 30, 590, 40), "Installing APK...\nPlease be " +
                  "patient...");
    }

    protected override void ProjectRunGUI()
    {
        GUI.Label(new Rect(5, 30, 590, 40), "Running APK...\nPlease be " +
                  "patient...");
    }

    protected override void SetCustomBuildPrefs()
    {
        EditorPrefs.SetString("appcoins_package_name", Application.identifier);
        EditorPrefs.SetString("appcoins_gradle_path", gradlePath);
        EditorPrefs.SetString("appcoins_adb_path", adbPath);
        EditorPrefs.SetString("appcoins_main_activity_path",
                              mainActivityPath);

        EditorPrefs.SetBool("appcoins_build_release", buildRelease);
        EditorPrefs.SetBool("appcoins_run_adb_install",
                            runAdbInstall);

        EditorPrefs.SetBool("appcoins_run_adb_run", runAdbRun);
        EditorPrefs.SetBool("appcoins_debug_mode", debugMode);
        EditorPrefs.SetString("appcoins_gradle_mem", gradleMem);
        EditorPrefs.SetString("appcoins_dex_mem", dexMem);

    }

    protected override void LoadCustomBuildPrefs()
    {
        packageName = EditorPrefs.GetString("appcoins_package_name", "");

        if (ExistsAndroidPath(SystemInfo.operatingSystemFamily ==
                              OperatingSystemFamily.Windows ? windowsPath : macPath))
        {

            androidVersion = GetAndroidVersion(devPath);

            //Print console message to help developer keep track of process
            Debug.Log("Android studio directory exists");

            string gradleVersion = GetGradleVersion(devPath + androidVersion + "/contents/gradle/");

            // If package name is different we assume that the user is working in 
            // a different unity project
            if (!Application.identifier.Equals(packageName))
            {
                gradlePath = SystemInfo.operatingSystemFamily ==
                                       OperatingSystemFamily.Windows ?
                                       windowsPath + androidVersion + "\\gradle\\" + gradleVersion + "\\bin\\gradle" :                                        macPath + androidVersion + "/contents/gradle/" + gradleVersion +                     "/bin/";

                correctFoundGradle = true;

                adbPath = EditorPrefs.GetString("AndroidSdkRoot") +
                                     "/platform-tools/adb";

                mainActivityPath = Application.identifier + "/.UnityPlayerActivity";
                buildRelease = false;
                runAdbInstall = false;
                runAdbRun = false;
                debugMode = false;
                gradleMem = defaultGradleMem;
                dexMem = defaultDexMem;
            }

            else
            {

                gradlePath = EditorPrefs.GetString(
                  "appcoins_gradle_path",
                    SystemInfo.operatingSystemFamily == OperatingSystemFamily.Windows ?
                    windowsPath + androidVersion + "\\gradle\\" + gradleVersion + "\\bin\\gradle" :                     macPath + androidVersion + "/contents/gradle/" + gradleVersion +
                        "/bin/"
                ); 
                correctFoundGradle = true;

                adbPath = EditorPrefs.GetString(
                    "appcoins_adb_path",
                    EditorPrefs.GetString("AndroidSdkRoot") +
                        "/platform-tools/adb"
                );

                mainActivityPath = EditorPrefs.GetString(
                    "appcoins_main_activity_path",
                    Application.identifier + "/.UnityPlayerActivity");

                buildRelease = EditorPrefs.GetBool("appcoins_build_release", false);

                runAdbInstall = EditorPrefs.GetBool("appcoins_run_adb_install", false);

                runAdbRun = EditorPrefs.GetBool("appcoins_run_adb_run", false);

                debugMode = EditorPrefs.GetBool("appcoins_debug_mode", false);

                gradleMem = EditorPrefs.GetString("appcoins_gradle_mem",
                                                  defaultGradleMem);

                dexMem = EditorPrefs.GetString("appcoins_dex_mem", defaultDexMem);
            }
        }

        else
        {
            //In case Android Studio is not Installed
            //User is asked to fill gradle path manually
            WarningPopup();
            correctFoundGradle = false;

            //Print console message to help developer keep track of process
            Debug.Log("Android studio directory is non existing");

            gradlePath = "Gradle Path not found.Please fill it manually!";

            adbPath = EditorPrefs.GetString(
               "appcoins_adb_path",
                EditorPrefs.GetString("AndroidSdkRoot") +
                      "/platform-tools/adb"
            );

            mainActivityPath = EditorPrefs.GetString(
                "appcoins_main_activity_path",
                Application.identifier + "/.UnityPlayerActivity");

            buildRelease = EditorPrefs.GetBool("appcoins_build_release", false);

            runAdbInstall = EditorPrefs.GetBool("appcoins_run_adb_install", false);

            runAdbRun = EditorPrefs.GetBool("appcoins_run_adb_run", false);

            debugMode = EditorPrefs.GetBool("appcoins_debug_mode", false);

            gradleMem = EditorPrefs.GetString("appcoins_gradle_mem",
                                              defaultGradleMem);

            dexMem = EditorPrefs.GetString("appcoins_dex_mem", defaultDexMem);

        }
    }

    // Process a directory 
    // and the subdirectories it contains searching for the Android app folder to get its version
    // Throws an error and returns NOT_FOUND string if not found
    protected string GetAndroidVersion(string targetDirectory)     {         androidVersion = "NOT_FOUND";          // Recurse into subdirectories of this directory.         string[] subdirectoryEntries = Directory.GetDirectories(targetDirectory);         foreach (string subdirectory in subdirectoryEntries)             if (subdirectory.Contains("Android Studio"))             {                 string[] vers = SystemInfo.operatingSystemFamily ==                                           OperatingSystemFamily.Windows ? subdirectory.Split(new string[] { "//" }, StringSplitOptions.None)                                           : subdirectory.Split('/');                 
                androidVersion = SystemInfo.operatingSystemFamily ==
                                           OperatingSystemFamily.Windows ? vers[3] : vers[2];                 
                Debug.Log("This is the Android Version" + "\n" + androidVersion);                 return androidVersion;              }          Debug.LogError("Unable to determine android version");          return androidVersion;     }

    // Process a directory 
    // and the subdirectories it contains searching for the gradle folder to get its version
    // Throws an error and returns NOT_FOUND string if not found
    protected string GetGradleVersion(string targetDirectory)
    {
        string gradleVersion = "NOT_FOUND";

        // Recurse into subdirectories of this directory.
        string[] subdirectoryEntries = Directory.GetDirectories(targetDirectory);
        foreach (string subdirectory in subdirectoryEntries)

            if (subdirectory.Contains("gradle-"))
            {
                string[] vers = subdirectory.Split('-');
                gradleVersion = "gradle-" + vers[1];
                Debug.Log("This is the gradVersion" + "\n" + gradleVersion);
                return gradleVersion;
            }


        Debug.LogError("Unable to determine gradle version");

        return gradleVersion;
    }

    //Check if android is installed either on mac or windows
    protected bool ExistsAndroidPath(string path1)
    {
        if (Directory.Exists(path1))
        {
            devPath = path1;
            return true;
        }
        return false;
    }

    //Display warning popup window if graddle path is not found
    protected void WarningPopup()
    {

        EditorUtility.DisplayDialog("Warning", "Gradle Path not found. Please fill it manually!", "Close");
    }
}
