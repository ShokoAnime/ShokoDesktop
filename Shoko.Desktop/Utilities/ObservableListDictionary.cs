using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.OData.Query.SemanticAst;

namespace Shoko.Desktop.Utilities
{
    public class ObservableListDictionary<TKey, TValue>
    {
        public Dictionary<TKey, TValue> dict=new Dictionary<TKey, TValue>();
        public ObservableCollectionEx<TValue> collection=new ObservableCollectionEx<TValue>();

        public TValue this[TKey key]
        {
            get
            {
                return dict[key];
            }
            set
            {
                TValue oldValue;
                if (dict.TryGetValue(key, out oldValue))
                    collection.Remove(oldValue);
                dict[key] = value;
                collection.Add(value);
            }
        }

        public bool TryGetValue(TKey key, out TValue value) => dict.TryGetValue(key, out value);

        public bool ContainsKey(TKey key) => dict.ContainsKey(key);
        public int Count => dict.Count;

        public ObservableCollectionEx<TValue> Values => collection;
        public Dictionary<TKey, TValue>.KeyCollection Keys => dict.Keys;
        public void Add(TKey key, TValue value)
        {
            TValue oldValue;
            if (dict.TryGetValue(key, out oldValue))
                collection.Remove(oldValue);
            dict[key] = value;
            collection.Add(value);
        }

        public bool Remove(TKey key)
        {
            TValue value;
            if (dict.TryGetValue(key, out value))
            {
                collection.Remove(value);
                dict.Remove(key);
                return true;
            }
            return false;
        }

        public void Clear()
        {
            collection.Clear();
            dict.Clear();
        }

    }
}
