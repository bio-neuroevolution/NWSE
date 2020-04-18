using NWSELib;
using NWSELib.common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using log4net;
using System.IO;
using NWSELib.net;

namespace NWSEExperiment.maze
{

    
    public class OptimaTraceFitness 
    {
        private Point2D goal_point;
        private List<Polyline> optimaTraces = new List<Polyline>();
        /// <summary>
        /// Returns an empty DiverseQualityMazeFitnses instance.
        /// </summary>
        public OptimaTraceFitness(List<Polyline> optimaTraces, Point2D goal_point)
        {
            this.optimaTraces = optimaTraces;
            this.goal_point = goal_point;
        }
        
        /// <summary>
        /// Calculates the fitness of an individual based on Distance to the goal. Not compatible with multiagent teams.
        /// </summary>
        public double calculate(List<Point2D> traces)
        {
            double end_distance = Session.GetConfiguration().evaluation.end_distance;
            if (traces.Count <= 0) return 0;
            //计算所有轨迹点与目标点的距离是否小于阈值
            double goal_reward = 0, max_goal_reward = 10.0;
            List<double> goaldis = traces.ConvertAll(t => t.manhattanDistance(goal_point));
            if (goaldis.Min() <= end_distance)
                goal_reward = max_goal_reward;

            //去掉冗余轨迹点
            for (int i = 1; i < traces.Count; i++)
            {
                if (traces[i].X == traces[i - 1].X && traces[i].Y == traces[i - 1].Y)
                {
                    traces.RemoveAt(i--);
                }
            }

            //与每一个最优轨迹比较,取适应度最高的
            double maxF = double.MinValue;
            for (int i = 0; i < optimaTraces.Count; i++)
            {
                double d = dtwDistance(traces, optimaTraces[i].Points);
                if(d > maxF)
                {
                    maxF = d;
                }
            }
            return goal_reward + maxF;
        }
        private double dtwDistance(List<Point2D> traces,List<Point2D> optimas,double max = 200)
        {
            double score= 0;
            List<double> distances = new List<double>();
            List<Point2D> temp = new List<Point2D>(traces);
            //寻找距离第i个最优点最近的点
            int tIndex = -1;
            for (int i=0;i<optimas.Count;i++)
            {
                List<double> dis = temp.ConvertAll(t => t.manhattanDistance(optimas[i]));
                tIndex = dis.argmin();
                if (dis[tIndex] > max)
                    break;
                
                score = i + 1;
                distances.Add(dis[tIndex]);
                if (tIndex == temp.Count) break;
                temp = temp.GetRange(tIndex + 1,temp.Count-tIndex-1);
                if (temp.Count <= 0) break;
            }

            return score + (max - distances.Average())/max;
            
            
        }
    }
}
