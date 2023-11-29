using System;
using MQTTnet.Client;

namespace Module.Accountant;

public class CommitService: IDisposable {

    private readonly IMqttClient _mqttClient;

    public CommitService(IMqttClient mqttClient) {
        _mqttClient = mqttClient;
    }

    public async Task<int> Routine(CancellationToken ct) {
        return 0;
    }

    public void Dispose() {
        _mqttClient?.Dispose();
    }
}