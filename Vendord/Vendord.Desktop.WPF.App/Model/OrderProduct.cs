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

        public static OrderProduct CreateOrderProduct(
            Guid OrderId)
        {
            return new OrderProduct
            {
                OrderId = OrderId
            };
        }

        protected OrderProduct()
        {
        }

        #endregion // Creation

        #region State Properties
        
        public Guid OrderId { get; set; }

        public int ProductId { get; set; }

        #endregion // State Properties
    }
}
