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
        readonly Repository _orderRepository;

        #endregion // Fields

        #region Constructor

        public OrderViewModel(Order order, Repository repository)
        {
            if (order == null)
                throw new ArgumentNullException("order");

            if (repository == null)
                throw new ArgumentNullException("repository");

            _order = order;
            _orderRepository = repository;
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

        #endregion // Order Properties
    }
}
