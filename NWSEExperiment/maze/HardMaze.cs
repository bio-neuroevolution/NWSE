﻿using NWSELib.common;
using NWSELib.env;
using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Drawing;
using System.IO;
using System.Text;
using System.Xml.Serialization;
using System.Linq;
using NWSELib.net;

namespace NWSEExperiment.maze
{
    public class HardMaze : IEnv
    {
        #region Instance variables

        // Legacy naming convention used to maintain compatibility with old experiment/CurrentEnvironment files. Do not modify.

        public List<Wall> walls;
        public String name;
        [XmlIgnore]
        public Random rng = new Random();

        public Rectangle AOIRectangle { get; set; }
        public List<Point> POIPosition { get; set; }
        public float maxDistance; // Distance from corner to corner. Determined by AOI.

        public Point2D start_point; //Start location of the agent
        public Point2D goal_point;

        public int group_orientation;
        public int robot_spacing;
        public int robot_heading;
        public int seed;
        public float view_x;
        public float view_y;
        public float view_scale;

        public float stagger;

        #endregion

        #region 增加的内容
        private ConcurrentDictionary<int, RobotAgent> agents = new ConcurrentDictionary<int, RobotAgent>();
        List<List<Point2D>> optimaTraces = new List<List<Point2D>>();
        public bool AgentVisible { get; set; }
        #endregion

        #region Constructors

        /// <summary>
        /// Creates a small, empty CurrentEnvironment.
        /// </summary>
        public HardMaze()
        {
            //reset();
            AgentVisible = true;
        }

        #endregion

        #region Methods

        

        /// <summary>
        /// Save the CurrentEnvironment to the specified XML file.
        /// </summary>
        public void save(string name)
        {
            System.Xml.Serialization.XmlSerializer x = new System.Xml.Serialization.XmlSerializer(this.GetType());
            TextWriter outfile = new StreamWriter(name);
            x.Serialize(outfile, this);
            outfile.Close();
        }

        /// <summary>
        /// Loads a CurrentEnvironment from the specified XML file and initializes it.
        /// </summary>
        public static HardMaze loadEnvironment(string name)
        {
            System.Xml.Serialization.XmlSerializer x = new System.Xml.Serialization.XmlSerializer(typeof(HardMaze));
            TextReader infile = new StreamReader(name);
            HardMaze k = (HardMaze)x.Deserialize(infile);
            k.maxDistance = (float)Math.Sqrt(k.AOIRectangle.Width * k.AOIRectangle.Width + k.AOIRectangle.Height * k.AOIRectangle.Height);

            k.name = name;

            Console.WriteLine("# walls: " + k.walls.Count);
            k.rng = new Random(k.seed);
            return k;
        }

        /// <summary>
        /// Finds the wall with the specified name. Pretty straightforward.
        /// </summary>
        public Wall findWallByName(string name)
        {
            foreach (Wall wall in walls)
            {
                if (wall.Name == name)
                    return wall;
            }
            return null;
        }

        /// <summary>
        /// Resets the CurrentEnvironment to a small, blank square.
        /// </summary>
        public void reset()
        {

            HardMaze newMaze = HardMaze.loadEnvironment(this.name);

            walls = newMaze.walls;
            name = newMaze.name;
            AOIRectangle = newMaze.AOIRectangle;
            POIPosition = newMaze.POIPosition;
            maxDistance = newMaze.maxDistance;
            start_point = newMaze.start_point;
            goal_point = newMaze.goal_point;

            group_orientation = newMaze.group_orientation;
            robot_spacing = newMaze.robot_spacing;
            robot_heading = newMaze.robot_heading;
            seed = newMaze.seed;
            view_x = newMaze.view_x;
            view_y = newMaze.view_y;
            view_scale = newMaze.view_scale;

            stagger = newMaze.stagger;

            this.agents.Clear();
            initOptimaTraces();
        }


        /// <summary>
        /// Draws the CurrentEnvironment to the screen.
        /// </summary>
        public void draw(Graphics g, CoordinateFrame frame)
        {
            float sx, sy;
            float gx, gy;

            frame.convertToDisplay((float)start_point.X, (float)start_point.Y, out sx, out sy);
            frame.convertToDisplay((float)goal_point.X, (float)goal_point.Y, out gx, out gy);
            Rectangle startrect = new Rectangle((int)sx - 3, (int)sy - 3, 6, 6);
            Rectangle goalrect = new Rectangle((int)gx - 3, (int)gy - 3, 6, 6);

            float rx, ry, rsx, rsy;
            frame.convertToDisplay((float)AOIRectangle.X, (float)AOIRectangle.Y, out rx, out ry);
            frame.convertToDisplayOffset((float)AOIRectangle.Width, (float)AOIRectangle.Height, out rsx, out rsy);
            Rectangle AOIDisplay = new Rectangle((int)rx, (int)ry, (int)rsx, (int)rsy);

            //Display Area of Interest rectangle
            g.DrawRectangle(EngineUtilities.DashedPen, AOIDisplay);

            g.DrawEllipse(EngineUtilities.BluePen, startrect);
            g.DrawEllipse(EngineUtilities.RedPen, goalrect);

            //Display Point Of Interests
            int index = 0;
            foreach (Point p in POIPosition)
            {
                Point p2 = frame.convertToDisplay(p);
                g.DrawEllipse(EngineUtilities.GreendPen, new Rectangle((int)p2.X - 3, (int)p2.Y - 3, 6, 6));
                g.DrawString(index.ToString(), new Font("Verdana", 8), new SolidBrush(Color.Black), p2.X, p2.Y);
                index++;
            }

            foreach (Wall wall in walls)
            {
                wall.draw(g, frame);
            }

            if (!this.AgentVisible) return;

            List<RobotAgent> agents = this.agents.Values.ToList();
            foreach(RobotAgent agent in agents)
            {
                agent.draw(g, frame);
            }
        }

        List<double> IEnv.reset(Network net)
        {
            IAgent agent = this.agents.Values.ToList().FirstOrDefault(a => a.getId() == net.Id);
            if (agent == null)
                agent = new RobotAgent(net, this);
            List<double> obs =agent.getObserve();
            obs.Add(0.0);obs.Add(0.0);
            return obs;
        }

        (List<double>, double) IEnv.action(Network net, List<double> actions)
        {
            IAgent agent = this.agents.Values.ToList().FirstOrDefault(a => a.getId() == net.Id);
            agent.doAction(actions.ToArray());
            List<double> obs = agent.getObserve();
            obs.AddRange(actions);
            double reward = this.compute_reward(agent);
            return (obs, reward);
        }

        private void initOptimaTraces()
        {
            this.optimaTraces = new List<List<Point2D>>();
            List<Point2D> t1 = new List<Point2D>();
            t1.Add(new Point2D(this.start_point.X, this.start_point.Y));
            t1.Add(new Point2D(469,738));
            t1.Add(new Point2D(254, 738));
            t1.Add(new Point2D(254, 1109));
            t1.Add(new Point2D(379, 1142));
            t1.Add(new Point2D(this.goal_point.X,this.goal_point.Y));
            optimaTraces.Add(t1);

            List<Point2D> t2 = new List<Point2D>();
            t2.Add(new Point2D(this.start_point.X, this.start_point.Y));
            t2.Add(new Point2D(469, 738));
            t2.Add(new Point2D(683, 738));
            t2.Add(new Point2D(683, 1129));
            t2.Add(new Point2D(568, 1129));
            t2.Add(new Point2D(this.goal_point.X, this.goal_point.Y));
            optimaTraces.Add(t2);
        }
        public double compute_reward(IAgent agent)
        {
            //计算离Agent的前一个点最近的最优点
            int optima_index=-1,posindex=-1;
            double min = double.MaxValue ;
            for(int i=0;i<optimaTraces.Count;i++)
            {
                for(int j=0;j<optimaTraces[i].Count;j++)
                {
                    double dis = EngineUtilities.euclideanDistance(agent.OldLocation, optimaTraces[i][j]);
                    if (dis < min)
                    {
                        optima_index = i;
                        posindex = j;
                        min = dis;
                    }
                }
            }
            if (posindex >= optimaTraces[optima_index].Count - 1) return 1.0;
            Point2D correctPoint = optimaTraces[optima_index][posindex + 1];

            double r = EngineUtilities.euclideanDistance(agent.Location, correctPoint) /
                       EngineUtilities.euclideanDistance(correctPoint, optimaTraces[optima_index][posindex]);
            return r >= 1 ? 1.0 : r;

        }

        #endregion

        #region 管理Agent
        /// <summary>
        /// 添加新的Agent
        /// </summary>
        /// <param name="net"></param>
        public void updateAgent(Network net)
        {
            if(!this.agents.ContainsKey(net.Id))
            {
                RobotAgent agent = new RobotAgent(net,this);
                this.agents.TryAdd(net.Id, agent);
            }
        }
        /// <summary>
        /// 删除Agent
        /// </summary>
        /// <param name="net"></param>
        /// <returns></returns>
        public RobotAgent removeAgent(Network net)
        {
            RobotAgent agent = null;
            if (this.agents.ContainsKey(net.Id))
                this.agents.TryRemove(net.Id, out agent);
            return agent;
        }
        /// <summary>
        /// 清空agent
        /// </summary>
        public void clearAgent()
        {
            this.agents.Clear();
        }
        #endregion
    }
}
