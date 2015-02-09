using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WinFormsClient
{
    public class BaseForm : Form
    {
        private const int WM_NCHITTEST = 0x84;
        private const int HTCLIENT = 0x1;
        private const int HTCAPTION = 0x2;

        public AgentInfo AgentInfo;
        public Form source;
        public BaseForm()
        {
            
        }
        protected override void WndProc(ref Message message)
        {
            base.WndProc(ref message);
            try
            {
                if (message.Msg == WM_NCHITTEST && (int)message.Result == HTCLIENT)
                    message.Result = (IntPtr)HTCAPTION;
            }
            catch (Exception)
            {
                
                
            }
            
        }

        protected override void Dispose(bool disposing)
        {
            if (source != null)
            {
                source.Dispose();
                source = null;
            }
            base.Dispose(disposing);
           
        }

    }
}
