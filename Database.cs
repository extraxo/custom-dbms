using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace KursovaSAAConsole2
{
    
    public class Column
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public string DefaultValue { get; set; }

        public Column(string name, string type, string defValue) 
        {
            Name = name;
            Type = type;
            DefaultValue = defValue;
        }
    }

    public class Table
    {
        public string TableName { get; set; }
        public CustomList<Column> Columns { get; set; }
        private readonly RecordStorage _recordStorage;

        public Table(string name, RecordStorage recordStorage)
        {
            TableName = name;
            Columns = new CustomList<Column>();
            _recordStorage = recordStorage;
        }

        public void AddColumn(string name, string type, string defaultValue = null)
        {
            var column = new Column(name, type, defaultValue);
            Columns.Add(column);
            StoreColumnInRecordStorage(column);
        }

        private void StoreColumnInRecordStorage(Column column)
        {
            var columnData = $"{column.Name}:{column.Type}";
            if (!string.IsNullOrEmpty(column.DefaultValue))
            {
                columnData += $" default {column.DefaultValue}";
            }
            _recordStorage.Create(System.Text.Encoding.UTF8.GetBytes(columnData));
        }
    }

    public class Database
    {
        ushort _minEntriesCountPerNode = 36;
        IComparer<string> _keyComparer = Comparer<string>.Default;
        private TreeManager<string, Table> _treeManager;
        private BTree<string, Table> _tables;
        private readonly Stream _mainDatabase;
        private readonly RecordStorage _recordStorage;
        public Database(string database)
        {
            _treeManager = new TreeMemoryManager<string, Table>(_minEntriesCountPerNode, _keyComparer);

            _tables = new BTree<string, Table>(_treeManager, false);

            _mainDatabase = new FileStream(database, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None, 4096);

            var blockStorage = new BlockStorage(_mainDatabase, 4096, 48);
            _recordStorage = new RecordStorage(blockStorage);
        }

        public void CreateTable(string command)
        {

            int createTableStart = CustomIndexOf.IndexOfSubstring(command, "CREATE TABLE");
            if (createTableStart < 0)
            {
                throw new ArgumentException("Missing 'CREATE TABLE' keyword.");
            }

            int tableNameStart = createTableStart + "CREATE TABLE".Length;
            int tableNameEnd = CustomIndexOf.IndexOfSubstring(command, "(");
            if (tableNameEnd < 0 || tableNameStart >= tableNameEnd)
            {
                throw new ArgumentException("Invalid table name syntax.");
            }

            string tableName = command.Substring(tableNameStart, tableNameEnd - tableNameStart).Trim();

            int columnSectionStart = command.IndexOf("(") + 1;
            int columnSectionEnd = command.IndexOf(")");

            if (columnSectionStart < 0 || columnSectionEnd < 0 || columnSectionEnd <= columnSectionStart)
            {
                throw new ArgumentException("Invalid column section syntax.");
            }

            var columnSection = command.Substring(columnSectionStart, columnSectionEnd - columnSectionStart);
            var columnDefinitions = CustomSplit.SplitString(columnSection.Trim(), ',');

            var columns = new CustomList<Column>();

            foreach (var columnDefinition in columnDefinitions)
            {
                var columnParts = CustomSplit.SplitString(columnDefinition.Trim(), ':');

                if (columnParts.Count < 2)
                {
                    throw new ArgumentException($"Invalid column definition: {columnDefinition}");
                }

                var columnName = columnParts[0];
                var columnType = columnParts[1];
                var defaultValue = columnParts.Count > 3 && columnParts[2].ToLower() == "default" ? columnParts[3].Trim() : null;

                columns.Add(new Column(columnName, columnType, defaultValue));
            }

            if (_tables.Get(tableName) != null)
            {
                throw new InvalidOperationException($"Table '{tableName}' already exists.");
            }

            var newTable = new Table(tableName, _recordStorage);
            foreach (var column in columns)
            {
                newTable.AddColumn(column.Name, column.Type, column.DefaultValue);
            }

            _tables.Insert(tableName, newTable);
        }


        public Table GetTable(string tableName)
        {
            if (string.IsNullOrWhiteSpace(tableName))
            {
                throw new ArgumentException("Table name cannot be null or empty.");
            }

            var result = _tables.Get(tableName);

            if (result == null)
            {
                throw new ArgumentException($"Table '{tableName}' does not exist or has been dropped.");
            }


            return result.Item2;
        }

        public bool TableExists(string tableName)
        {
            return _tables.Get(tableName) != null;
        }

        private byte[] SerializeTableMetadata(Table table)
        {
            var serializer = new TreeStringSerializer();
            var metadata = new MemoryStream();

            var tableNameBytes = serializer.Serialize(table.TableName);
            metadata.Write(tableNameBytes, 0, tableNameBytes.Length);

            foreach (var column in table.Columns)
            {
                var columnNameBytes = serializer.Serialize(column.Name);
                var columnTypeBytes = serializer.Serialize(column.Type);
                var defaultValueBytes = serializer.Serialize(column.DefaultValue ?? "");

                metadata.Write(columnNameBytes, 0, columnNameBytes.Length);
                metadata.Write(columnTypeBytes, 0, columnTypeBytes.Length);
                metadata.Write(defaultValueBytes, 0, defaultValueBytes.Length);
            }

            return metadata.ToArray();
        }

        public void DropTable(string command)
        {
            var commandParts = CustomSplit.SplitString(command, ' ');

            if (commandParts.Count != 3 || commandParts[0] != "DROP" || commandParts[1] != "TABLE")
            {
                throw new ArgumentException("Invalid DROP TABLE command syntax. Correct format: DROP TABLE <TableName>");
            }

            string tableName = commandParts[2].Trim();

            if (!TableExists(tableName))
            {
                Console.WriteLine($"Table '{tableName}' does not exist.");
                return;
            }

            DeleteAllRecordsForTable(tableName);

            _tables.Delete(tableName);
            Console.WriteLine($"Table '{tableName}' successfully dropped.");
        }

        public void DeleteAllRecordsForTable(string tableName)
        {
            var recordIds = _recordStorage.GetRecordIdsForTable(tableName);

            foreach (var id in recordIds)
            {
                _recordStorage.Delete(id);
            }

        }

        public void Dispose()
        {
            _mainDatabase?.Dispose();
        }
    }
}