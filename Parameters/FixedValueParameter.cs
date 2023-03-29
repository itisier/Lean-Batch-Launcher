using System;
using System.Collections;
using System.Collections.Generic;

namespace LeanBatchLauncher.Parameters
{
    public class FixedValueParameter<T>
    {
        private readonly T[] values;

        public FixedValueParameter(params T[] values)
        {
            this.values = values;
        }

        public IEnumerable<T> Values => values;

    }
}
