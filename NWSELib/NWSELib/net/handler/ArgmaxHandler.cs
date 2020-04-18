
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

using NWSELib.genome;
using NWSELib.common;

namespace NWSELib.net.handler
{
    public class ArgmaxHandler : Handler
    {
        public ArgmaxHandler(NodeGene gene,Network net) : base(gene,net)
        {
            this.Cataory = "index";
        }
        
        public override Object Activate(Network net, int time, Object value = null)
        {
            List<Node> inputs = net.getInputNodes(this.Id);
            if (!inputs.All(n => n.IsActivate(time)))
                return null;

            List<double> lengths = inputs.ConvertAll(input => input.GetValue(time).length());
            int index = lengths.argmax();
            
            base.Activate(net, time, (double)inputs[index].Id);
            return this.Value;
        }

        public override void think(Network net, int time, Vector value)
        {
            List<Node> inputs = net.getInputNodes(this.Id);
            if (!inputs.All(n => n.IsThinkCompleted(time)))
                return;

            List<double> lengths = inputs.ConvertAll(input => input.getThinkValues(time).length());
            int index = lengths.argmax();
            base.think(net, time, (double)inputs[index].Id);
           
        }


    }
}
