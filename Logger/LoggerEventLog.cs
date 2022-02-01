using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Management;
using System.ComponentModel;

namespace TestAsyncLog
{
    /// <summary>
    /// Logger to trace potential exception (or any debug/info information) into Windows event log WHEN errors occured during asynchnously logging process into a text file
    /// If all log processes into text file complete sucessfully, Windows event log will be empty.
    /// </summary>
    public class LoggerEventLog
    {
        private string _sMsgErrorLogging;
        private string _sDateTimeLog;
        private string _sIDLog;
        private string _sTraceHeader;
        private string _sTrace;
        private bool _bMutextContext;
        private string _sShortMutexName;
        private string _sFullMutexName;
        private Exception _ex;
        private string _sExInfoMaxAttempt;
        private string _sLogAssemblyName;
        private static Object _lockObj = new Object();

        public LoggerEventLog(string sMsgErrorLogging, string sDateTimeLog, string sIDLog, string sTraceHeader, string sTrace, string sLogAssemblyName) : this(sMsgErrorLogging, sDateTimeLog, sIDLog, null, sTraceHeader, sTrace, sLogAssemblyName)
        {
        }

        public LoggerEventLog(string sMsgErrorLogging, string sDateTimeLog, string sIDLog, Exception ex, string sTraceHeader, string sTrace, string sLogAssemblyName)
        {
            MsgErrorLogging = sMsgErrorLogging;
            DateTimeLog = sDateTimeLog;
            IDLog = sIDLog;
            Ex = ex;
            TraceHeader = sTraceHeader;
            Trace = sTrace;
            SetMutextContext(false);
            SetShortMutexName(String.Empty);
            SetFullMutexName(String.Empty);
            SetExInfoMaxAttempt(String.Empty);
            LogAssemblyName = sLogAssemblyName;
        }

        public void Log()
        {
            System.Diagnostics.Debug.WriteLine(MsgErrorLogging);
            StringBuilder sbExInfo = new StringBuilder();
            AddStdLogInfoToSB(sbExInfo, DateTimeLog, IDLog, Ex, TraceHeader, Trace);
            if (IsMutextContext == true)
            {
                sbExInfo.AppendLine(" [Short Mutex name: " + (String.IsNullOrEmpty(ShortMutexName) ? "Not available!" : ShortMutexName) + "]");
                sbExInfo.AppendLine(" [Full Mutex name: " + (String.IsNullOrEmpty(FullMutexName) ? "Not available!" : FullMutexName) + "]");
            }
            if (!String.IsNullOrEmpty(ExInfoMaxAttempt))
            {
                sbExInfo.AppendLine(ExInfoMaxAttempt);
            }
            LogIntoEventLog(MsgErrorLogging, sbExInfo.ToString());
        }

        public string MsgErrorLogging { get => _sMsgErrorLogging; set => _sMsgErrorLogging = value; }
        public string DateTimeLog { get => _sDateTimeLog; set => _sDateTimeLog = value; }
        public string IDLog { get => _sIDLog; set => _sIDLog = value; }
        public Exception Ex { get => _ex; set => _ex = value; }
        public string TraceHeader { get => _sTraceHeader; set => _sTraceHeader = value; }
        public string Trace { get => _sTrace; set => _sTrace = value; }
        public string ShortMutexName { get => _sShortMutexName; }
        public string FullMutexName { get => _sFullMutexName; }
        public string ExInfoMaxAttempt { get => _sExInfoMaxAttempt; }
        public string LogAssemblyName { get => _sLogAssemblyName; set => _sLogAssemblyName = value; }
        public bool IsMutextContext { get => _bMutextContext; }

        public void SetMutextContext(bool bMutextContext)
        {
            _bMutextContext = bMutextContext;
        }

        public void SetExInfoMaxAttempt(string sExInfoMaxAttempt)
        {
            _sExInfoMaxAttempt = sExInfoMaxAttempt;
        }

        public void SetShortMutexName(string sShortMutexName)
        {
            if (IsMutextContext == false)
            {
                SetMutextContext(true);
            }
            _sShortMutexName = sShortMutexName;
        }

        public void SetFullMutexName(string sFullMutexName)
        {
            if (IsMutextContext == false)
            {
                SetMutextContext(true);
            }
            _sFullMutexName = sFullMutexName;
        }

        private void AddStdLogInfoToSB(StringBuilder sbExInfo, string sDateTimeLog, string sIDLog, Exception ex, string sTraceHeader, string sTrace)
        {
            // Informations de trace destinées à l'EventLog
            sbExInfo.AppendLine(" ");
            if (!String.IsNullOrEmpty(sDateTimeLog) && !String.IsNullOrEmpty(sIDLog))
            {
                sbExInfo.AppendLine(" [DateTime (yyyy/MM/dd HH:mm:ss.fff) ID Log: " + $"{sDateTimeLog} {sIDLog}" + "]");
            }
            else
            {
                sbExInfo.AppendLine(" [DateTime (yyyy/MM/dd HH:mm:ss.fff) ID Log: Not available!]");
            }
            sbExInfo.AppendLine(" [Process information: " + GetCurrentProcessInformation() + "]");
            sbExInfo.AppendLine(" [Thread information: " + GetCurrentThreadInformation() + "]");
            sbExInfo.AppendLine(" [Log Assembly Name: " + (String.IsNullOrEmpty(LogAssemblyName) ? "Not available!" : LogAssemblyName) + "]");

            if (ex != null)
            {
                LoggerFile.AddExceptionInfoToSB(sbExInfo, ex);
            }

            if (!String.IsNullOrEmpty(sTraceHeader) && !String.IsNullOrEmpty(sTrace))
            {
                sbExInfo.AppendLine(" [Trace header: " + sTraceHeader + "]");
                sbExInfo.AppendLine(" [Trace: " + sTrace + "]");
            }
            else
            {
                sbExInfo.AppendLine(" [Trace: Not available!]");
            }
        }

        private void LogIntoEventLog(string sMainMsg, string sDetailMsg)
        {
            string sMSG_LOG_XSRF_EVENTLOG = Properties.Resources.sEventLogMessageFormat;
            string sSOURCE_MESSAGE_EVENTLOG = Properties.Resources.sSourceEventLog;

            WriteEventLogEntry(string.Format(sMSG_LOG_XSRF_EVENTLOG, sMainMsg) + sDetailMsg, sSOURCE_MESSAGE_EVENTLOG);
        }

        private string GetCurrentThreadInformation()
        {
            string sInfo = null;
            Thread curThread = Thread.CurrentThread;
            lock (_lockObj)
            {
                sInfo = String.Format("Name (thread representation): {0} ({1})\n", curThread.Name, curThread.ToString()) +
                        String.Format("                          Background: {0}\n", curThread.IsBackground) +
                        String.Format("                          Thread Pool: {0}\n", curThread.IsThreadPoolThread) +
                        String.Format("                          Thread ID: {0}\n", curThread.ManagedThreadId);
            }
            return sInfo;
        }

        private string GetCurrentProcessInformation()
        {
            string sInfo = null;
            string sCommandLine = "Not available!";
            Process curProcess = Process.GetCurrentProcess();
            lock (_lockObj)
            {
                try
                {
                    sCommandLine = GetCommandLine(curProcess);
                }
                catch (Win32Exception ex) when ((uint)ex.ErrorCode == 0x80004005)
                {
                    // Intentionally empty - no security access to the process.
                }
                catch (InvalidOperationException)
                {
                    // Intentionally empty - the process exited before getting details.
                }
                sInfo = String.Format("Name (Id): '{0}' ({1})\n", curProcess.ProcessName, curProcess.Id) +
                        String.Format("                           Total Process Time: {0}\n", curProcess.TotalProcessorTime) +
                        String.Format("                           Commande line: {0}\n", sCommandLine);
            }
            return sInfo;
        }

        private string GetCommandLine(Process process)
        {
            // Using all managed objects, but it does dip down into the WMI realm
            using (ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT CommandLine FROM Win32_Process WHERE ProcessId = " + process.Id))
            using (ManagementObjectCollection objects = searcher.Get())
            {
                return objects.Cast<ManagementBaseObject>().SingleOrDefault()?["CommandLine"]?.ToString();
            }

        }

        private static void WriteEventLogEntry(string sMessage, string sSource)
        {
            // Create an instance of EventLog
            System.Diagnostics.EventLog eventLog = new System.Diagnostics.EventLog();

            // Check if the event source exists. If not create it.
            if (!System.Diagnostics.EventLog.SourceExists(sSource))
            {
                System.Diagnostics.EventLog.CreateEventSource(sSource, "Application");
            }

            // Set the source name for writing log entries.
            eventLog.Source = sSource;

            // Create an event ID to add to the event log
            int eventID = 8;

            // Write an entry to the event log.
            eventLog.WriteEntry(sMessage,
                                System.Diagnostics.EventLogEntryType.Error,
                                eventID);

            // Close the Event Log
            eventLog.Close();
        }

    }
}
