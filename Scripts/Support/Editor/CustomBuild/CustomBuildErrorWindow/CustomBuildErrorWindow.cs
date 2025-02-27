using UnityEditor;
using UnityEngine;

using System;

public class CustomBuildErrorWindow : EditorWindow
{
    protected static CustomBuildErrorWindow instance;

    private string[] errorTitles;
    private BuildStage failStage;
    private BuildStage lastStage;
    private string errorMessage;

    public Vector2 scrollViewVector = Vector2.zero;

    //Create the custom Editor Window
    public static void CreateCustomBuildErrorWindow()
    {
        instance = (CustomBuildErrorWindow)
            EditorWindow.GetWindowWithRect(
                typeof(CustomBuildErrorWindow),
                new Rect(0, 0, 600, 500),
                false,
                "Custom Build Errors"
            );

        instance.minSize = new Vector2(600, 500);
        instance.ShowUtility();
    }

    void OnEnable()
    {
        instance = this;
    }

    void OnDisable()
    {
        instance = null;
    }

    //public void OnInspectorUpdate()
    //{
    //    // This will only get called 10 times per second.
    //    Repaint();
    //}

    void OnGUI()
    {
        EditorWindow.FocusWindowIfItsOpen(typeof(CustomBuildErrorWindow));
        BuildStage[] allStages = {BuildStage.SETUP_ENV, BuildStage.IDLE,
            BuildStage.UNITY_EXPORT, BuildStage.PROJECT_BUILD, 
            BuildStage.PROJECT_INSTALL, BuildStage.PROJECT_RUN
        };

        LoadErrorEditorPrefbs(allStages);

        if (instance != null)
        {
            instance.ErrorGUI(allStages);
        }

        else
        {
            instance = (CustomBuildErrorWindow)
            EditorWindow.GetWindowWithRect(
                typeof(CustomBuildErrorWindow),
                new Rect(0, 0, 600, 500),
                false,
                "Custom Build Errors"
            );
        }
    }

    protected virtual void ErrorGUI(BuildStage[] allStages)
    {
        float constMul = 6.8F;
        Texture2D success;
        Texture2D fail;

        int height = 10;

        int failStageIndex = ArrayUtility.IndexOf<BuildStage>(allStages,
                                                              failStage);

        int i = 0;
        while (i <= (int) lastStage)
        {
            bool foundImage = false;
            string resultString = "";

            if (i < failStageIndex)
            {
                success = (Texture2D)Resources.Load(
                    "icons/success", 
                    typeof(Texture2D)
                );

                if (success) {
                    GUI.DrawTexture(
                        new Rect(
                            errorTitles[i].Length * constMul,
                            height,
                            20,
                            20
                        ),
                        success
                    );
                    foundImage = true;
                }

                resultString = " SUCCEEDED";
            }

            else
            {
                fail = (Texture2D)Resources.Load(
                    "icons/false", 
                    typeof(Texture2D)
                );

                if (fail) {
                    GUI.DrawTexture(
                        new Rect(
                            errorTitles[i].Length * constMul,
                            height,
                            20,
                            20
                        ),
                        fail
                    );
                    foundImage = true;
                }

                resultString = " FAILED";
            }

            if (!foundImage) {
                if (resultString.Equals(" SUCCEEDED")) {
                    GUI.contentColor = Color.green;
                } 
                else if (resultString.Equals(" FAILED"))
                {
                    GUI.contentColor = Color.red;
                }
                GUI.Label(new Rect(5, height, 590, 20), errorTitles[i] + " " + 
                          resultString);

                GUI.contentColor = Color.black;

            } else {
                GUI.Label(new Rect(5, height, 590, 20), errorTitles[i]);
            }
                
            height += 40;
            i++;
        }

        GUI.Label(new Rect(10, height, 580, 200), "Error:\n" + 
                  errorMessage, GUI.skin.textArea);

        if (GUI.Button(new Rect(530, 470, 60, 20), "Got it"))
        {
            instance.Close();
        }
    }

    protected void LoadErrorEditorPrefbs(BuildStage[] allStages)
    {
        string[] genericErrorTitles = {
            "Setup Android Enviornment: ",
            "(GUI) Chose Custom Build Process: ",
            "Export Unity Project: ",
            "Build Exported Project: ",
            "Install .apk to device: ",
            "Run .apk in the device: "
        };

        failStage = (BuildStage) EditorPrefs.GetInt("appcoins_error_stage", 0);
        lastStage = (BuildStage) 
            EditorPrefs.GetInt("appcoins_last_error_stage", 2);

        errorMessage = EditorPrefs.GetString("appcoins_error_message", "");

        errorTitles = new string[allStages.Length];
        for (int i = 0; i < allStages.Length; i++)
        {
            errorTitles[i] = 
                EditorPrefs.GetString("appcoins_error_title_" + i.ToString(), 
                                      genericErrorTitles[i]);
        }
    }
}