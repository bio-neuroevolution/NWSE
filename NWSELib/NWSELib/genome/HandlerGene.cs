using System;
using System.Collections.Generic;
using System.Text;

namespace NWSELib.genome
{
    public class HandlerGene : NodeGene
    {
        public readonly String function;
        public readonly List<double> param = new List<double>();
    }
}
