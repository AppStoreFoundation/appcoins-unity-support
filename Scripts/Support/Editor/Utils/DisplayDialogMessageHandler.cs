using UnityEditor;

public class DisplayDialogMessageHandler : MessageHandler
{
    public override void HandleMessage(string title, string mess, string ok)
    {
        EditorUtility.DisplayDialog(title, mess, ok);
    }

    public override bool DualOptionWithMessage(string title, string mess, 
                                               string fail, string success)
    {
        return EditorUtility.DisplayDialog(title, mess, success, fail);
    }
}