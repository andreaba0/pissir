namespace Utility;

public class Registry<T> {
    private readonly Dictionary<string, List<T>> _dict;
    public Registry() {
        _dict = new Dictionary<string, List<T>>();
    }

    public void Add(string key, T value) {
        //add value to dict[key] only if value is not already in dict[key]
        if(!_dict.ContainsKey(key)) {
            _dict.Add(key, new List<T>());
        }
        if(!_dict[key].Contains(value)) {
            _dict[key].Add(value);
        }
    }

    public T[] Get(string key) {
        if(!_dict.ContainsKey(key)) {
            return new T[0];
        }
        return _dict[key].ToArray();
    }

    public bool ContainsValue(string key, T value) {
        if(!_dict.ContainsKey(key)) {
            return false;
        }
        return _dict[key].Contains(value);
    }
}