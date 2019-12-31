using System;
using System.Linq;

namespace NWSELib.net.handler
{
	public class MinHandler : Handler
	{
		public MinHandler()
		{
		}
		public override Object activate(Network net, int time, Object value = null)
        {
			List<Node> inputs = net.getInputNodes(this.Id);
			if (!inputs.All(n => n.isActivated()))
				return null;
			int t = time;

			//从记忆库中获取各个节点对应配置的输入
			


        }
	}
}

