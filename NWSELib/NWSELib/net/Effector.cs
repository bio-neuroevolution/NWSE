using System;
using System.Collections.Generic;
using NWSELib.common;
using NWSELib.genome;

namespace NWSELib.net
{
    public class Effector : Node
    {
        public Effector(NodeGene gene, Network net) : base(gene, net)
        {

        }
        

        public override String GetValueText(Vector value = null)
        {
            if (value == null) value = Value;
            if (value == null) return "";
            Receptor receptor = (Receptor)net["_" + this.Gene.Name];
            return receptor.GetValueText(value);
        }

    }
}
