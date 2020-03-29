﻿
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
        private static List<TimeSpan> times         = new List<TimeSpan>();
        private static List<TimeSpan> warning_times = new List<TimeSpan>();
        private static string server_cmd_file       = string.Empty;
        private static string server_working_dir    = string.Empty;
        private static string server_config         = string.Empty;
        private static string server_exe            = string.Empty;
        private static string server_citizen        = string.Empty;

        private static int previous_start              = -1;
        private static TimeSpan previous_warning       = new TimeSpan();
        private static Process server                  = new Process();
        private static List<Process> spawned_processes = new List<Process>();

        static void Main(string[] args)
        {

            SetServerConfigs();
            SetServerTimes();
            SetProcess();
            StartServer();

            Thread restarter = new Thread(ServerRestartChecker);
            restarter.Start();

            while (true)
            {
                Console.Write("[FivemStormManager]$ ");
                WriteToServer(Console.ReadLine(), false);
            }

        }

        private static void SetServerConfigs()
        {
            server_cmd_file    = ConfigurationManager.AppSettings["ServerExe"];
            server_working_dir = ConfigurationManager.AppSettings["WorkingDirectory"];
            server_config      = ConfigurationManager.AppSettings["ServerConfig"];
            server_exe         = ConfigurationManager.AppSettings["ServerExe"];
            server_citizen     = ConfigurationManager.AppSettings["ServerCitizen"];
        }

        private static void SetServerTimes()
        {
            List<string> temp_hours    = ConfigurationManager.AppSettings["TimesToRestart"].Split(',').ToList();
            List<string> temp_warnings = ConfigurationManager.AppSettings["MinutesToWarnBefore"].Split(',').ToList();
            times = temp_hours.Select(obj => new TimeSpan(int.Parse(obj), 0, 0)).ToList();

            Func<int, int> hour_calculator = (hour) => 
            { 
                if (hour == 0)
                    return 23;

                return hour - 1;
            };

            for (int i = 0; i < times.Count;i++)
            {
                for (int j = 0;j < temp_warnings.Count;j++)
                {
                    warning_times.Add(new TimeSpan(hour_calculator(times[i].Hours), 60 - int.Parse(temp_warnings[j]), 0));
                }
            }
        }

        private static void SetProcess()
        {
            server.StartInfo.UseShellExecute       = false;
            server.StartInfo.FileName              = server_exe;
            server.StartInfo.Arguments             = string.Format(" +set citizen_dir {0} +exec {1} +set onesync_enabled 1", server_citizen, server_config);
            server.StartInfo.WorkingDirectory      = server_working_dir;
            server.StartInfo.CreateNoWindow        = false;
            server.StartInfo.RedirectStandardInput = true;
        }

        private static void ServerRestartChecker()
        {
            while (true)
            {
                Thread.Sleep(1000);
                CheckToBroadcastWarning();
                CheckToRestartServer();
            }
        }

        private static void CheckToBroadcastWarning()
        {
            TimeSpan current_time = DateTime.Now.TimeOfDay;

            if (current_time.Hours == previous_warning.Hours && current_time.Minutes == previous_warning.Minutes)
                return;

            int warning_idx = warning_times.FindIndex(obj => obj.Hours == current_time.Hours && obj.Minutes == current_time.Minutes);
            if (warning_idx != -1)
            {
                previous_warning = current_time;
                WriteToServer(string.Format("restartwarning Incoming storm in {0} minutes", 60 - current_time.Minutes));
            }
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
            WriteToServer("kickall");
            Write("Waiting 20 seconds.");

            Thread.Sleep(20000);
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

        }

        private static void Write(string message)
        {
            Console.WriteLine("FivemStormManager " + DateTime.Now.ToString() + " : " + message);
        }

        private static void WriteToServer(string message, bool with_write_message = true)
        {
            if (with_write_message)
                Write("Executing " + message);

            try
            {
                server.StandardInput.WriteLine(message);
            }
            catch (Exception e) { Write(e.Message);  }
        }
    }
}
