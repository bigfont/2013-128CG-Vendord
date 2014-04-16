using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vendord.Desktop.WPF.App.DataAccess;
using Vendord.Desktop.WPF.App.Model;

namespace Vendord.Desktop.WPF.App.ViewModel
{
    public class ProductViewModel : WorkspaceViewModel
    {
        #region Fields

        readonly Product _product;
        readonly Repository _repository;
        bool _isSelected;

        #endregion // Fields

        #region Constructor

        public ProductViewModel(Product product, Repository repository)
        {
            if (product == null)
                throw new ArgumentNullException("product");

            if (repository == null)
                throw new ArgumentNullException("repository");

            _product = product;
            _repository = repository;

            this.CreateVendor();
        }

        void CreateVendor()
        {
            var query =
                from v in _repository.GetVendors()
                where v.Id == this.VendorId
                select new VendorViewModel(v, _repository);

            this.Vendor = query.FirstOrDefault();
        }

        #endregion // Constructor

        #region Product Properties

        public long Upc
        {
            get { return _product.Upc; }
            set
            {
                if (value == _product.Upc)
                    return;

                _product.Upc = value;

                base.OnPropertyChanged("Upc");
            }
        }
        public long CertCode
        {
            get { return _product.CertCode; }
            set
            {
                if (value == _product.CertCode)
                    return;

                _product.CertCode = value;

                base.OnPropertyChanged("CertCode");
            }
        }
        public string Name
        {
            get { return _product.Name; }
            set
            {
                if (value == _product.Name)
                    return;

                _product.Name = value;

                base.OnPropertyChanged("Name");
            }
        }
        public int? VendorId
        {
            get { return _product.VendorId; }
            set
            {
                if (value == _product.VendorId)
                    return;

                _product.VendorId = value;

                base.OnPropertyChanged("VendorId");
            }
        }

        public VendorViewModel Vendor { get; private set; }

        #endregion // Product Properties

        #region Presentation Properties

        /// <summary>
        /// Gets/sets whether this customer is selected in the UI.
        /// </summary>
        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                if (value == _isSelected)
                    return;

                _isSelected = value;

                base.OnPropertyChanged("IsSelected");
            }
        }

        #endregion
    }
}
