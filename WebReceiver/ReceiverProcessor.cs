using System;
using System.IO;
using System.Text;

namespace WebReceiver
{
    public class ReceiverProcessor
    {
        public void ShowMessage(int message)
        {
            using (FileStream fs = new FileStream(@"C:\data\logs.txt", FileMode.Open, FileAccess.ReadWrite))
            {
                using (StreamWriter srOutFile = new StreamWriter(fs, Encoding.UTF8))
                {
                    srOutFile.BaseStream.Seek(0, SeekOrigin.End);
                    srOutFile.WriteLine(message);
                    srOutFile.Flush();
                    srOutFile.Close();
                }
            }
        }
    }
}
