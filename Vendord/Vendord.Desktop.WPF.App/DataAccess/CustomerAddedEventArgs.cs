using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vendord.Desktop.WPF.App.Model;

namespace Vendord.Desktop.WPF.App.DataAccess
{
    /// <summary>
    /// Event arguments used by CustomerRepository's CustomerAdded event.
    /// </summary>
    public class CustomerAddedEventArgs : EventArgs
    {
        public CustomerAddedEventArgs(Customer newCustomer)
        {
            this.NewCustomer = newCustomer;
        }

        public Customer NewCustomer { get; private set; }
    }
}
