using Microsoft.AspNetCore.Http;
using System.IO;
using System.Threading.Tasks;

namespace XTI_WebApp.Extensions
{
    public sealed class JsonFromRequest
    {
        private readonly HttpRequest request;

        public JsonFromRequest(HttpRequest request)
        {
            this.request = request;
        }

        public async Task<string> Serialize()
        {
            request.EnableBuffering();
            var streamReader = new StreamReader(request.Body);
            var body = await streamReader.ReadToEndAsync();
            request.Body.Position = 0;
            return body;
        }
    }
}
