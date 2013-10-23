using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vendord.SmartDevice.Shared;

namespace Console.Test
{
    class Program
    {
        static void Main(string[] args)
        {
            Database db = new Database();
            Order order = new Order() { Name = "123" };
            order.UpsertIntoDB(db);
        }
    }
}
