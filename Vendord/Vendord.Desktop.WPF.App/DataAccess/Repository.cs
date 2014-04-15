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
    /// Represents a source of order related data in the application.
    /// </summary>
    public class Repository
    {
        #region Fields

        readonly List<Order> _orders;
        readonly List<OrderProduct> _orderProducts;
        readonly List<Product> _products;
        readonly List<Vendor> _vendors;

        #endregion // Fields

        #region Constructor

        /// <summary>
        /// Creates a new repository.
        /// </summary>
        public Repository()
        {
            _orders = LoadOrders();
            _orderProducts = LoadOrderProducts();
            _vendors = LoadVendors();
            _products = LoadProducts();
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

        public List<Order> GetOrdersIncludeProducts()
        {
            var groupJoinQuerySelectObj =
                from o in _orders
                join op in _orderProducts
                on o.Id equals op.OrderId into gj
                select Order.CreateOrder(o.Id, o.Name, gj.ToList<OrderProduct>());

            return groupJoinQuerySelectObj.ToList<Order>();
        }

        /// <summary>
        /// Returns a shallow-copied list of all orderProducts in the repository.
        /// </summary>
        public List<OrderProduct> GetOrderProducts()
        {
            return new List<OrderProduct>(_orderProducts);
        }

        /// <summary>
        /// Returns a shallow-copied list of all products in the repository.
        /// </summary>
        public List<Product> GetProducts()
        {
            return new List<Product>(_products);
        }

        #endregion // Public Interface

        #region Private Helpers

        static List<Order> LoadOrders()
        {
            List<Order> orders = new List<Order>();

            Vendord.SmartDevice.Linked.Database db = new Vendord.SmartDevice.Linked.Database();
            foreach (var o in db.Orders)
            {
                var order = Order.CreateOrder(o.Id, o.Name, null);
                orders.Add(order);
            }

            return orders;
        }

        static List<OrderProduct> LoadOrderProducts()
        {
            List<OrderProduct> orderProducts = new List<OrderProduct>();

            Vendord.SmartDevice.Linked.Database db = new Vendord.SmartDevice.Linked.Database();
            foreach (var op in db.OrderProducts)
            {
                var orderProduct = OrderProduct.CreateOrderProduct(op.OrderId, op.ProductUPC);
                orderProducts.Add(orderProduct);
            }

            return orderProducts;
        }

        static List<Vendor> LoadVendors()
        {
            List<Vendor> vendors = new List<Vendor>();

            Vendord.SmartDevice.Linked.Database db = new Vendord.SmartDevice.Linked.Database();
            foreach (var v in db.Vendors)
            {
                var vendor = Vendor.CreateVendor(v.Name);
                vendors.Add(vendor);
            }

            return vendors;
        }

        static List<Product> LoadProducts()
        {
            List<Product> products = new List<Product>();

            Vendord.SmartDevice.Linked.Database db = new Vendord.SmartDevice.Linked.Database();
            foreach (var p in db.Products)
            {
                var product = Product.CreateProduct(p.Upc, p.Name);
                products.Add(product);
            }

            return products;
        }

        #endregion // Private Helpers
    }
}
