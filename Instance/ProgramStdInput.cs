using LeanBatchLauncher.Util;
using Newtonsoft.Json;
using QuantConnect;
using QuantConnect.Configuration;
using QuantConnect.Lean.Engine;
using QuantConnect.Logging;
using QuantConnect.Util;
using System;
using System.Collections.Generic;
using System.IO;
using static LeanBatchLauncher.Launcher.Configuration;
using static CommandLineEncoder.Utils;
using System.ComponentModel.Composition;

namespace Instance
{
    internal class Program
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="args"></param>
        public static void Main(string[] args)
        {
            //System.Diagnostics.Debugger.Launch();
            byte[] inputBuffer = new byte[4096];
            Stream inputStream = Console.OpenStandardInput(inputBuffer.Length);
            //Console.SetIn(new StreamReader(inputStream, Console.InputEncoding, false, inputBuffer.Length));
            var reader = new StreamReader(inputStream, Console.InputEncoding, false, inputBuffer.Length);
            Log.Trace($"HALLO FRA PorgramStdInput {reader.ReadToEnd()}");


            // Initiate a thread safe operation, as it seems we need to do all of the below in a thread safe manner
            ThreadSafe.Execute("config", () =>
            {
                // Copy the config file thread safely
                File.Copy(Path.Combine("c:\\Projects\\QuantConnect\\Lean-Frode\\QuantConnect.Brokerages.FH\\bin\\Debug\\net5.0\\", "configb2020backtesting_FASTBACKTEST.json"), Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.json"), true);

                Config.SetConfigurationFile(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.json"));
                //Config.Reset();

                // Configure path to and name of algorithm
                Config.Set("algorithm-type-name", "BasicQuantBookTemplate2_algorithm3_FASTBACKTEST2");
                Config.Set("algorithm-location", "c:\\Projects\\QuantConnect\\BQBT\\BasicQuantBookTemplate2_algorithm3_FASTBACKTEST2.py");
                //Config.Set("plugin-directory", "c:\\Projects\\QuantConnect\\Lean-Batch-Launcher\\Launcher\\bin\\Debug");
                // Set some values local to this Launcher
                Config.Set("algorithm-language", "Python");
                Config.Set("composer-dll-directory", ".");
                Config.Set("environment", "backtesting");
                Config.Set("data-folder", "c:\\Projects\\LeanData\\");
                Config.Set("job-queue-handler", "LeanBatchLauncher.Launcher.Queue");
                //Config.Set("data-provider", "QuantConnect.Lean.Engine.DataFeeds.ApiDataProvider");
                Config.Set("data-provider", "QuantConnect.Lean.Engine.DataFeeds.DefaultDataProvider");
                //Config.Set("job-user-id", apiJobUserId);
                //Config.Set("api-access-token", apiAccessToken);


                // Deserialize parameters
                /*var parameters = JsonConvert.DeserializeObject<Dictionary<string, Parameter>>(parametersSerialized);

                // Save parameters
                foreach (KeyValuePair<string, Parameter> entry in parameters)
                {
                    Config.Set(entry.Key, entry.Value.Current.ToString());
                }*/

            }
            );

            Log.DebuggingEnabled = false;

            // We only need console output - no logging
            using (Log.LogHandler = new ConsoleLogHandler())
            {

                //AppDomain.CurrentDomain.SetData("APPBASE", "c:\\Projects\\QuantConnect\\Lean\\Launcher\\bin\\Debug\\");

                Log.Trace("Engine.Main(): LEAN ALGORITHMIC TRADING ENGINE v" + Globals.Version + " Mode: *CUSTOM BATCH* (" + (Environment.Is64BitProcess ? "64" : "32") + "bit)");
                Log.Trace("Engine.Main(): Started " + DateTime.Now.ToShortTimeString());

                // Import external libraries specific to physical server location (cloud/local)
                LeanEngineSystemHandlers leanEngineSystemHandlers;
                try
                {
                    leanEngineSystemHandlers = LeanEngineSystemHandlers.FromConfiguration(Composer.Instance);
                }
                catch (CompositionException compositionException)
                {
                    Log.Error("Engine.Main(): Failed to load library: " + compositionException);
                    throw;
                }

                // Setup packeting, queue and controls system: These don't do much locally.
                leanEngineSystemHandlers.Initialize();

                //-> Pull job from QuantConnect job queue, or, pull local build:
                var job = leanEngineSystemHandlers.JobQueue.NextJob(out string assemblyPath);

                if (job == null)
                {
                    throw new Exception("Engine.Main(): Job was null.");
                }

                LeanEngineAlgorithmHandlers leanEngineAlgorithmHandlers;
                try
                {
                    leanEngineAlgorithmHandlers = LeanEngineAlgorithmHandlers.FromConfiguration(Composer.Instance);
                }
                catch (System.ComponentModel.Composition.CompositionException compositionException)
                {
                    Log.Error("Engine.Main(): Failed to load library: " + compositionException);
                    throw;
                }

                try
                {
                    var algorithmManager = new AlgorithmManager(false, job);

                    leanEngineSystemHandlers.LeanManager.Initialize(leanEngineSystemHandlers, leanEngineAlgorithmHandlers, job, algorithmManager);

                    var engine = new Engine(leanEngineSystemHandlers, leanEngineAlgorithmHandlers, false);
                    engine.Run(job, algorithmManager, assemblyPath, WorkerThread.Instance);

                }
                finally
                {
                    // Delete the message from the job queue:
                    leanEngineSystemHandlers.JobQueue.AcknowledgeJob(job);
                    Log.Trace("Engine.Main(): Packet removed from queue: " + job.AlgorithmId);

                    // Clean up resources
                    leanEngineSystemHandlers.Dispose();
                    leanEngineAlgorithmHandlers.Dispose();
                    Log.LogHandler.Dispose();

                    Log.Trace("Program.Main(): Exiting Lean...");
                }
            }
        }

    }
}
