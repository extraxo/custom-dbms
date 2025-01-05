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
                Console.WriteLine("Enter commands (e.g., CREATE TABLE, DROP TABLE, TABLEINFO, INSERT INTO, GET ROW, DELETE FROM). Type EXIT to stop:");
                while (true)
                {
                    Console.Write(">> ");
                    string command = CustomTrimFunc.CustomTrim(Console.ReadLine());
                    if (string.IsNullOrEmpty(command))
                    {
                        Console.WriteLine("No command entered. Please try again.");
                        continue;
                    }

                    if (CustomIndexOf.EqualsIgnoreCase(command, "EXIT"))
                    {
                        Console.WriteLine("Exiting the program.");
                        break;
                    }

                    try
                    {
                        if (CustomIndexOf.StartsWith(command, "CREATE TABLE"))
                        {
                            db.CreateTable(command);
                            Console.WriteLine("Table created successfully!");
                        }
                        else if (CustomIndexOf.StartsWith(command, "DROP TABLE"))
                        {
                            db.DropTable(command);
                        }
                        else if (CustomIndexOf.StartsWith(command, "TABLEINFO"))
                        {
                            string tableName = CustomTrimFunc.CustomTrim(CustomIndexOf.IndexOfSubstring(command, "TABLEINFO".Length));
                            db.TableInfo(tableName);
                        }
                        else if (CustomIndexOf.StartsWith(command, "INSERT INTO"))
                        {
                            db.InsertInto(command);
                            Console.WriteLine("Record inserted successfully!");
                        }
                        else if (CustomIndexOf.StartsWith(command, "GET ROW"))
                        {
                            db.GetRow(command);
                        }
                        else if(CustomIndexOf.StartsWith(command, "DELETE FROM"))
                        {
                            db.DeleteFrom(command);
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
