using System.Collections.Generic;

namespace NWSELib.genome
{
    public class EffectorGene : NodeGene
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="genome">染色体</param>
        public EffectorGene(NWSEGenome genome) : base(genome)
        {

        }

        public override List<int> Dimensions { get => new List<int>();}


        public override T clone<T>() 
        {
            return new EffectorGene(this.owner).copy<T>(this);
            
        }

        /// <summary>
        /// 取得输入基因
        /// </summary>
        /// <returns></returns>
        public override List<NodeGene> getInputGenes()
        {
            return null;
        }
        





    }
}