using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vendord.Desktop.WPF.App.DataAccess;
using System.Collections.ObjectModel;
using Vendord.Desktop.WPF.App.Properties;

namespace Vendord.Desktop.WPF.App.ViewModel
{
    class AllOrderProductsViewModel : WorkspaceViewModel
    {
        #region Fields

        readonly OrderProductRepository _orderProductRepository;

        #endregion // Fields

        #region Constructor

        public AllOrderProductsViewModel(OrderProductRepository orderProductRepository)
        {
            if (orderProductRepository == null)
                throw new ArgumentNullException("orderProductRepository");

            _orderProductRepository = orderProductRepository;

            base.DisplayName = Strings.AllOrderProductsViewModel_DisplayName; 

            // Populate the AllOrderProducts collection with OrderViewModels.
            this.CreateAllOrderProducts();
        }

        void CreateAllOrderProducts()
        {
            List<OrderProductViewModel> all =
                (from ordProd in _orderProductRepository.GetOrderProducts()
                 select new OrderProductViewModel(ordProd, _orderProductRepository)).ToList();

            this.AllOrderProducts = new ObservableCollection<OrderProductViewModel>(all);
        }

        #endregion // Constructor

        #region Public Interface

        /// <summary>
        /// Returns a collection of all the OrderViewModel objects.
        /// </summary>
        public ObservableCollection<OrderProductViewModel> AllOrderProducts { get; private set; }

        #endregion // Public Interface

        #region  Base Class Overrides

        protected override void OnDispose()
        {
            foreach (OrderProductViewModel ordProdVM in this.AllOrderProducts)
                ordProdVM.Dispose();

            this.AllOrderProducts.Clear();
        }

        #endregion // Base Class Overrides     
    }
}
