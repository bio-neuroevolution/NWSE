using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

using NWSELib.common;
using NWSELib.genome;

namespace NWSELib.net.handler
{
	public class DiffHandler : Handler
	{
		public DiffHandler(NodeGene gene) : base(gene)
		{
		}
		public override Object activate(Network net, int time, Object value = null)
		{
			List<Node> inputs = net.getInputNodes(this.Id);
			if (!inputs.All(n => n.IsActivate(time)))
				return null;
			int t = (int)((HandlerGene)this.gene).param[0];

			Vector fv1 = inputs[0].Value;
			Vector fv2 = inputs[1].GetValue(time-t);
			Vector r = fv1 - fv2;
			base.activate(net, time, r);
			return r;

		}
	}
}
