using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vendord.Desktop.WPF.App.DataAccess;
using Vendord.Desktop.WPF.App.Model;
using System.Windows.Input;
using System.ComponentModel;
using Vendord.Desktop.WPF.App.Properties;
using System.Collections.ObjectModel;
using System.Windows.Data;

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

            OrderProductsView = CollectionViewSource.GetDefaultView(OrderProducts);
        }

        void CreateOrderProducts()
        {
            var query =
                from op in _repository.GetOrderProducts()
                where op.OrderId == this.Id                
                select new OrderProductViewModel(op, _repository);

            this.OrderProducts = new ObservableCollection<OrderProductViewModel>(query.ToList());
        }        
        
        void CreateOrderVendors()
        {
            var productViewModels =
                from op in this.OrderProducts
                select op.Product;

            var vendorViewModels =
                from p in productViewModels
                select p.Vendor;

            var distinct =
                from v in vendorViewModels
                group v by v.Name into g
                select g.FirstOrDefault();

            this.OrderVendors = new ObservableCollection<VendorViewModel>(distinct.ToList());
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

        ObservableCollection<OrderProductViewModel> _OrderProducts;
        public ObservableCollection<OrderProductViewModel> OrderProducts
        {
            get
            {
                return _OrderProducts;
            }
            set
            {
                if (_OrderProducts != value)
                {
                    _OrderProducts = value;
                    base.OnPropertyChanged("OrderProducts");
                }
            }
        }

        ObservableCollection<VendorViewModel> _OrderVendors;
        public ObservableCollection<VendorViewModel> OrderVendors
        {
            get
            {
                return _OrderVendors;
            }
            set
            {
                if (_OrderVendors != value)
                {
                    _OrderVendors = value;
                    base.OnPropertyChanged("OrderVendors");
                }
            }
        }

        public ICollectionView OrderProductsView { get; set; }

        object _SelectedVendor;
        public object SelectedVendor
        {
            get
            {
                return _SelectedVendor;
            }
            set
            {
                if (_SelectedVendor != value)
                {
                    _SelectedVendor = value;

                    FilterProductsOnVendor();                    

                    base.OnPropertyChanged("SelectedVendor");
                }
            }
        }

        #endregion // Order Properties       

        #region Private Helpers

        void FilterProductsOnVendor()
        {
            ListCollectionView list = OrderProductsView as ListCollectionView;
            list.Filter = new Predicate<object>(x => (x as OrderProductViewModel).Product.VendorId == (SelectedVendor as VendorViewModel).Id);
            OrderProductsView.Refresh();
        }

        #endregion
    }
}
