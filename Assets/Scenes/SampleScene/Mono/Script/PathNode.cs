using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Mono.PathFinding.PathNode
{

    public class PathNode
    {

        private bool isWalkeBlock;

        private PathNode cameFromNode;

        private PathFind.Grid.Grid<PathNode> grid;

        public int GCost { set; get; }

        public int HCost { set; get; }

        public int FCost
        {
            get
            {
                return  HCost + GCost;
            }
        }

        public int GridX { set; get; }

        public int GridY { set; get; }

        public bool IsWalkeBlock { set; get; }

        public PathNode CameFromNode { set; get; }

        public PathNode(PathFind.Grid.Grid<PathNode> grid, int gridX, int gridY, int gCost, int hCost)
        {
            this.grid  = grid;
            this.GCost = gCost;
            this.HCost = hCost;
            this.GridX = gridX;
            this.GridY = gridY;
        }

        public PathNode(PathFind.Grid.Grid<PathNode> grid, int gridX, int gridY)
        {
            this.grid  = grid;
            this.GridX = gridX;
            this.GridY = gridY;
        }

        public override string ToString() {

            return GridX + "," + GridY;
        }
    }
}
