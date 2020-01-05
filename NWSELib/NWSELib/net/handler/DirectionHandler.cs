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

            Vector v = (inputs[0].Value - inputs[1].Value).normalize();
            base.activate(net, time, v);
            return v;
        }
        
    }
}
