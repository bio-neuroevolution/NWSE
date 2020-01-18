using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Reflection;
using System.Linq;

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
        public virtual double distance(double v1, double v2) { throw new NotImplementedException(); }

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
        public virtual double getRankedValue(double originValue, int abstractLevel)
        {
            double unit = Level.Step;
            int sectionCount = (int)(Level.Distance / Level.Step);
            int levelIndex = (int)((originValue - Level.Min) / unit);
            if (levelIndex >= sectionCount)
                levelIndex = sectionCount - 1;
            double newValue = Level.Min + (levelIndex * unit + (levelIndex + 1) * unit) / 2.0;
            return newValue;
        }
        #endregion

        #region 显示
        public virtual String getText(double v,String format="F3")
        {
            return v.ToString(format);
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
            this.ranges = m == null ? "[0.0-1.0]" : m.ranges;
            this.tolerate = m == null ? 0 : m.tolerate;
        }
        #endregion

        #region 常用测量
        public static HeadingMeasure Heading
        {
            get
            {
                HeadingMeasure ma = (HeadingMeasure)Measures.FirstOrDefault(m => m.name == "heading");
                if (ma != null)
                    return ma;
                Measures.Add(new HeadingMeasure(null));
                return (HeadingMeasure)Measures.Last();
            }
        }
        public static PositionCodeMeasure Position
        {
            get
            {
                PositionCodeMeasure ma = (PositionCodeMeasure)Measures.FirstOrDefault(m => m.name == "position");
                if (ma != null)
                    return ma;
                Measures.Add(new PositionCodeMeasure(null));
                return (PositionCodeMeasure)Measures.Last();
            }
        }
        public static ActionRotateMeasure Rotate
        {
            get
            {
                ActionRotateMeasure ma = (ActionRotateMeasure)Measures.FirstOrDefault(m => m.name == "rotate");
                if (ma != null)
                    return ma;
                Measures.Add(new ActionRotateMeasure(null));
                return (ActionRotateMeasure)Measures.Last();
            }
        }

        public static CollisionMeasure Collision
        {
            get
            {
                CollisionMeasure ma = (CollisionMeasure)Measures.FirstOrDefault(m => m.name == "collision");
                if (ma != null)
                    return ma;
                Measures.Add(new CollisionMeasure(null));
                return (CollisionMeasure)Measures.Last();
            }
        }


        public static List<MeasureTools> Measures = new List<MeasureTools>();
        public static MeasureTools GetMeasure(String name)
        {
            return Measures.FirstOrDefault(m => m.name == name);
        }

        public static void init()
        {
            Measures.Clear();
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

            String s = Heading.ToString();
            s += Position.ToString();
            s += Rotate.ToString();
            s += Collision.ToString();

        }
            
            

        #endregion
    }

    
    public class HeadingMeasure : MeasureTools
    {
        public double default_tolerate_angle = 90.0 / 360;

        public HeadingMeasure(Configuration.Mensuration m):base(m)
        {
            this.caption = m == null ? "heading" : m.caption;
            this.name = m == null ? "heading" : m.name;
            if (tolerate <= 0) tolerate = default_tolerate_angle;
            if (this.levels == null || this.levels.Trim() == "")
                this.levels = "[0.0-1.0+0.125]";
        }

        public double headingToDegree(double heading)
        {
            return ((heading * 2 * Math.PI * MeasureTools.DRScale) % 360);
        }
        /// <summary>
        /// 两个角度的夹角计算，两个角度都是0-2*pi之间的值
        /// </summary>
        /// <param name="h1"></param>
        /// <param name="h2"></param>
        /// <returns></returns>
        public override double distance(double h1, double h2)
        {
            double x1 = Math.Cos(h1);
            double y1 = Math.Sin(h1);
            double x2 = Math.Cos(h2);
            double y2 = Math.Sin(h2);

            double l1 = Math.Sqrt((x1 * x1 + y1 * y1));
            double l2 = Math.Sqrt((x2 * x2 + y2 * y2));

            double cos = (x1 * x2 + y1 * y2) / (l1 * l2);
            double angle = Math.Acos(cos);
            if (double.IsNaN(angle)) return 0;
            if (angle < 0) angle += Math.PI * 2;
            return angle / tolerate;
        }

        #region 显示
        public override String getText(double v, String format = "F3")
        {
            return v.ToString("F2") + "(" + MeasureTools.Heading.headingToDegree(v).ToString("F2") + ")";
        }
        #endregion
    }



    public class PositionCodeMeasure : MeasureTools
    {
        public const double default_tolerate_distance = 32.0;

        public PositionCodeMeasure(Configuration.Mensuration m):base(m)
        {
            this.caption = m == null ? "position" : m.caption;
            this.name = m == null ? "position" : m.name;
            if (this.tolerate <= 0) tolerate = default_tolerate_distance;
            
        }

        public override double distance(double d1,double d2)
        {
            
            double c1 = d1 * 10000;
            double c2 = d2 * 10000;
            int x1 = (int)c1 / 100, y1 = (int)c1 % 100;
            int x2 = (int)c2 / 100, y2 = (int)c2 % 100;
            return (Math.Abs(x1 - x2) + Math.Abs(y1 - y2)) / tolerate;
        }

        #region 分级处理
        public override double getRankedValue(double originValue, int abstractLevel)
        {
            int group = 4;//每group*group组成一个
            (int x, int y) = poscodeSplit(originValue);
            x = x / group;
            y = y / group;

            int grid = 25;

            int newcode = grid * x + y;
            return newcode * 1.0 / (grid * grid);
        }
        #endregion

        public (double, (int, int)) poscodeCompute(Rectangle range, double x, double y)
        {
            int grid = 100;
            double wunit = range.Width / grid;
            double hunit = range.Height / grid;

            int px = (int)((x - range.X) / wunit);
            int py = (int)((y - range.Y) / hunit);

            int code = grid * py + px;
            return ((code * 1.0) / (grid * grid), (py, px));
        }

        public (int x, int y) poscodeSplit(double code)
        {
            double c = code * 10000;
            int x1 = (int)c / 100, y1 = (int)c % 100;
            return (x1, y1);
        }
        #region 显示
        public override String getText(double v, String format = "F3")
        {
            (int x, int y) = poscodeSplit(v);
            return v.ToString("F4") + "(" + x.ToString() + "," + y.ToString() + ")";
        }

        internal double getUpLevelValue(double code)
        {
            int group = 4;//每group*group组成一个
            (int x, int y) = poscodeSplit(code);
            x = x / group;
            y = y / group;

            int grid = 25;
            
            int newcode = grid * x + y;
            return newcode*1.0/(grid*grid);
        }
        #endregion

    }

    public class ActionRotateMeasure : MeasureTools
    {
        public const double default_tolerate_rotate = Math.PI / 6;

        public ActionRotateMeasure(Configuration.Mensuration m) : base(m)
        {
            this.caption = m == null ? "rotate" : m.caption;
            this.name = m == null ? "rotate" : m.name;
            if (tolerate <= 0) tolerate = default_tolerate_rotate;
            if (this.levels == null || this.levels.Trim() == "")
                this.levels = "[0.0-1.0+0.0625]";
        }

        public override double distance(double a1, double a2)
        {
            return Math.Abs(a1 - a2) * 2 / this.tolerate;
        }
        public  double actionRotateToDegree(double action)
        {
            return (((action - 0.5) * Math.PI * 2) * DRScale) % 360;
        }

        #region 显示
        public override String getText(double v, String format = "F3")
        {
            return v.ToString("F2") + "(" + actionRotateToDegree(v).ToString("F2") + ")";
        }
        #endregion
    }

    public class CollisionMeasure : MeasureTools
    {
       

        public CollisionMeasure(Configuration.Mensuration m) : base(m)
        {
            this.caption = m == null ? "collision" : m.caption;
            this.name = m == null ? "collision" : m.name;
            if (this.levels == null || this.levels.Trim() == "")
                this.levels = "[0.0-1.0+0.5]";
        }

        public override double distance(double a1, double a2)
        {
            return Math.Abs(a1 - a2);
        }
        

        #region 显示
        public override String getText(double v, String format = "F3")
        {
            return v.ToString("F2") + "(" + (v >= 0.5 ? "有障碍" : "无障碍 ") + ")";
        }
        #endregion
    }



}
