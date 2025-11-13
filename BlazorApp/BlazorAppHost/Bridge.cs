using BlazorApp.Contracts.Revit;
using BlazorApp.Contracts.Revit.Documents;
using BlazorApp.Contracts.Revit.Documents.GetActiveDocument;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace BlazorAppHost
{
    [ClassInterface(ClassInterfaceType.AutoDual)]
    [ComVisible(true)]
    public class Bridge
    {
        public string ReceiveMessage(string content)
        {
            var revitRequest = JsonSerializer.Deserialize<RevitRequest>(content);

            var request = JsonSerializer.Deserialize<GetActiveDocumentResponse>(revitRequest.Content);

            var response = new GetActiveDocumentResponse()
            {
                ActiveDocument = new Document()
                {
                    Title = "$DocumentTitle"
                }
            };

            var revitResponse = new RevitResponse()
            {
                StatusCode = 200,
                Value = JsonSerializer.Serialize(response)
            };

            return JsonSerializer.Serialize(revitResponse);
        }
    }
}
