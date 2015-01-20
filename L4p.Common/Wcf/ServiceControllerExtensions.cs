using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.ServiceProcess;
using L4p.Common.Extensions;

namespace L4p.Common.Wcf
{
    static class ServiceControllerExtensions
    {
        [StructLayout(LayoutKind.Sequential)]
        class QUERY_SERVICE_CONFIG
        {
            [MarshalAs(System.Runtime.InteropServices.UnmanagedType.U4)]
            public UInt32 dwServiceType;
            [MarshalAs(System.Runtime.InteropServices.UnmanagedType.U4)]
            public UInt32 dwStartType;
            [MarshalAs(System.Runtime.InteropServices.UnmanagedType.U4)]
            public UInt32 dwErrorControl;
            [MarshalAs(System.Runtime.InteropServices.UnmanagedType.LPWStr)]
            public String lpBinaryPathName;
            [MarshalAs(System.Runtime.InteropServices.UnmanagedType.LPWStr)]
            public String lpLoadOrderGroup;
            [MarshalAs(System.Runtime.InteropServices.UnmanagedType.U4)]
            public UInt32 dwTagID;
            [MarshalAs(System.Runtime.InteropServices.UnmanagedType.LPWStr)]
            public String lpDependencies;
            [MarshalAs(System.Runtime.InteropServices.UnmanagedType.LPWStr)]
            public String lpServiceStartName;
            [MarshalAs(System.Runtime.InteropServices.UnmanagedType.LPWStr)]
            public String lpDisplayName;
        };

        [DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern Boolean ChangeServiceConfig(
            IntPtr hService,
            UInt32 nServiceType,
            UInt32 nStartType,
            UInt32 nErrorControl,
            String lpBinaryPathName,
            String lpLoadOrderGroup,
            IntPtr lpdwTagId,
            [In] char[] lpDependencies,
            String lpServiceStartName,
            String lpPassword,
            String lpDisplayName);

        [DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern Boolean QueryServiceConfig(IntPtr hService, IntPtr intPtrQueryConfig, UInt32 cbBufSize, out UInt32 pcbBytesNeeded);

        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern IntPtr OpenService(IntPtr hSCManager, string lpServiceName, uint dwDesiredAccess);

        [DllImport("advapi32.dll", EntryPoint = "OpenSCManagerW", ExactSpelling = true, CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern IntPtr OpenSCManager(string machineName, string databaseName, uint dwAccess);

        [DllImport("advapi32.dll", EntryPoint = "CloseServiceHandle")]
        public static extern int CloseServiceHandle(IntPtr hSCObject);

        private const uint SERVICE_NO_CHANGE = 0xFFFFFFFF;
        private const uint SERVICE_QUERY_CONFIG = 0x00000001;
        private const uint SERVICE_CHANGE_CONFIG = 0x00000002;
        private const uint SC_MANAGER_ALL_ACCESS = 0x000F003F;

        public static void ChangeStartMode(this ServiceController svc, ServiceStartMode mode)
        {
            var scManagerHandle = OpenSCManager(null, null, SC_MANAGER_ALL_ACCESS);
            fail_if(scManagerHandle == IntPtr.Zero, "Open Service Manager Error");

            var serviceHandle = OpenService(
                scManagerHandle,
                svc.ServiceName,
                SERVICE_QUERY_CONFIG | SERVICE_CHANGE_CONFIG);

            fail_if(serviceHandle == IntPtr.Zero, "Open Service Error");

            var success = ChangeServiceConfig(
                serviceHandle,
                SERVICE_NO_CHANGE,
                (uint) mode,
                SERVICE_NO_CHANGE,
                null,
                null,
                IntPtr.Zero,
                null,
                null,
                null,
                null);

            fail_if(!success, "failed to change start-up mode to '{0}' of '{1}'", mode, svc.ServiceName);

            CloseServiceHandle(serviceHandle);
            CloseServiceHandle(scManagerHandle);
        }

        private static void fail_if(bool expr, string fmt, params object[] args)
        {
            if (expr == false)
                return;

            var errMsg = fmt.Fmt(args);

            int nError = Marshal.GetLastWin32Error();
            var win32Exception = new Win32Exception(nError);

            throw
                new ExternalException(errMsg, win32Exception);
        }

        public static ServiceStartMode QueryStartMode(this ServiceController svc)
        {
            var scManagerHandle = OpenSCManager(null, null, SC_MANAGER_ALL_ACCESS);
            fail_if(scManagerHandle == IntPtr.Zero, "Open Service Manager Error");

            var serviceHandle = OpenService(
                scManagerHandle,
                svc.ServiceName,
                SERVICE_QUERY_CONFIG | SERVICE_CHANGE_CONFIG);

            fail_if(serviceHandle == IntPtr.Zero, "Open Service Error");

            UInt32 dwBytesNeeded = 0;

            // Allocate memory for struct.
            IntPtr ptr = Marshal.AllocHGlobal(4096);
            fail_if(ptr == IntPtr.Zero, "can't allocate struct");

            bool success = QueryServiceConfig(serviceHandle, ptr, 4096, out dwBytesNeeded);
            fail_if(!success, "failed to query service config for '{0}'", svc.ServiceName);

            QUERY_SERVICE_CONFIG qUERY_SERVICE_CONFIG = new QUERY_SERVICE_CONFIG();
            Marshal.PtrToStructure(ptr, qUERY_SERVICE_CONFIG);
            Marshal.FreeHGlobal(ptr);

            CloseServiceHandle(serviceHandle);
            CloseServiceHandle(scManagerHandle);

            return
                (ServiceStartMode) qUERY_SERVICE_CONFIG.dwStartType;
        }
    }
}