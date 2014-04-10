﻿using System;
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

        public string Name
        {
            get { return _product.Name; }
            set
            {
                if (value == _product.Name)
                    return;

                _product.Name = value;

                base.OnPropertyChanged("Name");
            }
        }

        #endregion // Order Properties
    }
}
