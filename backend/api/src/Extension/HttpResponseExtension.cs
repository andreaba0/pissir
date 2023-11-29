namespace Extension;

public class HttpResponseExtension : IStaticWrapperHRE {
    public async Task WriteAsync(HttpResponse response, string message) {
        await response.WriteAsync(message);
    }
}