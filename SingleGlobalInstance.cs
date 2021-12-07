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

        private void InitMutex(string sMutexName)
        {
            string mutexId = String.Empty;
            // unique id for global mutex - Global prefix means it is global to the machine
            if (String.IsNullOrEmpty(sMutexName))
            {
                string appGuid = ((GuidAttribute)Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(GuidAttribute), false).GetValue(0)).Value;
                mutexId = string.Format("Global\\{{{0}}}", appGuid);
            }
            else
            {
                mutexId = string.Format("Global\\{{{0}}}", sMutexName);
            }
            _mutex = new Mutex(false, mutexId);

            var allowEveryoneRule = new MutexAccessRule(new SecurityIdentifier(WellKnownSidType.WorldSid, null), MutexRights.FullControl, AccessControlType.Allow);
            var securitySettings = new MutexSecurity();
            securitySettings.AddAccessRule(allowEveryoneRule);
            _mutex.SetAccessControl(securitySettings);
        }

        public SingleGlobalInstance(int timeOut) : this(timeOut, String.Empty)
        {
        }

        public SingleGlobalInstance(int timeOut, string sMutexName)
        {
            InitMutex(sMutexName);
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
                _mutex.Close();
            }
        }

    }
}