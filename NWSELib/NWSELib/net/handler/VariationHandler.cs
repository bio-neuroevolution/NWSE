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

        public override Object Activate(Network net, int time, Object value = null)
        {
            List<Node> inputs = net.getInputNodes(this.Id);
            if (!inputs.All(n => n.IsActivate(time)))
                return null;
            
            Node input1 = inputs[0];
            
            Vector fv1 = input1.Value;
            Vector fv2 = input1.GetValue(time - 1);
            if (fv1 == null || fv2 == null) return null;

            MeasureTools tool = this.GetMeasureTools();
            double v = tool.difference(fv1[0],fv2[0]);
            base.Activate(net, time, v);
            return v;

        }

        public override void think(Network net, int time, Vector value = null)
        {
            List<Node> inputs = net.getInputNodes(this.Id);
            if (!inputs.All(n => n.IsThinkCompleted(time)))
                return;

            Node input1 = inputs[0];

            Vector fv1 = input1.getThinkValues(time);
            Vector fv2 = input1.getThinkValues(time - 1);
            if (fv1 == null || fv2 == null) return;

            MeasureTools tool = this.GetMeasureTools();
            double v = tool.difference(fv1[0], fv2[0]);
            base.think(net, time, v);

        }
    }
}
