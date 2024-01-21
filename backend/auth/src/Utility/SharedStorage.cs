namespace Utility;

public class SharedStorage : ISharedStorage
{
    private object _value;
    private object _lock = new object();
    public SharedStorage(object value)
    {
        this._value = value;
    }
    public object GetValue()
    {
        lock (_lock)
        {
            return _value;
        }
    }
    public void SetValue(object value)
    {
        lock (_lock)
        {
            _value = value;
        }
    }
}