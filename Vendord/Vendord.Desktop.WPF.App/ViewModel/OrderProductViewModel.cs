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
        readonly OrderProductRepository _orderProductRepository;

        #endregion // Fields

        #region Constructor

        public OrderProductViewModel(OrderProduct orderProduct, OrderProductRepository orderProductRepository)
        {
            if (orderProduct == null)
                throw new ArgumentNullException("orderProduct");

            if (orderProductRepository == null)
                throw new ArgumentNullException("orderProductRepository");

            _orderProduct = orderProduct;
            _orderProductRepository = orderProductRepository;
        }

        #endregion // Constructor

        #region OrderProduct Properties

        public Guid OrderID
        {
            get { return _orderProduct.OrderID; }
            set
            {
                if (value == _orderProduct.OrderID)
                    return;

                _orderProduct.OrderID = value;

                base.OnPropertyChanged("OrderID");
            }
        }

        #endregion // OrderProduct Properties
    }
}
