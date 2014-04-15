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

            CreateProduct();
        }

        void CreateProduct()
        {
            var query =
                from p in _repository.GetProducts()
                where p.Upc == this._orderProduct.ProductUpc
                select new ProductViewModel(p, _repository);

            this.Product = query.FirstOrDefault();
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

        public long ProductUpc
        {
            get { return _orderProduct.ProductUpc; }
            set
            {
                if (value == _orderProduct.ProductUpc)
                    return;

                _orderProduct.ProductUpc = value;

                base.OnPropertyChanged("ProductUpc");
            }
        }

        public int CasesToOrder
        {
            get { return _orderProduct.CasesToOrder; }
            set
            {
                if (value == _orderProduct.CasesToOrder)
                    return;

                _orderProduct.CasesToOrder = value;

                base.OnPropertyChanged("CasesToOrder");
            }
        }

        public ProductViewModel Product { get; private set; }

        #endregion // OrderProduct Properties
    }
}
