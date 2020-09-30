// Generated Code
using XTI_WebAppClient;
using System.Net.Http;

namespace FakeWebAppClient
{
    public sealed class FakeAppClient : AppClient
    {
        public FakeAppClient(IHttpClientFactory httpClientFactory, XtiToken xtiToken, string baseUrl, string version = "V7"): base(httpClientFactory, baseUrl, "Fake", version)
        {
            this.xtiToken = xtiToken;
            Employee = new EmployeeGroup(httpClientFactory, xtiToken, url);
            Product = new ProductGroup(httpClientFactory, xtiToken, url);
        }

        public EmployeeGroup Employee
        {
            get;
        }

        public ProductGroup Product
        {
            get;
        }
    }
}