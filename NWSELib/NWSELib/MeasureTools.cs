﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Reflection;
using System.Linq;

using NWSELib.common;

namespace NWSELib
{
    /// <summary>
    /// 测量工具
    /// </summary>
    public class MeasureTools : Configuration.Mensuration
    {
        #region 常量
        public const double DRScale = 57.29578;
        #endregion

        #region 距离计算
        public virtual double move(double value,int direction,double distance)
        {
            value = value + direction*distance;
            if (value < 0) return 0;
            else if (value > 1) return 1;
            return value;
        }
        public virtual double moveTo(double src,double dest,int direction,double distance)
        {
            double value = src;
            if (src <= dest)
                value = src + direction * distance;
            else if(src > dest)
                value = src + direction * distance;
            if (value < 0) return 0;
            else if (value > 1) return 1;
            return value;
        }
        public virtual double difference(double v1,double v2)
        {
            return v1 - v2;
        }
        public virtual double distance(double v1, double v2) { throw new NotImplementedException(); }

        public virtual int getChangeDirection(double from,double to)
        {
            if (from < to) return 1;
            else if (from > to) return -1;
            return 0;
        }

        public bool match(double d1, double d2)
        {
            return distance(d1, d2) <= tolerate;
        }
        public bool match(double d1, double d2, out double dis)
        {
            dis = distance(d1, d2);
            return distance(d1, d2) <= tolerate;
        }


        #endregion

        #region 分级处理
        protected Dictionary<int, double[]> _cached_sample_values = new Dictionary<int, double[]>();
        /// <summary>
        /// 取得被分级的采样
        /// </summary>
        /// <param name="sectionCount"></param>
        /// <returns></returns>
        public virtual double[] getRankedSamples(int count, double unit=-1)
        {
            if (_cached_sample_values.ContainsKey(count))
                return _cached_sample_values[count];
            if (unit <= 0) unit = 1.0 / (count-1);
            double[] values = new double[count];
            for (int i = 0; i < values.Length; i++)
            {
                values[i] = i * unit;
                if (i == values.Length - 1) values[i] = 1.0;

            }
            _cached_sample_values.Add(count,values);
            return values;
        }
        /// <summary>
        /// 根据分级层次，分段数计算分级后的值
        /// </summary>
        /// <param name="originValue"></param>
        /// <param name="abstractLevel"></param>
        /// <param name="sectionCount"></param>
        /// <returns></returns>
        public virtual double getRankedValue(double originValue, int abstractLevel,int sampleCount=-1)
        {
            if (abstractLevel == 0) return originValue;

            if (sampleCount == -1) sampleCount = this.Levels[abstractLevel - 1];
            double[] values = this.getRankedSamples(sampleCount);
            return values[values.ToList().ConvertAll(v=>Math.Abs(v-originValue)).argmin()];
            
        }
        /// <summary>
        /// 根据分级层次，分段数计算分级后的索引
        /// </summary>
        /// <param name="value"></param>
        /// <param name="abstractLevel"></param>
        /// <param name="sectionCount"></param>
        /// <returns></returns>
        public virtual int getRankedIndex(double value, int abstractLevel, int sectionCount)
        {
            if (sectionCount == 0) sectionCount = this.Levels[0];
            double unit = this.Range.Distance / sectionCount;
            int levelIndex = (int)((value - Range.Min) / unit);
            if (levelIndex >= sectionCount)
                levelIndex = sectionCount - 1;
            return levelIndex;
        }

        #endregion

        #region 初始化
        public MeasureTools(Configuration.Mensuration m)
        {
            this.caption = m == null ? "" : m.caption;
            this.dimension = m == null ? 1 : m.dimension;
            this.levelNames = m == null ? "" : m.levelNames;
            this.levels = m == null ? "" : m.levels;
            this.name = m == null ? "" : m.name;
            this.range = m == null ? "[0.0-1.0]" : m.range;
            this.tolerate = m == null ? 0 : m.tolerate;
        }
        #endregion

        #region 常用测量
        public static DistanceMeasure Distance
        {
            get
            {
                return (DistanceMeasure)Measures.FirstOrDefault(m => m.name == "distance");
            }
        }
        public static DirectionMeasure Direction
        {
            get
            {
                return (DirectionMeasure)Measures.FirstOrDefault(m => m.name == "direction");
                
            }
        }
        public static DirectionMeasure Heading
        {
            get
            {
                return (DirectionMeasure)Measures.FirstOrDefault(m => m.name == "heading");

            }
        }
        public static DirectionMeasure Rotate
        {
            get
            {
                return (DirectionMeasure)Measures.FirstOrDefault(m => m.name == "rotate");

            }
        }
        public static PositionCodeMeasure Position
        {
            get
            {
                return (PositionCodeMeasure)Measures.FirstOrDefault(m => m.name == "poscode");
                
            }
        }
        public static OnoffMeasure Onoff
        {
            get
            {
                return (OnoffMeasure)Measures.FirstOrDefault(m => m.name == "onoff");
                
            }
        }
        public static IndexMeasure Index
        {
            get
            {
                return (IndexMeasure)Measures.FirstOrDefault(m => m.name == "index");
                
            }
        }

        public static List<MeasureTools> Measures = new List<MeasureTools>();
        public static MeasureTools GetMeasure(String name)
        {
            return Measures.FirstOrDefault(m => m.name == name);
        }

        public static void init()
        {
            if (Measures.Count > 0) return;
            List<Configuration.Mensuration> ms = Session.GetConfiguration().mensurations;
            foreach(Configuration.Mensuration m in ms)
            {
                if (m.typeName == null || m.typeName.Trim() == "")
                    m.typeName = "NWSELib.MeasureTools";

                Type type = Assembly.GetExecutingAssembly().GetType(m.typeName);
                if (type == null) continue;
                MeasureTools mt = (MeasureTools)(type.GetConstructor(new Type[] { typeof(Configuration.Mensuration) })).Invoke(new Object[] { m });
                if (mt != null)
                    Measures.Add(mt);
            }
        } 
        #endregion
    }

    public class DistanceMeasure : MeasureTools
    {
        public double default_tolerate_distance = 0.2;
        public DistanceMeasure(Configuration.Mensuration m) : base(m)
        {
            this.caption = m == null ? "distance" : m.caption;
            this.name = m == null ? "distance" : m.name;
            if (tolerate <= 0) tolerate = default_tolerate_distance;
            if (this.levels == null || this.levels.Trim() == "")
            {
                this.levels = "5";
                this.levelNames = "极小,较小,中等,较大,较大";
            }   
        }
        /// <summary>
        /// 两个角度的夹角计算，两个角度都是0-2*pi之间的值
        /// </summary>
        /// <param name="h1"></param>
        /// <param name="h2"></param>
        /// <returns></returns>
        public override double distance(double h1, double h2)
        {
            return Math.Abs(h1-h2);
        }

    }
    public class DirectionMeasure : MeasureTools
    {
        public double default_tolerate_angle = 30.0 / 360;

        public DirectionMeasure(Configuration.Mensuration m):base(m)
        {
            this.caption = m == null ? "direction" : m.caption;
            this.name = m == null ? "direction" : m.name;
            if (tolerate <= 0) tolerate = default_tolerate_angle;
            if (this.levels == null || this.levels.Trim() == "")
            {
                this.levels = "8";
                this.levelNames = "东,东南,南,西南,西,西北,北,东北";
            }
        }

        public override double moveTo(double src, double dest, int direction, double distance)
        {
            if (src == dest) return move(src, direction, distance);
            double value = src;
            double diff = src - dest;
            int t = diff < 0 ? 1 : -1;

            if (Math.Abs(diff) <= 0.5)
                value = src + t * direction * distance;
            else
                value = src - t * direction * distance;

            if (value >= 1) value = value - 1;
            else if (value < 0) value = value + 1;
            return value;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="direction">1表示顺时针，-1表示逆时针</param>
        /// <param name="distance"></param>
        /// <returns></returns>
        public override double move(double value, int direction, double distance)
        {
            if (direction >= 0) value = value + distance;
            else value -= distance;

            if (value >= 1) value = value - 1;
            else if (value < 0) value = value + 1;
            return value;
        }
        public override int getChangeDirection(double from, double to)
        {
            if (from == to) return 0;
            if (from >= 0 && from < 0.25)
            {
                if (to >= 0 && to < 0.25) return to > from ? 1 : -1;
                else if (to >= 0.25 && to < 0.5) return 1;
                else if (to >= 0.5 && to < 0.75) return to - from <= 0.5 ? 1 : -1;
                else return -1;
            } else if (from >= 0.25 && from < 0.5)
            {
                if (to >= 0 && to < 0.25) return -1;
                else if (to >= 0.25 && to < 0.5) return to > from ? 1 : -1;
                else if (to >= 0.5 && to < 0.75) return 1;
                else return to - from <= 0.5 ? 1 : -1;
            } else if (from >= 0.5 && from < 0.75)
            {
                if (to >= 0 && to < 0.25) return from-to>=0.5?1:-1;
                else if (to >= 0.25 && to < 0.5) return -1;
                else if (to >= 0.5 && to < 0.75) return to > from ? 1 : -1;
                else return to > from ? 1 : -1;
            }
            else
            {
                if (to >= 0 && to < 0.25) return 1;
                else if (to >= 0.25 && to < 0.5) return from - to >= 0.5 ? 1 : -1;
                else if (to >= 0.5 && to < 0.75) return -1;
                else return to > from ? 1 : -1;
            }
            
        }
        /// <summary>
        /// 位置朝向（0-1）（0-2pi）信息转换为角度（0-360）
        /// </summary>
        /// <param name="heading"></param>
        /// <returns></returns>
        public double headingToDegree(double heading)
        {
            return ((heading * 2 * Math.PI * MeasureTools.DRScale) % 360);
        }
        /// <summary>
        /// 将旋转角度（0-1）（-pi-pi）转换为角度（-180-180）
        /// </summary>
        /// <param name="rotate"></param>
        /// <returns></returns>
        public double actionRotateToDegree(double rotate)
        {
            return (rotate - 0.5) * 2 * Math.PI * MeasureTools.DRScale;
        }

        /// <summary>
        /// 取得被分级的采样
        /// </summary>
        /// <param name="sectionCount"></param>
        /// <returns></returns>
        public override double[] getRankedSamples(int count, double unit = -1)
        {
            if (_cached_sample_values.ContainsKey(count))
                return _cached_sample_values[count];
            
            
            double[] values = new double[count];
            if (count == 4) values = new double[] { 0, 0.25, 0.5, 0.75 };
            else if (count == 8) values = new double[] { 0, 0.125, 0.25, 0.375, 0.5, 0.625, 0.75, 0.875 };
            else if (count == 12) values = new double[] { 0, 0.083, 0.0167, 0.25, 0.3333, 0.4167, 0.5, 0.5833, 0.6667, 0.75, 0.8333, 0.9167 };
            else if (count == 16) values = new double[] { 0, 0.0625, 0.125, 0.1875, 0.25, 0.3125, 0.375, 0.4375, 0.5, 0.5625, 0.625, 0.6875, 0.75, 0.8125, 0.875, 0.9375 };
            
            _cached_sample_values.Add(count, values);
            return values;
        }

        /// <summary>
        /// 根据分级层次，分段数计算分级后的值
        /// </summary>
        /// <param name="originValue"></param>
        /// <param name="abstractLevel"></param>
        /// <param name="sectionCount"></param>
        /// <returns></returns>
        public override double getRankedValue(double originValue, int abstractLevel, int sampleCount = -1)
        {
            if (abstractLevel == 0) return originValue;

            if (sampleCount == -1) sampleCount = this.Levels[abstractLevel - 1];
            double[] values = this.getRankedSamples(sampleCount);
            List<double> temp = new List<double>(values);
            temp.Add(1.0);
            int index = temp.ConvertAll(v => Math.Abs(v - originValue)).argmin();
            if (index == temp.Count - 1) return values[0];
            else return values[index];
            

        }

        /// <summary>
        /// 两个角度的夹角计算，两个角度都是0-2*pi之间的值
        /// </summary>
        /// <param name="h1"></param>
        /// <param name="h2"></param>
        /// <returns></returns>
        public override double distance(double h1, double h2)
        {
            double x1 = Math.Cos(h1*2*Math.PI);
            double y1 = Math.Sin(h1 * 2 * Math.PI);
            double x2 = Math.Cos(h2 * 2 * Math.PI);
            double y2 = Math.Sin(h2 * 2 * Math.PI);

            double l1 = Math.Sqrt((x1 * x1 + y1 * y1));
            double l2 = Math.Sqrt((x2 * x2 + y2 * y2));

            double cos = (x1 * x2 + y1 * y2) / (l1 * l2);
            double angle = Math.Acos(cos);
            if (double.IsNaN(angle)) return 0;
            if (angle < 0) angle += Math.PI * 2;
            return angle / (2*Math.PI);
        }

        public override double difference(double v1, double v2)
        {
            return distance(v1,v2);
        }


    }

    public class HeadingMeasure : DirectionMeasure
    {
        
        public HeadingMeasure(Configuration.Mensuration m) : base(m)
        {
            this.caption = m == null ? "heading" : m.caption;
            this.name = m == null ? "heading" : m.name;
            if (tolerate <= 0) tolerate = default_tolerate_angle;
            if (this.levels == null || this.levels.Trim() == "")
            {
                this.levels = "8";
                this.levelNames = "东,东南,南,西南,西,西北,北,东北";
            }
        }
    }

    public class RotateMeasure : DirectionMeasure
    {

        public RotateMeasure(Configuration.Mensuration m) : base(m)
        {
            this.caption = m == null ? "rotate" : m.caption;
            this.name = m == null ? "rotate" : m.name;
            if (tolerate <= 0) tolerate = default_tolerate_angle;
            if (this.levels == null || this.levels.Trim() == "")
            {
                this.levels = "8";
                this.levelNames = "-180度,-135度,-90度,-45度,0度,45度,90度,135度,180度";
            }
        }
    }


    public class PositionCodeMeasure : MeasureTools
    {
        public const double default_tolerate_distance = 32.0;
        public const int default_grid = 100;

        public PositionCodeMeasure(Configuration.Mensuration m):base(m)
        {
            this.caption = m == null ? "poscode" : m.caption;
            this.name = m == null ? "poscode" : m.name;
            if (this.tolerate <= 0) tolerate = default_tolerate_distance;
            if (this.levels == null || this.levels.Trim() == "")
            {
                this.levels = "4";
                this.levelNames = "";
            }

        }

        public override double distance(double d1,double d2)
        {
            
            double c1 = d1 * 10000;
            double c2 = d2 * 10000;
            int x1 = (int)c1 / 100, y1 = (int)c1 % 100;
            int x2 = (int)c2 / 100, y2 = (int)c2 % 100;
            double d = (Math.Abs(x1 - x2) + Math.Abs(y1 - y2));
            if (d <= tolerate) return 0;
            return d / tolerate;
        }

        #region 分级处理
        /// <summary>
        /// 根据分级层次，分段数计算分级后的值
        /// </summary>
        /// <param name="originValue"></param>
        /// <param name="abstractLevel"></param>
        /// <param name="sectionCount"></param>
        /// <returns></returns>
        public override double getRankedValue(double originValue, int abstractLevel, int sectionCount)
        {
            if (abstractLevel == 0) return originValue;

            int group = sectionCount;//每group*group组成一个
            (int x, int y) = poscodeSplit(originValue);
            x = x / group;
            y = y / group;

            int grid = default_grid/group;

            int newcode = grid * x + y;
            return newcode * 1.0 / (grid * grid);
        }
        /// <summary>
        /// 根据分级层次，分段数计算分级后的索引
        /// </summary>
        /// <param name="value"></param>
        /// <param name="abstractLevel"></param>
        /// <param name="sectionCount"></param>
        /// <returns></returns>
        public override int getRankedIndex(double value, int abstractLevel, int sectionCount)
        {
            (int x, int y) = this.poscodeSplit(value);
            int grid = default_grid;
            if (abstractLevel > 0) grid = default_grid / sectionCount;
            return grid * x + y;
        }



        #endregion

        #region 编码处理
         /// <summary>
         /// 根据实际范围和坐标生成网格编码，以及网格行列号
         /// </summary>
         /// <param name="range"></param>
         /// <param name="x"></param>
         /// <param name="y"></param>
         /// <returns></returns>
        public (double, (int, int)) poscodeCompute(Rectangle range, double x, double y)
        {
            int grid = default_grid;
            double wunit = range.Width / grid;
            double hunit = range.Height / grid;

            int px = (int)((x - range.X) / wunit);
            int py = (int)((y - range.Y) / hunit);

            int code = grid * py + px;
            return ((code * 1.0) / (grid * grid), (py, px));
        }
        /// <summary>
        /// 将网格编码分解成行列号
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public (int x, int y) poscodeSplit(double code,int abstraceLevel=0,int sectionCount=0)
        {
            int grid = default_grid;
            if (sectionCount > 0) grid /= sectionCount;
            double c = code * (grid* grid);
            int x1 = (int)c / grid, y1 = (int)c % grid;
            return (x1, y1);
        }
        

        
        #endregion

    }

    public class IndexMeasure : MeasureTools
    {
        public IndexMeasure(Configuration.Mensuration m) : base(m)
        {
            name = "index";
        }
        public override double distance(double a1, double a2)
        {
            return a1 == a2 ? 0 : 1;
        }
    }
    

    public class OnoffMeasure : MeasureTools
    {
        public OnoffMeasure(Configuration.Mensuration m) : base(m)
        {
            this.caption = m == null ? "onoff" : m.caption;
            this.name = m == null ? "onoff" : m.name;
            if (this.levels == null || this.levels.Trim() == "")
            {
                this.levels = "2";
                this.levelNames = "无,有";
            }
        }

        public override double distance(double a1, double a2)
        {
            return Math.Abs(a1 - a2);
        }
        

        
    }



}
