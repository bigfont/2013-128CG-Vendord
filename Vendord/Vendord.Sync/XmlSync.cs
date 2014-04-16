using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using Vendord.SmartDevice.Linked;
using System.Data.SqlClient;
using System.Xml.Linq;

namespace Vendord.Sync
{
    public class XmlSync
    {
        public SyncResult PullVendorsFromItRetailXmlBackup(BackgroundWorker worker, string filePath)
        {
            SyncResult result;
            try
            {
                CopyVendorsFromItRetailXmlBackupFilesToDesktopDb(worker, filePath);
                result = SyncResult.Complete;
            }
            catch (SqlException ex)
            {
                result = SyncResult.Disconnected;
                IOHelpers.LogException(ex);
            }

            return result;
        }

        public SyncResult PullProductsFromItRetailXmlBackup(BackgroundWorker worker, string filePath)
        {
            SyncResult result;
            try
            {
                CopyProductsFromItRetailXmlBackupFilesToDesktopDb(worker, filePath);
                result = SyncResult.Complete;
            }
            catch (SqlException ex)
            {
                result = SyncResult.Disconnected;
                IOHelpers.LogException(ex);
            }

            return result;
        }

        private List<Vendor> GetVendorListFromXmlBackup(string filePath)
        {
            XElement vendorsXml = XElement.Load(filePath);
            var query =
                from v in vendorsXml.Descendants("Vendors")
                select new Vendor()
                {
                    Id = Convert.ToInt32(v.Element("vendor_id").Value.ToString().Trim()),
                    Name = (string)v.Element("name")
                };

            // for debugging
            int i = query.Count();

            List<Vendor> vendors = query.ToList<Vendor>();
            return vendors;
        }

        private List<Product> GetProductListFromXmlBackup(string filePath)
        {
            XElement productsXml = XElement.Load(filePath);

            Func<string, long> parseLong =
                value =>
                {
                    long longValue;
                    return Int64.TryParse(value, out longValue) ? longValue : -1;
                };

            Func<string, decimal> parseDecimal =
                value =>
                {
                    decimal decimalValue;
                    return Decimal.TryParse(value, out decimalValue) ? decimalValue : -1m;
                };

            Func<string, int> parseInt =
                value =>
                {
                    int intValue;
                    return Int32.TryParse(value, out intValue) ? intValue : -1;
                };

            Func<XElement, string, string> getXElementValue =
                (xElem, key) =>
                {
                    string result = string.Empty;
                    if (xElem != null && xElem.Element(key) != null && xElem.Element(key).Value != null)
                    {
                        result = xElem.Element(key).Value;
                    }
                    return result;
                };

            List<Product> products = new List<Product>();

            string normalPrice = string.Empty;

            try
            {
                var query = from p
                        in productsXml.Descendants("Products")
                            select p;

                foreach (XElement elem in query)
                {
                    Product p = new Product();
                    p.Upc = parseLong(getXElementValue(elem, "upc"));
                    p.CertCode = parseLong(getXElementValue(elem, "cert_code"));
                    p.Name = getXElementValue(elem, "description");
                    p.Price = parseDecimal(getXElementValue(elem, "normal_price"));
                    p.Department = new Department()
                    {
                        Id = parseInt(getXElementValue(elem, "department")),
                        Name = string.Empty
                    };
                    p.Vendor = new Vendor()
                    {
                        Id = parseInt(getXElementValue(elem, "vendor")),
                        Name = string.Empty
                    };
                    products.Add(p);
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }

            return products;
        }

        private void CopyVendorsFromItRetailXmlBackupFilesToDesktopDb(BackgroundWorker worker, string filePath)
        {
            List<Vendor> vendors = GetVendorListFromXmlBackup(filePath);

            int insertedRecords = 0;
            int totalRecords = vendors.Count();
            DateTime lastProgressReportTime = DateTime.MinValue;

            Database db = new Database();
            DbQueryExecutor queryExe = new DbQueryExecutor(db.ConnectionString);

            foreach (Vendor v in vendors)
            {
                v.QueryExecutor = queryExe;

                if (v.Name.Length == 0)
                {
                    v.Name = "Vendor #" + v.Id;
                }

                v.UpsertIntoDb();
                insertedRecords++;

                ReportWorkerProgress(worker, insertedRecords, totalRecords, lastProgressReportTime, v.Name);
            }
        }

        private void CopyProductsFromItRetailXmlBackupFilesToDesktopDb(BackgroundWorker worker, string filePath)
        {
            List<Product> products = GetProductListFromXmlBackup(filePath);

            int insertedRecords = 0;
            int totalRecords = products.Count();
            DateTime lastProgressReportTime = DateTime.MinValue;

            Database db = new Database();
            DbQueryExecutor queryExe = new DbQueryExecutor(db.ConnectionString);

            foreach (Product p in products)
            {
                System.Diagnostics.Debug.WriteLine("Importing product" + p.Name);

                Vendor v = new Vendor();
                v.QueryExecutor = queryExe;
                v.Id = p.Vendor.Id;
                v.Name = p.Vendor.Name;
                v.UpsertIntoDb();

                Department d = new Department();
                d.QueryExecutor = queryExe;
                d.Id = p.Department.Id;
                d.Name = p.Department.Name;
                d.UpsertIntoDb();

                p.QueryExecutor = queryExe;
                p.UpsertIntoDb();
                insertedRecords++;

                ReportWorkerProgress(worker, insertedRecords, totalRecords, lastProgressReportTime, p.Name);
            }
        }

        private void ReportWorkerProgress(BackgroundWorker worker, int insertedRecords, int totalRecords, DateTime lastProgressReportTime, string importedItem)
        {
            // if five seconds have passed, make the worker report progress
            if (worker != null)
            {
                double timeSinceLastReport = DateTime.Now.Subtract(lastProgressReportTime).TotalSeconds;
                int percentComplete = 100 * insertedRecords / totalRecords;
                string message = string.Format("Importing {0} - Item {1} of {2}", importedItem, insertedRecords, totalRecords);
                worker.ReportProgress(percentComplete, message);
                lastProgressReportTime = DateTime.Now;
            }
        }
    }
}
