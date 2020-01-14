using NWSELib.common;
using NWSELib.genome;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NWSELib.net
{
    /// <summary>
    /// 网络节点
    /// </summary>
    public abstract class Node
    {
        #region 基本信息

        /// <summary>节点基因</summary>
        protected NodeGene gene;

        /// <summary>节点Id</summary>
        public int Id { get => this.gene.Id;}
        public String Name
        {
            get => gene.Name;
        }
        public string Cataory
        {
            get => gene.Cataory;
        }
        public String Group
        {
            get => gene.Group;
        }
        public NodeGene Gene
        {
            get => this.gene;
        }
        #endregion

        #region 状态信息

        protected readonly List<Vector> values = new List<Vector>();
        protected readonly List<int> times = new List<int>();
        public int BeginTime
        {
            get => (times == null || times.Count<=0)?0: times[0];
        }
        public int CurTime
        {
            get { return times.Count<=0?-1:times[times.Count - 1]; }
        }
        public int TimeCapacity
        {
            get => Session.GetConfiguration().agent.shorttermcapacity;
        }
        

        public Vector Value
        {
            get { return values.Count<=0?null:values.ToArray()[values.Count - 1]; }
        }

        public int Dimension
        {
            get => values.Count<=0?0:values[0].Size;
        }

        public List<Vector> ValueList
        {
            get => new List<Vector>(this.values);
        }

        public List<Vector> GetValues(int new_time, int count)
        {
            List<int> ts = this.times.ToList();
            int tindex = times.IndexOf(new_time);
            if (tindex < 0) return null;

            
            List<Vector> r = new List<Vector>();
            for (int i=0;i<count;i++)
            {
                if (tindex >= values.Count) return r;
                r.Add(values[tindex++]);
            }
            return r;
        }

        public Vector GetValue(int time, int backIndex)
        {
            int tindex = times.IndexOf(time);
            if (tindex < 0) return null;
            if (tindex - backIndex < 0) return null;
            return this.ValueList[tindex - backIndex];
        }
        public Vector GetValue(int time)
        {
            int tindex = times.IndexOf(time);
            if (tindex < 0) return null;
            return this.values[time];
        }




        public Node(NodeGene gene)
        {
            this.gene = gene;

        }
        #endregion

        #region 评价信息
        /** 可靠度 */
        protected double reability;
        /// <summary>
        /// 可靠度
        /// </summary>
        public double Reability
        {
            get => this.reability;
            set => this.reability = value;
        }
        
        #endregion

        #region 激活与重置
        /// <summary>
        /// 设置当前值
        /// </summary>
        /// <param name="value"></param>
        /// <param name="time"></param>
        /// <returns></returns>
        public virtual Object activate(Network net, int time, Object value = null)
        {
            if (value != null && value is double) value = new Vector(new double[] { (double)value });
            
            if(times.Count<=0)
            {
                times.Add(time);
                values.Add((Vector)value);
                return null;
            }

            Object prev = null;
            int index = times.IndexOf(time);
            if(index >= 0)
            {
                times[index] = time;
                prev = values[index];
                values[index] = (Vector)value;
                return prev;

            }
            int lastime = times.Last();
            if(time > lastime)
            {
                times.Add(time);
                if (value is List<Vector>) value = ((List<Vector>)value).flatten();
                values.Add((Vector)value);
                return null;
            }
            else
            {
                index = 0;
                while (times[index++] < time) ;
                times.Insert(index - 1, time);
                values.Insert(index-1, (Vector)value);
            }

            while(values.Count>Session.GetConfiguration().agent.shorttermcapacity)
            {
                this.values.RemoveAt(0);
                this.times.RemoveAt(0);
            }

            return prev;
        }
        /// <summary>
        /// 重置计算
        /// </summary>
        public void Reset()
        {
            //this.times.Clear();
            //this.values.Clear();
        }
        /// <summary>
        /// 是否已经完成过计算
        /// </summary>
        /// <returns></returns>
        public bool IsActivate(int time)
        {
            return this.times.Contains(time);
        }

        internal void randomValue(Network net,int time)
        {
            double value = Session.GetConfiguration().agent.receptors.GetSensor("_"+this.Name).Range.random();
            this.activate(net, time, value);
        }
        #endregion
    }
}
