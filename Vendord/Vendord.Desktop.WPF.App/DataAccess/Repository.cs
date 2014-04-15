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
    public class Repository
    {
        #region Fields

        readonly List<Order> _orders;
        readonly List<OrderProduct> _orderProducts;
        readonly List<Vendor> _vendors;

        #endregion // Fields

        #region Constructor

        /// <summary>
        /// Creates a new repository of orders.
        /// </summary>
        public Repository()
        {
            _orders = LoadOrders();
            _orderProducts = LoadOrderProducts();
            _vendors = LoadVendors();
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
            Guid guid = new Guid("12345678-1234-1234-1234-123456789012");

            List<Order> orders = new List<Order>() 
            {
	            Order.CreateOrder(guid, "one", null)
            };
            List<OrderProduct> orderProducts = new List<OrderProduct>()
            {
	            OrderProduct.CreateOrderProduct(guid),
	            OrderProduct.CreateOrderProduct(guid)
            };
            var groupJoinQuery =
                from o in _orders
                join op in _orderProducts
                on o.Id equals op.OrderId into gj
                select new { o, gj };

            foreach (var joinResult in groupJoinQuery)
            {
                if (joinResult.gj != null)
                {
                    Console.WriteLine("gj is not null");
                }
            }

            var groupJoinQuerySelectObj =
                from o in orders
                join op in orderProducts
                on o.Id equals op.OrderId into gj
                select Order.CreateOrder(o.Id, o.Name, gj.ToList<OrderProduct>());

            foreach (Order order in groupJoinQuerySelectObj)
            {
                if (order.OrderProducts != null)
                {
                    Console.WriteLine("OrderItems is not null.");
                }
            }

            return groupJoinQuerySelectObj.ToList<Order>();
        }

        /// <summary>
        /// Returns a shallow-copied list of all orderProducts in the repository.
        /// </summary>
        public List<OrderProduct> GetOrderProducts()
        {
            return new List<OrderProduct>(_orderProducts);
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
                var orderProduct = OrderProduct.CreateOrderProduct(op.OrderId);
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

        #endregion // Private Helpers
    }
}
