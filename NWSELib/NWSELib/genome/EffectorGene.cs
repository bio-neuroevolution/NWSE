namespace NWSELib.genome
{
    public class EffectorGene : NodeGene
    {
        public override T clone<T>() 
        {
            return new EffectorGene(this.owner).copy<T>(this);
            
        }
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="genome">染色体</param>
        public EffectorGene(NWSEGenome genome):base(genome)
        {
           
        }


    }
}