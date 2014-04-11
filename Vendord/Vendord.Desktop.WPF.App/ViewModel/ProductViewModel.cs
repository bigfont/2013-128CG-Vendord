using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vendord.Desktop.WPF.App.DataAccess;
using Vendord.Desktop.WPF.App.Model;

namespace Vendord.Desktop.WPF.App.ViewModel
{
    public class ProductViewModel : WorkspaceViewModel
    {
        #region Fields

        readonly Product _product;
        readonly ProductRepository _productRepository;
        bool _isSelected;

        #endregion // Fields

        #region Constructor

        public ProductViewModel(Product product, ProductRepository productRepository)
        {
            if (product == null)
                throw new ArgumentNullException("product");

            if (productRepository == null)
                throw new ArgumentNullException("productRepository");

            _product = product;
            _productRepository = productRepository;
        }

        #endregion // Constructor

        #region Product Properties

        public string Upc { get; set; }
        public string Name { get; set; }

        #endregion // Product Properties

        #region Presentation Properties

        /// <summary>
        /// Gets/sets whether this customer is selected in the UI.
        /// </summary>
        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                if (value == _isSelected)
                    return;

                _isSelected = value;

                base.OnPropertyChanged("IsSelected");
            }
        }

        #endregion
    }
}
