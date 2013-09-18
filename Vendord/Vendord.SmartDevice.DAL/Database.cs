namespace Vendord.SmartDevice.DAL
{
    using System;
    using System.Collections.Generic;
    using System.Data.SqlServerCe;
    using System.Linq;
    using System.Text;
    using System.IO;
    using System.Collections.ObjectModel;

    public class Database
    {
        private string dataSource;
        private string connString;
        private SqlCeConnection conn;

        private SqlCeDataReader SelectAllFromDatabase(string tableName)
        {                        
            string cmd;            
            SqlCeDataReader reader;            
            cmd = String.Format(@"SELECT * FROM {0}", tableName);
            reader = ExecuteReader(cmd);
            return reader;
        }

        public IEnumerable<OrderSession> OrderSessions
        {
            get
            {
                Collection<OrderSession> result;
                SqlCeDataReader reader;
                result = new Collection<OrderSession>();
                reader = SelectAllFromDatabase("OrderSession");
                while (reader.Read())
                {
                    OrderSession item = new OrderSession()
                    {
                        ID = Convert.ToInt32(reader["ID"]),
                        Name = Convert.ToString(reader["Name"])
                    };
                    result.Add(item);
                }
                return result;
            }
        }
        public IEnumerable<Product> Products
        {
            get
            {
                Collection<Product> result;
                SqlCeDataReader reader;
                result = new Collection<Product>();
                reader = SelectAllFromDatabase("Product");
                while (reader.Read())
                {
                    Product item = new Product()
                    {
                        ID = Convert.ToInt32(reader["ID"]),
                        Name = Convert.ToString(reader["Name"]),
                        UPC = Convert.ToInt32(reader["UPC"])

                    };
                    result.Add(item);
                }
                return result;
            }
        }

        private SqlCeDataReader ExecuteReader(string cmdText)
        {
            SqlCeDataReader reader = null;

            try
            {
                SqlCeCommand cmd = conn.CreateCommand();
                cmd.CommandText = cmdText;
                reader = cmd.ExecuteReader();
            }
            catch (Exception ex)
            {
                int i = 0;
                ++i;
            }

            return reader;
        }

        private int ExecuteNonQuery(string cmdText)
        {
            int rowsAffected;
            rowsAffected = 0;

            try
            {
                SqlCeCommand cmd = conn.CreateCommand();
                cmd.CommandText = cmdText;
                rowsAffected = cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                int i = 0;
                ++i;
            }

            return rowsAffected;
        }

        private void SeedDB()
        {
            string cmd;

            for (int i = 0; i < 20; ++i)
            {
                cmd = String.Format(@"INSERT INTO OrderSession (Name) VALUES ('{0}')", DateTime.Now.Ticks.ToString());
                ExecuteNonQuery(cmd);
            }

            for (int i = 0; i < Int16.MaxValue; ++i)
            {
                cmd = String.Format(@"INSERT INTO Product (Name, UPC) VALUES ({0},{1})", DateTime.Now.Ticks.ToString(), i);
                ExecuteNonQuery(cmd);
            }

        }

        private void CreateTables()
        {
            ExecuteNonQuery(@"CREATE TABLE OrderSession (ID int IDENTITY(1,1) PRIMARY KEY, Name nvarchar(100))");
            ExecuteNonQuery(@"CREATE TABLE Product (ID int IDENTITY(1,1) PRIMARY KEY, Name nvarchar(100), UPC int)");
        }

        private void InstantiateAndOpenConnection()
        {
            conn = new SqlCeConnection(connString);
            conn.Open();
        }

        private void CreateDB()
        {
            SqlCeEngine engine;

            if (!File.Exists(dataSource))
            {
                engine = new SqlCeEngine(connString);
                engine.CreateDatabase();
                engine.Dispose();
            }

        }

        private void InstantiateConnString(string databaseName)
        {
            string applicationDataStore;

            applicationDataStore
                = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), Constants.APPLICATION_NAME);

            dataSource
                = Path.Combine(applicationDataStore, databaseName);

            connString
                = String.Format("Data Source={0};Persist Security Info=False;", dataSource);
        }

        private void CreateApplicationDataSubDirectory()
        {
            string applicationDataSubdirectory;

            applicationDataSubdirectory
                = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), Constants.APPLICATION_NAME);

            if (!Directory.Exists(applicationDataSubdirectory))
            {
                Directory.CreateDirectory(applicationDataSubdirectory);
            }
        }

        public Database(string databaseName)
        {
            CreateApplicationDataSubDirectory();
            InstantiateConnString(databaseName);
            CreateDB();
            InstantiateAndOpenConnection();
            CreateTables();
            SeedDB();
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
}
