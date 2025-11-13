using BlazorApp.Services.Revit;

namespace BlazorApp.Services.JSInterop
{
    public interface IJSInteropService
    {
        Task AlertAsync(string message);

        Task<string> SendMessageToHostAsync(string message);

        Task<TResponse> ExecuteAsync<TRequest, TResponse>(RevitAction<TRequest, TResponse> revitAction);
    }
}
