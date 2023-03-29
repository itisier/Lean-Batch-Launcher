using Params_Algo3FastBackTest.OHS.DTO;
using System;

namespace Params_Algo3FastBackTest
{
    public record Params(string Test, string orderdefinition)
    {

        public string bors2020OrderHandlerServiceBaseUrl { get; set; }
    }
}
