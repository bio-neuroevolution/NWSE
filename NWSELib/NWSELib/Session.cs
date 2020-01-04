using System;

using NWSELib.evolution;
using NWSELib.genome;

namespace NWSELib
{
    public class Session
    {
        public readonly Configuration config;

        public EvolutionTreeNode root;
        public IdGenerator idGenerator = new IdGenerator();

        public static Configuration GetConfiguration()
        {
            throw new NotImplementedException();
        }

        public static EvolutionTreeNode getEvolutionRootNode()
        {
            throw new NotImplementedException();
        }

        public static IdGenerator GetIdGenerator()
        {
            throw new NotImplementedException();
        }

    }
}
