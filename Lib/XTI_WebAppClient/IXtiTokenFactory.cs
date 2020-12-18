namespace XTI_WebAppClient
{
    public interface IXtiTokenFactory
    {
        IXtiToken Create(IAuthClient authClient);
    }
}
