﻿using System;

namespace NWSELib.genome
{
    /// <summary>
    /// 感知层基因
    /// </summary>
    public class ReceptorGene : NodeGene
    {


        /// <summary>
        /// 将动作感知基因转为动作基因
        /// </summary>
        public ReceptorGene toActionGene()
        {
            return new ReceptorGene()
            {
                Id = this.Id,
                name = this.name.Substring(1),
                generation = this.generation,
                cataory = this.cataory,
                sectionCount = this.sectionCount
            };
        }
        /// <summary>
        /// 转字符串
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return name + ":" + this.sectionCount;
        }
        /// <summary>
        /// 解析字符串
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static ReceptorGene parse(String s)
        {
            String[] ss = s.Split(':');
            ReceptorGene gene = new ReceptorGene();
            gene.name = ss[0].Trim();
            gene.sectionCount = int.Parse(ss[1].Trim());
            return gene;

        }

    }
}