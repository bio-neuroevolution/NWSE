using System;
using System.Collections.Generic;

namespace NWSELib.genome
{
    /// <summary>
    /// 感知层基因
    /// </summary>
    public class ReceptorGene : NodeGene
    {
        public override List<int> Dimensions { get => new List<int>(); }

        /// <summary>
        /// 将动作感知基因转为动作基因
        /// </summary>
        public ReceptorGene toActionGene()
        {
            return new ReceptorGene(this.owner)
            {
                Id = this.Id,
                name = this.name.Substring(1),
                generation = this.generation,
                cataory = this.cataory
            };
        }

        public ReceptorGene(NWSEGenome genome):base(genome)
        {

        }
        /// <summary>
        /// 转字符串
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return name + ":" ;
        }
        /// <summary>
        /// 解析字符串
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static ReceptorGene parse(String s)
        {
            String[] ss = s.Split(':');
            ReceptorGene gene = new ReceptorGene(null);
            gene.name = ss[0].Trim();
            return gene;

        }

        public override T clone<T>()
        {
            return (T)(Object)new ReceptorGene(this.owner).copy<ReceptorGene>(this);
        }
    }
}
