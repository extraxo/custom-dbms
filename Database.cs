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

        public int RowCount { get; private set; }

        public Table(string name, RecordStorage recordStorage)
        {
            TableName = name;
            Columns = new CustomList<Column>();
            _recordStorage = recordStorage;
            RowCount = 0; 
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
            _recordStorage.Create(Encoding.UTF8.GetBytes(columnData));
        }

        public void RowCountFunc()
        {
            RowCount++;
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
        private TreeManager<string, uint> _indexTreeManager;
        private CustomDictionary<string, BTree<string, uint>> _indexStorage;


        public Database(string database)
        {
            _treeManager = new TreeMemoryManager<string, Table>(_minEntriesCountPerNode, _keyComparer);

            _tables = new BTree<string, Table>(_treeManager, false);

            _mainDatabase = new FileStream(database, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None, 4096);

            _indexTreeManager = new TreeMemoryManager<string, uint>(_minEntriesCountPerNode, Comparer<string>.Default);
            _indexStorage = new CustomDictionary<string, BTree<string, uint>>();


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

            string tableName = CustomTrimFunc.CustomTrim(CustomIndexOf.IndexOfSubstring(command,tableNameStart, tableNameEnd - tableNameStart));

            int columnSectionStart = tableNameEnd + 1;  

            int columnSectionEnd = CustomIndexOf.IndexOf(command, ')');
            if (columnSectionStart < 0 || columnSectionEnd < 0 || columnSectionEnd <= columnSectionStart)
            {
                throw new ArgumentException("Invalid column section syntax.");
            }

            var columnSection = CustomIndexOf.IndexOfSubstring(command,columnSectionStart, columnSectionEnd - columnSectionStart);
            var columnDefinitions = CustomSplit.SplitString(columnSection, ',');

            var columns = new CustomList<Column>();

            foreach (var columnDefinition in columnDefinitions)
            {
                var columnParts = CustomSplit.SplitString(CustomTrimFunc.CustomTrim(columnDefinition), ':');

                if (columnParts.Count < 2)
                {
                    throw new ArgumentException($"Invalid column definition: {columnDefinition}");
                }

                var columnName = columnParts[0];
                var columnType = columnParts[1];
                var defaultValue = columnParts.Count > 3 && columnParts[2].ToLower() == "default" ? CustomTrimFunc.CustomTrim(columnParts[3]) : null;

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

            var result = _tables.Get(tableName);

            if (result == null)
            {
                throw new ArgumentException($"Table '{tableName}' does not exist or has been dropped.");
            }

            return result.Item2;
        }

        public void TableInfo(string tableName)
        {
            var table = _tables.Get(tableName).Item2;
            
            if (table == null)
            {
                Console.WriteLine($"Error: Table '{tableName}' does not exist.");
                return;
            }

            Console.WriteLine($"Table Name: {tableName}");

            int totalBytes = 0;
            int recordCount = 0;

            foreach (var recordId in _recordStorage.GetRecordIdsForTable(tableName))
            {
                var record = _recordStorage.Find(recordId);
                if (record != null)
                {
                    recordCount++;
                    totalBytes += record.Length;
                }
            }

            Console.WriteLine($"Total Records: {recordCount}");
            Console.WriteLine($"Total Space Used: {totalBytes} bytes");
        }
        public void InsertInto(string command)
        {
            Console.WriteLine($"Processing INSERT INTO command: {command}");

            int insertStart = CustomIndexOf.IndexOfSubstring(command, "INSERT INTO");
            if (insertStart < 0)
            {
                throw new ArgumentException("Missing 'INSERT INTO' keyword.");
            }

            int tableNameStart = insertStart + "INSERT INTO".Length;
            int tableNameEnd = CustomIndexOf.IndexOfSubstring(command, "(");
            if (tableNameEnd < 0 || tableNameStart >= tableNameEnd)
            {
                throw new ArgumentException("Invalid table name syntax.");
            }

            string tableName = CustomTrimFunc.CustomTrim(CustomIndexOf.IndexOfSubstring(command, tableNameStart, tableNameEnd - tableNameStart));

            var table = GetTable(tableName);

            if (table == null)
            {
                throw new InvalidOperationException($"Table '{tableName}' does not exist.");
            }

            int columnsStart = CustomIndexOf.IndexOf(command, '(') + 1;
            int columnsEnd = CustomIndexOf.IndexOf(command, ')');
            if (columnsStart >= columnsEnd)
            {
                throw new ArgumentException("Invalid column section syntax.");
            }
            string columnsSection = CustomTrimFunc.CustomTrim(CustomIndexOf.IndexOfSubstring(command,columnsStart, columnsEnd - columnsStart));

            var columnNames = CustomSplit.SplitString(columnsSection, ',');

            int valuesStart = CustomIndexOf.IndexOfSubstring(command, "VALUES (") + "VALUES (".Length;
            int valuesEnd = CustomIndexOf.IndexOf(command, ')', valuesStart);

            if (valuesStart >= valuesEnd || valuesStart < 0 || valuesEnd < 0)
            {
                throw new ArgumentException("Invalid values section syntax.");
            }

            string valuesSection = CustomTrimFunc.CustomTrim(CustomIndexOf.IndexOfSubstring(command, valuesStart, valuesEnd - valuesStart));
            var values = CustomSplit.SplitString(valuesSection, ',');

            if (columnNames.Count != values.Count)
            {
                throw new ArgumentException("Number of columns does not match number of values.");
            }

            var rowData = new CustomDictionary<string, string>();
            for (int i = 0; i < columnNames.Count; i++)
            {
                string columnName = CustomTrimFunc.CustomTrim(columnNames[i]);
                string value = CustomTrimFunc.CustomTrim(values[i]);

                Column column = null;
                foreach (var col in table.Columns)
                {
                    if (col.Name == columnName)
                    {
                        column = col;
                        break;
                    }
                }
                if (column == null)
                {
                    throw new ArgumentException($"Column '{columnName}' not found in table '{tableName}'.");
                }

                rowData[columnName] = value;
            }

            foreach (var column in table.Columns)
            {
                if (!rowData.ContainsKey(column.Name))
                {
                    if (column.DefaultValue != null)
                    {
                        rowData[column.Name] = column.DefaultValue;
                    }
                    else
                    {
                        throw new ArgumentException($"Missing value for column '{column.Name}' which has no default value.");
                    }
                }
            }
            CustomList<string> rowContentList = new CustomList<string>();
            foreach (var kv in rowData)
            {
                rowContentList.Add($"{kv.Key}:{kv.Value}");
            }
            string recordContent = CustomStringJoin.Join(", ", rowContentList);

            var recordData = Encoding.UTF8.GetBytes($"Table:{tableName}; {recordContent}");
            uint recordId = _recordStorage.Create(recordData);

            table.RowCountFunc();
            Console.WriteLine($"Table '{tableName}' now has {table.RowCount} rows.");
        }

        public bool TableExists(string tableName)
        {
            return _tables.Get(tableName) != null;
        }

        public void GetRow(string command)
        {
            Console.WriteLine($"Processing GET ROW command: {command}");

            int getRowStart = CustomIndexOf.IndexOfSubstring(command, "GET ROW");
            if (getRowStart < 0)
            {
                throw new ArgumentException("Missing 'GET ROW' keyword.");
            }

            int fromIndex = CustomIndexOf.IndexOfSubstring(command, "FROM");
            if (fromIndex < 0)
            {
                throw new ArgumentException("Missing 'FROM' keyword.");
            }

            string rowIdsSection = CustomIndexOf.IndexOfSubstring(command,getRowStart + "GET ROW".Length, fromIndex - getRowStart - "GET ROW".Length);
            string tableName = CustomTrimFunc.CustomTrim(CustomIndexOf.IndexOfSubstring(command, fromIndex + "FROM".Length));


            var rowIds = ParseRowIds(rowIdsSection);
            var table = _tables.Get(tableName)?.Item2;
            if (table == null)
            {
                Console.WriteLine($"Error: Table '{tableName}' does not exist.");
                return;
            }

            PrintColumnNames(table.Columns);

            foreach (var rowId in rowIds)
            {
                var recordData = _recordStorage.Find(rowId);
                if (recordData == null || recordData.Length == 0)
                {
                    Console.WriteLine($"Warning: Row with ID {rowId} not found or is invalid.");
                    continue;
                }

                var rowValues = ParseRow(recordData, table.Columns);
                Console.WriteLine($"{rowId}\t" + CustomStringJoin.Join("\t", rowValues));
            }
        }
        public void DeleteFrom(string command)
        {
            int deleteStart = CustomIndexOf.IndexOfSubstring(command, "DELETE FROM");
            if (deleteStart < 0)
            {
                throw new ArgumentException("Missing 'DELETE FROM' keyword.");
            }

            int tableNameStart = deleteStart + "DELETE FROM".Length;
            int rowSectionStart = CustomIndexOf.IndexOfSubstring(command, "ROW");
            if (rowSectionStart < 0 || tableNameStart >= rowSectionStart)
            {
                throw new ArgumentException("Invalid syntax. Expected 'ROW' clause.");
            }

            string tableName = CustomTrimFunc.CustomTrim(CustomIndexOf.IndexOfSubstring(command, tableNameStart, rowSectionStart - tableNameStart));

            string rowSection = CustomIndexOf.IndexOfSubstring(command,rowSectionStart + "ROW".Length);

            var rowNumbers = new CustomList<uint>();
            var rows = CustomSplit.SplitString(rowSection, ',');
            foreach (var row in rows)
            {
                rowNumbers.Add(uint.Parse(row));
            }

            var table = GetTable(tableName);
            if (table == null)
            {
                throw new InvalidOperationException($"Table '{tableName}' does not exist.");
            }

            foreach (var rowNumber in rowNumbers)
            {
                var recordId = rowNumber; 
                _recordStorage.Delete(recordId);
            }

            string deletedRows = "";
            foreach (var rowNumber in rowNumbers)
            {
                deletedRows += rowNumber + ", ";
            }
            deletedRows = deletedRows.TrimEnd(',', ' ');

            Console.WriteLine($"Deleted rows: {deletedRows} from table '{tableName}'.");
        }

        private CustomList<uint> ParseRowIds(string rowIdsSection)
        {
            var rowIds = new CustomList<uint>();
            int startIndex = 0;

            for (int i = 0; i < rowIdsSection.Length; i++)
            {
                if (rowIdsSection[i] == ',')
                {
                    string numberString = CustomIndexOf.IndexOfSubstring(rowIdsSection,startIndex, i - startIndex);
                    if (uint.TryParse(numberString, out uint rowId))
                    {
                        rowIds.Add(rowId);
                    }
                    startIndex = i + 1;
                }
                else if (i == rowIdsSection.Length - 1)
                {
                    string numberString = CustomIndexOf.IndexOfSubstring(rowIdsSection,startIndex);
                    if (uint.TryParse(numberString, out uint rowId))
                    {
                        rowIds.Add(rowId);
                    }
                }
            }

            return rowIds;
        }
        private void PrintColumnNames(CustomList<Column> columns)
        {
            CustomList<string> columnNames = new CustomList<string>();
            foreach (var col in columns)
            {
                columnNames.Add(col.Name);
            }

            Console.WriteLine(CustomStringJoin.Join("\t", columnNames)); 
        }

        private CustomList<string> ParseRow(byte[] recordData, CustomList<Column> columns)
        {
            var rowValues = new CustomList<string>();
            int offset = 0;

            foreach (var column in columns)
            {
                string value = string.Empty;

                if (column.Type == "int")
                {
                    if (recordData.Length >= offset + sizeof(int))
                    {
                        value = BitConverter.ToInt32(recordData, offset).ToString();
                        offset += sizeof(int);
                    }
                    else
                    {
                        value = "Invalid int data";
                    }
                }
                else if (column.Type == "string")
                {
                    if (recordData.Length >= offset + sizeof(int))
                    {
                        int stringLength = BitConverter.ToInt32(recordData, offset);
                        offset += sizeof(int);

                        if (recordData.Length >= offset + stringLength)
                        {
                            value = Encoding.UTF8.GetString(recordData, offset, stringLength);
                            offset += stringLength;
                        }
                        else
                        {
                            value = "Invalid string data";
                        }
                    }
                    else
                    {
                        value = "Invalid string length";
                    }
                }
                else
                {
                    value = "UnknownType";
                }

                rowValues.Add(value);
            }

            return rowValues;
        }

        public void DropTable(string command)
        {
            var commandParts = CustomSplit.SplitString(command, ' ');

            if (commandParts.Count != 3 || commandParts[0] != "DROP" || commandParts[1] != "TABLE")
            {
                throw new ArgumentException("Invalid DROP TABLE command syntax. Correct format: DROP TABLE <TableName>");
            }

            string tableName = CustomTrimFunc.CustomTrim(commandParts[2]);

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
            _mainDatabase.Dispose();
        }
    }
}