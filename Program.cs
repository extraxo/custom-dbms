using System;

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
                Console.WriteLine("Enter commands (e.g., CREATE TABLE, DROP TABLE, TABLEINFO, INSERT INTO, GET ROW). Type EXIT to stop:");
                while (true)
                {
                    Console.Write(">> ");
                    string command = Console.ReadLine()?.Trim();
                    if (string.IsNullOrEmpty(command))
                    {
                        Console.WriteLine("No command entered. Please try again.");
                        continue;
                    }

                    if (command.Equals("EXIT", StringComparison.OrdinalIgnoreCase))
                    {
                        Console.WriteLine("Exiting the program.");
                        break;
                    }

                    try
                    {
                        if (command.StartsWith("CREATE TABLE", StringComparison.OrdinalIgnoreCase))
                        {
                            db.CreateTable(command);
                            Console.WriteLine("Table created successfully!");
                        }
                        else if (command.StartsWith("DROP TABLE", StringComparison.OrdinalIgnoreCase))
                        {
                            db.DropTable(command);
                        }
                        else if (command.StartsWith("TABLEINFO", StringComparison.OrdinalIgnoreCase))
                        {
                            string tableName = command.Substring("TABLEINFO".Length).Trim();
                            db.TableInfo(tableName);
                        }
                        else if (command.StartsWith("INSERT INTO", StringComparison.OrdinalIgnoreCase))
                        {
                            db.InsertInto(command);
                            Console.WriteLine("Record inserted successfully!");
                        }
                        else if (command.StartsWith("GET ROW", StringComparison.OrdinalIgnoreCase))
                        {
                            db.GetRow(command);
                        }
                        else
                        {
                            Console.WriteLine("Unknown command. Please try again.");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error processing command: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Critical error: {ex.Message}");
            }
            finally
            {
                db.Dispose();
                Console.WriteLine("Database closed.");
            }
        }
    }
}
