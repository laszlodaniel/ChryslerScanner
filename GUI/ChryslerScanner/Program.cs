using System;
using System.Windows.Forms;
using ChryslerScanner.Services;

namespace ChryslerScanner
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(ContainerManager.Instance.GetInstance<MainForm>()); // resolve MainForm from the container and run app
        }
    }
}
