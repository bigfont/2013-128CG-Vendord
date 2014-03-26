﻿using System;
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

            Vendor vendor = new Vendor(qe) { Id = -2, Name = "Test Vendor" };
            vendor.UpsertIntoDb();

            Department department = new Department(qe) { Id = -2, Name = "Test Department" };
            department.UpsertIntoDb();

            Order order = new Order(qe) { Name = "Test Order" };
            order.UpsertIntoDb(db);

            Product product = new Product();

            product = new Product(qe);

            product.Name = "Renova Pink";
            product.Price = 10.00m;
            product.IsInTrash = 0;
            product.Vendor.Id = vendor.Id;
            product.Department.Id = department.Id;
            product.Upc = "5601028000741";
            product.UpsertIntoDb();

            product.Name = "Smoothies and Juices Book";
            product.Price = 10.00m;
            product.IsInTrash = 0;
            product.Vendor.Id = vendor.Id;
            product.Department.Id = department.Id;
            product.Upc = "9780752591483";
            product.UpsertIntoDb();

            OrderProduct orderProduct = new OrderProduct(qe)
            {
                OrderID = order.Id,
                CasesToOrder = 10
            };

            orderProduct.ProductUPC = "1";
            orderProduct.UpsertIntoDb();

            orderProduct.ProductUPC = "2";
            orderProduct.UpsertIntoDb();

            Console.WriteLine(vendor.SelectAll().Count());
            Console.WriteLine(department.SelectAll().Count());
            Console.WriteLine(order.SelectAll().Count());
            Console.WriteLine(product.SelectAll().Count());
            Console.WriteLine(orderProduct.SelectAll().Count());

            List<Product> productsJoinAll = product.SelectAllWithJoin();
            Console.WriteLine(productsJoinAll.Count());

            Console.ReadLine();

        }
    }
}
