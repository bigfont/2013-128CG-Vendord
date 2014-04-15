using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vendord.Desktop.WPF.App.DataAccess;
using System.Collections.ObjectModel;
using Vendord.Desktop.WPF.App.Properties;

namespace Vendord.Desktop.WPF.App.ViewModel
{
    public class AllProductsViewModel : WorkspaceViewModel
    {
         #region Fields

        readonly Repository _repository;

        #endregion // Fields

        #region Constructor

        public AllProductsViewModel(Repository repository)
        {
            if (repository == null)
                throw new ArgumentNullException("repository");

            _repository = repository;

            base.DisplayName = Strings.AllProductsViewModel_DisplayName;

            // Populate the AllProducts collection with OrderViewModels.
            this.CreateAllProducts();
        }

        void CreateAllProducts()
        {
            List<ProductViewModel> all =
                (from prod in _repository.GetProducts()
                 select new ProductViewModel(prod, _repository)).ToList();

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
