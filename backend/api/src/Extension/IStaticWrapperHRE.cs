namespace Extension;

public interface IStaticWrapperHRE {
    Task WriteAsync(HttpResponse response, string message);
}