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
    public class Order
    {
        #region Creation

        public static Order CreateNewOrder()
        {
            return new Order();
        }

        public static Order CreateOrder(
            string name)
        {
            return new Order
            {
                Name = name
            };
        }

        protected Order()
        {
        }

        #endregion // Creation

        #region State Properties

        /// <summary>
        /// Gets/sets the order's last name.
        /// </summary>
        public string Name { get; set; }

        #endregion // State Properties
    }
}
