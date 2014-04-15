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
        }

        #endregion // Constructor

        #region Order Properties

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

        public List<OrderProduct> OrderProducts
        {
            get { return _order.OrderProducts; }
            set
            {
                if (value == _order.OrderProducts)
                    return;

                _order.OrderProducts = value;

                base.OnPropertyChanged("OrderProducts");
            }
        }

        public List<Vendor> Vendors
        {
            get { return _order.Vendors; }
            set
            {
                if (value == _order.Vendors)
                    return;

                _order.Vendors = value;

                base.OnPropertyChanged("Vendors");
            }
        }

        #endregion // Order Properties
    }
}
