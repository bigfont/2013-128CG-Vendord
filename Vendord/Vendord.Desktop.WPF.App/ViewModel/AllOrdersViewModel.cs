using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using Vendord.Desktop.WPF.App.DataAccess;
using System.Collections.Specialized;
using System.ComponentModel;
using Vendord.Desktop.WPF.App.Properties;
using Vendord.Printer;

namespace Vendord.Desktop.WPF.App.ViewModel
{
    /// <summary>
    /// Represents a container of OrderViewModel objects
    /// that has support for staying synchronized with the
    /// Repository.  This class also provides information
    /// related to multiple selected orders.
    /// </summary>
    public class AllOrdersViewModel : WorkspaceViewModel
    {
        #region Fields

        readonly Repository _repository;
        ReadOnlyCollection<CommandViewModel> _commands;
        ObservableCollection<OrderViewModel> _allOrders;

        #endregion // Fields

        #region Constructor

        public AllOrdersViewModel(Repository repository)
        {
            if (repository == null)
                throw new ArgumentNullException("repository");

            _repository = repository;

            base.DisplayName = Strings.AllOrdersViewModel_DisplayName;
            
            this.CreateAllOrders();
        }

        void CreateAllOrders()
        {
            _repository.GetOrders().CollectionChanged += new NotifyCollectionChangedEventHandler((s, e) => {

                CreateAllOrders();

            });

            List<OrderViewModel> all =
                (from ord in _repository.GetOrders()
                 select new OrderViewModel(ord, _repository)).ToList();            

            this.AllOrders = new ObservableCollection<OrderViewModel>(all);
        }

        #endregion // Constructor

        #region Commands

        /// <summary>
        /// Returns a read-only list of commands 
        /// that the UI can display and execute.
        /// </summary>
        public ReadOnlyCollection<CommandViewModel> Commands
        {
            get
            {
                if (_commands == null)
                {
                    List<CommandViewModel> cmds = this.CreateCommands();
                    _commands = new ReadOnlyCollection<CommandViewModel>(cmds);
                }
                return _commands;
            }
        }

        List<CommandViewModel> CreateCommands()
        {
            return new List<CommandViewModel>
            {
                new CommandViewModel(
                    Strings.AllOrderViewModel_Command_PrintOrderForSelectedVendor,
                    new RelayCommand(param => this.PrintOrderForSelectedVendor())),

                new CommandViewModel(
                    Strings.AllOrderViewModel_Command_PrintOrderForAllVendors,
                    new RelayCommand(param => this.PrintOrderForAllVendors()))
            };
        }

        private void PrintOrderForAllVendors()
        {
            OrderViewModel selectedOrderVm = (OrderViewModel)SelectedOrder;

            // iterate vendors 
            selectedOrderVm.OrderVendors.ToList().ForEach(v =>
            {
                // set selected vendor
                selectedOrderVm.SelectedVendor = v;
                // print selected vendor
                PrintOrderForSelectedVendor();
                // wait until I/O completes, otherwise we print duplicate orders
                System.Threading.Thread.Sleep(500);
            });
        }

        private void PrintOrderForSelectedVendor()
        {
            // get the selected order and vendor
            OrderViewModel selectedOrderVm = (OrderViewModel)SelectedOrder;
            VendorViewModel selectedVendorVm = (VendorViewModel)(selectedOrderVm.SelectedVendor);

            // filter order products on selected vendor
            List<OrderProductViewModel> filteredOrderProductVms =
                selectedOrderVm.OrderProducts
                .Where(op => op.Product.VendorId == selectedVendorVm.Id)
                .ToList();

            // create order
            var pOrderProducts = MakePrintableOrderProductList(filteredOrderProductVms);
            var pOrder = MakePrintableOrder(
                selectedVendorVm.Name,
                "Country Grocer Salt Spring",
                "(Coming soon)",
                DateTime.Now,
                pOrderProducts);

            // print
            TxtPrinter.PrintOrderForOneVendor(pOrder);
        }

        #endregion // Commands

        #region Public Interface

        /// <summary>
        /// Returns a collection of all the OrderViewModel objects.
        /// </summary>
        public ObservableCollection<OrderViewModel> AllOrders { get; private set; }

        object _SelectedOrder;
        public object SelectedOrder
        {
            get
            {
                return _SelectedOrder;
            }
            set
            {
                if (_SelectedOrder != value)
                {
                    _SelectedOrder = value;
                    base.OnPropertyChanged("SelectedOrder");
                }
            }
        }

        #endregion // Public Interface

        #region  Base Class Overrides

        protected override void OnDispose()
        {
            foreach (OrderViewModel ordVM in this.AllOrders)
                ordVM.Dispose();

            this.AllOrders.Clear();
        }

        #endregion // Base Class Overrides

        #region Print Helpers

        private List<PrintableOrderProduct> MakePrintableOrderProductList(IEnumerable<OrderProductViewModel> orderProductVms)
        {
            var query = orderProductVms
                .Select(op =>
                {
                    PrintableOrderProduct pop = new PrintableOrderProduct();
                    pop.Upc = op.Product.Upc;
                    pop.CertCode = op.Product.CertCode;
                    pop.ProductName = op.Product.Name;
                    pop.CasesToOrder = op.CasesToOrder;
                    return pop;
                });

            return query.ToList();
        }

        private PrintableOrder MakePrintableOrder(string to, string from, string department, DateTime date, IEnumerable<PrintableOrderProduct> pOrderProducts)
        {
            PrintableOrder pOrder = new PrintableOrder();
            pOrder.To = to;
            pOrder.From = from;
            pOrder.Department = department;
            pOrder.Date = date;
            pOrder.PrintableOrderProducts = pOrderProducts.ToList();
            return pOrder;
        }

        #endregion
    }
}
