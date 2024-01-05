namespace Interface.Utility;

public interface IFetch {
    public Task<HttpResponseMessage?> Get(string url);
}