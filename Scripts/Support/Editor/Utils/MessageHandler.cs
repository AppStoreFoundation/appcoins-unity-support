public abstract class MessageHandler
{
    public abstract void HandleMessage(string title, string mess, string ok);
    public abstract bool DualOptionWithMessage(string title, string mess, 
                                               string fail, string success);
}