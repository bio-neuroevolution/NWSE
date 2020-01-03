using System;
using System.Collections.Generic;
using System.Threading;

namespace NWSELib.common
{


    public class ValueRange
    {
        public enum Cataory
        {
            Enum = 1,
            EdiffSeries = 2,
            EratioSeries = 4,

        }

        /// <summary>类别</summary>
        public readonly Cataory cataory;
        /// <summary>包括最小值</summary>
        public readonly bool includeMin;
        /// <summary>包括最大值</summary>
        public readonly bool includeMax;
        /// <summary>种子值列表，即用于生成值的列表</summary>
        public readonly List<double> values = new List<double>();
        /// <summary>距离,只能是三种类型：int，double，Range</summary>
        public readonly Object distance;
        /// <summary>距离递推函数</summary>
        public readonly Func<double[], double> recursiveFunc;
        /// <summary>距离递推函数第一个参数的元素个数</summary>
        public readonly int recursiveFuncMaxHistory;

        private ThreadLocal<Object> state_next = new ThreadLocal<Object>();

        private double _defaultRecursiveFunc()
        {
            if (recursiveFunc != null)
            {
                return recursiveFunc(((List<double>)this.state_next.Value).ToArray());
            }
            else if (cataory == Cataory.Enum)
            {
                if (!state_next.IsValueCreated)
                    state_next.Value = includeMin ? 0 : 1;

                if ((int)state_next.Value >= this.values.Count)
                    throw new ArgumentOutOfRangeException("Range");

                return this.values[(int)(state_next.Value)];
            }
            else
            {

            }
        }

        public double Distance
        {
            get => throw new NotImplementedException();
        }
        public double Min { get => this.values[0]; }
        public double Max { get => this.values[1]; }

        public ValueRange(String value)
        {
            if (value == null) value = "[0.0-1.0]";
            value = value.Trim();

            this.includeMin = value.StartsWith("(") ? false : true;
            this.includeMax = value.EndsWith(")") ? false : true;


        }
        /// <summary>
        /// 枚举形式的构造函数
        /// </summary>
        /// <param name="cataory">类别</param>
        /// <param name="includeMin">包括最小值</param>
        /// <param name="includeMax">包括最大值</param>
        /// <param name="values">值列表</param>
        public ValueRange(Cataory cataory, List<double> values, bool includeMin = true, bool includeMax = true)
        {
            this.cataory = cataory;
            this.includeMin = includeMin;
            this.includeMax = includeMax;
            this.values = values;
        }

        /// <summary>
        /// 范围形式的构造函数
        /// </summary>
        /// <param name="cataory">类别</param>
        /// <param name="includeMin">包括最小值</param>
        /// <param name="includeMax">包括最大值</param>
        /// <param name="values">值列表</param>
        public ValueRange(Cataory cataory, List<double> values, bool includeMin = true, bool includeMax = true, double distance = 1.0, Func<double[], double> recursiveFunc = null, int recursiveFuncMaxHistory = 1)
        {
            this.cataory = cataory;
            this.includeMin = includeMin;
            this.includeMax = includeMax;
            this.values = values;
            this.distance = distance;
            this.recursiveFunc = recursiveFunc;
            this.recursiveFuncMaxHistory = recursiveFuncMaxHistory;
        }
    }
}
