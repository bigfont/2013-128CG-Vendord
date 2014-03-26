﻿[module:
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
    using System.Diagnostics;

    public abstract class DbEntity<T>
    {
        public DbQueryExecutor queryExecutor;

        public DbEntity() { }

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

        public SqlCeDataReader SelectAllReader()
        {
            StringBuilder query = new StringBuilder();
            query.AppendFormat(@"SELECT * FROM {0} WHERE IsInTrash IS NULL OR IsInTrash = 0", this.TableName);
            return queryExecutor.ExecuteReader(query.ToString());
        }

        public abstract void AddToTrash();

        public abstract List<T> SelectAll();
    }

    public class Vendor : DbEntity<Vendor>
    {
        public Vendor() : base() { }

        public Vendor(DbQueryExecutor qe) : base(qe) { }

        public int? Id { get; set; }

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

        public void UpsertIntoDb()
        {
            string insertQuery;

            insertQuery = string.Format(
                @"INSERT INTO {0} (Id, Name) VALUES ({1}, '{2}');",
                this.TableName,
                this.Id,
                this.Name);

            queryExecutor.ExecuteNonQuery(insertQuery, null);
        }

        public override List<Vendor> SelectAll()
        {
            List<Vendor> list = new List<Vendor>();
            using (SqlCeDataReader reader = SelectAllReader())
            {
                while (reader.Read())
                {
                    Vendor item = new Vendor(this.queryExecutor);

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
        public Department() : base() { }

        public Department(DbQueryExecutor qe) : base(qe) { }

        public int? Id { get; set; }

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

        public void UpsertIntoDb()
        {
            string insertQuery;

            insertQuery = string.Format(
                @"INSERT INTO {0} (Id, Name) VALUES ({1}, '{2}');",
                this.TableName,
                this.Id,
                this.Name);

            queryExecutor.ExecuteNonQuery(insertQuery, null);
        }

        public override List<Department> SelectAll()
        {
            List<Department> list = new List<Department>();
            using (SqlCeDataReader reader = SelectAllReader())
            {
                while (reader.Read())
                {
                    Department item = new Department(this.queryExecutor);
                    try
                    {
                        item.Id = Convert.ToInt32(reader["id"]);
                    }
                    catch (Exception)
                    {
                        item.Id = null;
                    }

                    Name = Convert.ToString(reader["Name"]);
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
        public Order() : base() { }

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
            using (SqlCeDataReader reader = SelectAllReader())
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

        #region Properties

        // int64
        public long Upc { get; set; }

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
                new SqlCeParameter() { ParameterName = "@Name", SqlDbType = SqlDbType.NVarChar, Value = this.Name },
                new SqlCeParameter() { ParameterName = "@Price", SqlDbType = SqlDbType.Decimal, Value = this.Price },
                new SqlCeParameter() { ParameterName = "@VendorId", SqlDbType = SqlDbType.Int, Value = this.Vendor.Id },
                new SqlCeParameter() { ParameterName = "@DepartmentId", SqlDbType = SqlDbType.Int, Value = this.Department.Id }
            };

            // exists
            string selectQuery =
                string.Format(@"SELECT COUNT(*) FROM {0} WHERE Upc = @Upc",
                this.TableName);
            object result = queryExecutor.ExecuteScalar(selectQuery, parameters);
            bool exists = Convert.ToInt16(result.ToString()) > 0;

            // upsert
            if (exists)
            {
                // update
                string updateQuery =
                    string.Format(@"
                            UPDATE {0} SET
                            Name = @name, 
                            Price = @price, 
                            VendorId = @vendorId, 
                            DepartmentId = @departmentId
                            WHERE Upc = @Upc",
                        this.TableName
                    );

                this.queryExecutor.ExecuteNonQuery(updateQuery, parameters);
            }
            else
            {
                // insert
                string insertQuery =
                    string.Format(@"
                            INSERT INTO {0} (Upc, Name, Price, VendorId, DepartmentId) 
                            VALUES (@Upc, @name, @price, @vendorId, @departmentId)",
                    this.TableName);
                this.queryExecutor.ExecuteNonQuery(insertQuery, parameters);
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
            return SelectAllWithJoin();
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

            using (SqlCeDataReader reader = queryExecutor.ExecuteReader(query))
            {
                while (reader.Read())
                {
                    Product item = new Product(this.queryExecutor);

                    // upc
                    item.Upc = Convert.ToInt64(reader["Upc"].ToString());
                    // name                    
                    item.Name = reader["Name"].ToString();
                    // price
                    item.Price = Convert.ToDecimal(reader["Price"].ToString());
                    // vendor id
                    item.Vendor.Id = Convert.ToInt32(reader["VendorId"].ToString());
                    // department id
                    item.Department.Id = Convert.ToInt32(reader["DepartmentId"].ToString());
                    // vendor name
                    item.Vendor.Name = reader["VendorName"].ToString();
                    // department name
                    item.Department.Name = reader["DepartmentName"].ToString();
                    // add
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

        // int64
        public long ProductUPC { get; set; }

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
            using (SqlCeDataReader reader = SelectAllReader())
            {
                while (reader.Read())
                {
                    OrderProduct item = new OrderProduct(this.queryExecutor)
                    {
                        OrderID = new Guid(reader["OrderID"].ToString()),
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
