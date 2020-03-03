using System.Collections.Generic;

namespace NWSELib.genome
{
    /// <summary>
    /// ЧӦ������
    /// </summary>
    public class EffectorGene : NodeGene
    {
        /// <summary>
        /// ���캯��
        /// </summary>
        /// <param name="genome">Ⱦɫ��</param>
        public EffectorGene(NWSEGenome genome) : base(genome)
        {

        }
        /// <summary>
        /// ά��
        /// </summary>
        public override List<int> Dimensions { get => new List<int>();}

        /// <summary>
        /// ��¡
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public override T clone<T>() 
        {
            return new EffectorGene(this.owner).copy<T>(this);
            
        }

        /// <summary>
        /// ȡ���������
        /// </summary>
        /// <returns></returns>
        public override List<NodeGene> getInputGenes()
        {
            return null;
        }
        





    }
}