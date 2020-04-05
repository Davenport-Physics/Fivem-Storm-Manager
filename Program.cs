
using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace FivemStormManager
{
    class Program
    {
        static void Main(string[] args)
        {
            Git.InitGit();
            RestartManager.InitRestartManager();

            while (true)
            {
                Console.Write("[FivemStormManager]$ ");
                RestartManager.WriteToServer(Console.ReadLine(), false);
            }

            /*
            ExternalCommands.InitSocket();
            while (true)
            {
                WriteToServer(ExternalCommands.BeginReading());
            }*/
            
        }

        private static void ParseExternalCommand(string blob)
        {
            JObject obj      = JObject.Parse(blob);
            string blob_type = string.Empty;
            try
            {
                blob_type = obj["CommandType"].ToString();
            } catch { return; }
            
            switch (blob_type)
            {
                case "EmergencyRestart":
                    EmergencyRestart(obj);
                    break;
                case "SkipNextRestart":
                    RestartManager.SkipNextRestart();
                    break;
            }
        }
        
        private static void EmergencyRestart(JObject obj)
        {

        }
    }
}
