using System;
using System.Windows.Forms;

namespace ProxyChecker.Utils
{
    public class FileDialogUtils
    {
        [STAThread]
        public static string SelectFile()
        {
            var dialog = new OpenFileDialog
            {
                Multiselect = false,
                DefaultExt = "*.txt"
            };

            using (dialog)
            {
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    return dialog.FileName;
                }
            }
            return null;
        }
    }
}