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
    /// Represents a source of products in the application.
    /// </summary>
    public class ProductRepository
    {
        #region Fields

        readonly List<Product> _products;

        #endregion // Fields

        #region Constructor

        /// <summary>
        /// Creates a new repository of products.
        /// </summary>
        public ProductRepository()
        {
            _products = LoadProducts();
        }

        #endregion // Constructor

        #region Public Interface


        /// <summary>
        /// Returns a shallow-copied list of all products in the repository.
        /// </summary>
        public List<Product> GetProducts()
        {
            return new List<Product>(_products);
        }

        #endregion // Public Interface

        #region Private Helpers

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
