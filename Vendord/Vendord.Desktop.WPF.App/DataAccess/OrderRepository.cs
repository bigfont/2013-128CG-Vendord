using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Vendord.Desktop.WPF.App.Model;
using System.Xml.Linq;
using System.Xml;
using System.Windows.Resources;
using System.Windows;

namespace Vendord.Desktop.WPF.App.DataAccess
{
    /// <summary>
    /// Represents a source of orders in the application.
    /// </summary>
    public class OrderRepository
    {
        #region Fields

        readonly List<Order> _orders;

        #endregion // Fields

        #region Constructor

        /// <summary>
        /// Creates a new repository of orders.
        /// </summary>
        /// <param name="orderDataFile">The relative path to an XML resource file that contains order data.</param>
        public OrderRepository(string orderDataFile)
        {
            _orders = LoadOrders(orderDataFile);
        }

        #endregion // Constructor

        #region Public Interface


        /// <summary>
        /// Returns a shallow-copied list of all orders in the repository.
        /// </summary>
        public List<Order> GetOrders()
        {
            return new List<Order>(_orders);
        }

        #endregion // Public Interface

        #region Private Helpers

        static List<Order> LoadOrders(string orderDataFile)
        {
            List<Order> orders = new List<Order>();

            Vendord.SmartDevice.Linked.Database db = new Vendord.SmartDevice.Linked.Database();            
            foreach(var o in db.Orders)
            {
                var order = Order.CreateOrder(o.Name);
                orders.Add(order);
            }

            return orders;
        }

        #endregion // Private Helpers
    }
}
