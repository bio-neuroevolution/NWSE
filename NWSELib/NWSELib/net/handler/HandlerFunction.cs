using System;
using System.Collections.Generic;
using System.Linq;

namespace NWSELib.net.handler
{
    /// <summary>
    /// 处理器
    /// </summary>
    public class HandlerFunction
    {
        #region 基本信息
        public readonly String Name;
        public readonly int ParamCount;
        public readonly List<(double, double)> ParamRange;
        public readonly Type handlerType;

        public HandlerFunction(String name, int paramCount, Type handlerType, params (double, double)[] range)
        {
            this.Name = name;
            this.ParamCount = paramCount;
            this.ParamRange = new List<(double, double)>(range);
            this.handlerType = handlerType;
        }
        #endregion



        #region 处理器管理
        public readonly static List<HandlerFunction> Items = new List<HandlerFunction>(
            new HandlerFunction[]
            {
                //new HandlerFunction("min",1,(1,10)),
                //new HandlerFunction("max",1,(1,10)),
                new HandlerFunction("avg",1,typeof(AverageHandler),(1,10)),
                new HandlerFunction("var",1,typeof(VarianceHandler),(1,10)),
                new HandlerFunction("diff",2,typeof(DiffHandler),(0,10),(0,10)),
                new HandlerFunction("direction",0,typeof(DirectionHandler)),
                //new HandlerFunction("argmin",1,(1,10)),
                //new HandlerFunction("argmax",1,(1,10)),
                //new HandlerFunction("prj",1,(1,10)),
               // new HandlerFunction("seq",1,(1,10))
            }
            );

        public static HandlerFunction Find(String name)
        {
            return Items.FirstOrDefault<HandlerFunction>(h => h.Name == name);
        }

        public static HandlerFunction Random(double[] distribution = null)
        {
            if (distribution == null)
            {
                distribution = new double[Items.Count];
                for (int i = 0; i < distribution.Length; i++) distribution[i] = 1.0 / Items.Count;
            }
            int index = new Random().Next(0, Items.Count);
            return Items[index];
        }
        #endregion   

    }



}
