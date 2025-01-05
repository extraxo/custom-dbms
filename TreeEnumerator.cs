using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KursovaSAAConsole2
{
    public class TreeEnumerator<Key, Value> : IEnumerator<CustomTuple<Key, Value>>
    {
        readonly TreeManager<Key, Value> _manager;
        readonly TreeTraverseDirection _direction;

        bool _iterating = false;
        int _currEntry = 0;
        TreeNode<Key, Value> _currNode;

        CustomTuple<Key, Value> _currTuple;

        public TreeNode<Key, Value> CurrentNode
        {
            get
            {
                return _currNode;
            }
        }

        public int CurrEntry
        {
            get
            {
                return _currEntry;
            }
        }

        object IEnumerator.Current
        {
            get
            {
                return (object)Current;
            }
        }

        public CustomTuple<Key, Value> Current
        {
            get
            {
                return _currTuple;
            }
        }

        public TreeEnumerator(TreeManager<Key, Value> nodeManager, TreeNode<Key, Value> node, int index, TreeTraverseDirection direction)
        {
            _manager = nodeManager;
            _currNode = node;
            _currEntry = index;
            _direction = direction;
        }

        public bool MoveNext()
        {
            if (_iterating)
            {
                return false;
            }
            switch (_direction)
            {
                case TreeTraverseDirection.Ascending:
                    return MoveForward();
                case TreeTraverseDirection.Descending:
                    return MoveBackwards();
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        bool MoveForward()
        {
            if (_currNode.IsLeaf)
            {
                _currEntry++;

                while (true)
                {
                    if (_currEntry < _currNode.EntriesCount)
                    {
                        _currTuple = _currNode.GetEntry(_currEntry);
                        return true;
                    }
                    else if (_currNode.ParentId != 0)
                    {
                        _currEntry = _currNode.IndexInParent();
                        _currNode = _manager.Find(_currNode.ParentId);

                        if ((_currEntry < 0) || (_currNode == null))
                        {
                            throw new Exception("Something gone wrong with the BTree");
                        }
                    }

                    else
                    {
                        _currTuple = null;
                        _iterating = true;
                        return false;
                    }
                }

            }
            else
            {
                _currEntry++;

                do
                {
                    _currNode = _currNode.GetChildNode(_currEntry);
                    _currEntry = 0;
                }
                while (_currNode.IsLeaf == false);

                _currTuple = _currNode.GetEntry(_currEntry);
                return true;
            }
        }

        bool MoveBackwards()
        {
            if (_currNode.IsLeaf)
            {
                _currEntry--;
                while (true)
                {
                    if (_currEntry >= 0)
                    {
                        _currTuple = _currNode.GetEntry(_currEntry);
                        return true;
                    }
                    else if (_currNode.ParentId != 0)
                    {
                        _currEntry = _currNode.IndexInParent() - 1;
                        _currNode = _manager.Find(_currNode.ParentId);

                        if (_currNode == null)
                        {
                            throw new Exception("Something gone wrong with the BTree");
                        }
                    }

                    else
                    {
                        _iterating = true;
                        _currTuple = null;
                        return false;
                    }
                }
            }
            else
            {
                do
                {
                    _currNode = _currNode.GetChildNode(_currEntry);
                    _currEntry = _currNode.EntriesCount;

                    if ((_currEntry < 0) || (_currNode == null))
                    {
                        throw new Exception("Something gone wrong with the BTree");
                    }
                }
                while (_currNode.IsLeaf == false);

                _currEntry -= 1;
                _currTuple = _currNode.GetEntry(_currEntry);
                return true;
            }
        }

        public void Reset()
        {
            throw new NotSupportedException();
        }

        public void Dispose()
        {
            
        }
    }
}
