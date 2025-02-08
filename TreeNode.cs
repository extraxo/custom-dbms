using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KursovaSAAConsole2
{
    public class TreeNode<Key,Value>
    {
        protected uint _id = 0;
        protected uint _parentId;
        protected readonly TreeManager<Key, Value> _nodeManager;
        private readonly CustomList<uint> _childrenIds;
        private readonly CustomList<CustomTuple<Key, Value>> _entries;

        public Key MaxKey => _entries[_entries.Count - 1].Item1;
        public Key MinKey => _entries[0].Item1;
        public bool IsEmpty => _entries.Count == 0;
        public bool IsLeaf => _childrenIds.Count == 0;
        public bool IsOverflow => _entries.Count > (_nodeManager.MinEntriesPerNode * 2);
        public int EntriesCount => _entries.Count;
        public int ChildrenNodeCount => _childrenIds.Count;
        public uint ParentId => _parentId;
        public uint[] ChildrenIds => _childrenIds.ToArray();
        public CustomTuple<Key, Value>[] Entries => _entries.ToArray();
        public uint Id => _id;


        public TreeNode(TreeManager<Key, Value> nodeManager, uint id, uint parentId, IEnumerable<CustomTuple<Key, Value>> entries = null, IEnumerable<uint> childrenIds = null)
        {
            _id = id;
            _parentId = parentId;
            _nodeManager = nodeManager;
            _entries = new CustomList<CustomTuple<Key, Value>>(_nodeManager.MinEntriesPerNode * 2);

            if (entries != null)
            {
                foreach (var entry in entries)
                {
                    _entries.Add(entry);
                }
            }

            _childrenIds = new CustomList<uint>();

            if (childrenIds != null)
            {
                _childrenIds.AddRange(childrenIds);
            }
        }

        public void Remove(int RemoveAt)
        {
            if (IsLeaf)
            {
                _entries.RemoveAt(RemoveAt);
                _nodeManager.MarkAsChanged(this);

                if ((EntriesCount >= _nodeManager.MinEntriesPerNode) || (_parentId == 0))
                {
                    return;
                }
                else
                {
                    Rebalance();
                }
            }
            else
            {
                var leftSubTree = _nodeManager.Find(_childrenIds[RemoveAt]);
                TreeNode<Key, Value> largestNode;
                int largestIndex;
                leftSubTree.FindLargest(out largestNode, out largestIndex);
                var replacement = largestNode.GetEntry(largestIndex);

                _entries[RemoveAt] = replacement;
                _nodeManager.MarkAsChanged(this);
                largestNode.Remove(largestIndex);
            }
        }

        public int IndexInParent()
        {
            var parent = _nodeManager.Find(_parentId);
            var childrenIds = parent._childrenIds;
            for(var i = 0; i < childrenIds.Length; i++)
            {
                if (childrenIds[i] == _id)
                {
                    return i;
                }
            }

            throw new Exception("Failed to find index of node " + _id + " in its parent");
        }

        public void FindLargest(out TreeNode<Key,Value> node, out int index)
        {
            if (IsLeaf)
            {
                node = this;
                index = _entries.Count - 1;
                return;
            }
            else
            {
                var RightMost = _nodeManager.Find(_childrenIds[_childrenIds.Count - 1]);
                RightMost.FindLargest(out node, out index);
            }
        }

        public void FindSmallest(out TreeNode<Key, Value> node, out int index)
        {
            if (IsLeaf)
            {
                node = this;
                index = 0;
                return;
            }
            else
            {
                var leftMostNode = _nodeManager.Find(_childrenIds[0]);
                leftMostNode.FindSmallest(out node, out index);
            }
        }

        public void InsertAsLeaf(Key key, Value value, int insertPosition)
        {
            
            var entry = new CustomTuple<Key, Value>(key, value);
            _entries.Insert(insertPosition, entry);
            _nodeManager.MarkAsChanged(this);

        }

        public void InsertAsParent(Key key, Value value, uint leftReference, uint rightReference, out int insertPosition)
        {

            insertPosition = BinarySearchEntriesForKey(key);
            
            if (insertPosition < 0)
            {
                insertPosition = ~insertPosition;
            }

            _entries.Insert(insertPosition, new CustomTuple<Key, Value>(key, value));

            _childrenIds.Insert(insertPosition, leftReference);
            _childrenIds[insertPosition + 1] = rightReference;

            _nodeManager.MarkAsChanged(this);
        }

        public void Split(out TreeNode<Key, Value> leftNode, out TreeNode<Key, Value> rightNode)
        {
            int half = _nodeManager.MinEntriesPerNode;
            var middleEntry = _entries[half];

            var rightEntries = new CustomTuple<Key, Value>[half];
            uint[] rightChildren = null;

            _entries.CopyTo(half + 1, rightEntries, 0, rightEntries.Length);

            if (!IsLeaf)
            {
                rightChildren = new uint[half + 1];
                _childrenIds.CopyTo(half + 1, rightChildren, 0, rightChildren.Length);
            }

            rightNode = _nodeManager.Create(rightEntries, rightChildren);

            if (!IsLeaf)
            {
                foreach (var childId in rightChildren)
                {
                    var childNode = _nodeManager.Find(childId);
                    childNode._parentId = rightNode.Id;
                }
            }

            _entries.RemoveRange(half, _entries.Count - half);

            if (_parentId == 0)
            {
                leftNode = this;
                var parent = _nodeManager.CreateNewRoot(middleEntry.Item1, middleEntry.Item2, _id, rightNode.Id);
                _parentId = parent.Id;
                rightNode._parentId = parent.Id;
            }
            else
            {
                var parent = _nodeManager.Find(_parentId);
                parent.InsertAsParent(middleEntry.Item1, middleEntry.Item2, _id, rightNode.Id, out _);

                if (parent.IsOverflow)
                {
                    TreeNode<Key, Value> newLeft, newRight;
                    parent.Split(out newLeft, out newRight);
                }

                leftNode = this;
            }

        }
        public int BinarySearchEntriesForKey(Key key)
        {

            return _entries.BinarySearch(new CustomTuple<Key, Value>(key, default), _nodeManager.EntryComparer);
        }

        public int BinarySearchEntriesForKey(Key key, bool firstOccurence)
        {
            if (firstOccurence)
            {
                return _entries.BinarySearchFirst(new CustomTuple<Key, Value>(key, default(Value)), _nodeManager.EntryComparer);
            }
            else
            {
                return _entries.BinarySearchLast(new CustomTuple<Key, Value>(key, default(Value)), _nodeManager.EntryComparer);
            }
        }
        public TreeNode<Key, Value> GetChildNode(int atIndex)
        {
            return _nodeManager.Find(_childrenIds[atIndex]);
        }
        public CustomTuple<Key, Value> GetEntry(int atIndex)
        {
            return _entries[atIndex];
        }
        void Rebalance()
        {
            var indexInParent = IndexInParent();
            var parent = _nodeManager.Find(_parentId);
            var rightSibling = ((indexInParent + 1) < parent.ChildrenNodeCount) ? parent.GetChildNode(indexInParent + 1) : null;
            if ((rightSibling != null) && (rightSibling.EntriesCount > _nodeManager.MinEntriesPerNode))
            {
                _entries.Add(parent.GetEntry(indexInParent));

                parent._entries[indexInParent] = rightSibling._entries[0];
                rightSibling._entries.RemoveAt(0);

                if (false == rightSibling.IsLeaf)
                {
                    var n = _nodeManager.Find(rightSibling._childrenIds[0]);
                    n._parentId = _id;
                    _nodeManager.MarkAsChanged(n);
                    _childrenIds.Add(rightSibling._childrenIds[0]);
                    rightSibling._childrenIds.RemoveAt(0);
                }

                _nodeManager.MarkAsChanged(this);
                _nodeManager.MarkAsChanged(parent);
                _nodeManager.MarkAsChanged(rightSibling);
                return;
            }

            var leftSibling = ((indexInParent - 1) >= 0) ? parent.GetChildNode(indexInParent - 1) : null;
            if ((leftSibling != null) && (leftSibling.EntriesCount > _nodeManager.MinEntriesPerNode))
            {
                _entries.Insert(0, parent.GetEntry(indexInParent - 1));
                parent._entries[indexInParent - 1] = leftSibling._entries[leftSibling._entries.Count - 1];
                leftSibling._entries.RemoveAt(leftSibling._entries.Count - 1);

                if (false == IsLeaf)
                {
                    var n = _nodeManager.Find(leftSibling._childrenIds[leftSibling._childrenIds.Count - 1]);
                    n._parentId = _id;
                    _nodeManager.MarkAsChanged(n);
                    _childrenIds.Insert(0, leftSibling._childrenIds[leftSibling._childrenIds.Count - 1]);
                    leftSibling._childrenIds.RemoveAt(leftSibling._childrenIds.Count - 1);
                }

                _nodeManager.MarkAsChanged(this);
                _nodeManager.MarkAsChanged(parent);
                _nodeManager.MarkAsChanged(leftSibling);
                return;
            }

            var leftChild = rightSibling != null ? this : leftSibling;
            var rightChild = rightSibling != null ? rightSibling : this;
            var seperatorParentIndex = rightSibling != null ? indexInParent : (indexInParent - 1);

            leftChild._entries.Add(parent.GetEntry(seperatorParentIndex));

            leftChild._entries.AddRange(rightChild._entries);
            leftChild._childrenIds.AddRange(rightChild._childrenIds);
            
            foreach (var id in rightChild._childrenIds)
            {
                var n = _nodeManager.Find(id);
                n._parentId = leftChild._id;
                _nodeManager.MarkAsChanged(n); ;
            }

            parent._entries.RemoveAt(seperatorParentIndex);
            parent._childrenIds.RemoveAt(seperatorParentIndex + 1);
            _nodeManager.Delete(rightChild);

            if (parent._parentId == 0 && parent.EntriesCount == 0)
            {
                leftChild._parentId = 0;
                _nodeManager.MarkAsChanged(leftChild); 
                _nodeManager.MakeRoot(leftChild);
                _nodeManager.Delete(parent);           
            }
            else if ((parent._parentId != 0) && (parent.EntriesCount < _nodeManager.MinEntriesPerNode))
            {
                _nodeManager.MarkAsChanged(leftChild);  
                _nodeManager.MarkAsChanged(parent);     
                parent.Rebalance();
            }
            else
            {
                _nodeManager.MarkAsChanged(leftChild);  
                _nodeManager.MarkAsChanged(parent);    
            }
        }
    }
}

