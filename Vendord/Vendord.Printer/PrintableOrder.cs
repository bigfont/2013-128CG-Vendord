using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Vendord.Printer
{
    public class PrintableOrder
    {
        public string To { get; set; }
        public string From { get; set; }
        public DateTime Date { get; set; }
        public string Department { get; set; }
        public List<PrintableOrderProduct> PrintableOrderProducts { get; set; }

        // property formats
        private const string ToHeaderFormat = "{0,-50}";
        private const string FromHeaderFormat = "{0,-50}";
        private const string DateHeaderFormat = "{0,-50}";
        private const string DepartmentsHeaderFormat = "{0,-50}";

        // property names
        private static string ToMember { get { return MemberHelper.GetMemberName((PrintableOrder c) => c.To); } }
        private static string FromMember { get { return MemberHelper.GetMemberName((PrintableOrder c) => c.From); } }
        private static string DateMember { get { return MemberHelper.GetMemberName((PrintableOrder c) => c.Date); } }
        private static string DepartmentMember { get { return MemberHelper.GetMemberName((PrintableOrder c) => c.Department); } }

        public static string CreateOrderHeader(PrintableOrder pOrder)
        {
            string orderDate = pOrder.Date.ToString("ddd dd MMMM yyyy");

            StringBuilder builder = new StringBuilder();
            builder.AppendFormat(ToHeaderFormat, ToMember + ": " + pOrder.To);
            builder.AppendFormat(DateHeaderFormat, DateMember + ": " + orderDate);
            builder.AppendLine();
            builder.AppendFormat(FromHeaderFormat, FromMember + ": " + pOrder.From);
            builder.AppendFormat(DepartmentsHeaderFormat, DepartmentMember + ": " + pOrder.Department);
            return builder.ToString();
        }
    }
}
