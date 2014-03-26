using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Vendord.SmartDevice.Linked;

namespace DatabaseTestProj
{
    /// <summary>
    /// Summary description for UnitTest1
    /// </summary>
    [TestClass]
    public class UnitTest1
    {
        public UnitTest1()
        {
            //
            // TODO: Add constructor logic here
            //
        }

        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Additional test attributes
        //
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
        //
        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // Use TestInitialize to run code before running each test 
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion

        [TestMethod]
        public void UpsertProduct()
        {
            long upc = -00000000001;

            Database db = new Database();
            DbQueryExecutor qe = new DbQueryExecutor(db.ConnectionString);

            Vendor vendor = new Vendor(qe) { Id = -1, Name = "Test Vendor One" };
            vendor.UpsertIntoDb();

            vendor = new Vendor(qe) { Id = -2, Name = "Test Vendor Two" };
            vendor.UpsertIntoDb();

            Department department = new Department(qe) { Id = -1, Name = "Test Department One" };
            department.UpsertIntoDb();

            department = new Department(qe) { Id = -2, Name = "Test Department Two" };
            department.UpsertIntoDb();

            Product product = new Product();

            product = new Product(qe);
            product.Upc = upc;

            // this should insert
            product.Name = "Inserted";
            product.Price = 10.00m;
            product.IsInTrash = 0;
            product.Vendor.Id = -1;
            product.Department.Id = -1;
            product.UpsertIntoDb();

            Product result = product.SelectAll().Where<Product>(p => p.Upc == upc).FirstOrDefault();
            Assert.AreEqual(product.Name.ToString(), result.Name.ToString());

            // this should update           
            product.Name = "Updated";
            product.Price = 20.00m;
            product.IsInTrash = 0;
            product.Vendor.Id = -2;
            product.Department.Id = -2;
            product.UpsertIntoDb();

            result = product.SelectAll().Where<Product>(p => p.Upc.Equals(upc)).FirstOrDefault();
            Assert.AreEqual(product.Name.ToString(), result.Name.ToString());

        }
    }
}
