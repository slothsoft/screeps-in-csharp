using System;
using System.Collections.Generic;

namespace FriendlyWorldBot.Utils;

public static class CollectionUtil {
    
    public static TValue GetOrCreate<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key, Func<TKey, TValue> creator) {
        if (dict.TryGetValue(key, out var value)) {
            return value;
        }
        value = creator(key);
        dict.Add(key, value);
        return value;
    }
}