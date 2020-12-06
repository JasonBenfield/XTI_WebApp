using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace XTI_WebAppClient
{
    public class AppClientGroup
    {
        private readonly IHttpClientFactory httpClientFactory;
        private readonly XtiToken xtiToken;
        private readonly string baseUrl;
        private readonly string name;

        protected AppClientGroup(IHttpClientFactory httpClientFactory, XtiToken xtiToken, string baseUrl, string name)
        {
            this.httpClientFactory = httpClientFactory;
            this.xtiToken = xtiToken;
            this.baseUrl = baseUrl;
            this.name = name;
        }

        protected async Task<TResult> Post<TResult, TModel>(string action, string modifier, TModel model)
        {
            using var client = httpClientFactory.CreateClient();
            if (!action.Equals("Authenticate", StringComparison.OrdinalIgnoreCase))
            {
                var token = await xtiToken.Value();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
            var url = $"{baseUrl}/{name}/{action}";
            if (!string.IsNullOrWhiteSpace(modifier))
            {
                url = $"{url}/{modifier}";
            }
            var serialized = JsonSerializer.Serialize(model);
            var content = new StringContent(serialized, Encoding.UTF8, "application/json");
            var response = await client.PostAsync(url, content);
            var responseContent = await response.Content.ReadAsStringAsync();
            TResult result;
            try
            {
                if (response.IsSuccessStatusCode)
                {
                    var resultContainer = JsonSerializer.Deserialize<ResultContainer<TResult>>(responseContent);
                    result = resultContainer.Data;
                }
                else
                {
                    var resultContainer = string.IsNullOrWhiteSpace(responseContent)
                        ? new ResultContainer<ErrorModel[]>() { Data = new ErrorModel[] { } }
                        : JsonSerializer.Deserialize<ResultContainer<ErrorModel[]>>(responseContent);
                    throw new AppClientException(url, response.StatusCode, resultContainer.Data);
                }
            }
            catch (JsonException ex)
            {
                throw new AppClientException(url, response.StatusCode, new ErrorModel[] { new ErrorModel { Message = ex.Message } });
            }
            return result;
        }

    }
}
