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
    /// A UI-friendly wrapper for a Vendor object.
    /// </summary>
    public class VendorViewModel : ViewModelBase
    {
        #region Fields

        readonly Vendor _vendor;
        readonly Repository _repository;

        #endregion // Fields

        #region Constructor

        public VendorViewModel(Vendor vendor, Repository repository)
        {
            if (vendor == null)
                throw new ArgumentNullException("vendor");

            if (repository == null)
                throw new ArgumentNullException("repository");

            _vendor = vendor;
            _repository = repository;
        }

        #endregion // Constructor

        #region Order Properties

        public int? Id
        {
            get { return _vendor.Id; }
            set
            {
                if (value == _vendor.Id)
                    return;

                _vendor.Id = value;

                base.OnPropertyChanged("Id");
            }
        }

        public string Name
        {
            get { return _vendor.Name; }
            set
            {
                if (value == _vendor.Name)
                    return;

                _vendor.Name = value;

                base.OnPropertyChanged("Name");
            }
        }

        #endregion // Order Properties
    }
}
