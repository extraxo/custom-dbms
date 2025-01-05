using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace KursovaSAAConsole2
{
    public class TreeDiskManager<Key, Value> : TreeManager<Key, Value>
    {
        readonly IRecordStorage _recordStorage;
        readonly CustomDictionary<uint, TreeNode<Key, Value>> fullNodes = new CustomDictionary<uint, TreeNode<Key, Value>>();
        readonly CustomDictionary<uint, WeakReference<TreeNode<Key, Value>>> nodeWeakRefs = new CustomDictionary<uint, WeakReference<TreeNode<Key, Value>>>();
        readonly CustomQueue<TreeNode<Key, Value>> nodeStrongRefs = new CustomQueue<TreeNode<Key, Value>>();
        readonly int MaxStrongNodeRefs = 200;
        readonly TreeDiskSerializer<Key, Value> _serializer;
        readonly ushort _minEntriesPerNode = 36;

        TreeNode<Key, Value> _root;
        int _cleanUpCounter = 0;

        public ushort MinEntriesPerNode
        {
            get
            {
                return _minEntriesPerNode;
            }
        }

        public IComparer<CustomTuple<Key, Value>> EntryComparer
        {
            get;
            private set;
        }

        public IComparer<Key> KeyComparer
        {
            get;
            private set;
        }

        public TreeNode<Key, Value> Root
        {
            get
            {
                return _root;
            }
        }

        public TreeDiskManager(ISerializer<Key> keySerializer
             , ISerializer<Value> valueSerializer
             , IRecordStorage nodeStorage)
             : this(keySerializer, valueSerializer, nodeStorage, Comparer<Key>.Default)
        {
        }

        public TreeDiskManager(ISerializer<Key> keySerializer, ISerializer<Value> valueSerializer, IRecordStorage recordStorage, IComparer<Key> keyComparer)
        {
            _recordStorage = recordStorage;
            _serializer = new TreeDiskSerializer<Key, Value>(this, keySerializer, valueSerializer);
            this.KeyComparer = keyComparer;
            EntryComparer = Comparer<CustomTuple<Key, Value>>.Create((a, b) =>
            {
                return KeyComparer.Compare(a.Item1, b.Item1);
            });

            var firstBlockData = recordStorage.Find(1u);
            if (firstBlockData != null)
            {
                _root = Find(BufferReader.ReadBufferUInt32(firstBlockData, 0));
            }
            else
            {
                _root = CreateFirstRoot();
            }
        }

        public TreeNode<Key, Value> Create(IEnumerable<CustomTuple<Key, Value>> items, IEnumerable<uint> childrenIds)
        {
            TreeNode<Key, Value> node = null;

            _recordStorage.Create(nodeId =>
            {
                node = new TreeNode<Key, Value>(this, nodeId, 0, items, childrenIds);

                NodeInitialized(node);

                return _serializer.Serialize(node);
            });

            return node;
        }

        public TreeNode<Key, Value> Find(uint id)
        {
            if (nodeWeakRefs.ContainsKey(id))
            {
                TreeNode<Key, Value> node;
                if (nodeWeakRefs[id].TryGetTarget(out node))
                {
                    return node;
                }
                else
                {
                    nodeWeakRefs.Remove(id);
                }
            }

            var data = _recordStorage.Find(id);
            if (data == null)
            {
                return null;
            }

            var dNode = _serializer.Deserialize(id, data);

            NodeInitialized(dNode);
            return dNode;
        }

        public TreeNode<Key, Value> CreateNewRoot(Key key, Value value, uint leftNodeId, uint rightNodeId)
        {
            var node = Create(new CustomTuple<Key, Value>[]
            {
                new CustomTuple<Key,Value>(key, value)
            }, new uint[]
            {
                leftNodeId, rightNodeId
            });

            _root = node;
            _recordStorage.Update(1u, LittleEndianByteOrder.GetBytes(node.Id));

            return _root;
        }

        public void MakeRoot(TreeNode<Key, Value> node)
        {
            _root = node;
            _recordStorage.Update(1u, LittleEndianByteOrder.GetBytes(_root.Id));
        }

        public void Delete(TreeNode<Key, Value> node)
        {
            _recordStorage.Delete(node.Id);

            if (fullNodes.ContainsKey(node.Id))
            {
                fullNodes.Remove(node.Id);
            }
        }

        public void MarkAsChanged(TreeNode<Key, Value> node)
        {
            if (false == fullNodes.ContainsKey(node.Id))
            {
                fullNodes.Add(node.Id, node);
            }
        }

        public void SaveChanges()
        {
            foreach (var kv in fullNodes)
            {
                _recordStorage.Update(kv.Value.Id, _serializer.Serialize(kv.Value));
            }
            fullNodes.Clear();
        }

        TreeNode<Key, Value> CreateFirstRoot()
        {
            _recordStorage.Create(LittleEndianByteOrder.GetBytes((uint)2));
            return Create(null, null);
        }

        void NodeInitialized(TreeNode<Key, Value> node)
        {
            nodeWeakRefs.Add(node.Id, new WeakReference<TreeNode<Key, Value>>(node));
            nodeStrongRefs.Enqueue(node);

            if (nodeStrongRefs.Count >= MaxStrongNodeRefs)
            {
                while (nodeStrongRefs.Count >= (MaxStrongNodeRefs / 2f))
                {
                    nodeStrongRefs.Dequeue();
                }
            }

            if (_cleanUpCounter++ >= 1000)
            {
                _cleanUpCounter = 0;
                var tobeDeleted = new CustomList<uint>();
                foreach (var kv in this.nodeWeakRefs)
                {
                    TreeNode<Key, Value> target;
                    if (false == kv.Value.TryGetTarget(out target))
                    {
                        tobeDeleted.Add(kv.Key);
                    }
                }

                foreach (var key in tobeDeleted)
                {
                    this.nodeWeakRefs.Remove(key);
                }
            }
        }
    }
}
