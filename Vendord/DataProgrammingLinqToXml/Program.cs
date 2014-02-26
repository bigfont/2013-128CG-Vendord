using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace DataProgrammingLinqToXml
{
    class Product
    {
        public string Upc { get; set; }
        public string Description { get; set; }
        public decimal? Normal_Price { get; set; }
        public int? Vendor { get; set; }
    }
    class Program
    {
        static void Main(string[] args)
        {
            XElement products = XElement.Load("products.xml");
            var query =
                from p in products.Descendants("Products")
                select new Product()
                {
                    Upc = (string)p.Element("upc"),
                    Description = (string)p.Element("description"),
                    Normal_Price = (decimal?)p.Element("normal_price"),
                    Vendor = (int?)p.Element("vendor")
                };
            foreach (var p in query)
            {
                Console.WriteLine(p.Upc + ":" + p.Description + ":" + p.Normal_Price + ":" + p.Vendor);
            }
            Console.ReadLine();
        }        
    }
}
