namespace Vendord.SmartDevice.Shared
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.Common;
    using System.Data.SqlServerCe;
    using System.Linq;
    using System.Text;
    using System.IO;
    using System.Collections.ObjectModel;

    public class VendordDatabase
    {
        private List<Product> products;
        private List<Order> order;
        private List<Order_Product> order_Products;
        private string connectionString;

        private static string sqlCeConnectionStringTemplate = @"Data Source={0};Persist Security Info=False;";
        public static string GenerateSqlCeConnString(string databaseFullPath)
        {
            string sqlCeConnString;
            sqlCeConnString = String.Format(sqlCeConnectionStringTemplate, databaseFullPath);
            return sqlCeConnString;
        }

        // TODO Use System.Reflection to make a DRY upsert method for order, Product, and Order_Product        

        public class DbEntity
        {
            // non-database columns
            public string TableName
            {
                get
                {
                    string tableName;
                    tableName = "tbl" + this.GetType().Name;
                    return tableName;
                }
            }

            // database columns
            public int ID { get; set; }
            public int IsInTrash { get; set; }

            public void AddToTrash(VendordDatabase db)
            {
                string trashQuery;

                trashQuery = String.Format(@"UPDATE {0} SET IsInTrash = 1 WHERE ID = {1}",
                    this.TableName,
                    this.ID);
                
                db.ExecuteNonQuery(trashQuery);
            }

            public void EmptyTrash(VendordDatabase db)
            {
                string emptyTrashQuery;
                emptyTrashQuery = String.Format(@"DELETE {0} WHERE (IsInTrash = 1)", 
                    this.TableName);

                db.ExecuteNonQuery(emptyTrashQuery);

            }
        }

        public class Order : DbEntity
        {
            public string Name { get; set; }

            public void UpsertIntoDB(VendordDatabase db)
            {
                string insertQuery;

                insertQuery = String.Format(@"INSERT INTO tblOrder (Name) VALUES ('{0}');",
                    this.Name);

                db.ExecuteNonQuery(insertQuery);

                // set the ID to the newly generated ID
                this.ID = db.Orders.FirstOrDefault<Order>(os => os.Name.Equals(this.Name)).ID;
            }
        }

        public class Product : DbEntity
        {
            public string UPC { get; set; } // TODO Change UPC into an INTEGER
            public string Name { get; set; }
            public string VendorName { get; set; }

            public void UpsertIntoDB(VendordDatabase db)
            {
                string selectQuery;
                string insertQuery;
                string updateQuery;

                selectQuery = String.Format(@"SELECT COUNT(*) FROM tblProduct WHERE UPC = '{0}'",
                    this.UPC);

                insertQuery = String.Format(@"INSERT INTO tblProduct (UPC, Name, VendorName) VALUES ('{0}', '{1}', '{2}')",
                    this.UPC,
                    this.Name,
                    this.VendorName);

                updateQuery = null; // TODO Add an update query if appropriate.                

                if (Convert.ToInt16(db.ExecuteScalar(selectQuery)) == 0)
                {
                    db.ExecuteNonQuery(insertQuery);
                }
                else if (updateQuery != null)
                {
                    db.ExecuteNonQuery(updateQuery);
                }
            }
        }

        public class Order_Product : DbEntity
        {
            public int OrderID { get; set; }
            public int ProductID { get; set; }
            public int CasesToOrder { get; set; }

            public void UpsertIntoDB(VendordDatabase db)
            {
                string selectQuery;
                string insertQuery;
                string updateQuery;

                selectQuery = String.Format(@"SELECT COUNT(*) FROM tblOrder_Product WHERE OrderID = {0} AND ProductID = {1};",
                    this.OrderID,
                    this.ProductID);

                insertQuery = String.Format(@"INSERT INTO tblOrder_Product (OrderID, ProductID, CasesToOrder) VALUES ('{0}', '{1}', {2});",
                    this.OrderID,
                    this.ProductID,
                    this.CasesToOrder);

                updateQuery = String.Format(@"UPDATE tblOrder_Product SET CasesToOrder = {2} WHERE OrderID = {0} AND ProductID = {1};",
                    this.OrderID,
                    this.ProductID,
                    this.CasesToOrder);

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

        public List<Order> Orders
        {
            get
            {

                System.Data.SqlServerCe.SqlCeDataReader reader;
                System.Data.SqlServerCe.SqlCeCommand command;

                if (order == null)
                {
                    order = new List<Order>();
                    using (SqlCeConnection conn = new SqlCeConnection(connectionString))
                    {
                        conn.Open();
                        command = new SqlCeCommand(@"SELECT * FROM tblOrder WHERE IsInTrash IS NULL OR IsInTrash = 0", conn);
                        reader = command.ExecuteReader();
                        while (reader.Read())
                        {
                            Order item = new Order()
                            {
                                ID = Convert.ToInt32(reader["ID"]),
                                Name = Convert.ToString(reader["Name"])
                            };
                            order.Add(item);
                        }
                    }
                }
                return order;
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
                    using (SqlCeConnection conn = new SqlCeConnection(connectionString))
                    {
                        conn.Open();
                        command = new SqlCeCommand(@"SELECT * FROM tblProduct", conn);
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

        public List<Order_Product> Order_Products
        {
            get
            {
                if (order_Products == null)
                {
                    SqlCeDataReader reader;
                    SqlCeCommand command;

                    order_Products = new List<Order_Product>();
                    using (SqlCeConnection conn = new SqlCeConnection(connectionString))
                    {
                        conn.Open();
                        command = new SqlCeCommand(@"SELECT * FROM tblOrder_Product", conn);
                        reader = command.ExecuteReader();
                        while (reader.Read())
                        {
                            Order_Product item = new Order_Product()
                            {
                                ProductID = Convert.ToInt32(reader["ProductID"]),
                                OrderID = Convert.ToInt32(reader["OrderID"]),
                                CasesToOrder = Convert.ToInt32(reader["CasesToOrder"])
                            };
                            order_Products.Add(item);
                        }
                    }
                }
                return order_Products;
            }
        }

        public void EmptyTrash()
        {
            (new Order()).EmptyTrash(this);
            (new Product()).EmptyTrash(this);
            (new Order_Product()).EmptyTrash(this);
        }

        public object ExecuteScalar(string cmdText)
        {
            object result = null;
            using (SqlCeConnection conn = new SqlCeConnection(connectionString))
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

            using (SqlCeConnection conn = new SqlCeConnection(connectionString))
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
            if (!TableExists("tblOrder"))
            {
                createTableQuery
                    = @"CREATE TABLE tblOrder 
                    (ID INTEGER IDENTITY(1,1) PRIMARY KEY, Name NVARCHAR(100), IsInTrash BIT)";

                ExecuteNonQuery(createTableQuery);
            }

            //
            if (!TableExists("tblProduct"))
            {
                createTableQuery
                    = @"CREATE TABLE tblProduct 
                    (ID INTEGER IDENTITY(1,1) PRIMARY KEY, Name NVARCHAR(100), UPC NVARCHAR(100) UNIQUE, VendorName NVARCHAR(100), IsInTrash BIT)";

                ExecuteNonQuery(createTableQuery);
            }

            //
            if (!TableExists("tblOrder_Product"))
            {
                createTableQuery
                    = @"CREATE TABLE tblOrder_Product 
                    (OrderID INTEGER, ProductID INTEGER, CasesToOrder INTEGER, CONSTRAINT PK_Order_Product PRIMARY KEY (OrderID, ProductID), IsInTrash BIT)";

                ExecuteNonQuery(createTableQuery);
            }

        }

        public void CreateCeDB(string databaseFullPath)
        {
            IOHelpers.LogSubroutine("CreateDB");
            SqlCeEngine engine;

            // create the database
            IOHelpers.CreateDirectoryIfNotExists(Constants.ApplicationDataStoreFullPath);
            if (!File.Exists(databaseFullPath))
            {
                engine = new SqlCeEngine(connectionString);
                engine.CreateDatabase();
                engine.Dispose();
            }
        }

        public VendordDatabase()
        {
            string fullPath = Constants.VendordDatabaseFullPath;
            connectionString = GenerateSqlCeConnString(fullPath);
            CreateCeDB(fullPath);
            CreateTables();
        }

        public VendordDatabase(string fullPath)
        {
            connectionString = GenerateSqlCeConnString(fullPath);
            CreateCeDB(fullPath);
            CreateTables();
        }
    }
}
