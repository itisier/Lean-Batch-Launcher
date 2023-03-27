﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static LeanBatchLauncher.Launcher.Program;
using static  CommandLineEncoder.Utils;
using System.IO;
using System.Net.Sockets;
using System.Net;
using System.Net.NetworkInformation;



namespace LeanBatchLauncher.Launcher.Tasks
{


    internal class OrderHandlerServiceTask
    {
        private static object lockObj = new object();
        private static HashSet<int> usedPorts = new HashSet<int>();

        private static int GetAnyAvailablePort(IPAddress ip)
        {
            TcpListener l = new TcpListener(ip, 0);
            l.Start();
            int port = ((IPEndPoint)l.LocalEndpoint).Port;
            l.Stop();
            return port;
        }


        private static bool CheckAvailableServerPort(int port)
        {
            bool isAvailable = true;

            // Evaluate current system tcp connections. This is the same information provided
            // by the netstat command line application, just in .Net strongly-typed object
            // form.  We will look through the list, and if our port we would like to use
            // in our TcpClient is occupied, we will set isAvailable to false.
            IPGlobalProperties ipGlobalProperties = IPGlobalProperties.GetIPGlobalProperties();
            IPEndPoint[] tcpConnInfoArray = ipGlobalProperties.GetActiveTcpListeners();

            foreach (IPEndPoint endpoint in tcpConnInfoArray)
            {
                if (endpoint.Port == port)
                {
                    isAvailable = false;
                    break;
                }
            }

            return isAvailable;
        }


        internal static int NextPort()
        {
            lock(lockObj)
            {
                //start from port 5136
                const int startport = 5136;
                //Prøver max 1000 innenfor range
                for (int i = 0; i < 1000; i++)
                {
                    int port = startport + i;
                    if (usedPorts.Contains(port)) { continue; }
                    if (!CheckAvailableServerPort(port)) { continue; };
                    usedPorts.Add(port);
                    return port;
                }
                //Hvis ikke funnet noen, så returner en random ledig port
                return GetAnyAvailablePort(IPAddress.Loopback);
            }
        }

        internal static void ReleasePort(int port)
        {
            lock (lockObj)
            {
                if (usedPorts.Contains(port))
                    usedPorts.Remove(port);
            }
        }



        internal static void CtrlC(Process p)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                CreateNoWindow = false,
                UseShellExecute = false,
                //FileName = Path.Combine("c:\\Projects\\QuantConnect\\Lean\\Launcher\\bin\\Debug\\", "Instance.exe"),
                FileName = "ProcCtrlC.exe",
                WorkingDirectory = System.AppDomain.CurrentDomain.BaseDirectory,
            };

            startInfo.Arguments = p.Id.ToString();

            using (Process exeProcess = Process.Start(startInfo))
            {

                // Proceeed when process is finished
                exeProcess.WaitForExit();
            }
            p.WaitForExit();

        }


        internal static Process Start(Configuration userConfiguration, InstanceContext context, int port)
        {
            // Use ProcessStartInfo class.
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                CreateNoWindow = false,
                UseShellExecute = false,
                //FileName = Path.Combine("c:\\Projects\\QuantConnect\\Lean\\Launcher\\bin\\Debug\\", "Instance.exe"),
                FileName = "dotnet.exe",
                //WorkingDirectory = "c:\\Projects\\QuantConnect\\Lean\\Launcher\\bin\\Debug\\",
                WorkingDirectory = "\\Projects\\Bors2020\\QCService\\bin\\Backtesting_Parallel\\net6.0\\publish",

            };

            // Create arguments
            var builder = new StringBuilder();
            builder.Append(String.Format("\"{0}\" ", EncodeArgText("orderhandlerservice.dll")));
            builder.Append(String.Format("{0} ", $"--urls=http://localhost:{port}"));
            builder.Append(String.Format("{0} ", EncodeArgText("--environment \"backtest\"")));

            // Append to StartInfo
            startInfo.Arguments = builder.ToString();

            // Start the process with the info we specified
            Process exeProcess = Process.Start(startInfo);

                // Proceeed when process is finished
                //exeProcess.WaitForExit();
            return exeProcess;
        }
    }
}