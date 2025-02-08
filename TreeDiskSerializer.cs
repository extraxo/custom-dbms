using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KursovaSAAConsole2
{
    public class TreeDiskSerializer<Key,Value>
    {
        ISerializer<Key> _keySerializer;
        ISerializer<Value> _valueSerializer;
        TreeManager<Key,Value> _nodeManager;

        public TreeDiskSerializer(TreeManager<Key,Value> nodeManager, ISerializer<Key> serializer, ISerializer<Value> valueSerializer)
        {
            _nodeManager = nodeManager;
            _keySerializer = serializer;
            _valueSerializer = valueSerializer;
        }
        public byte[] Serialize(TreeNode<Key, Value> node)
        {
            if (_keySerializer.IsFixedSize && _valueSerializer.IsFixedSize)
            {
                return FixedLengthSerialize(node);
            }
            else if (_valueSerializer.IsFixedSize)
            {
                return VariableKeyLengthSerialize(node);
            }
            else
            {
                throw new NotSupportedException();
            }
        }

        public TreeNode<Key,Value> Deserialize(uint assignId, byte[] record)
        {
            if (_keySerializer.IsFixedSize && _valueSerializer.IsFixedSize)
            {
                return FixedLengthDeserialize(assignId, record);
            }
            else if (_valueSerializer.IsFixedSize)
            {
                return VariableKeyLengthDeserialize(assignId, record);
            }
            else
            {
                throw new NotSupportedException();
            }
        }
        byte[] FixedLengthSerialize(TreeNode<Key, Value> node)
        {
            var entrySize = _keySerializer.Length + _valueSerializer.Length;
            var size = 16 + node.Entries.Length * entrySize + node.ChildrenIds.Length;

            if(size >= (1024 * 64))
            {
                throw new Exception("Serialized node size too large: " + size);
            }
            var buffer = new byte[size];

            BufferReader.WriteBuffer(node.ParentId, buffer, 0);

            BufferReader.WriteBuffer((uint)node.EntriesCount, buffer, 4);

            BufferReader.WriteBuffer((uint)node.ChildrenNodeCount, buffer, 8);

            for (var i = 0; i < node.EntriesCount; i++)
            {
                var entry = node.GetEntry(i);
                Buffer.BlockCopy(_keySerializer.Serialize(entry.Item1), 0, buffer, 12 + i * entrySize, _keySerializer.Length);
                Buffer.BlockCopy(_valueSerializer.Serialize(entry.Item2), 0, buffer, 12 + i * entrySize + _keySerializer.Length, _valueSerializer.Length);
            }

            var childrenIds = node.ChildrenIds;
            for (var i = 0; i < node.ChildrenNodeCount; i++)
            {
                BufferReader.WriteBuffer(childrenIds[i], buffer, 12 + entrySize * node.EntriesCount + (i * 4));
            }

            return buffer;
        }

        TreeNode<Key,Value> FixedLengthDeserialize(uint assignId, byte[] buffer)
        {
            var entrySize = _keySerializer.Length + _valueSerializer.Length;

            var parentId = BufferReader.ReadBufferUInt32(buffer, 0);

            var entriesCount = BufferReader.ReadBufferUInt32(buffer, 4);

            var childrenCount = BufferReader.ReadBufferUInt32(buffer, 8);

            var entries = new CustomTuple<Key, Value>[entriesCount];
            for (var i = 0; i < entriesCount; i++)
            {
                var key = _keySerializer.Deserialize(buffer, 12 + i * entrySize, _keySerializer.Length);
                if (key == null)
                {
                    Console.WriteLine($"Deserialized null key at index {i} in FixedLengthDeserialize.");
                }

                var value = _valueSerializer.Deserialize(buffer, 12 + i * entrySize + _keySerializer.Length, _valueSerializer.Length);
                if (value == null)
                {
                    Console.WriteLine($"Deserialized null value at index {i} in FixedLengthDeserialize.");
                }

                entries[i] = new CustomTuple<Key, Value>(key, value);
            }


            var children = new uint[childrenCount];
            for (var i = 0; i < childrenCount; i++)
            {
                children[i] = BufferReader.ReadBufferUInt32(buffer, (int)(12 + entrySize * entriesCount + (i * 4)));
            }

            return new TreeNode<Key, Value>(_nodeManager, assignId, parentId, entries, children);
        }

        TreeNode<Key,Value> VariableKeyLengthDeserialize(uint assignId, byte[] buffer)
        {
            var parentId = BufferReader.ReadBufferUInt32(buffer, 0);

            var entriesCount = BufferReader.ReadBufferUInt32(buffer, 4);

            var childrenCount = BufferReader.ReadBufferUInt32(buffer, 8);

            var entries = new CustomTuple<Key, Value>[entriesCount];
            var p = 12;
            for (var i = 0; i < entriesCount; i++)
            {
                var keyLength = BufferReader.ReadBufferInt32(buffer, p);
                var key = _keySerializer.Deserialize(buffer, p + 4, keyLength);
                if (key == null)
                {
                    Console.WriteLine($"Deserialized null key at index {i} in VariableKeyLengthDeserialize.");
                }

                var value = _valueSerializer.Deserialize(buffer, p + 4 + keyLength, _valueSerializer.Length);
                if (value == null)
                {
                    Console.WriteLine($"Deserialized null value at index {i} in VariableKeyLengthDeserialize.");
                }

                entries[i] = new CustomTuple<Key, Value>(key, value);

                p += 4 + keyLength + _valueSerializer.Length;
            }

            var children = new uint[childrenCount];
            for (var i = 0; i < childrenCount; i++)
            {
                children[i] = BufferReader.ReadBufferUInt32(buffer, (int)(p + (i * 4)));
            }

            return new TreeNode<Key, Value>(_nodeManager, assignId, parentId, entries, children);
        }

        byte[] VariableKeyLengthSerialize(TreeNode<Key, Value> node)
        {
            using (var m = new MemoryStream())
            {
                m.Write(LittleEndianByteOrder.GetBytes((uint)node.ParentId), 0, 4);
                m.Write(LittleEndianByteOrder.GetBytes((uint)node.EntriesCount), 0, 4);
                m.Write(LittleEndianByteOrder.GetBytes((uint)node.ChildrenNodeCount), 0, 4);

                for (var i = 0; i < node.EntriesCount; i++)
                {
                    var entry = node.GetEntry(i);
                    var key = _keySerializer.Serialize(entry.Item1);
                    var value = _valueSerializer.Serialize(entry.Item2);

                    m.Write(LittleEndianByteOrder.GetBytes((int)key.Length), 0, 4);
                    m.Write(key, 0, key.Length);
                    m.Write(value, 0, value.Length);
                }

                var childrenIds = node.ChildrenIds;
                for (var i = 0; i < node.ChildrenNodeCount; i++)
                {
                    m.Write(LittleEndianByteOrder.GetBytes((uint)childrenIds[i]), 0, 4);
                }

                return m.ToArray();
            }
        }
    }
}
