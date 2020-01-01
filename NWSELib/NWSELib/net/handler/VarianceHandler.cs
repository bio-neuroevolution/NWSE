using System;
using System.Collections.Generic;
using System.Text;
using NWSELib.genome;
using System.Linq;
using NWSELib.common;

namespace NWSELib.net.handler
{
    public class VarianceHandler : Handler
    {
        public VarianceHandler(NodeGene gene) : base(gene)
        {
        }
        public override Object activate(Network net, int time, Object value = null)
        {
            List<Node> inputs = net.getInputNodes(this.Id);
            if (!inputs.All(n => n.IsActivate()))
                return null;

            int[] t = new int[inputs.Count];
            for (int i = 0; i < t.Length; i++)
                t[i] = time - (int)((HandlerGene)this.gene).param[i];

            Vector[] vs = new Vector[inputs.Count];
            for (int i = 0; i < inputs.Count; i++)
            {
                int index = net.idToIndex(inputs[i].Id);
                vs[i] = net.getMemoryItem(t[i], index);
            }
            Vector r = Vector.variance(vs);
            base.activate(net, time, r);
        }
    }
}
