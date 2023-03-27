using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Params_Algo3FastBackTest.OHS.DTO
{
    public record OrderDefinition_DTO(string Name,
        MaxOrdersInPeriodClass? MaxOrdersInPeriod,
        int MLSessionId, int MLIterationId, int MLCluster,
        int OrderCollectionId,
        ConcreteOrderDefinitions_DTO ConcreteOrderDefinitions
        )
    {
    }

    public record MaxOrdersInPeriodClass
    {
        public int MaxOrders { get; set; }
        public TimeSpan Period { get; set; }
    }
}
