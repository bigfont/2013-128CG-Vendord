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
            string lastName)
        {
            return new Product
            {
                LastName = lastName
            };
        }

        protected Product()
        {
        }

        #endregion // Creation

        #region State Properties

        /// <summary>
        /// Gets/sets the order's last name.
        /// </summary>
        public string LastName { get; set; }

        #endregion // State Properties
    }
}
