using System.Threading.Tasks;

namespace XTI_WebAppClient
{
    public interface IXtiToken
    {
        void Reset();
        Task<string> Value();
    }
}