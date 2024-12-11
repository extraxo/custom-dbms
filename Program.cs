namespace KursovaSAAConsole2
{
    internal class Program
    {
        static void Main(string[] args)
        {
            string databasePath = "database.db";
            Database db = new Database(databasePath);

            string createCommand = "CREATE TABLE Sample(Id:int, Name:string, BirthDate:date default '01.01.2022')";
            db.CreateTable(createCommand);

            try
            {
                var createdTable = db.GetTable("Sample");

                if (createdTable != null)
                {
                    Console.WriteLine("Table 'Sample' created successfully!");
                    Console.WriteLine("Columns:");
                    foreach (var column in createdTable.Columns)
                    {
                        Console.WriteLine($"- {column.Name} ({column.Type}), Default: {column.DefaultValue}");
                    }
                }
                else
                {
                    Console.WriteLine("Failed to create table or unable to retrieve table.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }

        }
    }
}