using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

using NWSELib.common;

namespace NWSEExperiment.maze
{
    /// <summary>
    /// A class containing methods for converting between simulator space and graphic display space.
    /// </summary>
	public class CoordinateFrame
    {
        #region Instance variables

        public float X,Y;
		public float Scale;
		public float Rotation;

        #endregion

        #region Constructors

        /// <summary>
        /// Creates a new CoordinateFrame object from the specified parameters.
        /// </summary>
        public CoordinateFrame(float x, float y, float scale, float rotation) 
        {
			X=x;
			Y=y;
			Scale=scale;
			Rotation=rotation;
		}

        #endregion Constructors

        
        /// <summary>
        /// Converts a, (x,y) coordinate from simulator space to display space.
        /// </summary>
		public Point convertToDisplay(float x, float y) 
        {
			float ox,oy;
			convertToDisplay(x,y,out ox,out oy);
			return new Point((int)ox,(int)oy);
		}

		/// <summary>
        /// Converts a Point coordinate from simulator space to display space.
		/// </summary>
		public Point convertToDisplay(Point point) 
        {
			return convertToDisplay(point.X,point.Y);
		}
		
        /// <summary>
        /// Converts a Point2D coordinate from simulator space to display space
        /// </summary>
		public Point2D convertToDisplay(Point2D point) 
        {
			float ox,oy;
			convertToDisplay((float)point.X,(float)point.Y,out ox, out oy);
			return new Point2D((double)ox,(double)oy);
		}
		
		/// <summary>
        /// Converts an (x,y) coordinate from simulator space to display space and returns an (x,y) coordinate in raw float format. Input is a delta, not an absolute point in terms of screen coordinates. Output is a delta in terms of simulator coordinates.
		/// </summary>
		public void convertToDisplay(float ix,float iy,out float ox, out float oy) 
		{
			ox = (ix-X)/Scale;
			oy = (iy-Y)/Scale;
		}
		
		/// <summary>
        /// Converts an (x,y) coordinate from display space to simulator space and returns an (x,y) coordinate in raw float format. Input is a delta, not an absolute point in terms of screen coordinates. Output is a delta in terms of simulator coordinates.
		/// </summary>
		public void convertFromDisplay(float ix,float iy,out float ox, out float oy) 
		{
			ox = ix*Scale+X;
			oy = iy*Scale+Y;
		}
		
	    /// <summary>
        /// Converts an (x,y) coordinate from simulator space to display space and returns an (x,y) coordinate in raw float format, leaving offsets intact. Input is a delta, not an absolute point in terms of screen coordinates. Output is a delta in terms of simulator coordinates.
	    /// </summary>
		public void convertToDisplayOffset(float ix, float iy, out float ox, out float oy)
		{
			ox = ix/Scale;
			oy = iy/Scale;
		}

        /// <summary>
        /// Converts an (x,y) coordinate from display space to simulator space and returns an (x,y) coordinate in raw float format, leaving offsets intact. Input is a delta, not an absolute point in terms of screen coordinates. Output is a delta in terms of simulator coordinates.
        /// </summary>
		public void convertFromDisplayOffset(float ix, float iy, out float ox, out float oy)
		{
			ox = ix*Scale;
			oy = iy*Scale;
        }

        
    }
	
    /// <summary>
    /// Base class for all collision managers. 
    /// </summary>
	public abstract class CollisionManager
    {
        #region Instance variables

        public bool AgentVisible;
        public bool AgentCollide;

        #endregion

        #region Methods

        //public abstract CollisionManager copy();
		//public abstract void initialize(Environment environment, SimulatorExperiment experiment, List<Robot> robots);
		public virtual void simulationStepCallback() { }
		//public abstract bool robotCollide(Robot robot);
		//public abstract double raycast(double angle, double maxRange, Point2D point, Robot owner, out SimulatorObject hit, bool absolute = false);

        #endregion
    }
	
    /// <summary>
    /// A class containing various math utilities and methods supporting sensing and collision detection.
    /// </summary>
    public class EngineUtilities
    {
        #region Instance variables

        public static Random RNG = new Random();

        #region Pen Colors
        public static Brush RedBrush = System.Drawing.Brushes.Red;
        public static Pen RedPen = new Pen(System.Drawing.Brushes.Red, 2.0f);
        public static Pen BluePen = new Pen(System.Drawing.Brushes.Blue, 2.0f);
        public static Pen GreendPen = new Pen(System.Drawing.Brushes.Green, 2.0f);
        public static Pen YellowPen = new Pen(System.Drawing.Brushes.Yellow, 2.0f);
        public static Pen DashedPen = new Pen(Brushes.Black, 1.0f);
        public static Pen TransGrayPen = new Pen(Color.FromArgb(64, Color.Black));

        public static SolidBrush voiceColorBrush = new SolidBrush(Color.FromArgb(64, Color.Black));
        public static SolidBrush backGroundColorBrush = new SolidBrush( Color.White ); 
        #endregion

        #endregion

        #region Methods

        #region Collision Handling

        public static float addNoise(float val,float percentage)
		{
			percentage/=100.0f;
			float p1 = 1.0f -percentage;
			float p2 = percentage;
			float rval = (float)new Random().NextDouble();
			return p1*val+p2*rval;
		}


        

        public static bool collide(Wall wall, Point2D robot)
        {
            Point2D a1 = new Point2D(wall.Line.Endpoint1);
            Point2D a2 = new Point2D(wall.Line.Endpoint2);
            Point2D b = robot;
            if (!wall.Visible)
                return false;
            double rad = RobotAgent.RADIUS;
            double r = ((b.X - a1.X) * (a2.X - a1.X) + (b.Y - a1.Y) * (a2.Y - a1.Y)) / wall.Line.squaredLength();
            double px = a1.X + r * (a2.X - a1.X);
            double py = a1.Y + r * (a2.Y - a1.Y);
            Point2D np = new Point2D(px, py);
            double rad_sq = rad * rad;

            if (r >= 0.0f && r <= 1.0f)
            {
                if (np.squaredDistance(b) < rad_sq)
                    return true;
                else
                    return false;
            }

            double d1 = b.squaredDistance(a1);
            double d2 = b.squaredDistance(a2);
            if (d1 < rad_sq || d2 < rad_sq)
                return true;
            else
                return false;
        }

        #endregion

        

        /// <summary>
        /// Calculates the Euclidean Distance between two Point objects.
        /// </summary>
        public static double euclideanDistance(Point p1, Point p2)
        {
            return Math.Sqrt(Math.Pow(p1.X - p2.X, 2) + Math.Pow(p1.Y - p2.Y, 2));
        }

        /// <summary>
        /// Calculates the Euclidean Distance between two Point2D objects.
        /// </summary>
        public static double euclideanDistance(Point2D p1, Point2D p2)
        {
            return Math.Sqrt(Math.Pow(p1.X - p2.X, 2) + Math.Pow(p1.Y - p2.Y, 2));
        }

        

        /// <summary>
        /// Calculates the squared Distance between two Point2D objects.
        /// </summary>
        public static double squaredDistance(Point2D p1, Point2D p2)
        {
            return Math.Pow(p1.X - p2.X, 2) + Math.Pow(p1.Y - p2.Y, 2);
        }

        /// <summary>
        /// Calculates the Euclidean Distance between a Point2D object and a Point object.
        /// </summary>
        public static double euclideanDistance(Point2D p1, Point p2)
        {
            return Math.Sqrt(Math.Pow(p1.X - p2.X, 2) + Math.Pow(p1.Y - p2.Y, 2));
        }

        /// <summary>
        /// Calculates the Euclidean Distance between a Point object and a Point2D object.
        /// </summary>
        public static double euclideanDistance(Point p1, Point2D p2)
        {
            return euclideanDistance(p2, p1);
        }

        /// <summary>
        /// Ensures that a given value does not exceed a specified max and min, but not scaling the value if it already fits within the specified bounds.
        /// </summary>
        public static double clamp(double val, double min, double max)
        {
            if (val > max)
                val = max;
            else if (val < min)
                val = min;
            
            return val;
        }
        public const double DRScale = 57.29578;

        public static double angletoradian(double value)
        {
            return value <= 180 ? (Math.PI / 180) * value : -(Math.PI / 180) * value;
        }
        #endregion
    }
}
