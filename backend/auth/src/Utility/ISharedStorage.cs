namespace Utility;

public interface ISharedStorage {
    object GetValue();
    void SetValue(object value);
}