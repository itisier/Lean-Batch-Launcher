using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static LeanBatchLauncher.Launcher.Program;
using static CommandLineEncoder.Utils;
using System.IO;
using System.Diagnostics;
using Newtonsoft.Json;
using Parameters;

namespace LeanBatchLauncher.Launcher.Tasks
{
    internal class InstanceTaskStdInput
    {
        private record CompositeParameters(Configuration userConfiguration, object customParameters, string backTestId);

        internal string Start(Configuration userConfiguration, IOptimizationParameters parameters, Guid uniqueId)
        {
            // Use ProcessStartInfo class.
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                CreateNoWindow = false,
                UseShellExecute = false,
                //FileName = Path.Combine("c:\\Projects\\QuantConnect\\Lean\\Launcher\\bin\\Debug\\", "Instance.exe"),
                FileName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Instance.exe"),
                //WorkingDirectory = "c:\\Projects\\QuantConnect\\Lean-Frode\\QuantConnect.Algorithm.FH\\bin\\Debug\\net5.0\\",
                WorkingDirectory = AppDomain.CurrentDomain.BaseDirectory,
                RedirectStandardInput= true,
            };

            // Start the process with the info we specified
            using (Process exeProcess = Process.Start(startInfo))
            {
                //Open standard input for the new process and write to stream
                StreamWriter myStreamWriter = exeProcess.StandardInput;

                //myStreamWriter.Write("{\r\n  \"Name\": \"test\",\r\n  \"MaxOrders\": 1,\r\n  \"Period\": \"2.00:10:00\",\r\n  \"MLSessionId\": 499,\r\n  \"MLIterationId\": 3078,\r\n  \"MLCluster\": 222,\r\n  \"ConcreteOrderDefinitions\": {\r\n    \"ProfitLoss\": {\r\n      \"ProfitPct\": 0.05,\r\n      \"SLPct\": -0.05,\r\n      \"TimeInForceTimeSpanEntry\": null,\r\n      \"TimeInForceTimeSpanProfit\": null,\r\n      \"FixedAmountToBuy\": 40000\r\n    }\r\n  }\r\n}");

                //((Configuration)parameters).BacktestId = ((Configuration)parameters).AlgorithmTypeName+"-"+uniqueId.ToString();

                var backTestId = userConfiguration.AlgorithmTypeName+"-"+uniqueId.ToString();
                var compParameters = new CompositeParameters(userConfiguration, parameters, backTestId);
                

                var jsonParameters = JsonConvert.SerializeObject(compParameters);

                myStreamWriter.Write(jsonParameters);

                myStreamWriter.Close();

                // Proceeed when process is finished
                exeProcess.WaitForExit();

                return backTestId;
            }
        }
    }
}
