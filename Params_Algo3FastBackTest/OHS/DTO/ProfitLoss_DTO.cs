using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Params_Algo3FastBackTest.OHS.DTO
{
    public record ProfitLoss_DTO(decimal ProfitPct, decimal SLPct, TimeSpan? TimeInForceTimeSpanEntry, TimeSpan? TimeInForceTimeSpanProfit, decimal FixedAmountToBuy)
    {
    }
}
