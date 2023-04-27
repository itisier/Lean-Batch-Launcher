using Newtonsoft.Json;
using Parameters;
using Params_Algo3FastBackTest.OHS.DTO;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace Params_Algo3FastBackTest
{
    public class OptimizationParams : IOptimizationParameters
    {
        public OptimizationParams(string test, OrderDefinition_DTO orderDefinition_DTO)
        {
            Test = test;
            orderdefinition = JsonConvert.SerializeObject(orderDefinition_DTO);
            OrderDefinition_DTO = orderDefinition_DTO;
        }
        public string bors2020OrderHandlerServiceBaseUrl { get; set; }
        public string Test { get; }
        public string orderdefinition { get; }

        public int[] backtestStartDate { get; set; }
        public int[] backtestEndDate { get; set; }

        public bool debugOutputSignals => false;

        public string signalFile { get; set; } = "c:/temp/testdftrain.json";

        [JsonIgnore] 
        public OrderDefinition_DTO OrderDefinition_DTO { get; set; }

        [JsonIgnore] 
        public IEnumerable<(string paramName,Type type, object value, string strValue)> Parameters { get {

                yield return ("ProfitLoss.TimeInForceTimeSpanEntry", typeof(TimeSpan?), OrderDefinition_DTO.ConcreteOrderDefinitions.ProfitLoss?.TimeInForceTimeSpanEntry, OrderDefinition_DTO.ConcreteOrderDefinitions.ProfitLoss?.TimeInForceTimeSpanEntry.ToString() ?? "");
                yield return ("ProfitLoss.TimeInForceTimeSpanProfit", typeof(TimeSpan?), OrderDefinition_DTO.ConcreteOrderDefinitions.ProfitLoss?.TimeInForceTimeSpanProfit, OrderDefinition_DTO.ConcreteOrderDefinitions.ProfitLoss?.TimeInForceTimeSpanProfit.ToString() ?? "");
                yield return ("ProfitLoss.FixedAmountToBuy", typeof(decimal?), OrderDefinition_DTO.ConcreteOrderDefinitions.ProfitLoss?.FixedAmountToBuy, OrderDefinition_DTO.ConcreteOrderDefinitions.ProfitLoss?.FixedAmountToBuy.ToString());
                yield return ("ProfitLoss.ProfitPct", typeof(decimal?), OrderDefinition_DTO.ConcreteOrderDefinitions.ProfitLoss?.ProfitPct, OrderDefinition_DTO.ConcreteOrderDefinitions.ProfitLoss?.ProfitPct.ToString(CultureInfo.InvariantCulture));
                yield return ("ProfitLoss.SLPct", typeof(decimal?), OrderDefinition_DTO.ConcreteOrderDefinitions.ProfitLoss?.SLPct, OrderDefinition_DTO.ConcreteOrderDefinitions.ProfitLoss?.SLPct.ToString(CultureInfo.InvariantCulture));
                yield return ("BacktestStartDate", typeof(DateTime), new DateTime(backtestStartDate[0], backtestStartDate[1], backtestStartDate[2]), backtestStartDate.ToString());
                yield return ("BacktestEndDate", typeof(DateTime), new DateTime(backtestEndDate[0], backtestEndDate[1], backtestEndDate[2]), backtestEndDate.ToString());
                yield return ("MaxOrdersInPeriod?.MaxOrders", typeof(int?), OrderDefinition_DTO.MaxOrdersInPeriod?.MaxOrders, OrderDefinition_DTO.MaxOrdersInPeriod?.MaxOrders.ToString());
                yield return ("MaxOrdersInPeriod?.Period", typeof(TimeSpan?), OrderDefinition_DTO.MaxOrdersInPeriod?.Period, OrderDefinition_DTO.MaxOrdersInPeriod?.Period.ToString());


                yield return ("MLCluster", typeof(int), OrderDefinition_DTO.MLCluster, OrderDefinition_DTO.MLCluster.ToString());
                yield return ("MLIterationId", typeof(int), OrderDefinition_DTO.MLIterationId, OrderDefinition_DTO.MLIterationId.ToString());
                yield return ("MLSessionId", typeof(int), OrderDefinition_DTO.MLSessionId, OrderDefinition_DTO.MLSessionId.ToString());
                yield return ("SignalFile", typeof(string), signalFile, signalFile);
                yield return ("OrderdefinitionJSON", typeof(string), orderdefinition, orderdefinition);
                yield break;
                /*
                */
            }
        }


        /*       [JsonIgnore] 
                yield return ("SignalFile", signalFile);
                yield return ("OrderdefinitionJSON", orderdefinition);
            } }*/
    }
}
