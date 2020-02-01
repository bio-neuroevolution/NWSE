using NWSELib.common;
using NWSELib.genome;
using NWSELib.net.handler;
using System;

namespace NWSELib.net
{
    public abstract class Handler : Node
    {
        /// <summary>
        /// 时间跨度
        /// </summary>
        public virtual double TimeSpan { get => 1; }

        public Handler(NodeGene gene, Network net) : base(gene,net)
        {

        }

        public static Handler create(HandlerGene gene,Network net)
        {
            string funcName = gene.function;
            Configuration.Handler hf = Session.GetConfiguration().Find(gene.function);
            return (Handler)hf.HandlerType.GetConstructor(new Type[] { typeof(NodeGene),typeof(Network)}).Invoke(new object[] { gene,net });
        }

        /// <summary>
        /// 取得给定值的文本显示信息
        /// </summary>
        /// <param name="value">为空，则取当前最新值</param>
        /// <returns></returns>
        public override String getValueText(Vector value = null)
        {
            if (value == null) value = Value;
            if (value == null) return "";
            return value.ToString();
        }
    }
}
