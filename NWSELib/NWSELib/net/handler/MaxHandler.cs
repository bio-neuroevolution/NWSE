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
        public override Object Activate(Network net, int time, Object value = null)
        {
            List<Node> inputs = net.getInputNodes(this.Id);
            if (!inputs.All(n => n.IsActivate(time)))
                return null;
            List<double> lengths = inputs.ConvertAll(input => input.GetValue(time).length());
            double v = lengths.Max();

            base.Activate(net, time, (double)v);
            return this.Value;
        }

        public override void think(Network net, int time, Vector value = null)
        {
            List<Node> inputs = net.getInputNodes(this.Id);
            if (!inputs.All(n => n.IsThinkCompleted(time)))
                return;
            List<double> lengths = inputs.ConvertAll(input => input.getThinkValues(time).length());
            double v = lengths.Max();

            base.think(net, time, new Vector((double)v));
        }
    }
}
