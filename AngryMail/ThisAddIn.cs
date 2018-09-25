using CameraPreview;
using System.Windows.Interop;
using Outlook = Microsoft.Office.Interop.Outlook;

namespace AngryMail
{
    public partial class ThisAddIn
    {
        private void ThisAddIn_Startup(object sender, System.EventArgs e)
        {
            Application.ItemSend += Application_ItemSend;
        }

        private void Application_ItemSend(object item, ref bool cancel)
        {
            //MailItemにキャストできないものは会議招待などメールではないものなので、何もしない。
            if (!(item is Outlook._MailItem)) return;

            var previewWindow = new PreviewWindow();
            var activeWindow = Globals.ThisAddIn.Application.ActiveWindow();
            var outlookHandle = new NativeMethods(activeWindow).Handle;
            var windowInteropHelper = new WindowInteropHelper(previewWindow) { Owner = outlookHandle };
            var dialogResult = previewWindow.ShowDialog();

            if (dialogResult == true)
            {
                //Send Mail.
            }
            else
            {
                cancel = true;
            }
        }

        private void InternalStartup() => Startup += ThisAddIn_Startup;
    }
}
