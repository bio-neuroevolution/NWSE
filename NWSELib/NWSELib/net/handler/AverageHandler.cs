using System;
using System.Collections.Generic;
using System.Linq;
using NWSELib.common;
using NWSELib.genome;

namespace NWSELib.net.handler
{
	public class AverageHandler : Handler
	{
		public AverageHandler(NodeGene gene) : base(gene)
		{
		}
		public override Object activate(Network net, int time, Object value = null)
		{
			List<Node> inputs = net.getInputNodes(this.Id);
			if (!inputs.All(n => n.IsActivate()))
				return null;
			int t = time;

			//取得时间参数
			int valueTime = (int)((HandlerGene)this.gene).param[0];
			if (valueTime < 0 || valueTime > Session.GetConfiguration().agent.shorttermcapacity)
				valueTime = 0;

			//从记忆库中获取各个节点对应配置的输入
			List<Vector> vs = new List<Vector>();
			if (inputs.Count == 1)
			{
				int index = net.idToIndex(inputs[0].Id);
				for (int i = 0; i < valueTime; i++)
				{
					(Vector ov, int section, Vector fv) = net.getMemoryItem(time - i, index);
					vs.Add(fv);
				}

			}
			else
			{
				for (int i = 0; i < inputs.Count; i++)
				{
					int index = net.idToIndex(inputs[0].Id);
					(Vector ov, int section, Vector fv) = net.getMemoryItem(time, index);
					vs.Add(fv);
				}
			}
			Vector result = vs.average();
			base.activate(net, time, result);
			return result;



		}
	}
}

