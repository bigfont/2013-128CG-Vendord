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

        public static OrderProduct CreateOrderProduct(Guid orderId, long productUpc, int casesToOrder)
        {
            return new OrderProduct
            {
                OrderId = orderId,
                ProductUpc = productUpc,
                CasesToOrder = casesToOrder
            };
        }

        protected OrderProduct()
        {
        }

        #endregion // Creation

        #region State Properties
        
        public Guid OrderId { get; set; }

        public long ProductUpc { get; set; }

        public int CasesToOrder { get; set; }

        #endregion // State Properties
    }
}
