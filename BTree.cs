using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KursovaSAAConsole2
{
    public class BTree<Key, Value> : Index<Key, Value>
    {
        readonly TreeManager<Key, Value> _nodeManager;
        readonly bool _duplicateKeys = false;
        public BTree(TreeManager<Key, Value> nodeManager, bool DuplicateKeys = false)
        {
            _nodeManager = nodeManager;
            _duplicateKeys = DuplicateKeys;

        }

        public bool Delete(Key key, Value value, IComparer<Value> valueComparer)
        {
            if (valueComparer == null)
            {
                valueComparer = Comparer<Value>.Default;
            }
            var deleted = false;
            var goContinue = false;

           
            while (goContinue)
            {
              using (var enumerator = (TreeEnumerator<Key, Value>)LargerThan(key).GetEnumerator())
              {
                while (true)
                {
                  if (false == enumerator.MoveNext())
                  {
                     goContinue = false;
                     break;
                  }

                   var entry = enumerator.Current;

                  if (_nodeManager.KeyComparer.Compare(entry.Item1, key) > 0)
                  {
                      goContinue = false;
                      break;
                  }

                  if (valueComparer.Compare(entry.Item2, value) == 0)
                  {
                      enumerator.CurrentNode.Remove(enumerator.CurrEntry);
                      deleted = true;
                      break;
                  }
                }
              }
            }
            _nodeManager.SaveChanges();
            return deleted;
        }
        public bool Delete(Key key)
        {
            if (true == _duplicateKeys)
            {
                throw new InvalidOperationException("This method should be called only from unique tree");
            }
            
            using (var enumerator = (TreeEnumerator<Key, Value>)LargerOrEqual(key).GetEnumerator())
            {
                if (enumerator.MoveNext() && (_nodeManager.KeyComparer.Compare(enumerator.Current.Item1, key) == 0))
                {
                    enumerator.CurrentNode.Remove(enumerator.CurrEntry);
                    return true;
                }
            }
            return false;
        }
        public void Insert(Key key, Value value)
        {
            int insertionIndex = 0;

            var node = FindNodeForInsertion(key, ref insertionIndex);

            int finalIndex = insertionIndex >= 0 ? insertionIndex : ~insertionIndex;

            node.InsertAsLeaf(key, value, finalIndex);

            if (node.IsOverflow)
            {
                TreeNode<Key, Value> left, right;
                Console.WriteLine("Node overflow detected, splitting node.");
                node.Split(out left, out right);

            }

            _nodeManager.SaveChanges();
        }
        public CustomTuple<Key, Value> Get(Key key)
        {

            int insertionIndex = 0;
            var node = FindNodeForInsertion(key, ref insertionIndex);

            if (insertionIndex >= 0 && insertionIndex < node.EntriesCount)
            {
                var entry = node.GetEntry(insertionIndex);
                if (entry.Item1.Equals(key))
                {
                    return entry;
                }
            }

            return null;
        }
        public IEnumerable<CustomTuple<Key, Value>> LargerOrEqual(Key key)
        {
            var startIterationIndex = 0;
            var node = FindNodeForIteration(key, _nodeManager.Root, true, ref startIterationIndex);

            return new TreeTraverser<Key, Value>(_nodeManager, node, (startIterationIndex >= 0 ? startIterationIndex : ~startIterationIndex) - 1, TreeTraverseDirection.Ascending);

        }
        public IEnumerable<CustomTuple<Key, Value>> LargerThan(Key key)
        {
            var startIterationIndex = 0;
            var node = FindNodeForIteration(key, _nodeManager.Root, false, ref startIterationIndex);

            return new TreeTraverser<Key, Value>(_nodeManager, node, (startIterationIndex >= 0 ? startIterationIndex : (~startIterationIndex - 1)), TreeTraverseDirection.Ascending);
        }
        public IEnumerable<CustomTuple<Key, Value>> LessOrEqual(Key key)
        {
            var startIterationIndex = 0;
            var node = FindNodeForIteration(key, _nodeManager.Root, false, ref startIterationIndex);

            return new TreeTraverser<Key, Value>(_nodeManager, node, startIterationIndex >= 0 ? (startIterationIndex + 1) : ~startIterationIndex, TreeTraverseDirection.Descending);
        }
        public IEnumerable<CustomTuple<Key, Value>> LessThan(Key key)
        {
            var startIterationIndex = 0;
            var node = FindNodeForIteration(key, _nodeManager.Root, true, ref startIterationIndex);

            return new TreeTraverser<Key, Value>(_nodeManager, node, startIterationIndex >= 0 ? startIterationIndex : ~startIterationIndex, TreeTraverseDirection.Descending);
        }
        TreeNode<Key, Value> FindNodeForIteration(Key key, TreeNode<Key, Value> node, bool moveLeft, ref int startIterationIndex)
        {
            if (node.IsEmpty)
            {
                startIterationIndex = ~0;
                return node;
            }

            var binarySearchResult = node.BinarySearchEntriesForKey(key, moveLeft ? true : false);

            if (binarySearchResult >= 0)
            {
                if (node.IsLeaf)
                {
                    startIterationIndex = binarySearchResult;
                    return node;
                }
                else
                {
                    return FindNodeForIteration(key, node.GetChildNode(moveLeft ? binarySearchResult : binarySearchResult + 1), moveLeft, ref startIterationIndex);
                }
            }
            else if (false == node.IsLeaf)
            {
                return FindNodeForIteration(key, node.GetChildNode(~binarySearchResult), moveLeft, ref startIterationIndex);
            }
            else
            {
                startIterationIndex = binarySearchResult;
                return node;
            }
        }
        TreeNode<Key, Value> FindNodeForInsertion(Key key, TreeNode<Key, Value> node, ref int insertionIndex)
        {
            if (node.IsEmpty)
            {
                insertionIndex = ~0;
                return node;
            }

            var binarySearchResult = node.BinarySearchEntriesForKey(key);
            if (binarySearchResult >= 0)
            {
                if (_duplicateKeys && false == node.IsLeaf)
                {
                    return FindNodeForInsertion(key, node.GetChildNode(binarySearchResult), ref insertionIndex);
                }
                else
                {
                    insertionIndex = binarySearchResult;
                    return node;
                }
            }
            else if (false == node.IsLeaf)
            {
                return FindNodeForInsertion(key, node.GetChildNode(~binarySearchResult), ref insertionIndex);
            }
            else
            {
                insertionIndex = binarySearchResult;
                return node;
            }
        }
        private TreeNode<Key, Value> FindNodeForInsertion(Key key, ref int index)
        {
            var currentNode = _nodeManager.Root;
            while (!currentNode.IsLeaf)
            {
                index = currentNode.BinarySearchEntriesForKey(key);
                currentNode = index < 0 ? currentNode.GetChildNode(~index) : currentNode.GetChildNode(index + 1);
            }

            index = currentNode.BinarySearchEntriesForKey(key);
            return currentNode;
        }

    }
}
