using System;
using System.Data.OleDb;

namespace DataProgramming_MicrosoftAccess2010
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            const string dam = "DAM";

            // Connection string for ADO.NET via OleDB
            var cn =
                new OleDbConnection(
                    "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=PosBack.mdb;Jet OLEDB:Database Password=1L0v3Acce55;");

            // Prepare SQL query
            const string query = "SELECT upc, description FROM ProductSales;"; // cost, totalcost
            var cmd = new OleDbCommand(query, cn);

            try
            {
                cn.Open();
                Console.WriteLine("{0}: Successfully connected to database. Data source name:\n {1}",
                    dam, cn.DataSource);
                Console.WriteLine("{0}: SQL query:\n {1}", dam, query);

                // Run the query and create a record set
                OleDbDataReader dr = cmd.ExecuteReader();
                Console.WriteLine("{0}: Retrieve schema info for the given result set:", dam);

                if (dr != null)
                {
                    for (int column = 0; column < dr.FieldCount; column++)
                    {
                        Console.Write(" | {0}", dr.GetName(column));
                    }
                    Console.WriteLine("\n{0}: Fetch the actual data: ", dam);
                    int row = 0;
                    while (dr.Read())
                    {
                        Console.WriteLine(" | {0} | {1} ", dr.GetValue(0), dr.GetValue(1));
                        row++;
                    }
                    Console.WriteLine("{0}: Total Row Count: {1}", dam, row);
                    dr.Close();
                }
            }
            catch (OleDbException ex)
            {
                Console.WriteLine("{0}: OleDbException: Unable to connect or retrieve data from data source: {1}.",
                    dam, ex);
            }
            catch (Exception ex)
            {
                Console.WriteLine("{0}: Exception: Unable to connect or retrieve data from data source: {1}.",
                    dam, ex);
            }
            finally
            {
                cn.Close();
                Console.WriteLine("{0}: Cleanup. Done.", dam);
                Console.ReadLine();
            }
        }
    }
}