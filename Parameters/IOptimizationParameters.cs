using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parameters
{
    public interface IOptimizationParameters
    {
        IEnumerable<(string paramName, Type type, object value, string strValue)> Parameters { get; }
    }
}
