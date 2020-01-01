using System;
using System.Collections.Generic;
using System.Linq;
using NWSELib.common;
using NWSELib.genome;

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
        public int Id { get => this.gene.Id; }
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
        
        protected readonly Queue<Vector> values;
        protected readonly Queue<int> times;
        public int BeginTime
        {
            get => times.Peek();
        }
        public int CurTime
        {
            get { return times.ToArray()[times.Count - 1]; }
        }
        public int TimeCapacity
        {
            get => this.times.Count;
        }
        public int TimeScale
        {
            get => (CurTime - BeginTime) / TimeCapacity;
        }

        public Vector Value
        {
            get { return values.ToArray()[values.Count-1]; }
        }

        public int Dimension
        {
            get => Value.Size;
        }

        public List<Vector> ValueList
        {
            get => this.values.ToList();
        }

        public List<Vector> GetValues(int new_time,int count)
        {
            List<int> ts= this.times.ToList();
            int tindex = ts.IndexOf(new_time);
            if (tindex < 0) return null;
            if (tindex < count - 1) return null;
            return this.ValueList.GetRange(tindex - count + 1, count);
        }

        public Vector GetValue(int time,int backIndex)
        {
            List<int> ts = this.times.ToList();
            int tindex = ts.IndexOf(time);
            if (tindex < 0) return null;
            if (tindex - backIndex < 0) return null;
            return this.ValueList[tindex - backIndex];
        }
        public Vector GetValue(int time)
        {
            List<int> ts = this.times.ToList();
            int tindex = ts.IndexOf(time);
            if (tindex < 0) return null;
            return this.ValueList[tindex];
        }


        

        public Node(NodeGene gene)
        {
            this.gene = gene;

            int memoryCapacity = Session.GetConfiguration().agent.shorttermcapacity;
            values = new Queue<Vector>(memoryCapacity);
            times = new Queue<int>(memoryCapacity);
    }
        #endregion

        #region 评价信息
        /** 可靠度 */
        protected double realiability;
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
            if(CurTime == time)return this.Value;
            Object prev = this.Value;

            this.values.Enqueue((Vector)value);
            this.times.Enqueue(time);
            
            return prev;
        }
        /// <summary>
        /// 重置计算
        /// </summary>
        public void Reset()
        {
            
        }
        /// <summary>
        /// 是否已经完成过计算
        /// </summary>
        /// <returns></returns>
        public bool IsActivate(int time)
        {
            return this.CurTime == time;
        }
        #endregion
    }
}
