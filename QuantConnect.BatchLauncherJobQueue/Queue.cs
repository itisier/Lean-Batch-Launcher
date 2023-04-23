using System;
using System.Collections.Generic;
using System.IO;

using Newtonsoft.Json;

using QuantConnect;
using QuantConnect.Configuration;
using QuantConnect.Interfaces;
using QuantConnect.Logging;
using QuantConnect.Packets;
using QuantConnect.Python;
using QuantConnect.Queues;

namespace LeanBatchLauncher.Launcher
{
    public class Queue : IJobQueueHandler
    {
        /// <summary>
        /// The shadow queue object - we don't want to reimplement everything so we use it instead.
        /// </summary>
        private readonly JobQueue _shadow;

        private readonly bool _liveMode = Config.GetBool("live-mode");
        private static readonly string AccessToken = Config.Get("api-access-token");
        private static readonly int UserId = Config.GetInt("job-user-id", 0);
        private static readonly int ProjectId = Config.GetInt("job-project-id", 0);
        private readonly string AlgorithmTypeName = Config.Get("algorithm-type-name");
        private readonly Language Language = (Language)Enum.Parse(typeof(Language), Config.Get("algorithm-language"), ignoreCase: true);
        private readonly string BacktestId = Config.Get("BacktestId");

        /// <summary>
        /// Creates the queue and a parent queue.
        /// </summary>
        public Queue()
        {
            _shadow = new JobQueue();
        }

        /// <summary>
        /// Physical location of Algorithm DLL.
        /// </summary>
        private string AlgorithmLocation
        {
            get
            {
                // we expect this dll to be copied into the output directory
                return Config.Get("algorithm-location", "Algorithm.dll");
            }
        }

        /// <summary>
        /// Desktop/Local acknowledge the task processed. Nothing to do.
        /// </summary>
        /// <param name="job"></param>
        public void AcknowledgeJob(AlgorithmNodePacket job)
        {

            // We don't want to wait for a key press
            // Console.Read();
            Console.WriteLine("Engine.Main(): Analysis Complete.");
        }

        /// <summary>
        /// Facade for the parent's NextJob() function.
        /// We're exposing this in order to change the BacktestId.
        /// </summary>
        /// <param name="location"></param>
        /// <returns></returns>
        public AlgorithmNodePacket NextJob(out string location)
        {

            location = GetAlgorithmLocation();

            Log.Trace("JobQueue.NextJob(): Selected " + location);

            // check for parameters in the config
            var parameters = new Dictionary<string, string>();

            var parametersConfigString = Config.Get("parameters");
            if (parametersConfigString != string.Empty)
            {
                parameters = JsonConvert.DeserializeObject<Dictionary<string, string>>(parametersConfigString);
            }

            var controls = new Controls()
            {
                MinuteLimit = Config.GetInt("symbol-minute-limit", 10000),
                SecondLimit = Config.GetInt("symbol-second-limit", 10000),
                TickLimit = Config.GetInt("symbol-tick-limit", 10000),
                RamAllocation = int.MaxValue,
                MaximumDataPointsPerChartSeries = Config.GetInt("maximum-data-points-per-chart-series", 4000)
            };

            if ((Language)Enum.Parse(typeof(Language), Config.Get("algorithm-language")) == Language.Python)
            {
                // Set the python path for loading python algorithms ("algorithm-location" config parameter)
                var pythonFile = new FileInfo(location);

                // PythonInitializer automatically adds the current working directory for us
                //PythonInitializer.SetPythonPathEnvironmentVariable(new string[] { pythonFile.Directory.FullName });
                PythonInitializer.Initialize();
            }

            // We currently offer no live job functionality

            //Default run a backtesting job.
            var backtestJob = new BacktestNodePacket(0, 0, "", new byte[] { }, "local")
            {
                Type = PacketType.BacktestNode,
                Algorithm = File.ReadAllBytes(location),
                HistoryProvider = Config.Get("history-provider", "SubscriptionDataReaderHistoryProvider"),
                Channel = AccessToken,
                UserId = UserId,
                ProjectId = ProjectId,
                Version = Globals.Version,
                BacktestId = BacktestId, //AlgorithmTypeName + "-" + Guid.NewGuid(),
                Language = (Language)Enum.Parse(typeof(Language), Config.Get("algorithm-language")),
                Parameters = parameters,
                Controls = controls,
            };

            return backtestJob;
        }

        /// <summary>
        /// Get the algorithm location for client side backtests.
        /// </summary>
        /// <returns></returns>
        private string GetAlgorithmLocation()
        {
            if (Language == Language.Python)
            {
                if (!File.Exists(AlgorithmLocation))
                {
                    throw new FileNotFoundException($"JobQueue.TryCreatePythonAlgorithm(): Unable to find py file: {AlgorithmLocation}");
                }

                // Add this directory to our Python Path so it may be imported properly
                var pythonFile = new FileInfo(AlgorithmLocation);
                PythonInitializer.AddPythonPaths(new string[] { pythonFile.Directory.FullName });
            }

            return AlgorithmLocation;
        }


        #region Shadow methods
        public void Initialize(IApi api)
        {

        }
        #endregion
    }
}

