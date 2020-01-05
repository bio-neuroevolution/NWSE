using NWSELib.genome;
using NWSELib.net.handler;
using System;

namespace NWSELib.net
{
    public abstract class Handler : Node
    {
        public Handler(NodeGene gene) : base(gene)
        {

        }
        

        public static Handler create(HandlerGene gene)
        {
            string funcName = gene.function;
            Configuration.Handler hf = Session.GetConfiguration().Find(gene.function);
            return (Handler)hf.HandlerType.GetConstructor(new Type[] { typeof(NodeGene) }).Invoke(new object[] { gene });
        }
    }
}
