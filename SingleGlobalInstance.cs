using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Threading;

namespace TestAsyncLog
{
    public class SingleGlobalInstance : IDisposable
    {
        public bool _hasHandle = false;
        Mutex _mutex;
        string _fullMutexName = string.Empty;

        public string MutexName { 
            get {
                if (_mutex != null)
                {
                    return _fullMutexName;
                }
                else
                { 
                    return String.Empty; 
                }
            } 
        }

        private void InitMutex(string sShortMutexName)
        {
            string sMutexId = String.Empty;
            // unique id for global mutex - Global prefix means it is global to the machine
            if (String.IsNullOrEmpty(sShortMutexName))
            {
                string appGuid = ((GuidAttribute)Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(GuidAttribute), false).GetValue(0)).Value;
                sMutexId = string.Format("Global\\{{{0}}}", appGuid);
            }
            else
            {
                sMutexId = string.Format("Global\\{{{0}}}", sShortMutexName);
            }
            _fullMutexName = sMutexId;
            _mutex = new Mutex(false, _fullMutexName);

            var allowEveryoneRule = new MutexAccessRule(new SecurityIdentifier(WellKnownSidType.WorldSid, null), MutexRights.FullControl, AccessControlType.Allow);
            var securitySettings = new MutexSecurity();
            securitySettings.AddAccessRule(allowEveryoneRule);
            _mutex.SetAccessControl(securitySettings);
        }

        public SingleGlobalInstance(int timeOut, out string sFullMutexName) : this(timeOut, String.Empty, out sFullMutexName)
        {        
        }

        

        public SingleGlobalInstance(int timeOut, string sShortMutexName, out string sFullMutexName)
        {
            InitMutex(sShortMutexName);
            sFullMutexName = this.MutexName;
            try
            {
                if (timeOut < 0)
                    _hasHandle = _mutex.WaitOne(Timeout.Infinite, false);
                else
                    _hasHandle = _mutex.WaitOne(timeOut, false);

                if (_hasHandle == false)
                    throw new TimeoutException("Timeout waiting for exclusive access on SingleInstance");
            }
            catch (AbandonedMutexException)
            {
                // Log the fact that the mutex was abandoned in another process,
                // it will still get acquired
                _hasHandle = true;
            }
        }

        public void Dispose()
        {
            if (_mutex != null)
            {
                if (_hasHandle)
                    _mutex.ReleaseMutex();
            }
        }

    }
}
