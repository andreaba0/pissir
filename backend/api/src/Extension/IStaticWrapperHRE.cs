namespace Extension;

public interface IStaticWrapperHRE {
    public Task WriteAsync(HttpResponse response, string message);
}