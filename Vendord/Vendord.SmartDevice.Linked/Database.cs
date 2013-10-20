[module:
    System.Diagnostics.CodeAnalysis.SuppressMessage(
        "StyleCop.CSharp.DocumentationRules", "*",
        Justification = "Reviewed. Suppression of all documentation rules is OK here.")]

namespace Vendord.SmartDevice.Shared
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Data;
    using System.Data.Common;
    using System.Data.SqlServerCe;
    using System.IO;
    using System.Linq;
    using System.Text;

    public class VendordDatabase
    {
        private static string sqlCeConnectionStringTemplate = @"Data Source={0};Persist Security Info=False;";

        private List<Product> products;
        private List<Order> order;
        private List<OrderProduct> orderProducts;
        private string connectionString;

        public VendordDatabase()
        {
            string fullPath = Constants.VendordDatabaseFullPath;
            this.connectionString = GenerateSqlCeConnString(fullPath);
            this.CreateCeDB(fullPath);
            this.CreateTables();
        }

        public VendordDatabase(string fullPath)
        {
            this.connectionString = GenerateSqlCeConnString(fullPath);
            this.CreateCeDB(fullPath);
            this.CreateTables();
        }

        public List<Order> Orders
        {
            get
            {
                SqlCeDataReader reader;
                System.Data.SqlServerCe.SqlCeCommand command;

                if (this.order == null)
                {
                    this.order = new List<Order>();
                    using (SqlCeConnection conn = new SqlCeConnection(this.connectionString))
                    {
                        conn.Open();
                        command = new SqlCeCommand(@"SELECT * FROM tblOrder WHERE IsInTrash IS NULL OR IsInTrash = 0", conn);
                        reader = command.ExecuteReader();
                        while (reader.Read())
                        {
                            Order item = new Order()
                            {
                                ID = new Guid(reader["ID"].ToString()),
                                Name = Convert.ToString(reader["Name"])
                            };
                            this.order.Add(item);
                        }
                    }
                }

                return this.order;
            }
        }

        public List<Product> Products
        {
            get
            {
                SqlCeDataReader reader;
                SqlCeCommand command;

                if (this.products == null)
                {
                    this.products = new List<Product>();
                    using (SqlCeConnection conn = new SqlCeConnection(this.connectionString))
                    {
                        conn.Open();
                        command = new SqlCeCommand(@"SELECT * FROM tblProduct", conn);
                        reader = command.ExecuteReader();
                        while (reader.Read())
                        {
                            Product item = new Product()
                            {
                                ID = new Guid(reader["ID"].ToString()),
                                Name = Convert.ToString(reader["Name"]),
                                UPC = Convert.ToString(reader["UPC"]),
                                VendorName = Convert.ToString(reader["VendorName"])
                            };
                            this.products.Add(item);
                        }
                    }
                }

                return this.products;
            }
        }

        public List<OrderProduct> OrderProducts
        {
            get
            {
                if (this.orderProducts == null)
                {
                    SqlCeDataReader reader;
                    SqlCeCommand command;

                    this.orderProducts = new List<OrderProduct>();
                    using (SqlCeConnection conn = new SqlCeConnection(this.connectionString))
                    {
                        conn.Open();
                        command = new SqlCeCommand(@"SELECT * FROM tblOrderProduct", conn);
                        reader = command.ExecuteReader();
                        while (reader.Read())
                        {
                            OrderProduct item = new OrderProduct()
                            {
                                ProductID = new Guid(reader["ProductID"].ToString()),
                                OrderID = new Guid(reader["OrderID"].ToString()),
                                CasesToOrder = Convert.ToInt32(reader["CasesToOrder"])
                            };
                            this.orderProducts.Add(item);
                        }
                    }
                }

                return this.orderProducts;
            }
        }

        public static string GenerateSqlCeConnString(string databaseFullPath)
        {
            string sqlCeConnString;
            sqlCeConnString = string.Format(sqlCeConnectionStringTemplate, databaseFullPath);
            return sqlCeConnString;
        }

        public void EmptyTrash()
        {
            (new Order()).EmptyTrash(this);
            (new Product()).EmptyTrash(this);
            (new OrderProduct()).EmptyTrash(this);
        }

        public object ExecuteScalar(string cmdText)
        {
            object result = null;
            using (SqlCeConnection conn = new SqlCeConnection(this.connectionString))
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

            using (SqlCeConnection conn = new SqlCeConnection(this.connectionString))
            {
                conn.Open();
                SqlCeCommand cmd = conn.CreateCommand();
                cmd.CommandText = cmdText;
                rowsAffected = cmd.ExecuteNonQuery();
            }

            return rowsAffected;
        }

        public void CreateCeDB(string databaseFullPath)
        {
            IOHelpers.LogSubroutine("CreateDB");
            SqlCeEngine engine;

            // create the database
            IOHelpers.CreateDirectoryIfNotExists(Constants.ApplicationDataStoreFullPath);
            if (!File.Exists(databaseFullPath))
            {
                engine = new SqlCeEngine(this.connectionString);
                engine.CreateDatabase();
                engine.Dispose();
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
            query = string.Format(queryTemplate, tableName);

            count = (int)this.ExecuteScalar(query);

            tableExists = count > 0;

            return tableExists;
        }

        private void CreateTables()
        {
            IOHelpers.LogSubroutine("CreateTables");

            string createTableQuery;

            if (!this.TableExists("tblOrder"))
            {
                createTableQuery
                    = @"CREATE TABLE tblOrder 
                    (ID uniqueidentifier PRIMARY KEY, Name NVARCHAR(100), IsInTrash BIT)";

                this.ExecuteNonQuery(createTableQuery);
            }

            if (!this.TableExists("tblProduct"))
            {
                createTableQuery
                    = @"CREATE TABLE tblProduct 
                    (ID uniqueidentifier PRIMARY KEY, Name NVARCHAR(100), UPC NVARCHAR(100) UNIQUE, VendorName NVARCHAR(100), IsInTrash BIT)";

                this.ExecuteNonQuery(createTableQuery);
            }

            if (!this.TableExists("tblOrderProduct"))
            {
                createTableQuery
                    = @"CREATE TABLE tblOrderProduct 
                    (OrderID uniqueidentifier, ProductID uniqueidentifier, CasesToOrder INTEGER, CONSTRAINT PK_OrderProduct PRIMARY KEY (OrderID, ProductID), IsInTrash BIT)";

                this.ExecuteNonQuery(createTableQuery);
            }
        }

        /// <summary>
        /// Represents an business object.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The order system works in a distributed environment, where users can INSERT orders on sereral machines (nodes). 
        /// Ergo, the PRIMARY KEY must be unique across all nodes and must not be reused. Options:
        /// 1. Auto-increment (Identity) Columns. High probability of PK collision if INSERT operations occur in multiple nodes. 
        /// 2. GUIDs. Essentially no probability of PK collisions but can lead to large and fragmented clustered indexes.
        /// 3. Keys that Include a Node Identifier. Add a node identifier to the autogenerated identity value.
        /// 4. Natural Keys. Use column values such as Social Insurance Numbers or UPC codes that must be unique already.
        /// 5. Online insertion.
        /// </para>
        /// <seealso>
        /// Selecting PKs for SYNC http://msdn.microsoft.com/en-us/library/bb726011.aspx
        /// GUID Generation http://social.msdn.microsoft.com/Forums/sqlserver/en-US/af52661f-7eb5-4c73-87e8-2d9ad195e112/algorithm-to-generate-guids-in-sql-server?forum=transactsql
        /// </seealso>
        /// </remarks>       
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
            public Guid ID { get; set; }

            public int IsInTrash { get; set; }

            public void AddToTrash(VendordDatabase db)
            {
                string trashQuery;

                trashQuery = string.Format(
                    @"UPDATE {0} SET IsInTrash = 1 WHERE ID = '{1}'",
                    this.TableName,
                    this.ID);

                db.ExecuteNonQuery(trashQuery);
            }

            public void EmptyTrash(VendordDatabase db)
            {
                string emptyTrashQuery;
                emptyTrashQuery = string.Format(
                    @"DELETE {0} WHERE (IsInTrash = 1)",
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

                insertQuery = string.Format(
                    @"INSERT INTO tblOrder (ID, Name) VALUES (NEWID(), '{0}');",
                    this.Name);

                db.ExecuteNonQuery(insertQuery);

                // set the ID to the newly generated ID
                this.ID = db.Orders.FirstOrDefault<Order>(os => os.Name.Equals(this.Name)).ID;
            }

            public override string ToString()
            {
                return this.Name;
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

                selectQuery = string.Format(
                    @"SELECT COUNT(*) FROM tblProduct WHERE UPC = '{0}'",
                    this.UPC);

                insertQuery = string.Format(
                    @"INSERT INTO tblProduct (ID, UPC, Name, VendorName) VALUES (NEWID(), '{0}', '{1}', '{2}')",
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

            public override string ToString()
            {
                return this.Name;
            }
        }

        public class OrderProduct : DbEntity
        {
            public Guid OrderID { get; set; }

            public Guid ProductID { get; set; }

            public int CasesToOrder { get; set; }

            public void UpsertIntoDB(VendordDatabase db)
            {
                string selectQuery;
                string insertQuery;
                string updateQuery;

                selectQuery = string.Format(
                    @"SELECT COUNT(*) FROM tblOrderProduct WHERE OrderID = '{0}' AND ProductID = '{1}';",
                    this.OrderID,
                    this.ProductID);

                insertQuery = string.Format(
                    @"INSERT INTO tblOrderProduct (OrderID, ProductID, CasesToOrder) VALUES ('{0}', '{1}', {2});",
                    this.OrderID,
                    this.ProductID,
                    this.CasesToOrder);

                updateQuery = string.Format(
                    @"UPDATE tblOrderProduct SET CasesToOrder = {2} WHERE OrderID = '{0}' AND ProductID = '{1}';",
                    this.OrderID,
                    this.ProductID,
                    this.CasesToOrder);

                if (Convert.ToInt16(db.ExecuteScalar(selectQuery)) == 0)
                {
                    // TODO Add a code contract to ensure that both the order and the product exist in the database before insert
                    db.ExecuteNonQuery(insertQuery);
                }
                else
                {
                    db.ExecuteNonQuery(updateQuery);
                }
            }
        }
    }
}
