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
        public DirectionHandler(NodeGene gene, Network net) : base(gene,net)
        {
        }
        
        public override Object Activate(Network net, int time, Object value = null)
        {
            List<Node> inputs = net.getInputNodes(this.Id);
            if (!inputs.All(n => n.IsActivate(time)))
                return null;
            int t = time;

            Node input1 = inputs[0];
            Node input2 = inputs.Count <= 1 ? inputs[0] : inputs[1];
            Vector v1 = input1.Value;
            Vector v2 = input2.GetValue(t - 1);
            if (v2 == null || v1 == null) return null;
            double v = this.GetMeasureTools().getChangeDirection(v1[0], v2[0]);
            base.Activate(net, time, v);
            return v;
        }

        public override void think(Network net, int time, Vector value = null)
        {
            List<Node> inputs = net.getInputNodes(this.Id);
            if (!inputs.All(n => n.IsThinkCompleted(time)))
                return;
            int t = time;

            Node input1 = inputs[0];
            Node input2 = inputs.Count <= 1 ? inputs[0] : inputs[1];
            Vector v1 = input1.getThinkValues(t);
            Vector v2 = input2.getThinkValues(t - 1);
            if (v2 == null || v1 == null) return;
            double v = this.GetMeasureTools().getChangeDirection(v1[0], v2[0]);
            base.think(net, time, v);
        }

    }
}
