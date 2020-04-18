using NWSELib.common;
using NWSELib.genome;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NWSELib.net.handler
{
    public class DiffHandler : Handler
    {
        public DiffHandler(NodeGene gene,Network net) : base(gene,net)
        {
        }
        
        public override Object Activate(Network net, int time, Object value = null)
        {
            List<Node> inputs = net.getInputNodes(this.Id);
            if (!inputs.All(n => n.IsActivate(time)))
                return null;
            
            Node input1 = inputs[0];
            Node input2 = inputs[1];
            
            Vector fv1 = input1.GetValue(time);
            Vector fv2 = input2.GetValue(time);
            if (fv1 == null || fv2 == null) return null;
            double r = this.GetMeasureTools().difference(fv1,fv2);
            base.Activate(net, time, r);
            return r;

        }

        public override void think(Network net, int time, Vector value = null)
        {
            List<Node> inputs = net.getInputNodes(this.Id);
            if (!inputs.All(n => n.IsThinkCompleted(time)))
                return;

            Node input1 = inputs[0];
            Node input2 = inputs[1];

            Vector fv1 = input1.getThinkValues(time);
            Vector fv2 = input2.getThinkValues(time);
            if (fv1 == null || fv2 == null) return;
            double r = this.GetMeasureTools().difference(fv1, fv2);
            base.think(net, time, r);

        }


    }
}
