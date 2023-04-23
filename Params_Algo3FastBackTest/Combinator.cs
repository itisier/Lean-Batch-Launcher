using LeanBatchLauncher.Launcher;
using LeanBatchLauncher.Parameters;
using Newtonsoft.Json;
using Params_Algo3FastBackTest.OHS.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Params_Algo3FastBackTest
{
    public static class Combinator
    {


        public static IEnumerable<OptimizationParams> GetCombinations(string name, int MLSessionId, int MLIterationId, int MLCluster, Configuration userConfiguration)
        {
            
            foreach(var timeinForceEntryMinute in new FixedValueParameter<int?>(null, 1, 5, 60, 470).Values)
            {
                foreach(var slPct in new StepValueParameterDecimal(-0.17m, -0.02m, 0.03m).Values)
                {
                    foreach (var profitPct in new StepValueParameterDecimal(0.02m, 0.17m, 0.03m).Values)
                    {
                        TimeSpan? timeInForceEntry = timeinForceEntryMinute.HasValue ? TimeSpan.FromMinutes(timeinForceEntryMinute.Value) : null;
                        ProfitLoss_DTO profitLoss = new ProfitLoss_DTO(profitPct, slPct, timeInForceEntry, null, 40000);
                        ConcreteOrderDefinitions_DTO concreteOrder = new ConcreteOrderDefinitions_DTO(profitLoss);
                        OrderDefinition_DTO orderDefinition = new OrderDefinition_DTO(name, null, MLSessionId, MLIterationId, MLCluster, concreteOrder);
                        OptimizationParams result = new(name, orderDefinition);

                        result.backtestStartDate = new int[]{userConfiguration.StartDate.Year, userConfiguration.StartDate.Month, userConfiguration.StartDate.Day};
                        result.backtestEndDate = new int[] {userConfiguration.EndDate.Year,userConfiguration.EndDate.Month, userConfiguration.EndDate.Day};


                        yield return result;
                    }
                }
            }
            

        }

    }
}
