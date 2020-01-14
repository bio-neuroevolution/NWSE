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
        public static string toString(List<double> values,String sep =",",int precise=3)
        {
            if (values == null || values.Count <= 0) return "";
            else if (values.Count == 1) return String.Format("{0:f"+ precise.ToString()+"}", values[0]);
            else return values.ConvertAll(v => String.Format("{0:f"+ precise.ToString()+"}", v)).Aggregate((a, b) => a + sep + b);
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

        public static List<int> argsort(this List<double> values)
        {
            List<double> temp = new List<double>(values);
            List<int> index = new List<int>();
            for (int i = 0; i < temp.Count; i++)
                index.Add(i);
            for(int i=0;i< temp.Count;i++)
            {
                for(int j=i+1;j< temp.Count;j++)
                {
                    if(temp[i] > temp[j])
                    {
                        double t = temp[i];
                        temp[i] = temp[j];
                        temp[j] = t;

                        int k = index[i];
                        index[i] = index[j];
                        index[j] = k;
                    }
                }
            }
            return index;
        }

        /// <summary>
        /// 给定概率，计算对应的x值
        /// </summary>
        /// <param name="mean"></param>
        /// <param name="variance"></param>
        /// <param name="prob"></param>
        /// <returns></returns>
        public static double[] getGaussianByProb(double mean,double variance,double prob)
        {
            double t1 = System.Math.Log(System.Math.Sqrt(2 * System.Math.PI * variance)* prob)
                    * -2 * variance;
            double t2 = Math.Sqrt(t1);
            double t3 = -t2;

            t2 += mean;
            t3 += mean;
            return t2 < t3 ? new double[] { t2,t3}: new double[] { t3,t2};
   
        }

        internal static bool intersection<T>(List<T> l1, List<T> l2)
        {
            for(int i=0;i<l1.Count;i++)
            {
                for(int j = 0;j<l2.Count;j++)
                {
                    if (l1[i].Equals(l2[j])) return true;
                }
            }
            return false;
        }
        /// <summary>
        /// v1是否全部包含v2
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <returns></returns>
        public static bool ContainsAll(List<int> v1, List<int> v2)
        {
            if (v1 == null || v1.Count <= 0) return false;
            if (v2 == null || v2.Count <= 0) return false;

            for(int i=0;i<v2.Count;i++)
            {
                if (!v1.Contains(v2[i])) return false;
            }
            return true;
        }
    }
}
