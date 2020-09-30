// Generated Code
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using XTI_WebApp.Api;
using FakeWebApp.Api;

namespace FakeWebApp.ApiControllers
{
    [AllowAnonymous]
    public class ProductController : Controller
    {
        public ProductController(FakeAppApi api)
        {
            this.api = api;
        }

        private readonly FakeAppApi api;
        public async Task<IActionResult> Index()
        {
            var result = await api.Group("Product").Action<EmptyRequest, AppActionViewResult>("Index").Execute(new EmptyRequest());
            return View(result.Data.ViewName);
        }

        [HttpPost]
        public Task<ResultContainer<string>> GetInfo()
        {
            return api.Group("Product").Action<EmptyRequest, string>("GetInfo").Execute(new EmptyRequest());
        }

        [HttpPost]
        public Task<ResultContainer<int>> AddProduct(AddProductModel model)
        {
            return api.Group("Product").Action<AddProductModel, int>("AddProduct").Execute(model);
        }

        [HttpPost]
        public Task<ResultContainer<Product>> Product(int model)
        {
            return api.Group("Product").Action<int, Product>("Product").Execute(model);
        }
    }
}