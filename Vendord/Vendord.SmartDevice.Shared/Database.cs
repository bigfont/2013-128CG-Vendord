﻿namespace Vendord.SmartDevice.Shared
{
    using System;
    using System.Collections.Generic;
    using System.Data.SqlServerCe;
    using System.Linq;
    using System.Text;
    using System.IO;
    using System.Collections.ObjectModel;

    public class VendordDatabase
    {        
        private List<Product> products;
        private List<OrderSession> orderSessions;
        private List<OrderSession_Product> orderSession_Products;

        private static string sqlCeConnectionStringTemplate = @"Data Source={0};Persist Security Info=False;";
        public static string GenerateSqlCeConnString(string databaseFullPath)
        {
            string sqlCeConnString;
            sqlCeConnString = String.Format(sqlCeConnectionStringTemplate, databaseFullPath);
            return sqlCeConnString;
        }

        public string ConnectionString
        {
            get
            {
                string result;
                result = GenerateSqlCeConnString(Constants.VendordDatabaseFullPath);
                return result;
            }
        }

        // TODO Use System.Reflection to make a DRY upsert method for OrderSession, Product, and Order_Product
        // TODO Change OrderSession to Order, because the former is misleading

        public class OrderSession
        {
            public int ID { get; set; }
            public string Name { get; set; }

            public void UpsertIntoDB()
            {
                string insertQuery;

                insertQuery = String.Format(@"INSERT INTO OrderSession (Name) VALUES ('{0}');",
                    this.Name);

                VendordDatabase db = new VendordDatabase();
                db.ExecuteNonQuery(insertQuery);

                // set the ID to the newly generated ID
                this.ID = db.OrderSessions.FirstOrDefault<OrderSession>(os => os.Name.Equals(this.Name)).ID;
            }
        }

        public class Product
        {
            public int ID { get; set; }
            public string UPC { get; set; } // TODO Change UPC into an INTEGER
            public string Name { get; set; }
            public string VendorName { get; set; }
            
            public void UpsertIntoDB()
            {
                string selectQuery;
                string insertQuery;
                string updateQuery;

                selectQuery = String.Format(@"SELECT COUNT(*) FROM Product WHERE UPC = '{0}'", 
                    this.UPC);

                insertQuery = String.Format(@"INSERT INTO Product (UPC, Name, VendorName) VALUES ('{0}', '{1}', '{2}')",
                    this.UPC,
                    this.Name,
                    this.VendorName);

                updateQuery = null; // TODO Add an update query if appropriate.

                VendordDatabase db = new VendordDatabase();

                if (Convert.ToInt16(db.ExecuteScalar(selectQuery)) == 0)
                {
                    db.ExecuteNonQuery(insertQuery);
                }
                else if(updateQuery != null)
                {
                    db.ExecuteNonQuery(updateQuery);
                }
            }
        }

        public class OrderSession_Product
        {
            public int OrderSessionID { get; set; }
            public int ProductID { get; set; }
            public int CasesToOrder { get; set; }

            public void UpsertIntoDB()
            {
                string selectQuery;
                string insertQuery;
                string updateQuery;

                selectQuery = String.Format(@"SELECT COUNT(*) FROM OrderSession_Product WHERE OrderSessionID = {0} AND ProductID = {1};",
                    this.OrderSessionID,
                    this.ProductID);

                insertQuery = String.Format(@"INSERT INTO OrderSession_Product (OrderSessionID, ProductID, CasesToOrder) VALUES ('{0}', '{1}', {2});",
                    this.OrderSessionID,
                    this.ProductID,
                    this.CasesToOrder);

                updateQuery = String.Format(@"UPDATE OrderSession_Product SET CasesToOrder = {2} WHERE OrderSessionID = {0} AND ProductID = {1};",
                    this.OrderSessionID,
                    this.ProductID,
                    this.CasesToOrder);

                VendordDatabase db = new VendordDatabase();

                if (Convert.ToInt16(db.ExecuteScalar(selectQuery)) == 0)
                {
                    db.ExecuteNonQuery(insertQuery);
                }
                else
                {
                    db.ExecuteNonQuery(updateQuery);
                }
            }
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
                                UPC = Convert.ToString(reader["UPC"]),
                                VendorName = Convert.ToString(reader["VendorName"])
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

        public object ExecuteScalar(string cmdText)
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

        public int ExecuteNonQuery(string cmdText)
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
                createTableQuery = @"CREATE TABLE OrderSession (ID INTEGER IDENTITY(1,1) PRIMARY KEY, Name NVARCHAR(100) UNIQUE)";
                ExecuteNonQuery(createTableQuery);
            }

            //
            if (!TableExists("Product"))
            {
                createTableQuery = @"CREATE TABLE Product (ID INTEGER IDENTITY(1,1) PRIMARY KEY, Name NVARCHAR(100), UPC NVARCHAR(100) UNIQUE, VendorName NVARCHAR(100))";
                ExecuteNonQuery(createTableQuery);
            }

            //
            if (!TableExists("OrderSession_Product"))
            {
                createTableQuery = @"CREATE TABLE OrderSession_Product (OrderSessionID INTEGER, ProductID INTEGER, CasesToOrder INTEGER, CONSTRAINT PK_OrderSession_Product PRIMARY KEY (OrderSessionID, ProductID))";
                ExecuteNonQuery(createTableQuery);
            }

        }

        public static void CreateCeDB(string databaseFullPath)
        {
            IOHelpers.LogSubroutine("CreateDB");

            SqlCeEngine engine;
            string connString;

            IOHelpers.CreateDirectoryIfNotExists(Constants.ApplicationDataStoreFullPath);

            if (!File.Exists(databaseFullPath))
            {
                connString = GenerateSqlCeConnString(databaseFullPath);
                engine = new SqlCeEngine(connString);
                engine.CreateDatabase();
                engine.Dispose();
            }
        }

        public VendordDatabase()
        {
            CreateCeDB(Constants.VendordDatabaseFullPath);
            CreateTables();
        }

    }
}
