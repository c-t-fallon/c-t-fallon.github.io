using BlazorApp.Contracts.Revit.Documents;

namespace BlazorApp.Services.Revit
{
    public interface IRevitService
    {
        Task<Document> GetActiveDocumentAsync();
    }
}