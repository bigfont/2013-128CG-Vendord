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
    public class OrderProductRepository
    {
        #region Fields

        readonly List<OrderProduct> _orderProduct;

        #endregion // Fields

        #region Constructor

        /// <summary>
        /// Creates a new repository of orders.
        /// </summary>
        public OrderProductRepository()
        {
            _orderProduct = LoadOrderProducts();
        }

        #endregion // Constructor

        #region Public Interface


        /// <summary>
        /// Returns a shallow-copied list of all orders in the repository.
        /// </summary>
        public List<OrderProduct> GetOrderProducts()
        {
            return new List<OrderProduct>(_orderProduct);
        }

        #endregion // Public Interface

        #region Private Helpers

        static List<OrderProduct> LoadOrderProducts()
        {
            List<OrderProduct> orderProducts = new List<OrderProduct>();

            Vendord.SmartDevice.Linked.Database db = new Vendord.SmartDevice.Linked.Database();            
            foreach(var op in db.OrderProducts)
            {
                var orderProduct = OrderProduct.CreateOrderProduct(op.OrderID);
                orderProducts.Add(orderProduct);
            }

            return orderProducts;
        }

        #endregion // Private Helpers
    }
}
