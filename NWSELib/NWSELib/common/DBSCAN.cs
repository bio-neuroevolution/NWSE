using System;
using System.Collections.Generic;
using System.Text;

namespace NWSELib.common
{
    public class DBSCANAlgorithm
    {
        private readonly Func<Vector, Vector, double> _metricFunc;


        public DBSCANAlgorithm(Func<Vector, Vector, double> metricFunc=null)
        {
            _metricFunc = metricFunc;
            if (_metricFunc == null)
                _metricFunc = (v1, v2) => v1.distance(v2);
;        }

        public class Feature
        {
            public bool IsVisited;
            public Vector sample;
            public int ClusterId;
            public int type;

            public Feature(Vector sample)
            {
                this.sample = sample;
                IsVisited = false;
                ClusterId = (int)ClusterIds.UNCLASSIFIED;
            }
        }

        public List<List<Vector>> ComputeClusterDbscan(List<Vector> samples, double epsilon, int minPts)
        {
            clusters = null;
            var features = samples.ConvertAll(x => new Feature(x)).ToArray();

            var tree = new KDTree.KDTree<DbscanPoint>(2);
            for (var i = 0; i < allPointsDbscan.Length; ++i)
            {
                tree.AddPoint(new double[] { allPointsDbscan[i].ClusterPoint.point.X, allPointsDbscan[i].ClusterPoint.point.Y }, allPointsDbscan[i]);
            }

            var C = 0;
            for (int i = 0; i < features.Length; i++)
            {
                var p = features[i];
                if (p.IsVisited)
                    continue;
                p.IsVisited = true;

                Feature[] neighborPts = null;
                RegionQuery(tree, p.ClusterPoint.point, epsilon, out neighborPts);
                if (neighborPts.Length < minPts)
                    p.ClusterId = (int)ClusterIds.NOISE;
                else
                {
                    C++;
                    ExpandCluster(tree, p, neighborPts, C, epsilon, minPts);
                }
            }
            clusters = new HashSet<ScanPoint[]>(
                allPointsDbscan
                    .Where(x => x.ClusterId > 0)
                    .GroupBy(x => x.ClusterId)
                    .Select(x => x.Select(y => y.ClusterPoint).ToArray())
                );

            return;
        }

        private void ExpandCluster(KDTree.KDTree<DbscanPoint> tree, DbscanPoint p, DbscanPoint[] neighborPts, int c, double epsilon, int minPts)
        {
            p.ClusterId = c;
            for (int i = 0; i < neighborPts.Length; i++)
            {
                var pn = neighborPts[i];
                if (!pn.IsVisited)
                {
                    pn.IsVisited = true;
                    DbscanPoint[] neighborPts2 = null;
                    RegionQuery(tree, pn.ClusterPoint.point, epsilon, out neighborPts2);
                    if (neighborPts2.Length >= minPts)
                    {
                        neighborPts = neighborPts.Union(neighborPts2).ToArray();
                    }
                }
                if (pn.ClusterId == (int)ClusterIds.UNCLASSIFIED)
                    pn.ClusterId = c;
            }
        }

        private void RegionQuery(KDTree.KDTree<DbscanPoint> tree, PointD p, double epsilon, out DbscanPoint[] neighborPts)
        {
            int totalCount = 0;
            var pIter = tree.NearestNeighbors(new double[] { p.X, p.Y }, 10, epsilon);
            while (pIter.MoveNext())
            {
                totalCount++;
            }
            neighborPts = new DbscanPoint[totalCount];
            int currCount = 0;
            pIter.Reset();
            while (pIter.MoveNext())
            {
                neighborPts[currCount] = pIter.Current;
                currCount++;
            }

            return;
        }
    }

    //Dbscan clustering identifiers
    public enum ClusterIds
    {
        UNCLASSIFIED = 0,
        NOISE = -1
    }

    //Point container for Dbscan clustering
    
}
