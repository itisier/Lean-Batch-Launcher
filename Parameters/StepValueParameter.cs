using System;
using System.Collections;
using System.Collections.Generic;

namespace LeanBatchLauncher.Parameters
{
    public class StepValueParameterDecimal 
    {
        private readonly decimal start;
        private readonly decimal end;
        private readonly decimal step;

        public StepValueParameterDecimal(decimal start, decimal end, decimal step)
        {
            this.start = start;
            this.end = end;
            this.step = step;
        }

        public IEnumerable<decimal> Values
        {
            get
            {
                var value = start;
                while(value <= end)
                {
                    yield return value;
                    value += step;
                }
            }
        }

    }
}
