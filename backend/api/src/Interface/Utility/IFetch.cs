namespace Interface.Utility;

public interface IFetch {
    public Task<FetchResponse> Get(string url);
}