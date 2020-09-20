namespace XTI_WebApp.Api
{
    public class FriendlyNameFromActionName
    {
        public FriendlyNameFromActionName(string actionName)
        {
            Value = actionName;
        }

        public string Value { get; }
    }
}
