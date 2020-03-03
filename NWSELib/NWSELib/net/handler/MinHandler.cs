using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

using NWSELib.common;
using NWSELib.genome;

namespace NWSELib.net.handler
{
    public class MinHandler : Handler
    {
        public MinHandler(NodeGene gene, Network net) : base(gene, net)
        {

        }
        public override Object activate(Network net, int time, Object value = null)
        {
            List<Node> inputs = net.getInputNodes(this.Id);
            if (!inputs.All(n => n.IsActivate(time)))
                return null;
            List<double> lengths = inputs.ConvertAll(input => input.GetValue(time).length());
            double v = lengths.Min();

            base.activate(net, time, (double)v);
            return this.Value;
        }
    }
}
