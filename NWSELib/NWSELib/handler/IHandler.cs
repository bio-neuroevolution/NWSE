using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace NWSELib.handler
{
    /// <summary>
    /// 处理器
    /// </summary>
    public abstract class IHandler
    {
        String Name { get; }
        int ParamCount { get; }
        List<ValueTuple<double, double>> ParamRange { get; }
        public abstract ValueTuple<double, double> GetParamRange(int xh);

        public readonly static List<IHandler> Items = new List<IHandler>(
            new IHandler[]
            {
                new MinHandler(),new MaxHandler(),new AvgHandler(),new VarianceHandler(),
                new DiffHandler(),new DirectionHandler(),new ArgMinHandler(),new ArgMaxHandler(),
                new PrjHandler(),new SeqHandler()

            }
            );

        public static IHandler Find(String name)
        {
            return Items.FirstOrDefault<IHandler>(h => h.Name == name);
        }

        public static IHandler Random(double[] distribution=null)
        {
            if (distribution == null)
            {
                distribution = new double[Items.Count];
                for (int i = 0; i < distribution.Length; i++) distribution[i] = 1.0 / Items.Count;
            }
            int index  = new Random().Next(0, Items.Count);
            return Items[index];
        }
            

    }

    
    public class MinHandler : IHandler
    {
        public String Name { get { return "min"; } }
    }
    public class MaxHandler : IHandler
    {
        public String Name { get { return "max"; } }
    }

    public class AvgHandler : IHandler
    {
        public String Name { get { return "avg"; } }
    }

    public class VarianceHandler : IHandler
    {
        public String Name { get { return "variane"; } }
    }
    public class DiffHandler : IHandler
    {
        public String Name { get { return "diff"; } }
    }
    public class DirectionHandler : IHandler
    {
        public String Name { get { return "direction"; } }
    }

    public class ArgMinHandler : IHandler
    {
        public String Name { get { return "argmin"; } }
    }
    public class ArgMaxHandler : IHandler
    {
        public String Name { get { return "argmax"; } }
    }

    public class PrjHandler : IHandler
    {
        public String Name { get { return "prj"; } }
    }

    public class SeqHandler : IHandler
    {
        public String Name { get { return "seq"; } }
    }
}
