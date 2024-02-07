using System.Threading.Tasks;
using System.Threading.Channels;

namespace MQTTConcurrent;

public interface IMQTTnetConcurrent
{
    public Task RunAsync(CancellationToken ct);
}