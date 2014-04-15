[module:
    System.Diagnostics.CodeAnalysis.SuppressMessage(
        "StyleCop.CSharp.DocumentationRules", "*",
        Justification = "Reviewed. Suppression of all documentation rules is OK here.")]

namespace Vendord.SmartDevice.Linked
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.SqlServerCe;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Text;

    public abstract class DbEntity<T>
    {
        public DbEntity() 
        { 
        }

        public DbEntity(DbQueryExecutor queryExecutor)
        {
            this.QueryExecutor = queryExecutor;
        }

        public int? IsInTrash { get; set; }

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

        public DbQueryExecutor QueryExecutor
        {
            get;
            set;
        }

        public void EmptyTrash()
        {
            string emptyTrashQuery;
            emptyTrashQuery = string.Format(
                @"DELETE {0} WHERE (IsInTrash = 1)",
                this.TableName);

            this.QueryExecutor.ExecuteNonQuery(emptyTrashQuery, null);
        }

        public SqlCeDataReader SelectAllReader()
        {
            StringBuilder query = new StringBuilder();
            query.AppendFormat(@"SELECT * FROM {0} WHERE IsInTrash IS NULL OR IsInTrash = 0", this.TableName);
            return this.QueryExecutor.ExecuteReader(query.ToString());
        }

        public abstract void AddToTrash();

        public abstract List<T> SelectAll();
    }

    public class Vendor : DbEntity<Vendor>
    {
        public Vendor() : base() 
        { 
        }

        public Vendor(DbQueryExecutor qe) : base(qe) 
        { 
        }

        public int? Id { get; set; }

        public string Name { get; set; }

        public override void AddToTrash()
        {
            string trashQuery;

            trashQuery = string.Format(
                @"UPDATE {0} SET IsInTrash = 1 WHERE Id = '{1}'",
                this.TableName,
                this.Id);

            QueryExecutor.ExecuteNonQuery(trashQuery, null);
        }

        public void UpsertIntoDb()
        {
            // create parameters
            var parameters = new SqlCeParameter[]
            {
                new SqlCeParameter() { ParameterName = "@Id", SqlDbType = SqlDbType.Int, Value = this.Id },                
                new SqlCeParameter() { ParameterName = "@Name", SqlDbType = SqlDbType.NVarChar, Value = this.Name }                
            };

            // exists
            string selectQuery =
                string.Format(
                    @"SELECT COUNT(*) FROM {0} WHERE Id = @Id",
                    this.TableName);
            object result = QueryExecutor.ExecuteScalar(selectQuery, parameters);
            bool exists = Convert.ToInt16(result.ToString()) > 0;

            // upsert
            if (exists)
            {
                // update
                string updateQuery =
                    string.Format(
                        @"UPDATE {0} SET
                            Name = @Name                            
                            WHERE Id = @Id",
                        this.TableName);

                this.QueryExecutor.ExecuteNonQuery(updateQuery, parameters);
            }
            else
            {
                // insert
                string insertQuery =
                    string.Format(
                        @"INSERT INTO {0} (Id, Name) 
                            VALUES (@Id, @Name)",
                    this.TableName);
                this.QueryExecutor.ExecuteNonQuery(insertQuery, parameters);
            }            
        }

        public override List<Vendor> SelectAll()
        {
            List<Vendor> list = new List<Vendor>();
            using (SqlCeDataReader reader = SelectAllReader())
            {
                while (reader.Read())
                {
                    Vendor item = new Vendor(this.QueryExecutor);

                    try
                    {
                        item.Id = Convert.ToInt32(reader["Id"].ToString());
                    }
                    catch (Exception)
                    {
                        item.Id = null;
                    }

                    item.Name = Convert.ToString(reader["Name"]);

                    list.Add(item);
                }
            }

            return list;
        }

        public override string ToString()
        {
            return this.Name;
        }
    }

    public class Department : DbEntity<Department>
    {
        public Department() : base() 
        { 
        }

        public Department(DbQueryExecutor qe) : base(qe) 
        { 
        }

        public int? Id { get; set; }

        public string Name { get; set; }

        public override void AddToTrash()
        {
            string trashQuery;

            trashQuery = string.Format(
                @"UPDATE {0} SET IsInTrash = 1 WHERE Id = '{1}'",
                this.TableName,
                this.Id);

            QueryExecutor.ExecuteNonQuery(trashQuery, null);
        }

        public void UpsertIntoDb()
        {
            // create parameters
            var parameters = new SqlCeParameter[]
            {
                new SqlCeParameter() { ParameterName = "@Id", SqlDbType = SqlDbType.Int, Value = this.Id },                
                new SqlCeParameter() { ParameterName = "@Name", SqlDbType = SqlDbType.NVarChar, Value = this.Name }                
            };

            // exists
            string selectQuery =
                string.Format(
                    @"SELECT COUNT(*) FROM {0} WHERE Id = @Id",
                    this.TableName);
            object result = QueryExecutor.ExecuteScalar(selectQuery, parameters);
            bool exists = Convert.ToInt16(result.ToString()) > 0;

            // upsert
            if (exists)
            {
                // update
                string updateQuery =
                    string.Format(
                        @"UPDATE {0} SET
                            Name = @Name                            
                            WHERE Id = @Id",
                        this.TableName);

                this.QueryExecutor.ExecuteNonQuery(updateQuery, parameters);
            }
            else
            {
                // insert
                string insertQuery =
                    string.Format(
                        @"INSERT INTO {0} (Id, Name) 
                            VALUES (@Id, @Name)",
                    this.TableName);
                this.QueryExecutor.ExecuteNonQuery(insertQuery, parameters);
            }   
        }

        public override List<Department> SelectAll()
        {
            List<Department> list = new List<Department>();
            using (SqlCeDataReader reader = SelectAllReader())
            {
                while (reader.Read())
                {
                    Department item = new Department(this.QueryExecutor);
                    try
                    {
                        item.Id = Convert.ToInt32(reader["id"]);
                    }
                    catch (Exception)
                    {
                        item.Id = null;
                    }

                    this.Name = Convert.ToString(reader["Name"]);
                    list.Add(item);
                }
            }

            return list;
        }

        public override string ToString()
        {
            return this.Name;
        }
    }

    public class Order : DbEntity<Order>
    {
        public Order() : base()
        {
        }

        public Order(DbQueryExecutor qe) : base(qe) 
        { 
        }

        public Guid Id { get; set; }

        public string Name { get; set; }

        public void UpsertIntoDb(Database db)
        {
            // create parameters
            var parameters = new SqlCeParameter[]
            {
                new SqlCeParameter() { ParameterName = "@Id", SqlDbType = SqlDbType.UniqueIdentifier, Value = this.Id },                
                new SqlCeParameter() { ParameterName = "@Name", SqlDbType = SqlDbType.NVarChar, Value = this.Name }                
            };

            // exists
            string selectQuery =
                string.Format(
                    @"SELECT COUNT(*) FROM {0} WHERE Id = @Id",
                    this.TableName);
            object result = QueryExecutor.ExecuteScalar(selectQuery, parameters);
            bool exists = Convert.ToInt16(result.ToString()) > 0;

            // upsert
            if (exists)
            {
                // update
                string updateQuery =
                    string.Format(
                        @"UPDATE {0} SET
                            Name = @Name                            
                            WHERE Id = @Id",
                        this.TableName);

                this.QueryExecutor.ExecuteNonQuery(updateQuery, parameters);
            }
            else
            {
                // insert
                string insertQuery =
                    string.Format(
                        @"INSERT INTO {0} (Id, Name) 
                            VALUES (NEWID(), @Name)",
                    this.TableName);
                this.QueryExecutor.ExecuteNonQuery(insertQuery, parameters);
            }  

            // set the Id to the newly generated Id
            this.Id = db.Orders.FirstOrDefault<Order>(os => os.Name.Equals(this.Name)).Id;
        }

        public override void AddToTrash()
        {
            string trashQuery;

            trashQuery = string.Format(
                @"UPDATE {0} SET IsInTrash = 1 WHERE Id = '{1}'",
                this.TableName,
                this.Id);

            QueryExecutor.ExecuteNonQuery(trashQuery, null);
        }

        public override List<Order> SelectAll()
        {
            List<Order> list = new List<Order>();
            using (SqlCeDataReader reader = SelectAllReader())
            {
                while (reader.Read())
                {
                    Order item = new Order(this.QueryExecutor)
                    {
                        Id = new Guid(reader["Id"].ToString()),
                        Name = Convert.ToString(reader["Name"])
                    };
                    list.Add(item);
                }
            }

            return list;
        }

        public override string ToString()
        {
            return this.Name;
        }
    }

    public class Product : DbEntity<Product>
    {
        public Product() : base() 
        { 
        }

        public Product(DbQueryExecutor qe)
            : base(qe)
        {
            Department = new Department(qe);
            Vendor = new Vendor(qe);
        }

        #region Properties

        // int64
        public long Upc { get; set; }

        public long CertCode { get; set; }

        public string Name { get; set; }

        public decimal? Price { get; set; }

        public Vendor Vendor { get; set; }

        public Department Department { get; set; }

        #endregion

        public void UpsertIntoDb()
        {
            // create parameters
            var parameters = new SqlCeParameter[]
            {
                // BigInt is an int64
                new SqlCeParameter() { ParameterName = "@Upc", SqlDbType = SqlDbType.BigInt, Value = this.Upc },
                new SqlCeParameter() { ParameterName = "@CertCode", SqlDbType = SqlDbType.BigInt, Value = this.CertCode },
                new SqlCeParameter() { ParameterName = "@Name", SqlDbType = SqlDbType.NVarChar, Value = this.Name },
                new SqlCeParameter() { ParameterName = "@Price", SqlDbType = SqlDbType.Decimal, Value = this.Price },
                new SqlCeParameter() { ParameterName = "@VendorId", SqlDbType = SqlDbType.Int, Value = this.Vendor.Id },
                new SqlCeParameter() { ParameterName = "@DepartmentId", SqlDbType = SqlDbType.Int, Value = this.Department.Id }
            };

            // exists
            string selectQuery =
                string.Format(
                    @"SELECT COUNT(*) FROM {0} WHERE Upc = @Upc",
                    this.TableName);
            object result = QueryExecutor.ExecuteScalar(selectQuery, parameters);
            bool exists = Convert.ToInt16(result.ToString()) > 0;

            // upsert
            if (exists)
            {
                // update
                string updateQuery =
                    string.Format(
                        @"UPDATE {0} SET
                            CertCode = @CertCode,
                            Name = @Name, 
                            Price = @Price, 
                            VendorId = @VendorId, 
                            DepartmentId = @DepartmentId
                            WHERE Upc = @Upc",
                        this.TableName);

                this.QueryExecutor.ExecuteNonQuery(updateQuery, parameters);
            }
            else
            {
                // insert
                string insertQuery =
                    string.Format(
                        @"INSERT INTO {0} (Upc, CertCode, Name, Price, VendorId, DepartmentId) 
                            VALUES (@Upc, @CertCode, @Name, @Price, @VendorId, @DepartmentId)",
                    this.TableName);
                this.QueryExecutor.ExecuteNonQuery(insertQuery, parameters);
            }
        }

        public override void AddToTrash()
        {
            string trashQuery;

            trashQuery = string.Format(
                @"UPDATE {0} SET IsInTrash = 1 WHERE Upc = '{1}'",
                this.TableName,
                this.Upc);

            QueryExecutor.ExecuteNonQuery(trashQuery, null);
        }

        public override List<Product> SelectAll()
        {
            return this.SelectAllWithJoin();
        }

        public Product SelectOne(long upc)
        {
            string query = string.Format(
                @"SELECT * FROM {0} WHERE Upc = {1}",
                this.TableName, 
                upc);

            Product product = null;
            using (SqlCeDataReader reader = this.QueryExecutor.ExecuteReader(query))
            {
                while (reader.Read())
                {
                    product = this.CreateProductFromReader(reader);
                }
            }

            return product;
        }

        public List<Product> SelectAllWithJoin()
        {
            List<Product> list = new List<Product>();

            string query = @"
                SELECT main.*, v.Name as VendorName, d.Name as DepartmentName
                FROM tblProduct main 
                JOIN tblVendor v ON main.VendorId = v.Id 
                JOIN tblDepartment d ON main.DepartmentId = d.Id 
                WHERE main.IsInTrash IS NULL OR main.IsInTrash = 0
            ";

            using (SqlCeDataReader reader = QueryExecutor.ExecuteReader(query))
            {
                while (reader.Read())
                {
                    Product product = this.CreateProductFromReader(reader);
                    list.Add(product);
                }
            }

            return list;
        }

        public override string ToString()
        {
            return this.Name;
        }

        private Product CreateProductFromReader(SqlCeDataReader reader)
        {
            Product product = new Product(this.QueryExecutor);

            string colName;

            // upc         
            colName = "Upc";
            if (QueryExecutor.ReaderHasColumn(reader, colName) && reader[colName] != null)
            {
                product.Upc = Convert.ToInt64(reader[colName].ToString());
            }

            // cert code
            colName = "CertCode";
            if (QueryExecutor.ReaderHasColumn(reader, colName) && reader[colName] != null)
            {
                product.CertCode = Convert.ToInt64(reader[colName].ToString());
            }

            // name         
            colName = "Name";
            if (QueryExecutor.ReaderHasColumn(reader, colName) && reader[colName] != null)
            {
                product.Name = reader[colName].ToString();
            }

            // price
            colName = "Price";
            if (QueryExecutor.ReaderHasColumn(reader, colName) && reader[colName] != null)
            {
                product.Price = Convert.ToDecimal(reader[colName].ToString());
            }

            // vendor id
            colName = "VendorId";
            if (QueryExecutor.ReaderHasColumn(reader, colName) && reader[colName] != null)
            {
                product.Vendor.Id = Convert.ToInt32(reader[colName].ToString());
            }

            // department id
            colName = "DepartmentId";
            if (QueryExecutor.ReaderHasColumn(reader, colName) && reader[colName] != null)
            {
                product.Department.Id = Convert.ToInt32(reader[colName].ToString());
            }

            // vendor name
            colName = "VendorName";
            if (QueryExecutor.ReaderHasColumn(reader, colName) && reader[colName] != null)
            {
                product.Vendor.Name = reader[colName].ToString();
            }

            // department name
            colName = "DepartmentName";
            if (QueryExecutor.ReaderHasColumn(reader, colName) && reader[colName] != null)
            {
                product.Department.Name = reader[colName].ToString();
            }

            return product;
        }
    }

    public class OrderProduct : DbEntity<OrderProduct>
    {
        public OrderProduct(DbQueryExecutor qe) : base(qe) 
        { 
        }

        public Guid OrderId { get; set; }

        // int64
        public long ProductUPC { get; set; }

        public int CasesToOrder { get; set; }

        public void UpsertIntoDb()
        {
            string selectQuery = string.Format(
                @"SELECT COUNT(*) FROM " + this.TableName + " WHERE OrderId = '{0}' AND ProductUPC = '{1}';",
                this.OrderId,
                this.ProductUPC);

            string insertQuery = string.Format(
                @"INSERT INTO " + this.TableName + " (OrderId, ProductUPC, CasesToOrder) VALUES ('{0}', '{1}', {2});",
                this.OrderId,
                this.ProductUPC,
                this.CasesToOrder);

            string updateQuery = string.Format(
                @"UPDATE " + this.TableName + " SET CasesToOrder = {2}, IsInTrash = 0 WHERE OrderId = '{0}' AND ProductUPC = '{1}';",
                this.OrderId,
                this.ProductUPC,
                this.CasesToOrder);

            if (Convert.ToInt16(QueryExecutor.ExecuteScalar(selectQuery, null)) == 0)
            {
                // TODO Add a code contract to ensure that both the order and the product exist in the database before insert
                QueryExecutor.ExecuteNonQuery(insertQuery, null);
            }
            else
            {
                QueryExecutor.ExecuteNonQuery(updateQuery, null);
            }
        }

        public override void AddToTrash()
        {
            string trashQuery;

            trashQuery = string.Format(
                @"UPDATE {0} SET IsInTrash = 1 WHERE OrderId = '{1}' AND ProductUPC = '{2}'",
                this.TableName,
                this.OrderId,
                this.ProductUPC);

            QueryExecutor.ExecuteNonQuery(trashQuery, null);
        }

        public override List<OrderProduct> SelectAll()
        {
            List<OrderProduct> list = new List<OrderProduct>();
            using (SqlCeDataReader reader = SelectAllReader())
            {
                while (reader.Read())
                {
                    OrderProduct item = new OrderProduct(this.QueryExecutor)
                    {
                        OrderId = new Guid(reader["OrderId"].ToString()),
                        ProductUPC = Convert.ToInt64(reader["ProductUPC"].ToString()),
                        CasesToOrder = Convert.ToInt16(reader["CasesToOrder"])
                    };
                    list.Add(item);
                }
            }

            return list;
        }
    }
}
