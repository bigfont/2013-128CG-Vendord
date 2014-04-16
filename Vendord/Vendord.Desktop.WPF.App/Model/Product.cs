using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using Vendord.Desktop.WPF.App.Properties;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace Vendord.Desktop.WPF.App.Model
{
    /// <summary>
    /// Represents an order.  It is wrapped
    /// by the OrderViewModel class, which enables it to
    /// be easily displayed and edited by a WPF user interface.
    /// </summary>
    public class Product
    {
        #region Creation

        public static Product CreateNewProduct()
        {
            return new Product();
        }

        public static Product CreateProduct(
            long upc,
            long certCode,
            string name, 
            int? vendorId)
        {
            return new Product
            {
                Upc = upc,
                CertCode = certCode,
                Name = name,
                VendorId = vendorId
            };
        }

        protected Product()
        {
        }

        #endregion // Creation

        #region State Properties

        public long Upc { get; set; }
        public long CertCode { get; set; }
        public string Name { get; set; }
        public int? VendorId { get; set; }

        #endregion // State Properties
    }
}
