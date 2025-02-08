using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KursovaSAAConsole2
{
    public class TreeTraverser<Key,Value> : IEnumerable<CustomTuple<Key,Value>>
    {
        readonly TreeNode<Key, Value> _node;
        readonly int _index;
        readonly TreeTraverseDirection _direction;
        readonly TreeManager<Key, Value> _manager;

        public TreeTraverser(TreeManager<Key,Value> nodeManager, TreeNode<Key,Value> fromNode, int fromIndex, TreeTraverseDirection direction)
        {
            _manager = nodeManager;
            _node = fromNode;
            _index = fromIndex;
            _direction = direction;
        }

        IEnumerator<CustomTuple<Key, Value>> IEnumerable<CustomTuple<Key, Value>>.GetEnumerator()
        {
            return new TreeEnumerator<Key,Value>(_manager, _node, _index,_direction);
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<CustomTuple<Key, Value>>)this).GetEnumerator();
        }
    }
}
