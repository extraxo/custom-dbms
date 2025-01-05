using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KursovaSAAConsole2
{
    public interface TreeManager<Key,Value>
    {
        ushort MinEntriesPerNode
        {
            get;
        }

        IComparer<Key> KeyComparer 
        { 
            get; 
        }

        IComparer<CustomTuple<Key,Value>> EntryComparer
        {
            get;
        }

        TreeNode<Key,Value> Root
        {
            get;
        }
        TreeNode<Key, Value> Create(IEnumerable<CustomTuple<Key, Value>> entries, IEnumerable<uint> childrenIds);
        TreeNode<Key, Value> Find(uint id);

        TreeNode<Key, Value> CreateNewRoot(Key key, Value value, uint leftNodeId, uint rightNodeId);

        void MakeRoot(TreeNode<Key, Value> node);
        void MarkAsChanged(TreeNode<Key, Value> node);
        void Delete(TreeNode<Key, Value> node);
        void SaveChanges();
    }
}

