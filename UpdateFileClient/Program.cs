using System;
using System.Collections.Generic;
using System.Net;
using System.Windows.Forms;

namespace UpdateFileCommon
{
    static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3
                                        | SecurityProtocolType.Tls
                                        | (SecurityProtocolType)0x300 //Tls11
                                        | (SecurityProtocolType)0xC00; //Tls12
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new UpdateForm());
        }
    }
}
