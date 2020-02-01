using NWSELib.common;
using NWSELib.genome;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NWSELib.net.handler
{
    public class CompositeHandler : Handler
    {
        public CompositeHandler(NodeGene gene, Network net) : base(gene, net)
        {
        }
        public override Object activate(Network net, int time, Object value = null)
        {
            List<Node> inputs = net.getInputNodes(this.Id);
            if (!inputs.All(n => n.IsActivate(time)))
                return null;
            int t = time;

            List<Vector> vs = inputs.ConvertAll(node => node.GetValue(time));
            Vector v = vs.flatten().Item1;

            base.activate(net, time, v);
            return v;
        }
    }
}
