using NWSELib.genome;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

using NWSELib.common;

namespace NWSELib.net.handler
{
    public class MaxHandler : Handler
    {
        public MaxHandler(NodeGene gene, Network net) : base(gene, net)
        {
            
        }
        public override Object activate(Network net, int time, Object value = null)
        {
            List<Node> inputs = net.getInputNodes(this.Id);
            if (!inputs.All(n => n.IsActivate(time)))
                return null;
            List<double> lengths = inputs.ConvertAll(input => input.GetValue(time).length());
            double v = lengths.Max();

            base.activate(net, time, (double)v);
            return this.Value;
        }
    }
}
