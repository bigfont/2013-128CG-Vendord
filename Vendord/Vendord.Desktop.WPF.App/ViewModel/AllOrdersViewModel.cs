using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using Vendord.Desktop.WPF.App.DataAccess;
using System.Collections.Specialized;
using System.ComponentModel;
using Vendord.Desktop.WPF.App.Properties;

namespace Vendord.Desktop.WPF.App.ViewModel
{
    /// <summary>
    /// Represents a container of OrderViewModel objects
    /// that has support for staying synchronized with the
    /// OrderRepository.  This class also provides information
    /// related to multiple selected orders.
    /// </summary>
    public class AllOrdersViewModel : WorkspaceViewModel
    {
        #region Fields

        readonly OrderRepository _orderRepository;

        #endregion // Fields

        #region Constructor

        public AllOrdersViewModel(OrderRepository orderRepository)
        {
            if (orderRepository == null)
                throw new ArgumentNullException("orderRepository");

            _orderRepository = orderRepository;

            // Populate the AllOrders collection with OrderViewModels.
            this.CreateAllOrders();
        }

        void CreateAllOrders()
        {
            List<OrderViewModel> all =
                (from cust in _orderRepository.GetOrders()
                 select new OrderViewModel(cust, _orderRepository)).ToList();

            this.AllOrders = new ObservableCollection<OrderViewModel>(all);
        }

        #endregion // Constructor

        #region Public Interface

        /// <summary>
        /// Returns a collection of all the OrderViewModel objects.
        /// </summary>
        public ObservableCollection<OrderViewModel> AllOrders { get; private set; }

        #endregion // Public Interface

        #region  Base Class Overrides

        protected override void OnDispose()
        {
            foreach (OrderViewModel custVM in this.AllOrders)
                custVM.Dispose();

            this.AllOrders.Clear();
        }

        #endregion // Base Class Overrides     
    }
}
