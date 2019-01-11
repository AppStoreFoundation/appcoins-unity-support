using UnityEditor;
using UnityEngine;
using System;

public class ParameterWindow : EditorWindow
{
    string developerWalletAddress;
    string developerBDSPublicKey;
    bool shouldLog;
    bool useAdsSDK = true;
    bool groupEnabled;
    //float myFloat = 1.23f;

    void OnGUI()
    {
        GUILayout.Label("Base Settings", EditorStyles.boldLabel);
        developerWalletAddress = EditorGUILayout.TextField("Developer Wallet Adress", "");
        developerBDSPublicKey = EditorGUILayout.TextField("Developer BDS Public Key", "");


        shouldLog = EditorGUILayout.Toggle("Should Log", shouldLog);
        useAdsSDK = EditorGUILayout.Toggle("Use Ads SDK", useAdsSDK);
        EditorGUILayout.EndToggleGroup();
        //myFloat = EditorGUILayout.Slider("Slider", myFloat, -3, 3);
        groupEnabled = EditorGUILayout.BeginToggleGroup("Optional Settings", groupEnabled);

    }
}