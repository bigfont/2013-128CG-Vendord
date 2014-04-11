using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlServerCe;
using System.Diagnostics;

namespace SqlCeRepairUtility
{
    class Program
    {
        static void Main(string[] args)
        {
            SqlCeEngine engine = new SqlCeEngine("Data Source = C:\\Users\\Shaun\\Documents\\VENDORD\\VendordDB.sdf");
            if (false == engine.Verify())
            {
                Console.WriteLine("Database is corrupted.");
                try
                {
                    engine.Repair(null, RepairOption.DeleteCorruptedRows);
                }
                catch(SqlCeException ex)
                {
                    Console.WriteLine(ex.Message);
                    Console.ReadLine();
                }
            }
            Console.WriteLine("Press any key to continue.");
            Console.ReadLine();
        }
    }
}
