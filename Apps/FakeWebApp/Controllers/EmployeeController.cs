// Generated Code
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using XTI_WebApp.Api;
using FakeWebApp.Api;

namespace FakeWebApp.ApiControllers
{
    [AllowAnonymous]
    public class EmployeeController : Controller
    {
        public EmployeeController(FakeAppApi api)
        {
            this.api = api;
        }

        private readonly FakeAppApi api;
        public async Task<IActionResult> Index()
        {
            var result = await api.Group("Employee").Action<EmptyRequest, AppActionViewResult>("Index").Execute(new EmptyRequest());
            return View(result.Data.ViewName);
        }

        [HttpPost]
        public Task<ResultContainer<int>> AddEmployee(AddEmployeeModel model)
        {
            return api.Group("Employee").Action<AddEmployeeModel, int>("AddEmployee").Execute(model);
        }

        [HttpPost]
        public Task<ResultContainer<Employee>> Employee(int model)
        {
            return api.Group("Employee").Action<int, Employee>("Employee").Execute(model);
        }
    }
}