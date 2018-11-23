using UnityEditor;
using UnityEngine.Events;

using System;

public class CustomBuild
{
    private SelectScenes scenesSelector;
    private string[] scenesPath = null;
    private string projPath;

    private CustomBuildSetupEnv customBuildSetup;
    private CustomBuildWindow customBuildWindow;
    private CustomBuildUnityExport customBuildUnityExport;
    private CustomBuildProjectBuild customBuildProjectBuild;
    private CustomBuildProjectInstall customBuildProjectInstall;
    private CustomBuildProjectRun customBuildProjectRun;

    private CustomBuildErrorTitles errorTitles;

    private BuildStageEvent buildStagesToRun;

    public BuildStage stage;
    private BuildStage lastBuildStage;

    public CustomBuild(CustomBuildSetupEnv setupEnv, CustomBuildWindow window,
                       CustomBuildUnityExport unityExport, 
                       CustomBuildProjectBuild projectBuild,
                       CustomBuildProjectInstall projectInstall,
                       CustomBuildProjectRun projectRun,
                       CustomBuildErrorTitles eT
                      )
    {
        scenesSelector = new SelectScenes();

        customBuildSetup = setupEnv;
        customBuildWindow = window;
        customBuildUnityExport = unityExport;
        customBuildProjectBuild = projectBuild;
        customBuildProjectInstall = projectInstall;
        customBuildProjectRun = projectRun;

        errorTitles = eT;

        buildStagesToRun = new BuildStageEvent();
    }

    // Run all custom build phases
    public virtual void RunProcess()
    {
        // Phase 1: Setup Enviornment
        try
        {
            StateSetupEnv();
            customBuildSetup.Setup();
        }
        catch (Exception e)
        {
            HandleExceptions(e);
            return;
        }

        // Phase 2: GUI (Chose custom build process)
        try
        {
            StateBuildIdle();
            CustomBuildWindow.CreateCustomBuildWindow(stage,
                                                      customBuildWindow, 
                                                      scenesSelector,
                                                      buildStagesToRun
                                                     );
            buildStagesToRun.AddListener(
                delegate(BuildStage lastStage)
            {
                lastBuildStage = lastStage;
                scenesPath = scenesSelector.ScenesToString();
                RunInstalationProcess();
            }
            );
        }
        catch (Exception e)
        {
            HandleExceptions(e);
            return;
        }
    }

    public virtual void RunInstalationProcess()
    {
        errorTitles.SetErrorTitles();

        try
        {
            // Phase 3: Export Unity Project
            StateUnityExport();
            customBuildUnityExport.UnityExport(stage, scenesPath, out projPath);
            if (lastBuildStage == stage) { return; }
        }
        catch (ExportProjectPathIsEqualToUnityProjectPathException e)
        {
            HandleExceptions(e);
            return;
        }
        catch (ExportProjectPathIsNullException e)
        {
            HandleExceptions(e);
            return;
        }
        catch (ExportProjectFailedException e)
        {
            HandleExceptions(e);
            return;
        }

        try
        {
            // Phase 5: Build Exported Project
            StateProjectBuild();
            customBuildProjectBuild.BuildProject(stage, projPath);
            if (lastBuildStage == stage) { BuildSuccess(); return; }
        }
        catch (TerminalProcessFailedException e)
        {
            HandleExceptions(e);
            return;
        }

        try
        {
            // Phase 6: Intall apk
            StateProjectInstall();
            customBuildProjectInstall.InstallProject(stage, projPath);
            if (lastBuildStage == stage) { BuildSuccess(); return; }
        }
        catch (TerminalProcessFailedException e)
        {
            HandleExceptions(e);
            return;
        }

        try
        {
            // Phase 7: Run apk
            StateProjectRun();
            customBuildProjectRun.RunProject(stage, projPath);
            if (lastBuildStage == stage) { BuildSuccess(); return; }
        }
        catch (TerminalProcessFailedException e)
        {
            HandleExceptions(e);
            return;
        }
    }

    private void HandleExceptions(Exception e)
    {
        EditorPrefs.SetInt("appcoins_error_stage", (int) stage);
        EditorPrefs.SetInt("appcoins_last_error_stage", (int) lastBuildStage);
        EditorPrefs.SetString("appcoins_error_message", e.Message);
        CustomBuildErrorWindow.CreateCustomBuildErrorWindow();
    }

    private void BuildSuccess()
    {
        EditorUtility.DisplayDialog("Custom Build", "Custom Build completed " +
                                    "without any errors.", "OK");
    }

    #region State Handling

    private void ChangeStage(BuildStage theStage)
    {
        stage = theStage;
    }

    public void StateSetupEnv()
    {
        ChangeStage(BuildStage.SETUP_ENV);
    }

    public void StateBuildIdle()
    {
        ChangeStage(BuildStage.IDLE);
    }

    public void StateUnityExport()
    {
        ChangeStage(BuildStage.UNITY_EXPORT);
    }

    public void StateProjectBuild()
    {
        ChangeStage(BuildStage.PROJECT_BUILD);
    }

    public void StateProjectInstall()
    {
        ChangeStage(BuildStage.PROJECT_INSTALL);
    }

    public void StateProjectRun()
    {
        ChangeStage(BuildStage.PROJECT_RUN);
    }

    #endregion
}
