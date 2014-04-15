using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Vendord.Desktop.WPF.App.Model
{
    public class OrderProduct
    {
        #region Creation

        public static OrderProduct CreateNewOrderProduct()
        {
            return new OrderProduct();
        }

        public static OrderProduct CreateOrderProduct(Guid orderId, long productUpc)
        {
            return new OrderProduct
            {
                OrderId = orderId,
                ProductUpc = productUpc
            };
        }

        protected OrderProduct()
        {
        }

        #endregion // Creation

        #region State Properties
        
        public Guid OrderId { get; set; }

        public long ProductUpc { get; set; }

        #endregion // State Properties
    }
}
