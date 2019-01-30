using UnityEditor;
using UnityEngine;
using System;

public class AppcoinsSettingsWindow : EditorWindow
{
    //Default values for sample to work
    string developerWalletAddress = "0x95cee221da20e0e02bb9c233b0055b1779a7d926";
    //Default values for sample to work
    string developerBDSPublicKey = "MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEAyEt94j9rt0UvpkZ2jPMZZ16yUrBOtjpIQCWi/F3HN0+iwSAeEJyDw7xIKfNTEc0msm+m6ud1kJpLK3oCsK61syZ8bYQlNZkUxTaWNof1nMnbw3Xu5nuYMuowmzDqNMWg5jNooy6oxwIgVcdvbyGi5RIlxqbo2vSAwpbAAZE2HbUrysKhLME7IOrdRR8MQbSbKEy/9MtfKz0uZCJGi9h+dQb0b69H7Yo+/BN/ayBSJzOPlaqmiHK5lZsnZhK+ixpB883fr+PgSczU7qGoktqoe6Fs+nhk9bLElljCs5ZIl9/NmOSteipkbplhqLY7KwapDmhrtBgrTetmnW9PU/eCWQIDAQAB";
    bool shouldLog;
    bool useUserAcquistionSDK = true;

    private void Awake()
    {
        string storeValue = EditorPrefs.GetString(AppcoinsConstants.APPCOINS_WALLET_ADDRESS_KEY);
        string storedValue = EditorPrefs.GetString(AppcoinsConstants.APPCOINS_PUBLIC_KEY_KEY);

        //Debug.Log("stored wallet address is " + storeValue + "\n stored developer key is " + storedValue);
    }

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

        //Debug.Log("SAVED wallet address is " + developerWalletAddress + "\n SAVED developer key is " + developerBDSPublicKey);
    }

}