﻿using LeanBatchLauncher.Util;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static LeanBatchLauncher.Launcher.Configuration;
using static CommandLineEncoder.Utils;
using LeanBatchLauncher.Launcher.Tasks;
using QuantConnect.Packets;
using Panoptes.Model;
using System.Threading;
using LeanBatchLauncher.Launcher.Export;
using System.Collections.Concurrent;
using Parameters;

namespace LeanBatchLauncher.Launcher
{
    internal class Program
    {
        /// <summary>
        /// Main function.
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {

            // Start stopwatch
            var watch = Stopwatch.StartNew();

            // Read and parse the config file
            var userConfiguration = JsonConvert.DeserializeObject<Configuration>(File.ReadAllText("batch.config.json"));

            // Read and parse the data start dates for each symbol
            //var dataStartDateBySymbol = JsonConvert.DeserializeObject<Dictionary<string, DateTime>>(File.ReadAllText("data-start-date-by-symbol.json"));

            // If no alphas are provided, we must give one
            if (userConfiguration.AlphaModelNames.Length == 0)
                userConfiguration.AlphaModelNames = new string[] { "COMPOSITE" };

            // Generate ranges of optimizationParameters
            userConfiguration.PopulateParameterRanges();

            // If no optimizationParameters are provided, we must give one
            if (userConfiguration.ParameterRanges.Count == 0)
            {
                userConfiguration.Parameters.Add("NA", new Parameter());
                userConfiguration.ParameterRanges.Add("NA", EnumerableUtils.SteppedRange(0, 0));
            }

            // Generate list of dates
            //userConfiguration.GenerateDates();

            // We need to first store each instance optimizationParameters in order to then run a `Parallel.ForEach()` on all of them
            var instanceContexts = new List<IOptimizationParameters>();

            // Loop over start dates, alpha models + optimizationParameters, symbols, minute resolutions and eventually all parameter combinations
            //foreach ( var startDate in userConfiguration.Dates ) {
            /*foreach (string alphaModelName in userConfiguration.AlphaModelNames)
            {
                foreach (string symbol in userConfiguration.Symbols)
                {
                    foreach (int minuteResolution in userConfiguration.MinuteResolutions)
                    {
                        foreach (var parameterCombination in userConfiguration.GenerateParameterCombinations(userConfiguration.ParameterRanges, new Dictionary<string, double>()))
                        {

                            // Loop over each paramter provided and set it accordingly
                            foreach (var parameter in parameterCombination)
                                userConfiguration.Parameters[parameter.Key].Current = parameter.Value;

                            // Store the instance optimizationParameters to be looped over later
                            instanceContexts.Add(new InstanceContext()
                            {
                                //StartDate = startDate < dataStartDateBySymbol[symbol] ? dataStartDateBySymbol[symbol] : startDate,
                                StartDate = userConfiguration.StartDate,
                                //EndDate = startDate.AddMonths( userConfiguration.Duration ),    // We keep this at the same end date as all other symbols
                                EndDate = userConfiguration.EndDate,
                                AlphaModelName = alphaModelName,
                                Symbol = symbol,
                                MinuteResolution = minuteResolution,
                                ParametersSerialized = JsonConvert.SerializeObject(userConfiguration.Parameters)
                            });
                        }
                    }
                }
            }*/
            //foreach(var parameter in Params_Algo3FastBackTest.Combinator.GetCombinations("test", 499, 3078, 222, "c:\\Projects\\QuantConnect\\BQBT\\BQBT\\BQBT_MakeModel_Cluster_Agglomerate_Results\\499\\3000-3099\\3078\\cluster222_predictedFromTrain.json", userConfiguration))
            //foreach (var parameter in Params_Algo3FastBackTest.Combinator.GetCombinations("test", 501, 5212, 5, "c:\\Projects\\QuantConnect\\BQBT\\BQBT\\BQBT_MakeModel_Cluster_Agglomerate_Results\\501\\5200-5299\\5212\\cluster5_predictedFromTrain.json", userConfiguration))
            foreach (var parameter in Params_Algo3FastBackTest.Combinator.GetCombinations("test", 501, 1771, 85, "c:\\Projects\\QuantConnect\\BQBT\\BQBT\\BQBT_MakeModel_Cluster_Agglomerate_Results\\501\\1700-1799\\1771\\cluster85_predictedFromcustom.json", userConfiguration))
            {
                instanceContexts.Add(parameter);

            }


            // Shuffle instances
            var rng = new Random();
            instanceContexts = instanceContexts.OrderBy(r => rng.Next()).Take(int.MaxValue).ToList();

            Console.WriteLine("Launching {0} threads at a time. Total of {1} backtests.", Math.Min(userConfiguration.ParallelProcesses, instanceContexts.Count), instanceContexts.Count);

            // Run each instance in parallel
            var runInstances = new ConcurrentBag<(string batchId, IOptimizationParameters optimizationParameters)>();

            try
            {
                Parallel.ForEach(instanceContexts, new ParallelOptions { MaxDegreeOfParallelism = userConfiguration.ParallelProcesses }, (optimizationParameters) =>
                {
                    //var oshTask = new OrderHandlerServiceTask();
                    //var ohsProcess = oshTask.Start(optimizationParameters);
                    InstanceTaskStdInput instance = new InstanceTaskStdInput();
                    string backtestId = instance.Start(userConfiguration, optimizationParameters, Guid.NewGuid());
                    runInstances.Add( (backtestId, optimizationParameters));
                    //WriteBatchIdsFile(userConfiguration.BatchIdsFile, runInstances.ToList());
                    //oshTask.CtrlC(ohsProcess);
                    Console.WriteLine($"Done with {backtestId}");
                });
            }
            catch (Exception e)
            {

                Console.WriteLine($"error {e.Message}");
                Environment.Exit(-1);
            }

            
            IResultSerializer resultSerializer = new AdvancedResultSerializer(new ResultConverter(), null);
            ConcurrentBag< (QCResult qcResult, IOptimizationParameters optimizationParameters, string batchId)> resultList = new ();
            foreach(var runInstance in runInstances )
            {
                QCResult batchResult = resultSerializer.DeserializeAsync($"{Path.Join(userConfiguration.ResultsDestinationFolder, runInstance.batchId)}.json", new CancellationTokenSource().Token).Result;
                resultList.Add((batchResult, runInstance.optimizationParameters, runInstance.batchId));
            }
            ExcelExport.Export(resultList);


            // Exit and close
            watch.Stop();
            Console.WriteLine("SUCCESS");
            Console.WriteLine(string.Format("Execution time: {0:0.0} minutes (total), {1:0.0} minute(s) (per backtest)", watch.ElapsedMilliseconds / 1000.0 / 60.0, watch.ElapsedMilliseconds / 1000.0 / 60.0 / instanceContexts.Count));
            
            Console.WriteLine("Waiting for key press...");
            Console.Read();
            Environment.Exit(0);
        }


        private static object batchidsFileLock = new object();
        private static void WriteBatchIdsFile(string filePath, IEnumerable<string> batchIds)
        {
            lock(batchidsFileLock)
            {
                File.WriteAllText(filePath, JsonConvert.SerializeObject(batchIds));
            }
        }


        /// <summary>
        /// Contains one instance optimizationParameters to be passed into a backtest.
        /// </summary>
        public class InstanceContext
        {
            public DateTime StartDate;
            public DateTime EndDate;
            public string AlphaModelName;
            public string Symbol;
            public int MinuteResolution;
            public string ParametersSerialized;
        }
    }
}
