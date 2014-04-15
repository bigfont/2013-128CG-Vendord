using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vendord.Desktop.WPF.App.Model;
using Vendord.Desktop.WPF.App.DataAccess;

namespace Vendord.Desktop.WPF.App.ViewModel
{
    public class OrderProductViewModel : WorkspaceViewModel
    {
        #region Fields

        readonly OrderProduct _orderProduct;
        readonly Repository _repository;

        #endregion // Fields

        #region Constructor

        public OrderProductViewModel(OrderProduct orderProduct, Repository repository)
        {
            if (orderProduct == null)
                throw new ArgumentNullException("orderProduct");

            if (repository == null)
                throw new ArgumentNullException("repository");

            _orderProduct = orderProduct;
            _repository = repository;
        }

        #endregion // Constructor

        #region OrderProduct Properties

        public Guid OrderId
        {
            get { return _orderProduct.OrderId; }
            set
            {
                if (value == _orderProduct.OrderId)
                    return;

                _orderProduct.OrderId = value;

                base.OnPropertyChanged("OrderId");
            }
        }

        #endregion // OrderProduct Properties
    }
}
