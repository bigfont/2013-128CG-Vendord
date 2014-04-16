using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Vendord.Printer
{
    public class PrintableOrderProduct
    {
        // properties
        public long Upc { get; set; }
        public long CertCode { get; set; }
        public string ProductName { get; set; }
        public int CasesToOrder { get; set; }

        // property formats
        private const string UpcColumnFormat = "{0,-20}";
        private const string CertCodeColumnFormat = "{0,-20}";
        private const string ProductNameColumnFormat = "{0,-40}";
        private const string CasesToOrderColumnFormat = "{0,-20}";

        // property names
        private static string UpcMember { get { return MemberHelper.GetMemberName((PrintableOrderProduct c) => c.Upc); } }
        private static string CertCodeMember { get { return MemberHelper.GetMemberName((PrintableOrderProduct c) => c.CertCode); } }
        private static string ProductNameMember { get { return MemberHelper.GetMemberName((PrintableOrderProduct c) => c.ProductName); } }
        private static string CasesToOrderMember { get { return MemberHelper.GetMemberName((PrintableOrderProduct c) => c.CasesToOrder); } }
        
        public static string CreateColumnHeaders()
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendFormat(UpcColumnFormat, UpcMember);
            builder.AppendFormat(CertCodeColumnFormat, CertCodeMember);
            builder.AppendFormat(ProductNameColumnFormat, ProductNameMember);
            builder.AppendFormat(CasesToOrderColumnFormat, CasesToOrderMember);
            return builder.ToString();
        }

        public static string CreateRow(PrintableOrderProduct pop)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendFormat(UpcColumnFormat, pop.Upc);
            builder.AppendFormat(CertCodeColumnFormat, pop.CertCode);
            builder.AppendFormat(ProductNameColumnFormat, pop.ProductName);
            builder.AppendFormat(CasesToOrderColumnFormat, pop.CasesToOrder);
            return builder.ToString();
        }
    }
}
