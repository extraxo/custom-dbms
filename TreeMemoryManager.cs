using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace KursovaSAAConsole2
{
    public class TreeMemoryManager<Key,Value> : TreeManager<Key,Value>
    {
        readonly CustomDictionary<uint,TreeNode<Key,Value>> _nodes = new CustomDictionary<uint, TreeNode<Key, Value>>();
        readonly ushort _minEntriesCountPerNode;
        readonly IComparer<Key> _keyComparer;
        readonly IComparer<CustomTuple<Key, Value>> _entryComparer;
        int idCounter = 1;
        TreeNode<Key, Value> _root;

        public IComparer<CustomTuple<Key,Value>> EntryComparer
        {
            get
            {
                return _entryComparer;
            }
        }
        public ushort MinEntriesPerNode
        {
            get
            {
                return _minEntriesCountPerNode;
            }
        }
        public IComparer<Key> KeyComparer
        {
            get
            {
                return _keyComparer;
            }
        }
        public TreeNode<Key, Value> Root
        {
            get
            {
                return _root;
            }
        }

        public TreeMemoryManager(ushort minEntriesCountPerNode, IComparer<Key> keyComparer)
        {
            _keyComparer = keyComparer;
            _entryComparer = Comparer<CustomTuple<Key, Value>>.Create((t1, t2) =>
            {
                if (t1 == null && t2 == null) return 0;
                if (t1 == null) return -1;
                if (t2 == null) return 1;

                return _keyComparer.Compare(t1.Item1, t2.Item1);
            });
            _minEntriesCountPerNode = minEntriesCountPerNode;
            _root = Create(null, null);
        }

        public TreeNode<Key,Value> Create(IEnumerable<CustomTuple<Key,Value>> entries, IEnumerable<uint> childrenIds)
        {
            var newNode = new TreeNode<Key, Value>(this, (uint)(idCounter++), 0, entries, childrenIds);
            _nodes[newNode.Id]= newNode;
            return newNode;
        }
        public TreeNode<Key,Value> Find(uint id)
        {
            return _nodes[id];
        }

        public TreeNode<Key, Value> CreateNewRoot(Key key, Value value, uint leftNodeId, uint rightNodeId)
        {
            var newNode = Create(new CustomTuple<Key, Value>[]
            {
                new CustomTuple<Key,Value>(key, value) },new uint[] {leftNodeId, rightNodeId});

            _root = newNode;
            return newNode;
        }
        public void Delete(TreeNode<Key, Value> node)
        {
            if (node == _root)
            {
                _root = null;
            }
            if (_nodes.ContainsKey(node.Id))
            {
                _nodes.Remove(node.Id);
            }
        }

        public void MakeRoot(TreeNode<Key,Value> node)
        {
            _root = node;
        }

        public void MarkAsChanged(TreeNode<Key, Value> node)
        {

        }

        public void SaveChanges()
        {

        }
    }
}
