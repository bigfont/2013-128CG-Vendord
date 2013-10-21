[module:
    System.Diagnostics.CodeAnalysis.SuppressMessage(
        "StyleCop.CSharp.DocumentationRules", "*",
        Justification = "Reviewed. Suppression of all documentation rules is OK here.")]

namespace Vendord.SmartDevice.Shared
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Windows.Forms;

    internal static class FormHelper
    {
        internal static List<T> GetControlsByType<T>(Control controlToSearch, bool searchDescendents) where T : class
        {
            List<T> result;
            result = new List<T>();
            foreach (Control c in controlToSearch.Controls)
            {
                if (c.GetType() == typeof(T))
                {
                    result.Add(c as T);
                }

                if (searchDescendents)
                {
                    result.AddRange(GetControlsByType<T>(c, true));
                }
            }

            return result;
        }

        internal static List<T> GetControlsByName<T>(
            Control controlToSearch, string nameOfControlsToFind, bool searchDescendants)
            where T : class
        {
            List<T> result;
            result = new List<T>();
            foreach (Control c in controlToSearch.Controls)
            {
                if (c.Name == nameOfControlsToFind && c.GetType() == typeof(T))
                {
                    result.Add(c as T);
                }

                if (searchDescendants)
                {
                    result.AddRange(GetControlsByName<T>(c, nameOfControlsToFind, true));
                }
            }

            return result;
        }        

        internal static bool KeyPressIsDigit(KeyPressEventArgs e)
        {
            return char.IsDigit(e.KeyChar);
        }

        internal static bool KeyPressIsControlKey(KeyPressEventArgs e)
        {
            return char.IsControl(e.KeyChar);
        }

        internal static bool TextboxValueIsInt32(object sender, KeyPressEventArgs e)
        {
            string newValue;
            bool isInt32;

            isInt32 = false;
            if (sender is TextBox)
            {
                newValue = (sender as TextBox).Text + e.KeyChar;
                try
                {
                    Convert.ToInt32(newValue);
                    isInt32 = true;
                }
                catch (Exception)
                {
                    // do nothing
                }
            }
                        
            return isInt32;
        }
    }
}