using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using Vendord.MyDatabaseDataSetTableAdapters;

namespace Vendord
{
    internal static class DataUtilities
    {
        internal static void SeedDB()
        {
            MyDatabaseDataSet.ProductsDataTable products = 
                new MyDatabaseDataSet.ProductsDataTable();
            ProductsTableAdapter productsTableAdapter = 
                new ProductsTableAdapter();
            for (int i = 0; i < 10; ++i)
            {
                products.AddProductsRow(i.ToString());    
            }            
            productsTableAdapter.Update(products);
        }
    }
}
