using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace NWSELib.common
{


    public class Vector
    {
        private readonly List<double> values = new List<double>();
        private int maxCapacity;
        private bool fixedSize;
        public Vector(bool fixedSize = false,int maxCapacity=0)
        {
            this.maxCapacity = maxCapacity;
            this.fixedSize = fixedSize;
            this.initSize();
            
        }
        public Vector(bool fixedSize = false, int maxCapacity = 0,params double[] values)
        {
            this.values.AddRange(values);
            this.maxCapacity = maxCapacity;
            this.fixedSize = fixedSize;
            this.initSize();
        }
        public Vector(bool fixedSize = false, int maxCapacity = 0,List<double> values)
        {
            this.values.AddRange(values);
            this.maxCapacity = maxCapacity;
            this.fixedSize = fixedSize;
            this.initSize();
        }
        public Vector(bool fixedSize = false, int maxCapacity = 0,Tuple tuple)
        {
            this.values.AddRange(tuple);
            this.maxCapacity = maxCapacity;
            this.fixedSize = fixedSize;
            this.initSize();
        }

        private void initSize()
        {
            if (fixedSize && this.maxCapacity <= 0) this.maxCapacity = 1;
            if (this.fixedSize)
            {
                for (int i = this.values.Count; i < maxCapacity; i++)
                    this.values.Add(0.0);
            }
        }
        public List<double> ToList()
        {
            return new List<double>(this.values);
        }
        public double[] ToArray()
        {
            return this.values.ToArray();
        }
        public double this[int index]{
            get=>this.values[index];
            set => this.values[index] = value;
        }


    }
}