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
    /// A class containing various math utilities and methods supporting sensing and collision detection.
    /// </summary>
    public class EngineUtilities
    {


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

    }
}
