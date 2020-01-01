using System;
using System.Collections.Generic;
using NWSELib.genome;
using NWSELib.net.handler;

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
            HandlerFunction hf = HandlerFunction.Find(funcName);
            return (Handler)hf.handlerType.GetConstructor(new Type[] { typeof(NodeGene) }).Invoke(new object[] { gene });
        }
    }
}
