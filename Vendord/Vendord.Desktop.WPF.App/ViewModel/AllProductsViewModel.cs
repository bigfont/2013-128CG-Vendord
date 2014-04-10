using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vendord.Desktop.WPF.App.DataAccess;
using System.Collections.ObjectModel;

namespace Vendord.Desktop.WPF.App.ViewModel
{
    public class AllProductsViewModel : WorkspaceViewModel
    {
         #region Fields

        readonly ProductRepository _productRepository;

        #endregion // Fields

        #region Constructor

        public AllProductsViewModel(ProductRepository productRepository)
        {
            if (productRepository == null)
                throw new ArgumentNullException("productRepository");

            _productRepository = productRepository;

            // Populate the AllProducts collection with OrderViewModels.
            this.CreateAllProducts();
        }

        void CreateAllProducts()
        {
            List<ProductViewModel> all =
                (from prod in _productRepository.GetProducts()
                 select new ProductViewModel(prod, _productRepository)).ToList();

            this.AllProducts = new ObservableCollection<ProductViewModel>(all);
        }

        #endregion // Constructor

        #region Public Interface

        /// <summary>
        /// Returns a collection of all the OrderViewModel objects.
        /// </summary>
        public ObservableCollection<ProductViewModel> AllProducts { get; private set; }

        #endregion // Public Interface

        #region  Base Class Overrides

        protected override void OnDispose()
        {
            foreach (ProductViewModel prodVM in this.AllProducts)
                prodVM.Dispose();

            this.AllProducts.Clear();
        }

        #endregion // Base Class Overrides     
    }
}
