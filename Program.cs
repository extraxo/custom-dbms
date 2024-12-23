namespace KursovaSAAConsole2
{
    internal class Program
    {
        static void Main(string[] args)
        {
            string databasePath = "database.db";
            Database db = new Database(databasePath);

            try
            {
                string createCommand = "CREATE TABLE TestTable(Id:int, Name:string)";
                Console.WriteLine("Executing CREATE TABLE...");
                db.CreateTable(createCommand);

                var createdTable = db.GetTable("TestTable");
                if (createdTable != null)
                {
                    Console.WriteLine("Table 'TestTable' created successfully!");
                }
                else
                {
                    Console.WriteLine("Table creation failed.");
                    return;
                }

                Console.WriteLine("\nExecuting DROP TABLE...");
                db.DropTable("DROP TABLE TestTable");
               
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
            finally
            {
                db.Dispose();
            }

            Console.WriteLine("\nTest complete. Press any key to exit.");
            Console.ReadKey();
        }
    }
}