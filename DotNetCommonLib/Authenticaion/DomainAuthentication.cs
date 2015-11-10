using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace DotNetCommonLib
{
    /// <summary>
    /// 
    /// </summary>
    public class DomainAuthentication
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="domain"></param>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public static bool CheckAD(string domain,string username,string password)
        {
            const int LOGON32_LOGON_INTERACTIVE = 2;
            const int LOGON32_PROVIDER_DEFAULT = 0;

            IntPtr tokenHandle = IntPtr.Zero;

            return LogonUser(username, domain, password, LOGON32_LOGON_INTERACTIVE, LOGON32_PROVIDER_DEFAULT, ref tokenHandle);
            
        }

        [DllImport("advapi32.dll")]//調用Win32API
        private static extern bool LogonUser(string username, string domain, string password, int logonType, int logonProvider, ref IntPtr token);
    }
}
