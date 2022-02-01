using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace TestAsyncLog
{
    class TestAsyncLog
    {
        public void StartTest()
        {
            // A compléter avec du multitask qui effectue des appels à LogAsync !
            LoggerFile moduleLogger = new LoggerFile();
            HttpContext context = HttpContext.Current;
            string sLogEventName = "PreRequestHandlerExecute";

            string sDetailLog = String.Empty;

            int iIndiceLog = 0;
            for (int i = 1; i <= 200; i++)
            {
                sDetailLog = String.Format("Test A{0}", i.ToString());
                moduleLogger.LogAsync(ref iIndiceLog, LogCategory.Information, sDetailLog, context, sLogEventName, true);
                Thread.Sleep(100);
            }
        }
    }

}
