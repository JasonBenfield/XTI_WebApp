// Generated Code
using XTI_WebAppClient;
using System.Net.Http;
using System.Threading.Tasks;

namespace FakeWebAppClient
{
    public sealed class EmployeeGroup : AppClientGroup
    {
        public EmployeeGroup(IHttpClientFactory httpClientFactory, XtiToken xtiToken, string baseUrl): base(httpClientFactory, xtiToken, baseUrl, "Employee")
        {
        }

        public Task<int> AddEmployee(AddEmployeeModel model) => Post<int, AddEmployeeModel>("AddEmployee", model);
        public Task<Employee> Employee(int model) => Post<Employee, int>("Employee", model);
    }
}