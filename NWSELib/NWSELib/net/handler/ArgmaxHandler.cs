
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

using NWSELib.genome;

namespace NWSELib.net.handler
{
    public class ArgmaxHandler : Handler
    {
        public ArgmaxHandler(NodeGene gene) : base(gene)
        {
        }
        public override Object activate(Network net, int time, Object value = null)
        {
            List<Node> inputs = net.getInputNodes(this.Id);
            if (!inputs.All(n => n.IsActivate(time)))
                return null;
            double max = double.MinValue;
            int nodeid = -1;
            for(int i=0;i<inputs.Count;i++)
            {
                double v = inputs[i].GetValue(time).length();
                if(v > max)
                {
                    max = v;
                    nodeid = inputs[i].Id;
                }
            }

            base.activate(net, time, (double)nodeid);
            return this.Value;
        }
    }
}
