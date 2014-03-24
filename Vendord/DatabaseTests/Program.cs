using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vendord.SmartDevice.Linked;

namespace DatabaseTests
{
    class Program
    {
        static void Main(string[] args)
        {
            Database db = new Database();
            DbQueryExecutor qe = new DbQueryExecutor(db.ConnectionString);

            Vendor vendor = new Vendor() { Name = "test" };
            vendor.UpsertIntoDb(db, qe);

            Department department = new Department() { Name = "test" };
            department.UpsertIntoDb(db, qe);

            Order order = new Order() { Name = "test" };
            order.UpsertIntoDb(db, qe);

            Product product = new Product() 
            { 
                Upc = "0000000", 
                Name = "test", 
                Price = 10.00m,
                IsInTrash = 0, 
                VendorId = vendor.Id,
                DepartmentId = department.Id
            };
            product.UpsertIntoDb(qe);

            OrderProduct op = new OrderProduct()
            {
                OrderID = order.Id,
                ProductUPC = product.Upc,
                CasesToOrder = 10
            };
            op.UpsertIntoDb(qe);
            
        }
    }
}
