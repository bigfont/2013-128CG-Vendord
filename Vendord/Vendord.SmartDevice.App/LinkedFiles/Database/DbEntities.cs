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
    using System.IO;
    using System.Linq;
    using System.Text;

    public abstract class DbEntity<T>
    {
        public DbQueryExecutor queryExecutor;

        public DbEntity()
        { 
        
        }

        public DbEntity(DbQueryExecutor queryExecutor)
        {
            this.queryExecutor = queryExecutor;
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

        public void EmptyTrash()
        {
            string emptyTrashQuery;
            emptyTrashQuery = string.Format(
                @"DELETE {0} WHERE (IsInTrash = 1)",
                this.TableName);

            queryExecutor.ExecuteNonQuery(emptyTrashQuery, null);
        }

        public SqlCeDataReader SelectAllReader(string[][] joinColumnToTableColumn)
        {
            StringBuilder query = new StringBuilder();
            query.AppendFormat(@"SELECT * FROM {0} main ", this.TableName);

            // add joins
            if (joinColumnToTableColumn != null)
            {
                char[] alpha = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();
                for (int i = 0; i < joinColumnToTableColumn.Length; ++i)
                {
                    query.AppendFormat("JOIN {2} {0} ON main.{1} = {0}.{3} ", 
                        alpha[i], 
                        joinColumnToTableColumn[i][0], 
                        joinColumnToTableColumn[i][1], 
                        joinColumnToTableColumn[i][2]);
                }
            }


            // add where
            query.Append("WHERE main.IsInTrash IS NULL OR main.IsInTrash = 0");
            return queryExecutor.ExecuteReader(query.ToString());
        }

        public abstract void AddToTrash();

        public abstract List<T> SelectAll();
    }

    public class Vendor : DbEntity<Vendor>
    {
        public Vendor() : base() { }

        public Vendor(DbQueryExecutor qe) : base(qe) { }

        public Guid Id { get; set; }

        public string Name { get; set; }

        public override void AddToTrash()
        {
            string trashQuery;

            trashQuery = string.Format(
                @"UPDATE {0} SET IsInTrash = 1 WHERE Id = '{1}'",
                this.TableName,
                this.Id);

            queryExecutor.ExecuteNonQuery(trashQuery, null);
        }

        public void UpsertIntoDb(Database db)
        {
            string insertQuery;

            insertQuery = string.Format(
                @"INSERT INTO {0} (Id, Name) VALUES (NEWID(), '{1}');",
                this.TableName,
                this.Name);

            queryExecutor.ExecuteNonQuery(insertQuery, null);

            // set the Id to the newly generated Id
            this.Id = db.Vendors.FirstOrDefault<Vendor>(os => os.Name.Equals(this.Name)).Id;
        }

        public override List<Vendor> SelectAll()
        {
            List<Vendor> list = new List<Vendor>();
            using (SqlCeDataReader reader = SelectAllReader(null))
            {
                while (reader.Read())
                {
                    Vendor item = new Vendor(this.queryExecutor)
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

    public class Department : DbEntity<Department>
    {
        public Department(DbQueryExecutor qe) : base(qe) { }

        public Guid Id { get; set; }

        public string Name { get; set; }

        public override void AddToTrash()
        {
            string trashQuery;

            trashQuery = string.Format(
                @"UPDATE {0} SET IsInTrash = 1 WHERE Id = '{1}'",
                this.TableName,
                this.Id);

            queryExecutor.ExecuteNonQuery(trashQuery, null);
        }

        public void UpsertIntoDb(Database db)
        {
            string insertQuery;

            insertQuery = string.Format(
                @"INSERT INTO {0} (Id, Name) VALUES (NEWID(), '{1}');",
                this.TableName,
                this.Name);

            queryExecutor.ExecuteNonQuery(insertQuery, null);

            // set the Id to the newly generated Id
            this.Id = db.Departments.FirstOrDefault<Department>(os => os.Name.Equals(this.Name)).Id;
        }

        public override List<Department> SelectAll()
        {
            List<Department> list = new List<Department>();
            using (SqlCeDataReader reader = SelectAllReader(null))
            {
                while (reader.Read())
                {
                    Department item = new Department(this.queryExecutor)
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

    public class Order : DbEntity<Order>
    {
        public Order(DbQueryExecutor qe) : base(qe) { }

        public Guid Id { get; set; }

        public string Name { get; set; }

        public void UpsertIntoDb(Database db)
        {
            string insertQuery;

            insertQuery = string.Format(
                @"INSERT INTO {0} (Id, Name) VALUES (NEWID(), '{1}');",
                this.TableName,
                this.Name);

            queryExecutor.ExecuteNonQuery(insertQuery, null);

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

            queryExecutor.ExecuteNonQuery(trashQuery, null);
        }

        public override List<Order> SelectAll()
        {
            List<Order> list = new List<Order>();
            using (SqlCeDataReader reader = SelectAllReader(null))
            {
                while (reader.Read())
                {
                    Order item = new Order(this.queryExecutor)
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
        public Product() : base() { }

        public Product(DbQueryExecutor qe)
            : base(qe)
        {
            Department = new Department(qe);
            Vendor = new Vendor(qe);
        }

        public string Upc { get; set; } // TODO Change Upc into an INTEGER

        public string Name { get; set; }

        public decimal? Price { get; set; }

        public Vendor Vendor { get; set; }

        public Department Department { get; set; }

        public void UpsertIntoDb()
        {
            var parameters = new SqlCeParameter[]
            {
                new SqlCeParameter() { ParameterName = "@Upc", SqlDbType = SqlDbType.NVarChar, Value = this.Upc },
                new SqlCeParameter() { ParameterName = "@Name", SqlDbType = SqlDbType.NVarChar, Value = this.Name },
                new SqlCeParameter() { ParameterName = "@Price", SqlDbType = SqlDbType.Decimal, Value = this.Price },
                new SqlCeParameter() { ParameterName = "@VendorId", SqlDbType = SqlDbType.UniqueIdentifier, Value = this.Vendor.Id },
                new SqlCeParameter() { ParameterName = "@DepartmentId", SqlDbType = SqlDbType.UniqueIdentifier, Value = this.Department.Id }
            };

            string selectQuery = string.Format(
                @"SELECT COUNT(*) FROM {0} WHERE Upc = @upc",
                this.TableName);

            string insertQuery = string.Format(
                @"INSERT INTO {0} (Upc, Name, Price, VendorId, DepartmentId) VALUES (@upc, @name, @price, @vendorId, @departmentId)",
                this.TableName);

            if (Convert.ToInt16(queryExecutor.ExecuteScalar(selectQuery, parameters)) == 0)
            {
                queryExecutor.ExecuteNonQuery(insertQuery, parameters);
            }
        }

        public override void AddToTrash()
        {
            string trashQuery;

            trashQuery = string.Format(
                @"UPDATE {0} SET IsInTrash = 1 WHERE Upc = '{1}'",
                this.TableName,
                this.Upc);

            queryExecutor.ExecuteNonQuery(trashQuery, null);
        }

        public override List<Product> SelectAll()
        {
            List<Product> list = new List<Product>();
            using (SqlCeDataReader reader = SelectAllReader(null))
            {
                while (reader.Read())
                {
                    Product item = new Product(this.queryExecutor);
                    item.Name = Convert.ToString(reader["Name"]);
                    item.Upc = Convert.ToString(reader["Upc"]);
                    item.Price = Convert.ToDecimal(reader["Price"]);
                    item.Department.Id = new Guid(reader["DepartmentId"].ToString());
                    item.Vendor.Id = new Guid(reader["VendorId"].ToString());

                    list.Add(item);
                }
            }
            return list;
        }

        public List<Product> SelectAllWithJoin()
        {
            List<Product> list = new List<Product>();

            string[][] joinColumnToTableColumn = new string[][] 
            { 
                new string[] { "VendorId", "tblVendor", "Id" },
                new string[] { "DepartmentId", "tblDepartment", "Id" },
            };

            using (SqlCeDataReader reader = SelectAllReader(joinColumnToTableColumn))
            {
                while (reader.Read())
                {
#if DEBUG
                    // print column names
                    string[] keys = new string[reader.FieldCount];
                    for (int i = 0; i < reader.FieldCount; ++i)
                    {                        
                        Console.WriteLine(reader.GetName(i));
                    }
#endif

                    Product item = new Product(this.queryExecutor);
                    int columnNum = 0;

                    item.Upc = Convert.ToString(reader[columnNum++]);//Upc
                    item.Name = Convert.ToString(reader[columnNum++]);//Name
                    item.Price = Convert.ToDecimal(reader[columnNum++]);//Price
                    columnNum++; //IsInTrash

                    item.Vendor.Id = new Guid(reader[columnNum++].ToString());//VendorId
                    item.Department.Id = new Guid(reader[columnNum++].ToString());//DepartmentId
                    item.Vendor.Id = new Guid(reader[columnNum++].ToString());//Id
                    item.Vendor.Name = Convert.ToString(reader[columnNum++].ToString());//Name
                    columnNum++;//IsInTrash

                    item.Department.Id = new Guid(reader[columnNum++].ToString());//Id
                    item.Department.Name = Convert.ToString(reader[columnNum++].ToString());//Name
                    columnNum++;//IsInTrash

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

    public class OrderProduct : DbEntity<OrderProduct>
    {
        public OrderProduct(DbQueryExecutor qe) : base(qe) { }

        public Guid OrderID { get; set; }

        public string ProductUPC { get; set; }

        public int CasesToOrder { get; set; }

        public void UpsertIntoDb()
        {
            string selectQuery = string.Format(
                @"SELECT COUNT(*) FROM " + this.TableName + " WHERE OrderID = '{0}' AND ProductUPC = '{1}';",
                this.OrderID,
                this.ProductUPC);

            string insertQuery = string.Format(
                @"INSERT INTO " + this.TableName + " (OrderID, ProductUPC, CasesToOrder) VALUES ('{0}', '{1}', {2});",
                this.OrderID,
                this.ProductUPC,
                this.CasesToOrder);

            string updateQuery = string.Format(
                @"UPDATE " + this.TableName + " SET CasesToOrder = {2}, IsInTrash = 0 WHERE OrderID = '{0}' AND ProductUPC = '{1}';",
                this.OrderID,
                this.ProductUPC,
                this.CasesToOrder);

            if (Convert.ToInt16(queryExecutor.ExecuteScalar(selectQuery, null)) == 0)
            {
                // TODO Add a code contract to ensure that both the order and the product exist in the database before insert
                queryExecutor.ExecuteNonQuery(insertQuery, null);
            }
            else
            {
                queryExecutor.ExecuteNonQuery(updateQuery, null);
            }
        }

        public override void AddToTrash()
        {
            string trashQuery;

            trashQuery = string.Format(
                @"UPDATE {0} SET IsInTrash = 1 WHERE OrderID = '{1}' AND ProductUPC = '{2}'",
                this.TableName,
                this.OrderID,
                this.ProductUPC);

            queryExecutor.ExecuteNonQuery(trashQuery, null);
        }

        public override List<OrderProduct> SelectAll()
        {
            List<OrderProduct> list = new List<OrderProduct>();
            using (SqlCeDataReader reader = SelectAllReader(null))
            {
                while (reader.Read())
                {
                    OrderProduct item = new OrderProduct(this.queryExecutor)
                    {
                        OrderID = new Guid(reader["OrderID"].ToString()),
                        ProductUPC = Convert.ToString(reader["ProductUPC"]),
                        CasesToOrder = Convert.ToInt16(reader["CasesToOrder"])
                    };
                    list.Add(item);
                }
            }
            return list;
        }
    }
}
