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
     
    public class DbSchemaBuilder
    {
        DbQueryExecutor queryExecutor;

        public DbSchemaBuilder(DbQueryExecutor queryExecutor)
        {
            this.queryExecutor = queryExecutor;
        }

        private bool TableExists(string tableName)
        {
            IOHelpers.LogSubroutine("TableExists");

            const string queryTemplate = @"SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = '{0}'";
            string query = string.Format(queryTemplate, tableName);

            var count = (int)queryExecutor.ExecuteScalar(query, null);

            bool tableExists = count > 0;

            return tableExists;
        }

        internal void CreateTables()
        {
            IOHelpers.LogSubroutine("CreateTables");

            string query;

            if (!this.TableExists("tblDepartment"))
            {
                query = @"

                    CREATE TABLE [tblDepartment]
                    (
                       [Id] int NOT NULL,
                       [Name] NVARCHAR(100),
                       [IsInTrash] BIT
                    )";

                queryExecutor.ExecuteNonQuery(query, null);

                query = @"

                    ALTER TABLE [tblDepartment] 
                    ADD CONSTRAINT [PK_tblDepartment] PRIMARY KEY ([Id])";

                queryExecutor.ExecuteNonQuery(query, null);

                query = @"INSERT INTO tblDepartment VALUES (-1, 'No Department', 0)";
                queryExecutor.ExecuteNonQuery(query, null);
            }

            if (!this.TableExists("tblVendor"))
            {
                query = @"
                    CREATE TABLE [tblVendor]
                    (
                       [Id] int NOT NULL,
                       [Name] NVARCHAR(100),
                       [IsInTrash] BIT
                    )";

                queryExecutor.ExecuteNonQuery(query, null);

                query = @"

                    ALTER TABLE [tblVendor] 
                    ADD CONSTRAINT [PK_tblVendor] PRIMARY KEY ([Id])";

                queryExecutor.ExecuteNonQuery(query, null);

                query = @"INSERT INTO tblVendor VALUES (-1, 'No Vendor Specified', 0)";
                queryExecutor.ExecuteNonQuery(query, null);
            }

            if (!this.TableExists("tblOrder"))
            {
                query = @"
                    CREATE TABLE [tblOrder]
                    (
                       [Id] UNIQUEIDENTIFIER NOT NULL,
                       [Name] NVARCHAR(100),
                       [IsInTrash] BIT
                    )";

                queryExecutor.ExecuteNonQuery(query, null);

                query = @"

                    ALTER TABLE [tblOrder] 
                    ADD CONSTRAINT [PK_tblOrder] PRIMARY KEY ([Id])";

                queryExecutor.ExecuteNonQuery(query, null);
            }

            if (!this.TableExists("tblProduct"))
            {
                query = @"
                    CREATE TABLE [tblProduct]
                    (
                       [Upc] NVARCHAR(100) NOT NULL,
                       [Name] NVARCHAR(100),
                       [Price] DECIMAL,
                       [IsInTrash] BIT,
                       [VendorId] int,
                       [DepartmentId] int
                    )";

                queryExecutor.ExecuteNonQuery(query, null);

                query = @"
                    ALTER TABLE [tblProduct] 
                    ADD CONSTRAINT [PK_tblProduct] PRIMARY KEY ([Upc])
                    ";

                queryExecutor.ExecuteNonQuery(query, null);

                query = @"
                    ALTER TABLE [tblProduct] 
                    ADD CONSTRAINT [FK_DepartmentId] FOREIGN KEY ([DepartmentId])
                       REFERENCES [tblDepartment] ([Id]) ON DELETE NO ACTION ON UPDATE NO ACTION
                    ";

                queryExecutor.ExecuteNonQuery(query, null);

                query = @"
                    ALTER TABLE [tblProduct] 
                    ADD CONSTRAINT [FK_VendordId] FOREIGN KEY ([VendorId])
                       REFERENCES [tblVendor] ([Id]) ON DELETE NO ACTION ON UPDATE NO ACTION
                    ";

                queryExecutor.ExecuteNonQuery(query, null);
            }

            if (!this.TableExists("tblOrderProduct"))
            {
                query = @"
                    CREATE TABLE [tblOrderProduct]
                    (
                       [OrderID] UNIQUEIDENTIFIER NOT NULL,
                       [ProductUPC] NVARCHAR(100) NOT NULL,
                       [CasesToOrder] INT,
                       [IsInTrash] BIT
                    )";

                queryExecutor.ExecuteNonQuery(query, null);

                query = @"
                    ALTER TABLE [tblOrderProduct] 
                    ADD CONSTRAINT [PK_OrderProduct] PRIMARY KEY ([OrderID], [ProductUPC])
                    ";

                queryExecutor.ExecuteNonQuery(query, null);

                query = @"
                    ALTER TABLE [tblOrderProduct] 
                    ADD CONSTRAINT [FK_OrderId] FOREIGN KEY ([OrderID])
                       REFERENCES [tblOrder] ([Id]) ON DELETE NO ACTION ON UPDATE NO ACTION
                    ";

                queryExecutor.ExecuteNonQuery(query, null);

                query = @"
                    ALTER TABLE [tblOrderProduct]
                    ADD CONSTRAINT [FK_ProductUpc] FOREIGN KEY ([ProductUPC])
                       REFERENCES [tblProduct] ([Upc]) ON DELETE NO ACTION ON UPDATE NO ACTION
                    ";

                queryExecutor.ExecuteNonQuery(query, null);
            }
        }

        internal void CreateCeDb(string databaseFullPath, string connectionString)
        {
            IOHelpers.LogSubroutine("CreateDB");

            // create the database
            IOHelpers.CreateDirectoryIfNotExists(Constants.ApplicationDataStoreFullPath);
            if (!File.Exists(databaseFullPath))
            {
                var engine = new SqlCeEngine(connectionString);
                engine.CreateDatabase();
                engine.Dispose();
            }
        } 
    }
}
