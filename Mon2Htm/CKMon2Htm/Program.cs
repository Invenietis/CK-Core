using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CK.Mon2Htm
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            using( Mutex m = new Mutex( false, "HtmlGenerator.AppMainMutex" ) )
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault( false );

                Application.Run( new MainForm() );
            }
        }
    }
}
