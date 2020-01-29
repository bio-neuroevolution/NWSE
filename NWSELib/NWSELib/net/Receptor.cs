using NWSELib.common;
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

        
        public Receptor(NodeGene gene,Network net) : base(gene,net)
        {

        }

        public ReceptorGene getGene()
        {
            return (ReceptorGene)gene;
        }

        /// <summary>
        /// 设置当前值
        /// </summary>
        /// <param name="value"></param>
        /// <param name="time"></param>
        /// <returns></returns>
        public override Object activate(Network net, int time, Object value = null)
        {
            //Object prevValue = base.activate(net, time, new Vector(sectionIndex));
            Object prevValue = base.activate(net, time, value);
            return prevValue;
        }

        

        public override Vector Value
        {
            get 
            { 
                Vector v = values.Count <= 0 ? null : values.ToArray()[values.Count - 1];
                if (v == null) return v;
                return MeasureTools.GetMeasure(this.Cataory).getRankedValue(v, this.getGene().AbstractLevel, this.getGene().AbstractSectionCount);
            }
        }

        public override String getValueText(Vector value = null)
        {
            if (value == null) value = Value;
            if (value == null) return "";
            if (this.getGene().AbstractLevel == 0)
                return value[0].ToString("F3");

            List<String> names = this.getGene().AbstractLevelNames;
            if (names == null) return value.ToString();
            int sectionCount = this.getGene().AbstractSectionCount;
            int rankIndex = MeasureTools.GetMeasure(this.Cataory).getRankedIndex(value, this.getGene().AbstractLevel, sectionCount);
            return names[rankIndex]+"("+value[0].ToString("F4")+")";
        }

        public override List<Vector> ValueList
        {
            get => new List<Vector>(this.values).ConvertAll(v=>
                new Vector(MeasureTools.GetMeasure(this.Cataory).getRankedValue(v, this.getGene().AbstractLevel, this.getGene().AbstractSectionCount)));
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
                new Vector(MeasureTools.GetMeasure(this.Cataory).getRankedValue(v, this.getGene().AbstractLevel, this.getGene().AbstractSectionCount))
            );
        }

        public override Vector GetValue(int time, int backIndex)
        {
            int tindex = times.IndexOf(time);
            if (tindex < 0) return null;
            if (tindex - backIndex < 0) return null;
            return new Vector(MeasureTools.GetMeasure(this.Cataory).getRankedValue(this.ValueList[tindex - backIndex], this.getGene().AbstractLevel, this.getGene().AbstractSectionCount));
            
        }
        public override Vector GetValue(int time)
        {
            int tindex = times.IndexOf(time);
            if (tindex < 0) return null;
            return new Vector(MeasureTools.GetMeasure(this.Cataory).getRankedValue(this.values[tindex], this.getGene().AbstractLevel, this.getGene().AbstractSectionCount));
        }
    }
}
