using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vendord.Desktop.WPF.App.DataAccess;
using Vendord.Desktop.WPF.App.Model;
using System.Windows.Input;
using System.ComponentModel;
using Vendord.Desktop.WPF.App.Properties;

namespace Vendord.Desktop.WPF.App.ViewModel
{
    /// <summary>
    /// A UI-friendly wrapper for a Order object.
    /// </summary>
    public class OrderViewModel : WorkspaceViewModel
    {
        #region Fields

        readonly Order _order;
        readonly Repository _repository;

        #endregion // Fields

        #region Constructor

        public OrderViewModel(Order order, Repository repository)
        {
            if (order == null)
                throw new ArgumentNullException("order");

            if (repository == null)
                throw new ArgumentNullException("repository");

            _order = order;
            _repository = repository;

            CreateOrderProducts();
            CreateOrderVendors();
        }

        void CreateOrderProducts()
        {
            var query =
                from o in _repository.GetOrders()
                join op in _repository.GetOrderProducts()
                on o.Id equals op.OrderId
                select new OrderProductViewModel(op, _repository);

            this.OrderProducts = query.ToList<OrderProductViewModel>();
        }

        void CreateOrderVendors()
        {
            var productViewModels =
                from op in OrderProducts
                select op.Product;

            var vendorViewModels =
                from p in productViewModels
                select p.Vendor;

            this.OrderVendors = vendorViewModels.ToList();
        }

        #endregion // Constructor

        #region Order Properties

        public Guid Id
        {
            get { return _order.Id; }
            set
            {
                if (value == _order.Id)
                    return;

                _order.Id = value;

                base.OnPropertyChanged("Id");
            }
        }

        public string Name
        {
            get { return _order.Name; }
            set
            {
                if (value == _order.Name)
                    return;

                _order.Name = value;

                base.OnPropertyChanged("Name");
            }
        }

        public List<OrderProductViewModel> OrderProducts { get; private set; }

        public List<VendorViewModel> OrderVendors { get; private set; }

        #endregion // Order Properties        
    }
}
