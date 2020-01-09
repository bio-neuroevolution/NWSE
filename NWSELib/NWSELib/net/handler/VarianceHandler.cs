using NWSELib.common;
using NWSELib.genome;
using System;
using System.Collections.Generic;
using System.Linq;

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
            if (!inputs.All(n => n.IsActivate(time)))
                return null;

            int[] t = new int[inputs.Count];
            for (int i = 0; i < t.Length; i++)
                t[i] = time - (int)((HandlerGene)this.gene).param[i];

            Vector[] vs = new Vector[inputs.Count];
            for (int i = 0; i < inputs.Count; i++)
            {
                int index = net.idToIndex(inputs[i].Id);
                vs[i] = net.getMemoryItem(index, t[i]);
            }
            double[,] r = Vector.covariance(vs);
            base.activate(net, time, r);
            return r;
        }
        
    }
}
