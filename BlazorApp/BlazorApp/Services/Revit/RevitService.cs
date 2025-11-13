using BlazorApp.Contracts.Revit.Documents;
using BlazorApp.Contracts.Revit.Documents.GetActiveDocument;
using BlazorApp.Services.JSInterop;

namespace BlazorApp.Services.Revit
{
    public class RevitService(IJSInteropService jsInteropService) : IRevitService
    {
        public async Task<Document> GetActiveDocumentAsync()
        {
            var revitAction = new RevitAction<GetActiveDocumentRequest, GetActiveDocumentResponse>
            {
                MethodName = "GetActiveDocument",
                Request = new GetActiveDocumentRequest()
            };

            var response = await jsInteropService.ExecuteAsync(revitAction);

            return response.ActiveDocument;
        }
    }
}
