﻿namespace Vendord.SmartDevice.Shared
{
    using System;
    using System.Collections.Generic;
    using System.Data.SqlServerCe;
    using System.Linq;
    using System.Text;
    using System.IO;
    using System.Collections.ObjectModel;
    using Vendord.SmartDevice.Shared;

    public class Database
    {
        private List<Product> products;
        private List<OrderSession> orderSessions;
        private List<OrderSession_Product> orderSession_Products;

        public string ConnectionString
        {
            get
            {
                string result;
                result = String.Format("Data Source={0};Persist Security Info=False;", Constants.DatabaseFullPath);
                return result;
            }
        }

        public class OrderSession
        {
            public int ID { get; set; }
            public string Name { get; set; }
        }

        public class Product
        {
            public int ID { get; set; }
            public string Name { get; set; }
            public int UPC { get; set; }
        }

        public class OrderSession_Product
        {
            public int OrderSessionID { get; set; }
            public int ProductID { get; set; }
            public int CasesToOrder { get; set; }
        }

        public List<OrderSession> OrderSessions
        {
            get
            {
                SqlCeDataReader reader;
                SqlCeCommand command;

                if (orderSessions == null)
                {
                    orderSessions = new List<OrderSession>();
                    using (SqlCeConnection conn = new SqlCeConnection(ConnectionString))
                    {
                        conn.Open();
                        command = new SqlCeCommand(@"SELECT * FROM OrderSession", conn);
                        reader = command.ExecuteReader();
                        while (reader.Read())
                        {
                            OrderSession item = new OrderSession()
                            {
                                ID = Convert.ToInt32(reader["ID"]),
                                Name = Convert.ToString(reader["Name"])
                            };
                            orderSessions.Add(item);
                        }
                    }
                }
                return orderSessions;
            }
        }

        public List<Product> Products
        {
            get
            {
                SqlCeDataReader reader;
                SqlCeCommand command;

                if (products == null)
                {
                    products = new List<Product>();
                    using (SqlCeConnection conn = new SqlCeConnection(ConnectionString))
                    {
                        conn.Open();
                        command = new SqlCeCommand(@"SELECT * FROM Product", conn);
                        reader = command.ExecuteReader();
                        while (reader.Read())
                        {
                            Product item = new Product()
                            {
                                ID = Convert.ToInt32(reader["ID"]),
                                Name = Convert.ToString(reader["Name"]),
                                UPC = Convert.ToInt32(reader["UPC"])
                            };
                            products.Add(item);
                        }
                    }
                }
                return products;
            }
        }

        public List<OrderSession_Product> OrderSession_Products
        {
            get
            {
                if (orderSession_Products == null)
                {
                    SqlCeDataReader reader;
                    SqlCeCommand command;

                    orderSession_Products = new List<OrderSession_Product>();
                    using (SqlCeConnection conn = new SqlCeConnection(ConnectionString))
                    {
                        conn.Open();
                        command = new SqlCeCommand(@"SELECT * FROM OrderSession_Product", conn);
                        reader = command.ExecuteReader();
                        while (reader.Read())
                        {
                            OrderSession_Product item = new OrderSession_Product()
                            {
                                ProductID = Convert.ToInt32(reader["ProductID"]),
                                OrderSessionID = Convert.ToInt32(reader["OrderSessionID"]),
                                CasesToOrder = Convert.ToInt32(reader["CasesToOrder"])
                            };
                            orderSession_Products.Add(item);
                        }
                    }
                }
                return orderSession_Products;
            }
        }

        private object ExecuteScalar(string cmdText)
        {
            object result = null;
            using (SqlCeConnection conn = new SqlCeConnection(ConnectionString))
            {
                conn.Open();
                SqlCeCommand cmd = conn.CreateCommand();
                cmd.CommandText = cmdText;
                result = cmd.ExecuteScalar();
            }
            return result;
        }

        private int ExecuteNonQuery(string cmdText)
        {
            int rowsAffected;
            rowsAffected = 0;

            using (SqlCeConnection conn = new SqlCeConnection(ConnectionString))
            {
                conn.Open();
                SqlCeCommand cmd = conn.CreateCommand();
                cmd.CommandText = cmdText;
                rowsAffected = cmd.ExecuteNonQuery();
            }

            return rowsAffected;
        }

        private void SeedDB()
        {
            IOHelpers.LogSubroutine("SeedDB");

            string cmd;

            ExecuteNonQuery(@"DELETE OrderSession");
            ExecuteNonQuery(@"DELETE Product");
            ExecuteNonQuery(@"DELETE OrderSession_Product");

            for (int i = 0; i < 5; ++i)
            {
                cmd = String.Format(@"INSERT INTO OrderSession (Name) VALUES ('{0}')", DateTime.Now.Ticks.ToString());
                ExecuteNonQuery(cmd);
            }

            for (int i = 0; i < 50; ++i)
            {
                cmd = String.Format(@"INSERT INTO Product (Name, UPC) VALUES ({0},{1})", DateTime.Now.Ticks.ToString(), i);
                ExecuteNonQuery(cmd);
            }

            int casesToOrder = 0;
            foreach (OrderSession orderSession in OrderSessions)
            {
                foreach (Product product in Products)
                {
                    cmd = String.Format(@"INSERT INTO OrderSession_Product (OrderSessionID, ProductID, CasesToOrder) VALUES ({0},{1},{2})",
                        orderSession.ID, product.ID, ++casesToOrder);
                    ExecuteNonQuery(cmd);
                }
            }
        }

        private bool TableExists(string tableName)
        {
            IOHelpers.LogSubroutine("TableExists");

            string queryTemplate;
            string query;
            int count;
            bool tableExists;

            queryTemplate = @"SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = '{0}'";
            query = String.Format(queryTemplate, tableName);

            count = (int)ExecuteScalar(query);

            tableExists = count > 0;

            return tableExists;
        }

        private void CreateTables()
        {
            IOHelpers.LogSubroutine("CreateTables");

            string createTableQuery;    

            // 
            if (!TableExists("OrderSession"))
            {
                createTableQuery = @"CREATE TABLE OrderSession (ID INTEGER IDENTITY(1,1) PRIMARY KEY, Name NVARCHAR(100))";
                ExecuteNonQuery(createTableQuery);
            }                        

            //
            if (!TableExists("Product"))
            { 
                createTableQuery = @"CREATE TABLE Product (ID INTEGER IDENTITY(1,1) PRIMARY KEY, Name NVARCHAR(100), UPC INTEGER)";
                ExecuteNonQuery(createTableQuery);          
            }

            //
            if (!TableExists("OrderSession_Product"))
            {
                createTableQuery = @"CREATE TABLE OrderSession_Product (OrderSessionID INTEGER, ProductID INTEGER, CasesToOrder INTEGER, CONSTRAINT PK_OrderSession_Product PRIMARY KEY (OrderSessionID, ProductID))";
                ExecuteNonQuery(createTableQuery);
            }

        }

        private void CreateDB()
        {
            IOHelpers.LogSubroutine("CreateDB");

            SqlCeEngine engine;

            IOHelpers.CreateItemIfNotExists(Constants.ApplicationDataStoreFullPath);

            if (!File.Exists(Constants.DatabaseFullPath))
            {
                engine = new SqlCeEngine(ConnectionString);
                engine.CreateDatabase();
                engine.Dispose();
            }
        }

        public Database(string databaseName)
        {
            CreateDB();
            CreateTables();
            SeedDB();
        }

    }
}
