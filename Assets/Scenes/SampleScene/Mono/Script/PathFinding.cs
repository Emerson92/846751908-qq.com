using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mono.PathFinding.PathNode;
using System;

namespace Mono.PathFinding
{


    public class PathFinding
    {
        //直线权值
        public const int MOVE_STRAIGHT_COST = 10;

        //斜线权值
        public const int MOVE_DIAGONAL_COST = 14;


        private PathFind.Grid.Grid<PathNode.PathNode> grid;

        public PathFind.Grid.Grid<PathNode.PathNode> Grid
        {
            get { return grid; }
        }

        public PathFinding(int width, int height)
        {
            this.grid = new PathFind.Grid.Grid<PathNode.PathNode>(width, height, 10, new Vector3(-50, -50, 100), (PathFind.Grid.Grid<PathNode.PathNode> g, int x, int y) => new PathNode.PathNode(g, x, y));
        }

        public List<PathNode.PathNode> FindingPath(Vector3 startPos, Vector3 endPos)
        {
            int startX;
            int startY;
            int endX;
            int endY;
            grid.GetGridXY(startPos, out startX, out startY);
            grid.GetGridXY(endPos, out endX, out endY);
            //Debug.Log("起始格子:"+ startX+","+ startY);
            //Debug.Log("终止格子:" + endX + "," + endY);
            return FindingPath(grid.GetValue(startX, startY), grid.GetValue(endX, endY));
        }

        public List<PathNode.PathNode> FindingPath(PathNode.PathNode startNode, PathNode.PathNode endNode)
        {
            if (startNode == null || endNode == null)
            { 
                Debug.Log("没有找到对应的起点与终点，查询失败!");
                return null;
            }

            List<PathNode.PathNode> openList = new List<PathNode.PathNode>() { startNode };
            List<PathNode.PathNode> closeList = new List<PathNode.PathNode>();

            //初始化所有权值
            for (int x = 0; x < grid.Width; x++)
            {
                for (int y = 0; y < grid.Height; y++)
                {
                    PathNode.PathNode node = grid.GetValue(x, y);
                    node.GCost = 99999999;
                    node.HCost = 0;
                    node.CameFromNode = null;
                }
            }

            startNode.GCost = 0;
            startNode.HCost = CalculateDistanceCost(startNode, endNode);

            while (openList.Count > 0)
            {
                PathNode.PathNode currentNode = GetLowestFCostNode(openList);


                if (currentNode == endNode)
                {
                    return CalculatePath(endNode);
                }

                openList.Remove(currentNode);
                closeList.Add(currentNode);

                List<PathNode.PathNode> neighborList = GetNeighborPathNode(currentNode);
                for (int index = 0; index < neighborList.Count; index++)
                {
                    if (closeList.Contains(neighborList[index])) continue;
                    if (neighborList[index].IsWalkeBlock)
                    {
                        closeList.Add(neighborList[index]);
                        continue;
                    }

                    //得到neighborList[index]到currentNode到起始点的GCost
                    int tempGCost = currentNode.GCost + CalculateDistanceCost(currentNode, neighborList[index]);
                    if (tempGCost < neighborList[index].GCost)
                    {
                        neighborList[index].CameFromNode = currentNode;
                        neighborList[index].GCost = tempGCost;
                        neighborList[index].HCost = CalculateDistanceCost(neighborList[index], endNode);
                        if (!openList.Contains(neighborList[index]))
                            openList.Add(neighborList[index]);
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// 获取当前待检测节点的最小权值
        /// </summary>
        /// <param name="openList"></param>
        /// <returns></returns>
        private PathNode.PathNode GetLowestFCostNode(List<PathNode.PathNode> openList)
        {
            PathNode.PathNode lowestFCostNode = openList[0];
            for (int i = 0; i < openList.Count; i++)
            {
                if (lowestFCostNode.FCost > openList[i].FCost)
                    lowestFCostNode = openList[i];
            }
            return lowestFCostNode;
        }

        private List<PathNode.PathNode> GetNeighborPathNode(PathNode.PathNode currentNode)
        {
            List<PathNode.PathNode> neighbourList = new List<PathNode.PathNode>();
            if (currentNode.GridX - 1 >= 0)
            {
                // Left
                neighbourList.Add(GetNode(currentNode.GridX - 1, currentNode.GridY));
                // Left Down
                if (currentNode.GridY - 1 >= 0) neighbourList.Add(GetNode(currentNode.GridX - 1, currentNode.GridY - 1));
                // Left Up
                if (currentNode.GridY + 1 < grid.Height) neighbourList.Add(GetNode(currentNode.GridX - 1, currentNode.GridY + 1));
            }
            if (currentNode.GridX + 1 < grid.Width)
            {
                // Right
                neighbourList.Add(GetNode(currentNode.GridX + 1, currentNode.GridY));
                // Right Down
                if (currentNode.GridY - 1 >= 0) neighbourList.Add(GetNode(currentNode.GridX + 1, currentNode.GridY - 1));
                // Right Up
                if (currentNode.GridY + 1 < grid.Height) neighbourList.Add(GetNode(currentNode.GridX + 1, currentNode.GridY + 1));
            }
            // Down
            if (currentNode.GridY - 1 >= 0) neighbourList.Add(GetNode(currentNode.GridX, currentNode.GridY - 1));
            // Up
            if (currentNode.GridY + 1 < grid.Height) neighbourList.Add(GetNode(currentNode.GridX, currentNode.GridY + 1));

            return neighbourList;
        }


        private int CalculateDistanceCost(PathNode.PathNode a, PathNode.PathNode b)
        {
            int xDistance = Mathf.Abs(a.GridX - b.GridX);
            int yDistance = Mathf.Abs(a.GridY - b.GridY);
            int remaining = Mathf.Abs(xDistance - yDistance);
            return MOVE_DIAGONAL_COST * Mathf.Min(xDistance, yDistance) + MOVE_STRAIGHT_COST * remaining;
        }

        /// <summary>
        /// 统计路径集合
        /// </summary>
        /// <param name="endNode"></param>
        /// <returns></returns>
        private List<PathNode.PathNode> CalculatePath(PathNode.PathNode endNode)
        {
            List<PathNode.PathNode> path = new List<PathNode.PathNode>();
            path.Add(endNode);
            PathNode.PathNode currentNode = endNode;
            while (currentNode.CameFromNode != null)
            {
                path.Add(currentNode.CameFromNode);
                currentNode = currentNode.CameFromNode;
            }
            path.Reverse();
            return path;
        }

        public PathNode.PathNode GetNode(int x, int y)
        {
            return grid.GetValue(x, y);
        }
    }
}
