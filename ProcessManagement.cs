using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Management;

namespace FivemStormManager
{
    static public class ProcessManagement
    {
        public static void KillProcessAndChildrens(int pid)
        {

            ManagementObjectCollection processCollection = GetManagementCollection(pid);
            try
            {
                Process proc = Process.GetProcessById(pid);
                if (!proc.HasExited) proc.Kill();
            }
            catch (ArgumentException) { }

            if (processCollection != null)
            {
                foreach (ManagementObject mo in processCollection)
                {
                    KillProcessAndChildrens(Convert.ToInt32(mo["ProcessID"]));
                }
            }

        }

        public static List<Process> GetListOfAllServerProcesses(int pid)
        {
            List<Process> processes = new List<Process>();
            try
            {
                processes.Add(Process.GetProcessById(pid));
            }
            catch (ArgumentException) { }


            ManagementObjectCollection processCollection = GetManagementCollection(pid);

            foreach (ManagementObject mo in processCollection)
            {
                processes.AddRange(GetListOfAllServerProcesses(Convert.ToInt32(mo["ProcessId"])));
            }

            return processes;
        }

        public static ManagementObjectCollection GetManagementCollection(int pid)
        {
            ManagementObjectSearcher processSearcher = new ManagementObjectSearcher
            ("Select * From Win32_Process Where ParentProcessID=" + pid);

            return processSearcher.Get();
        }
    }
}
