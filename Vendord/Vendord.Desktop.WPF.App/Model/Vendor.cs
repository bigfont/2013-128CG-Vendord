using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Vendord.Desktop.WPF.App.Model
{
    public class Vendor
    {
        #region Creation

        public static Vendor CreateNewVendor()
        {
            return new Vendor();
        }

        public static Vendor CreateVendor(
            string name)
        {
            return new Vendor
            {
                Name = name
            };
        }

        protected Vendor()
        {
        }

        #endregion // Creation

        #region State Properties

        /// <summary>
        /// Gets/sets the Vendor's last name.
        /// </summary>
        public string Name { get; set; }        

        #endregion // State Properties
    }
}
