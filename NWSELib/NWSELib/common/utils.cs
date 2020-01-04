using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace NWSELib.common
{
    /// <summary>
    /// 工具类
    /// Utility
    /// </summary>
    public static class Utility
    {
        /// <summary>
        /// 字符串转double集合
        /// str to List<double>
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static List<double> parse(String s)
        {
            return s.Split(',').ToList().ConvertAll(x => Double.Parse(x)).ToList<double>();
        }
        /// <summary>
        /// 判断两个集合是否完全想等
        /// Wether two sets are equal.
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <returns></returns>
        public static bool equals<T>(List<T> v1,List<T> v2)
        {
            for(int i=0;i<v1.Count;i++)
            {
                if (!v2.Exists(v => v.Equals(v1[i]))) return false;
            }
            for(int i=0;i<v2.Count;i++)
            {
                if (!v1.Exists(v => v.Equals(v2[i]))) return false;
            }
            return true;
        }

        /// <summary>
        /// argmax
        /// </summary>
        /// <param name="values"></param>
        /// <returns></returns>
        public static int argmax(this List<double> values)
        {
            double max = values.Max();
            for(int i=0;i<values.Count;i++)
            {
                if (max == values[i]) return i;
            }
            return 0;
        }
        /// <summary>
        /// argmin
        /// </summary>
        /// <param name="values"></param>
        /// <returns></returns>
        public static int argmin(this List<double> values)
        {
            double max = values.Min();
            for (int i = 0; i < values.Count; i++)
            {
                if (max == values[i]) return i;
            }
            return 0;
        }

    }
}
