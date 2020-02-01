using NWSELib.common;
using NWSELib.genome;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NWSELib.net.handler
{
    public class ProjectionHandler : Handler
    {
        public ProjectionHandler(NodeGene gene, Network net) : base(gene, net)
        {
        }
        public override Object activate(Network net, int time, Object value = null)
        {
            List<Node> inputs = net.getInputNodes(this.Id);
            if (!inputs.All(n => n.IsActivate(time)))
                return null;

            List<int> dimensions = ((HandlerGene)this.gene).param.ConvertAll(p => (int)p);
            Vector v = inputs[0].GetValue(time);
            Vector nv = new Vector(true, dimensions.Count);
            for (int i = 0; i < dimensions.Count; i++)
                nv[i] = v[dimensions[i]];
            base.activate(net, time, nv);
            return nv;
        }
    }
}
