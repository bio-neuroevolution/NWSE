
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

using NWSELib.genome;
using NWSELib.common;

namespace NWSELib.net.handler
{
    public class ArgminHandler : Handler
    {
        public ArgminHandler(NodeGene gene, Network net) : base(gene, net)
        {
           
        }

        
        public override Object Activate(Network net, int time, Object value = null)
        {
            List<Node> inputs = net.getInputNodes(this.Id);
            if (!inputs.All(n => n.IsActivate(time)))
                return null;
            List<double> lengths = inputs.ConvertAll(input => input.GetValue(time).length());
            int index = lengths.argmin();
            int nodeid = inputs[index].Id;

            base.Activate(net, time, (double)index);
            return this.Value;
        }

        public override void think(Network net, int time, Vector value)
        {
            List<Node> inputs = net.getInputNodes(this.Id);
            if (!inputs.All(n => n.IsThinkCompleted(time)))
                return;

            List<double> lengths = inputs.ConvertAll(input => input.getThinkValues(time).length());
            int index = lengths.argmin();
            base.think(net, time, (double)inputs[index].Id);

        }

    }
}
