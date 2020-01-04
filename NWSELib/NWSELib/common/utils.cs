using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace NWSELib.common
{
    /// <summary>
    /// 工具类
    /// </summary>
    public static class Utility
    {
        /// <summary>
        /// 字符串转double集合
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static List<double> parse(String s)
        {
            return s.Split(',').ToList().ConvertAll(x => Double.Parse(x)).ToList<double>();
        }
        /// <summary>
        /// 判断两个集合是否完全想等
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
    }
}
