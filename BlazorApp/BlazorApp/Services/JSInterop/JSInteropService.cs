using BlazorApp.Contracts.Revit;
using BlazorApp.Services.Revit;
using Microsoft.JSInterop;
using System.Text.Json;

namespace BlazorApp.Services.JSInterop
{
    public class JSInteropService(IJSRuntime jsRuntime) : IJSInteropService
    {
        public async Task AlertAsync(string message)
        {
            await jsRuntime.InvokeVoidAsync("alert", message);
        }

        public async Task<string> SendMessageToHostAsync(string message)
        {
            return await jsRuntime.InvokeAsync<string>("sendMessageToHost", message);
        }

        public async Task<TResponse> ExecuteAsync<TRequest, TResponse>(RevitAction<TRequest, TResponse> revitAction)
        {
            var revitRequest = new RevitRequest()
            {
                MethodName = revitAction.MethodName,
                Content = JsonSerializer.Serialize(revitAction.Request)
            };

            var request = JsonSerializer.Serialize(revitRequest);
            var response = await SendMessageToHostAsync(request);

            var revitResponse = JsonSerializer.Deserialize<RevitResponse>(response);

            return JsonSerializer.Deserialize<TResponse>(revitResponse.Value);
        }
    }
}
