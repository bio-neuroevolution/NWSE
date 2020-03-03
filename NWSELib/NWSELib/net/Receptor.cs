﻿using NWSELib.common;
using NWSELib.genome;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NWSELib.net
{
    /// <summary>
    /// 感受器
    /// </summary>
    public class Receptor : Node
    {

        #region 初始化
        public Receptor(NodeGene gene,Network net) : base(gene,net)
        {

        }

        public ReceptorGene getGene()
        {
            return (ReceptorGene)gene;
        }
        #endregion

        #region 值管理
        /// <summary>
        /// 设置当前值
        /// </summary>
        /// <param name="value"></param>
        /// <param name="time"></param>
        /// <returns></returns>
        public override Object activate(Network net, int time, Object value = null)
        {
            //double rankedvalue = getRankedValue((double)value);
            Object prevValue = base.activate(net, time, value);
            return prevValue;
        }

        public double getRankedValue(double originValue)
        {
            if (this.getGene().AbstractLevel == 0)
                return originValue;


            int sampleCount = getGene().SampleCount;
            if (sampleCount <= 0) return originValue;

            return MeasureTools.GetMeasure(this.Cataory).getRankedValue(originValue, this.getGene().AbstractLevel, sampleCount);


        }

        public override Vector Value
        {
            get 
            { 
                Vector v = values.Count <= 0 ? null : values.ToArray()[values.Count - 1];
                if (v == null) return v;
                return getRankedValue((double)v[0]);
            }
        }

        public override String getValueText(Vector value = null)
        {
            if (value == null) value = Value;
            if (value == null) return "";
            if (this.getGene().AbstractLevel == 0)
                return value[0].ToString("F4");

            List<String> names = this.getGene().AbstractLevelNames;
            if (names == null) return value.ToString();
            int sectionCount = this.getGene().SampleCount;
            int rankIndex = MeasureTools.GetMeasure(this.Cataory).getRankedIndex(value, this.getGene().AbstractLevel, sectionCount);
            return value[0].ToString("F4")+"("+ names[rankIndex] + ")";
        }

        public override List<Vector> ValueList
        {
            get => new List<Vector>(this.values).ConvertAll(v=>
                new Vector(this.getRankedValue(v)));
        }

        public override List<Vector> GetValues(int new_time, int count)
        {
            List<int> ts = this.times.ToList();
            int tindex = times.IndexOf(new_time);
            if (tindex < 0) return null;


            List<Vector> r = new List<Vector>();
            for (int i = 0; i < count; i++)
            {
                if (tindex >= values.Count) return r;
                r.Add(values[tindex++]);
            }
            return r.ConvertAll(v=>
                new Vector(this.getRankedValue(v[0]))
            );
        }

        public override Vector GetValue(int time, int backIndex)
        {
            int tindex = times.IndexOf(time);
            if (tindex < 0) return null;
            if (tindex - backIndex < 0) return null;
            return new Vector(getRankedValue(this.ValueList[tindex - backIndex]));
            
        }
        public override Vector GetValue(int time)
        {
            int tindex = times.IndexOf(time);
            if (tindex < 0) return null;
            return new Vector(getRankedValue(this.values[tindex]));
        }
        #endregion

        #region 值距离
        public double distance(double v,int time=-1)
        {
            double v2 = (time < 0 ? this.Value[0] : this.GetValue(time)[0]);
            return distance(v,v2);
        }
        public double distance(double v1,double v2)
        {
            MeasureTools measure =
                MeasureTools.GetMeasure(Gene.Cataory);
            return measure.distance(v1, v2);
        }

        public bool IsTolerateDistance(double distance)
        {
           return MeasureTools.GetMeasure(Gene.Cataory).tolerate >= distance;
        }

        
        public double[] GetSampleValues()
        {
            if (getGene().AbstractLevel <= 0) return null;
            
            int count = getGene().SampleCount;
            double unit = getGene().LevelUnitDistance;
            return MeasureTools.GetMeasure(this.gene.Cataory).getRankedSamples(count, unit);
            
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="randomType">uniform，gaussian，maintain</param>
        /// <returns></returns>
        internal double randomValue(String randomType = "uniform", int aligncount = 0,double center=0.5)
        {
            double value = 0;
            if (randomType == "gaussian")
                value = Session.GetConfiguration().agent.receptors.GetSensor("_" + this.Name).Range.gaussian_random(center);
            else
                value = Network.rng.NextDouble();

            if (aligncount <= 0) return value;
            List<double> values = MeasureTools.GetMeasure(this.Cataory).getRankedSamples(aligncount).ToList();
            List<double> diffs = values.ConvertAll(v => Math.Abs(v - value));
            return values[diffs.argmin()];
            
        }
        #endregion
    }
}
