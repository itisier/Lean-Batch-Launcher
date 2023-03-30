using Params_Algo3FastBackTest.OHS.DTO;
using System;

namespace Params_Algo3FastBackTest
{
    public class Params
    {
        public Params(string test, string orderdefinition)
        {
            Test = test;
            Orderdefinition = orderdefinition;
        }
        public string bors2020OrderHandlerServiceBaseUrl { get; set; }
        public string Test { get; }
        public string Orderdefinition { get; }

        public int[] backtestStartDate { get; set; }
        public int[] backtestEndDate { get; set; }

        public string signalFile { get; set; } = "c:/temp/testdftrain.json";
    }
}
