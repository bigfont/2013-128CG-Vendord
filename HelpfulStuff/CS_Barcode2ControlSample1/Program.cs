using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace CS_Barcode2ControlSample1
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [MTAThread]
        static void Main()
        {
            Form1 frm1 = new Form1();
            frm1.DoScale();
            Application.Run(frm1);
        }
    }
}