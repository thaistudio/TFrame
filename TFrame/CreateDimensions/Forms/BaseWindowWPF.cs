using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
namespace TFrame
{
    public class BaseWindowWPF : Window, IDisposable
    {
        public BaseWindowWPF()
        {
            System.Windows.Interop.WindowInteropHelper helper = new System.Windows.Interop.WindowInteropHelper(this);
            helper.Owner = System.Diagnostics.Process.GetCurrentProcess().MainWindowHandle;

            WindowStyle = System.Windows.WindowStyle.ToolWindow;
            WindowStartupLocation = System.Windows.WindowStartupLocation.CenterOwner;
            ShowInTaskbar = true;
        }
        public void Dispose()
        {
            this.Dispose();
        }
    }
}
