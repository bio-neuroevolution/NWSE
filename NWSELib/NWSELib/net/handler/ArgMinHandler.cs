
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

using NWSELib.genome;
using NWSELib.common;

namespace NWSELib.net.handler
{
    public class ArgMinHandler : Handler
    {
        public ArgMinHandler(NodeGene gene, Network net) : base(gene, net)
        {
           
        }
        public override Object activate(Network net, int time, Object value = null)
        {
            List<Node> inputs = net.getInputNodes(this.Id);
            if (!inputs.All(n => n.IsActivate(time)))
                return null;
            List<double> lengths = inputs.ConvertAll(input => input.GetValue(time).length());
            int index = lengths.argmin();
            int nodeid = inputs[index].Id;

            base.activate(net, time, (double)index);
            return this.Value;
        }
    }
}
