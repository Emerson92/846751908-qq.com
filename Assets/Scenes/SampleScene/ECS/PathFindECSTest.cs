
using ECS.PathFinding;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using static ECS.PathFinding.PathFindingECS;

public class PathFindECSTest : MonoBehaviour
{
    private PathFind.Grid.Grid<PathNode> grid;
    private Stopwatch watch;
    [Header("路线寻找次数")]
    public int FindPathExcuteNum = 1;

    private Vector3 startPos;

    NativeList<PathNode> pathNodeArray;

    // Start is called before the first frame update
    void Start()
    {
        pathNodeArray = new NativeList<PathNode>(Allocator.Persistent);
        this.grid = new PathFind.Grid.Grid<PathNode>(10, 10, 10, new Vector3(-50, -50, 100), (PathFind.Grid.Grid<PathNode> g, int x, int y) =>
        {
            PathNode node = new PathNode(x, y);
            pathNodeArray.Add(node);
            return node;
        });
        watch = new System.Diagnostics.Stopwatch();
        startPos = this.grid.GetGridWorldPosition(0, 0);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            //Debug.Log(Input.mousePosition);
            
            Vector3 endPos = Camera.main.ScreenToWorldPoint(Input.mousePosition + new Vector3(0, 0, 100 - Camera.main.transform.position.z));
            int endX;
            int endY;
            int startX;
            int startY;
            this.grid.GetGridXY(endPos, out endX, out endY);
            this.grid.GetGridXY(startPos, out startX, out startY);
            PathNode endNode = this.grid.GetValue(endX, endY);
            PathNode startNode = this.grid.GetValue(startX, startY);

            NativeArray<JobHandle> handlesList = new NativeArray<JobHandle>(FindPathExcuteNum, Allocator.Temp);
            PathFindingECS[] jobArray = new PathFindingECS[FindPathExcuteNum];
            NativeArray<PathNode>[] subArrayList = new NativeArray<PathNode>[FindPathExcuteNum];

            float startTime = Time.realtimeSinceStartup;
            for (int i = 0; i < FindPathExcuteNum; i++)
            {
                subArrayList[i] = pathNodeArray.ToArray(Allocator.TempJob);
                PathFindingECS job = new PathFindingECS(startNode, endNode, this.grid.Width, this.grid.Height, (int)this.grid.CellSize, subArrayList[i]);
                JobHandle handle = job.Schedule();
                jobArray[i] = job;
                handlesList[i] = handle;
            }
            JobHandle.CompleteAll(handlesList);
            float endtTime = Time.realtimeSinceStartup;
            UnityEngine.Debug.Log("耗时:" + (endtTime - startTime)*1000 +"ms");
            watch.Stop();

            if (jobArray[0].PathResult.Length <= 0)
            {
                UnityEngine.Debug.Log("没有查找到路线");
            }
            for (int i = 0; i < jobArray[0].PathResult.Length - 1; i++)
            {
                UnityEngine.Debug.DrawLine(this.grid.GetGridWorldPosition(jobArray[0].PathResult[i].X, jobArray[0].PathResult[i].Y) + new Vector3(this.grid.CellSize / 2, this.grid.CellSize / 2, 0), this.grid.GetGridWorldPosition(jobArray[0].PathResult[i + 1].X, jobArray[0].PathResult[i + 1].Y) + new Vector3(this.grid.CellSize / 2, this.grid.CellSize / 2, 0), Color.green, 5);
            }

            handlesList.Dispose();
            for (int j = 0; j < jobArray.Length; j++)
            {
                jobArray[j].PathResult.Dispose();
            }
            for (int j = 0; j < jobArray.Length; j++)
            {
                subArrayList[j].Dispose();
            }
        }

        if (Input.GetMouseButtonDown(1))
        {
            Vector3 endPos = Camera.main.ScreenToWorldPoint(Input.mousePosition + new Vector3(0, 0, 100 - Camera.main.transform.position.z));
            this.grid.SetBlockAble(endPos, "X", (p) =>
            {
                for (int i = 0; i < pathNodeArray.Length; i++)
                {
                    if (p.X == pathNodeArray[i].X && p.Y == pathNodeArray[i].Y)
                    {
                        PathNode node = pathNodeArray[i];
                        node.IsBlockWalk = true;
                        pathNodeArray[i] = node;
                    }
                }

            });
        }
        if (Input.GetMouseButtonDown(2))
        {
            startPos = Camera.main.ScreenToWorldPoint(Input.mousePosition + new Vector3(0, 0, 100 - Camera.main.transform.position.z));
        }
    }

    private void OnDestroy()
    {
        pathNodeArray.Dispose();
    }


}
