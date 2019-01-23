using UnityEditor;
using UnityEngine;
using System;

public class AppcoinsSettingsWindow : EditorWindow
{
    string developerWalletAddress;
    string developerBDSPublicKey;
    bool shouldLog;
    bool useUserAcquistionSDK = true;
    bool groupEnabled;
    //float myFloat = 1.23f;

    void OnGUI()
    {
        GUILayout.Label("Base Settings", EditorStyles.boldLabel);
        developerWalletAddress = EditorGUILayout.TextField("Developer Wallet Adress", 
                                                           EditorPrefs.GetString(AppcoinsConstants.APPCOINS_WALLET_ADDRESS_KEY, developerWalletAddress));
        developerBDSPublicKey = EditorGUILayout.TextField("Developer BDS Public Key", 
                                                          EditorPrefs.GetString(AppcoinsConstants.APPCOINS_PUBLIC_KEY_KEY, developerBDSPublicKey));


        shouldLog = EditorGUILayout.Toggle("Should Log", EditorPrefs.GetBool(AppcoinsConstants.APPCOINS_SHOULD_LOG_KEY, shouldLog));
        useUserAcquistionSDK = EditorGUILayout.Toggle("Use User Acquisition SDK", EditorPrefs.GetBool(AppcoinsConstants.APPCOINS_USE_UA_KEY, useUserAcquistionSDK));

        UpdateValues();
    }

    void UpdateValues() {
        EditorPrefs.SetString(AppcoinsConstants.APPCOINS_WALLET_ADDRESS_KEY, developerWalletAddress);
        EditorPrefs.SetString(AppcoinsConstants.APPCOINS_PUBLIC_KEY_KEY, developerBDSPublicKey);

        EditorPrefs.SetBool(AppcoinsConstants.APPCOINS_SHOULD_LOG_KEY, shouldLog);
        EditorPrefs.SetBool(AppcoinsConstants.APPCOINS_USE_UA_KEY, useUserAcquistionSDK);   
    }

    void OnValidate() {
        UpdateValues();
    }
}