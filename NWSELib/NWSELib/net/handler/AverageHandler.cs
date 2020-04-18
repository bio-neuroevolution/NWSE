using NWSELib.common;
using NWSELib.genome;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NWSELib.net.handler
{
    public class AverageHandler : Handler
    {
        public AverageHandler(NodeGene gene, Network net) : base(gene,net)
        {
        }
        

        public override Object Activate(Network net, int time, Object value = null)
        {
            List<Node> inputs = net.getInputNodes(this.Id);
            if (!inputs.All(n => n.IsActivate(time)))
                return null;
            int t = time;


            List<Vector> vs = inputs.ConvertAll(node => node.GetValue(time));
            Vector result = vs.average();
            base.Activate(net, time, result);
            return result;
        }

        public override void think(Network net, int time, Vector value)
        {
            List<Node> inputs = net.getInputNodes(this.Id);
            if (!inputs.All(n => n.IsThinkCompleted(time)))
                return;

            List<Vector> vs = inputs.ConvertAll(node => node.getThinkValues(time));
            Vector result = vs.average();
            base.think(net, time, result);

        }

    }
}

