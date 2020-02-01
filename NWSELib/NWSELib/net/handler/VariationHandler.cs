using NWSELib.common;
using NWSELib.genome;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NWSELib.net.handler
{
    /// <summary>
    /// 变动量处理器
    /// </summary>
    public class VariationHandler : Handler
    {
        public VariationHandler(NodeGene gene, Network net) : base(gene, net)
        {
        }

        public override Object activate(Network net, int time, Object value = null)
        {
            List<Node> inputs = net.getInputNodes(this.Id);
            if (!inputs.All(n => n.IsActivate(time)))
                return null;
            
            Node input1 = inputs[0];
            
            Vector fv1 = input1.Value;
            Vector fv2 = input1.GetValue(time - 1);
            if (fv1 == null || fv2 == null) return null; 
            Vector r = fv1 - fv2;
            base.activate(net, time, r);
            return r;

        }
    }
}
