using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KursovaSAAConsole2
{
    public interface Index<Key,Value>
    {
        void Insert(Key key, Value value);

        CustomTuple<Key, Value> Get(Key key);

        IEnumerable<CustomTuple<Key,Value>> LargerOrEqual(Key key);

        IEnumerable<CustomTuple<Key,Value>> LargerThan(Key key);

        IEnumerable<CustomTuple<Key,Value>> LessOrEqual(Key key);

        IEnumerable<CustomTuple<Key,Value>> LessThan(Key key);

        bool Delete(Key key, Value value, IComparer<Value> valueComparer = null);

        bool Delete(Key key);
    }
}
