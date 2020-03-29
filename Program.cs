
using System;
using System.Linq;
using System.Configuration;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace ServerRestartConsole
{
    class Program
    {
        private static List<TimeSpan> times      = new List<TimeSpan>();
        private static string server_cmd_file    = string.Empty;
        private static string server_working_dir = string.Empty;

        private static int previous_start              = -1;
        private static Process server                  = new Process();
        private static List<Process> spawned_processes = new List<Process>();

        static void Main(string[] args)
        {
            server_cmd_file         = ConfigurationManager.AppSettings["ServerCmd"];
            server_working_dir      = ConfigurationManager.AppSettings["WorkingDirectory"];
            List<string> temp_hours = ConfigurationManager.AppSettings["TimesToRestart"].Split(',').ToList<string>();
            times = temp_hours.Select(obj => new TimeSpan(int.Parse(obj), 0, 0)).ToList<TimeSpan>();


            SetProcess();
            StartServer();

            while (true)
            {
                Thread.Sleep(1000);
                CheckToRestartServer();
            }

        }

        private static void SetProcess()
        {
            server.StartInfo.UseShellExecute       = true;
            server.StartInfo.FileName              = "cmd.exe";
            server.StartInfo.Arguments             = " /C " + server_working_dir + server_cmd_file;
            server.StartInfo.WorkingDirectory      = server_working_dir;
            server.StartInfo.CreateNoWindow        = false;
            //server.StartInfo.RedirectStandardInput = true;
        }

        private static void CheckToRestartServer()
        {
            int times_idx = times.FindIndex(obj => obj.Hours == DateTime.Now.TimeOfDay.Hours);
            if (times_idx == -1 || previous_start == times[times_idx].Hours)
                return;

            RestartServer();
        }

        private static void RestartServer()
        {
            Write("Restarting Server.");
            KillServer();
            StartServer();
        }

        private static void KillServer()
        {
            try
            {
                Write("Attempting to kill server.");
                ProcessManagement.KillProcessAndChildrens(server.Id);
                Write("Killed server.");
            }
            catch (Exception e) { Write(e.Message); }
        }

        private static void StartServer()
        {

            previous_start = DateTime.Now.TimeOfDay.Hours;
            try
            {
                Write("Attempting to start server.");
                server.Start();
                Thread.Sleep(2000);
                Write("Server started.");
            }
            catch (Exception e) { Write(e.Message); return; }

            spawned_processes = ProcessManagement.GetListOfAllServerProcesses(server.Id);
            Write(string.Format("Spawned {0} processes", spawned_processes.Count));
            HookIntoFXServer();

        }

        private static void HookIntoFXServer()
        {
            Process process = spawned_processes.Find(obj => obj.ProcessName == "FXServer");
            try
            {
                process.StandardInput.WriteLine("kickall\n");
            }
            catch (Exception e) { Write(e.Message); }

        }

        private static void Write(string message)
        {
            Console.WriteLine(DateTime.Now.ToString() + " : " + message);
        }

        private static void WriteToServer(string message)
        {
            Write("Attempting to write to server " + message);
            try
            {
                server.StandardInput.WriteLine(message);
            }
            catch (Exception e) { Write(e.Message);  }
        }
    }
}
