public interface IPubSubClient {
    public Task<int> Routine(CancellationToken ct);
}