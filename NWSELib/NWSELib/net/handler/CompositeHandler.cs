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
        
        public override Object Activate(Network net, int time, Object value = null)
        {
            List<Node> inputs = net.getInputNodes(this.Id);
            if (!inputs.All(n => n.IsActivate(time)))
                return null;
            int t = time;

            List<Vector> vs = inputs.ConvertAll(node => node.GetValue(time));
            Vector v = vs.flatten().Item1;

            base.Activate(net, time, v);
            return v;
        }

        public override void think(Network net, int time, Vector value)
        {
            List<Node> inputs = net.getInputNodes(this.Id);
            if (!inputs.All(n => n.IsThinkCompleted(time)))
                return;
            int t = time;

            List<Vector> vs = inputs.ConvertAll(node => node.getThinkValues(time));
            Vector v = vs.flatten().Item1;

            base.think(net, time, v);


        }
    }
}
