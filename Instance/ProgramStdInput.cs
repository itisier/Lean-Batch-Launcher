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
using Fasterflect;
using Newtonsoft.Json.Linq;
using QuantConnect.Api;
using Accord.Diagnostics;
using System.Diagnostics;

namespace Instance
{
    internal class Program
    {

        public static List<string> GetPropertyKeysForDynamic(dynamic dynamicToGetPropertiesFor)
        {
            JObject attributesAsJObject = dynamicToGetPropertiesFor;
            Dictionary<string, object> values = attributesAsJObject.ToObject<Dictionary<string, object>>();
            List<string> toReturn = new List<string>();
            foreach (string key in values.Keys)
            {
                toReturn.Add(key);
            }
            return toReturn;
        }

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
            var jsonParameters = reader.ReadToEnd();
            Log.Trace($"Parameters = {jsonParameters}");

            var parameters = JsonConvert.DeserializeObject<dynamic>(jsonParameters);

            var customparameters = parameters["customParameters"];
            Log.Trace($"customparameters = {customparameters}");
            var userConfiguration = parameters["userConfiguration"];
            Log.Trace($"userConfiguration = {userConfiguration}");
            var backTestId = (string)parameters["backTestId"];
            Log.Trace($"backTestId = {backTestId}");

            
            
            //For at dette skal fungere må launcher startes fra kommandolinje
            //Debugger.Launch();


            // Initiate a thread safe operation, as it seems we need to do all of the below in a thread safe manner
            ThreadSafe.Execute("config", () =>
            {
                // Copy the config file thread safely
                //File.Copy(Path.Combine("c:\\Projects\\QuantConnect\\Lean-Frode\\QuantConnect.Brokerages.FH\\", "configb2020backtesting_FASTBACKTEST.json"), Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.json"), true);
                //Log.Trace($"ConfigFile = {userConfiguration["ConfigFile"]}");
                //Log.Trace($"DestFile = {Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.json")}");
                File.Copy((string)userConfiguration["ConfigFile"].ToString(), Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.json"), true);

                Config.SetConfigurationFile(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.json"));
                //Config.Reset();

                /*Config.Set("algorithm-location", "c:\\Projects\\QuantConnect\\BQBT\\BasicQuantBookTemplate2_algorithm3_FASTBACKTEST.py");
                var algorithmTypeName = "BasicQuantBookTemplate2_algorithm3_FASTBACKTEST";
                var backTestGuid = Guid.NewGuid();
                Config.Set("algorithm-type-name", algorithmTypeName);
                var backTestId = $"{algorithmTypeName}-{backTestGuid}";
                Config.Set("BacktestId", backTestId);*/


                // Configure path to and name of algorithm
                Config.Set("algorithm-location", userConfiguration["Algorithmlocation"].ToString());
                Config.Set("algorithm-type-name", userConfiguration["AlgorithmTypeName"].ToString());
                Config.Set("BacktestId", backTestId);
                Config.Set("object-store-root", $"./storage/{backTestId}");
                Config.Set("data-directory", userConfiguration["DataFolder"].ToString());
                Config.Set("results-destination-folder", userConfiguration["ResultsDestinationFolder"]);

                //Config.Set("plugin-directory", "c:\\Projects\\QuantConnect\\Lean-Batch-Launcher\\Launcher\\bin\\Debug");
                // Set some values local to this Launcher
                Config.Set("algorithm-language", userConfiguration["AlgorithmLanguage"].ToString());
                Config.Set("composer-dll-directory", ".");
                Config.Set("environment", "backtesting");

                Config.Set("job-queue-handler", "LeanBatchLauncher.Launcher.Queue");
                //Config.Set("data-provider", "QuantConnect.Lean.Engine.DataFeeds.ApiDataProvider");
                Config.Set("data-provider", "QuantConnect.Lean.Engine.DataFeeds.DefaultDataProvider");
                //Config.Set("job-user-id", apiJobUserId);
                //Config.Set("api-access-token", apiAccessToken);


                foreach (string propertyName in GetPropertyKeysForDynamic(customparameters))
                {
                    /*object obj = customparameters[propertyName];
                    Console.WriteLine($"{obj.GetType().FullName}");
                    if (obj.GetType() == typeof(JValue)) {
                        string propertyValue = customparameters[propertyName];
                        // And
                        Config.Set(propertyName, propertyValue);
                    }*/

                    Config.Set(propertyName, customparameters[propertyName]);
                }





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
