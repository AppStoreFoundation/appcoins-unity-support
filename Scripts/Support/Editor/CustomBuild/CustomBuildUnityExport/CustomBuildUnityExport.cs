using UnityEditor;
using UnityEngine;

using System;

public abstract class CustomBuildUnityExport
{
    protected BuildTarget buildTarget;
    protected BuildTargetGroup buildTargetGroup;
    protected BuildOptions buildOptions;
    protected ICustomBuildTarget platformTarget;

    public CustomBuildUnityExport(BuildTarget bT, BuildTargetGroup bTG, 
                                  BuildOptions bO, ICustomBuildTarget target)
    {
        buildTarget = bT;
        buildTargetGroup = bTG;
        buildOptions = bO;
        platformTarget = target;
    }

    public abstract void UnityExport(BuildStage stage, string[] scenesPath,
                                       out string projPath);
                                       
    protected string SelectProjectPath(string folderName)
    {
        string buildPath = Tools.SelectPath();

        if (buildPath == null || buildPath.Length == 0)
        {
            throw new ExportProjectPathIsNullException();
        }

        string unityProjPath = Tools.GetUnityProjectPath();
        if (buildPath == unityProjPath)
        {
            throw new ExportProjectPathIsEqualToUnityProjectPathException();
        }

        Tools.DeleteIfFolderAlreadyExists(buildPath, folderName);
        return buildPath;
    }
}