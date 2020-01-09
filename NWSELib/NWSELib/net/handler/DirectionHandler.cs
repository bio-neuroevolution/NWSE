using NWSELib.common;
using NWSELib.genome;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NWSELib.net.handler
{
    /// <summary>
    /// 方向处理器
    /// </summary>
    public class DirectionHandler : Handler
    {
        public DirectionHandler(NodeGene gene) : base(gene)
        {
        }
        public override Object activate(Network net, int time, Object value = null)
        {
            List<Node> inputs = net.getInputNodes(this.Id);
            if (!inputs.All(n => n.IsActivate(time)))
                return null;
            int t = time;

            Node input1 = inputs[0];
            Node input2 = inputs.Count <= 1 ? inputs[0] : inputs[1];
            Vector v1 = input1.Value;
            Vector v2 = input2.GetValue(t - 1);
            Vector v = (v1-v2).normalize();
            base.activate(net, time, v);
            return v;
        }
        
    }
}
