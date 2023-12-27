namespace MQTTConcurrent;

public class MqttConcurrentException : Exception
{
    public MqttConcurrentException() : base() { }
    public MqttConcurrentException(string message) : base(message) { }
    public MqttConcurrentException(string message, Exception innerException) : base(message, innerException) { }
}