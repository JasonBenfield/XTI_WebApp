// Generated Code
using XTI_WebAppClient;
using System.Net.Http;
using System.Threading.Tasks;

namespace FakeWebAppClient
{
    public sealed class ProductGroup : AppClientGroup
    {
        public ProductGroup(IHttpClientFactory httpClientFactory, XtiToken xtiToken, string baseUrl): base(httpClientFactory, xtiToken, baseUrl, "Product")
        {
        }

        public Task<string> GetInfo() => Post<string, EmptyRequest>("GetInfo", new EmptyRequest());
        public Task<int> AddProduct(AddProductModel model) => Post<int, AddProductModel>("AddProduct", model);
        public Task<Product> Product(int model) => Post<Product, int>("Product", model);
    }
}