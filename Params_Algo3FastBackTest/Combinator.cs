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


        public static IEnumerable<Params> GetCombinations(string name, int MLSessionId, int MLIterationId, int MLCluster)
        {
            
            foreach(var timeinForceEntryMinute in new FixedValueParameter<int?>(null, 1, 5, 60, 470).Values)
            {
                foreach(var slPct in new StepValueParameterDecimal(-0.1m, -0.02m, 0.02m).Values)
                {
                    foreach (var profitPct in new StepValueParameterDecimal(0.02m, 0.1m, 0.02m).Values)
                    {
                        TimeSpan? timeInForceEntry = timeinForceEntryMinute.HasValue ? TimeSpan.FromMinutes(timeinForceEntryMinute.Value) : null;
                        ProfitLoss_DTO profitLoss = new ProfitLoss_DTO(profitPct, slPct, timeInForceEntry, null, 40000);
                        ConcreteOrderDefinitions_DTO concreteOrder = new ConcreteOrderDefinitions_DTO(profitLoss);
                        OrderDefinition_DTO orderDefinition = new OrderDefinition_DTO(name, null, MLSessionId, MLIterationId, MLCluster, concreteOrder);
                        Params result = new(name, JsonConvert.SerializeObject(orderDefinition));
                        yield return result;
                    }
                }
            }
            

        }

    }
}
