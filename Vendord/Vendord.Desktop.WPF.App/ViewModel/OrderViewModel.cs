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
        readonly OrderRepository _orderRepository;

        #endregion // Fields

        #region Constructor

        public OrderViewModel(Order order, OrderRepository orderRepository)
        {
            if (order == null)
                throw new ArgumentNullException("order");

            if (orderRepository == null)
                throw new ArgumentNullException("orderRepository");

            _order = order;
            _orderRepository = orderRepository;
        }

        #endregion // Constructor

        #region Order Properties

        public string LastName
        {
            get { return _order.LastName; }
            set
            {
                if (value == _order.LastName)
                    return;

                _order.LastName = value;

                base.OnPropertyChanged("LastName");
            }
        }

        #endregion // Order Properties
    }
}
