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

    public abstract class DbEntity
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

        public int IsInTrash { get; set; }

        public void EmptyTrash(DbQueryExecutor db)
        {
            string emptyTrashQuery;
            emptyTrashQuery = string.Format(
                @"DELETE {0} WHERE (IsInTrash = 1)",
                this.TableName);

            db.ExecuteNonQuery(emptyTrashQuery, null);
        }

        public abstract void AddToTrash(DbQueryExecutor db);
    }

    public class Vendor : DbEntity
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public override void AddToTrash(DbQueryExecutor db)
        {
            string trashQuery;

            trashQuery = string.Format(
                @"UPDATE {0} SET IsInTrash = 1 WHERE Id = '{1}'",
                this.TableName,
                this.Id);

            db.ExecuteNonQuery(trashQuery, null);
        }

        public void UpsertIntoDb(Database db, DbQueryExecutor queryExecutor)
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
    }

    public class Department : DbEntity
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public override void AddToTrash(DbQueryExecutor db)
        {
            string trashQuery;

            trashQuery = string.Format(
                @"UPDATE {0} SET IsInTrash = 1 WHERE Id = '{1}'",
                this.TableName,
                this.Id);

            db.ExecuteNonQuery(trashQuery, null);
        }

        public void UpsertIntoDb(Database db, DbQueryExecutor queryExecutor)
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
    }

    public class Order : DbEntity
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public void UpsertIntoDb(Database db, DbQueryExecutor queryExecutor)
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

        public override void AddToTrash(DbQueryExecutor db)
        {
            string trashQuery;

            trashQuery = string.Format(
                @"UPDATE {0} SET IsInTrash = 1 WHERE Id = '{1}'",
                this.TableName,
                this.Id);

            db.ExecuteNonQuery(trashQuery, null);
        }

        public override string ToString()
        {
            return this.Name;
        }
    }

    public class Product : DbEntity
    {
        public string Upc { get; set; } // TODO Change Upc into an INTEGER

        public string Name { get; set; }

        public decimal? Price { get; set; }

        public Guid VendorId { get; set; }

        public Guid DepartmentId { get; set; }        

        public void UpsertIntoDb(DbQueryExecutor queryExecutor)
        {
            var parameters = new SqlCeParameter[]
            {
                new SqlCeParameter() { ParameterName = "@Upc", SqlDbType = SqlDbType.NVarChar, Value = this.Upc },
                new SqlCeParameter() { ParameterName = "@Name", SqlDbType = SqlDbType.NVarChar, Value = this.Name },
                new SqlCeParameter() { ParameterName = "@Price", SqlDbType = SqlDbType.Decimal, Value = this.Price },
                new SqlCeParameter() { ParameterName = "@VendorId", SqlDbType = SqlDbType.UniqueIdentifier, Value = this.VendorId },
                new SqlCeParameter() { ParameterName = "@DepartmentId", SqlDbType = SqlDbType.UniqueIdentifier, Value = this.DepartmentId }
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

        public override void AddToTrash(DbQueryExecutor queryExecutor)
        {
            string trashQuery;

            trashQuery = string.Format(
                @"UPDATE {0} SET IsInTrash = 1 WHERE Upc = '{1}'",
                this.TableName,
                this.Upc);

            queryExecutor.ExecuteNonQuery(trashQuery, null);
        }

        public override string ToString()
        {
            return this.Name;
        }
    }

    public class OrderProduct : DbEntity
    {
        public Guid OrderID { get; set; }

        public string ProductUPC { get; set; }

        public int CasesToOrder { get; set; }

        public void UpsertIntoDb(DbQueryExecutor queryExecutor)
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

        public override void AddToTrash(DbQueryExecutor queryExecutor)
        {
            string trashQuery;

            trashQuery = string.Format(
                @"UPDATE {0} SET IsInTrash = 1 WHERE OrderID = '{1}' AND ProductUPC = '{2}'",
                this.TableName,
                this.OrderID,
                this.ProductUPC);

            queryExecutor.ExecuteNonQuery(trashQuery, null);
        }
    }
}
