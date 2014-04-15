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
            int? id, string name)
        {
            return new Vendor
            {
                Id = id,
                Name = name                
            };
        }

        protected Vendor()
        {
        }

        #endregion // Creation

        #region State Properties

        /// <summary>
        /// Gets/sets the Vendor's Id.
        /// </summary>
        public int? Id { get; set; }        

        /// <summary>
        /// Gets/sets the Vendor's name.
        /// </summary>
        public string Name { get; set; }        

        #endregion // State Properties
    }
}
