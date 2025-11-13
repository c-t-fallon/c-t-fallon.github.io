namespace BlazorApp.Services.Revit
{
    public class RevitAction<TRequest, TResponse>
    {
        public string MethodName { get; set; }

        public TRequest Request { get; set; }
    }
}