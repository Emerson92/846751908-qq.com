using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
namespace ECS.PathFinding
{
    [BurstCompile]
    public struct PathFindingECS : IJob
    {

        //直线权值
        private const int MOVE_STRAIGHT_COST = 10;

        //斜线权值
        private const int MOVE_DIAGONAL_COST = 14;


        public PathNode StartNode;

        public PathNode EndNode;

        //格子的X轴
        public int GridX;

        //格子的Y轴
        public int GridY;

        /// <summary>
        /// 格子大小s
        /// </summary>
        public int GridCell;

        /// <summary>
        /// 当前所有路径的节点信息
        /// </summary>
        public NativeArray<PathNode> PathNodeArray;

        public NativeList<PathNode> PathResult;

        public PathFindingECS(PathNode StartNode, PathNode EndNode,int GridX, int GridY, int GridCell, NativeArray<PathNode> PathNodeArray) {
            this.StartNode = StartNode;
            this.EndNode = EndNode;
            this.GridX = GridX;
            this.GridY = GridY;
            this.GridCell = GridCell;
            this.PathNodeArray = PathNodeArray;
            this.PathResult = new NativeList<PathNode>(Allocator.Persistent);
        }

        public void Execute()
        {

            ///获取计算的路径节点
            int2 Grid = new int2(GridX, GridY);
            int EndNodeIndex = CaculatePathNodeIndex(EndNode.X, EndNode.Y);
            //NativeArray<int2> neighborArray = new NativeArray<int2>(new int2[]{
            //     new int2(-1, 1),//LeftUp
            //     new int2(0, 1),//Up
            //     new int2(1, 1),//RightUp
            //     new int2(-1, 0),//Left
            //     new int2(1, 0),//Right
            //     new int2(-1, -1),//LeftDown
            //     new int2(0, -1),//Down
            //     new int2(1, -1),//RightDown
            //    }, Allocator.Temp);

            NativeArray<int2> neighborArray = new NativeArray<int2>(8, Allocator.Temp);
            neighborArray[0] = new int2(-1, 0); // Left
            neighborArray[1] = new int2(+1, 0); // Right
            neighborArray[2] = new int2(0, +1); // Up
            neighborArray[3] = new int2(0, -1); // Down
            neighborArray[4] = new int2(-1, -1); // Left Down
            neighborArray[5] = new int2(-1, +1); // Left Up
            neighborArray[6] = new int2(+1, -1); // Right Down
            neighborArray[7] = new int2(+1, +1); // Right Up

            if (PathNodeArray.Length <= 0)
            {
                //PathResult = -1;
                Debug.Log("PathNodeArray is null!");
                return;
            }

            ///init PathNodeArray
            for (int x = 0; x < Grid.x; x++)
            {
                for (int y = 0; y < Grid.y; y++)
                {
                    PathNode node = PathNodeArray[CaculatePathNodeIndex(x, y)];
                    node.Index = CaculatePathNodeIndex(x, y);
                    node.GGost = int.MaxValue;
                    node.HCost = CalculateDistanceCost(node, EndNode);
                    node.CameFromIndex = -1;
                    PathNodeArray[node.Index] = node;
                }
            }

            NativeList<int> openList = new NativeList<int>(Allocator.Temp);
            NativeList<int> closeList = new NativeList<int>(Allocator.Temp);

            int startIndex = CaculatePathNodeIndex(StartNode.X, StartNode.Y);
            int endIndex   = CaculatePathNodeIndex(EndNode.X,EndNode.Y);
            PathNode startNode = PathNodeArray[startIndex];
            startNode.GGost = 0;
            startNode.HCost = CalculateDistanceCost(startNode, EndNode);
            PathNodeArray[startIndex] = startNode;

            openList.Add(startIndex);

            while (openList.Length > 0)
            {
                int currentPathNodeIndex = GetLowestCostPathNode(openList, PathNodeArray);
                if (currentPathNodeIndex == EndNodeIndex)
                {
                   
                    break;
                }

                for (int i = 0; i < openList.Length; i++)
                {
                    if (openList[i] == currentPathNodeIndex)
                    {
                        openList.RemoveAtSwapBack(i);
                        break;
                    }
                }
                closeList.Add(currentPathNodeIndex);

                PathNode currentNode = PathNodeArray[currentPathNodeIndex];
                for (int i = 0; i < neighborArray.Length; i++)
                {
                    int2 neighborPos = new int2(currentNode.X + neighborArray[i].x, currentNode.Y + neighborArray[i].y);

                    ///判断是否在格子里
                    if (!IsInsideGrid(neighborPos, Grid)) continue; //不再格子中的也不再查找

                    int neighborIndex = CaculatePathNodeIndex(neighborPos.x, neighborPos.y);

                    if (closeList.Contains(neighborIndex)) continue;///已经遍历过的，不再查找

                    PathNode neighborNode = PathNodeArray[neighborIndex];

                    if (neighborNode.IsBlockWalk) continue;//不能行走，继续遍历

                    int tentiveCost = currentNode.GGost + CalculateDistanceCost(currentNode, neighborNode);
                    if (tentiveCost < neighborNode.GGost)
                    {
                        neighborNode.CameFromIndex = currentNode.Index;
                        neighborNode.GGost = tentiveCost;
                        neighborNode.HCost = CalculateDistanceCost(neighborNode, EndNode);
                        PathNodeArray[neighborIndex] = neighborNode;

                        ///把最小开销的邻居添加到OpenList中
                        if (!openList.Contains(neighborIndex))
                        {
                            openList.Add(neighborIndex);
                        }
                    }

                }

                
            }

            CaculateNodePath(PathNodeArray[endIndex], PathNodeArray, PathResult);
            openList.Dispose();
            closeList.Dispose();
            //neighborArray.Dispose();
        }

        private void CaculateNodePath(PathNode endNode, NativeArray<PathNode> pathNodeArray, NativeList<PathNode> pathResult)
        {
            int index = endNode.CameFromIndex;
            if (index == -1) return;
            pathResult.Add(endNode);
            while (index != -1)
            {
                PathNode tempNode = pathNodeArray[index];
                pathResult.Add(tempNode);
                index = tempNode.CameFromIndex;
            }
        }

        /// <summary>
        /// 是否在格子中间
        /// </summary>
        /// <param name="neighborPos"></param>
        /// <param name="grid"></param>
        /// <returns></returns>
        private bool IsInsideGrid(int2 neighborPos, int2 grid)
        {
            return (neighborPos.x >= 0 && neighborPos.y >= 0 && grid.x > neighborPos.x && grid.y > neighborPos.y);
        }

        private int CalculateDistanceCost(PathNode a, PathNode b)
        {
            int xDistance = Mathf.Abs(a.X - b.X);
            int yDistance = Mathf.Abs(a.Y - b.Y);
            int remaining = Mathf.Abs(xDistance - yDistance);
            return MOVE_DIAGONAL_COST * Mathf.Min(xDistance, yDistance) + MOVE_STRAIGHT_COST * remaining;
        }


        /// <summary>
        /// 计算得到最小开销权值节点
        /// </summary>
        /// <param name="openList"></param>
        /// <param name="pathNodeArray"></param>
        /// <returns></returns>
        private int GetLowestCostPathNode(NativeArray<int> openList, NativeArray<PathNode> pathNodeArray)
        {
            PathNode lowCostNode = pathNodeArray[openList[0]];
            for (int i = 1; i < openList.Length; i++)
            {
                PathNode testNode = pathNodeArray[openList[i]];
                if (lowCostNode.FCost > testNode.FCost)
                {
                    lowCostNode = testNode;
                }
            }
            return CaculatePathNodeIndex(lowCostNode.X, lowCostNode.Y);
        }

        public int CaculatePathNodeIndex(int x, int y)
        {
            return (x * GridCell + y );
        }


        public struct PathNode
        {

            public int Index;

            public int CameFromIndex;

            public int X;

            public int Y;

            public int HCost;

            public int GGost;

            public int FCost
            {
                get
                {
                    return HCost + GGost;
                }
            }

            public bool IsBlockWalk;

            public PathNode(int x, int y)
            {
                this.X = x;
                this.Y = y;
                this.Index = 0;
                this.CameFromIndex = 0;
                this.HCost = 0;
                this.GGost = 0;
                this.IsBlockWalk = false;
            }
            public override string ToString()
            {

                return X + "," + Y;
            }
        }

    }


}
