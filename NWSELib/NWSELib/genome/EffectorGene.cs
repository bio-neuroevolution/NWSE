namespace NWSELib.genome
{
    public class EffectorGene : NodeGene
    {
        public override T clone<T>() 
        {
            return new EffectorGene().copy<T>(this);
            
        }

        
    }
}