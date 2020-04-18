using NWSELib.common;
using NWSELib.genome;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NWSELib.net.handler
{
    public class PropertyHandler : Handler
    {
        public PropertyHandler(NodeGene gene, Network net) : base(gene, net)
        {
        }
        public override Object Activate(Network net, int time, Object value = null)
        {
            List<Node> inputs = net.getInputNodes(this.Id);
            if (!inputs.All(n => n.IsActivate(time)))
                return null;
            Vector v = Session.GetConfiguration().agent.receptors.GetSensor(inputs[0].Gene.Name).properties[0].Value;
            base.Activate(net, time, v);
            return v;
        }

        public override void think(Network net, int time, Vector value = null)
        {
            List<Node> inputs = net.getInputNodes(this.Id);
            if (!inputs.All(n => n.IsThinkCompleted(time)))
                return;
            Vector v = Session.GetConfiguration().agent.receptors.GetSensor(inputs[0].Gene.Name).properties[0].Value;
            base.think(net, time, v);
         
        }
    }
}