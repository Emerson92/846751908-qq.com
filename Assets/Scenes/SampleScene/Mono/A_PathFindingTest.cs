using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PathFinding.Test
{
    public class A_PathFindingTest : MonoBehaviour
    {
        Mono.PathFinding.PathFinding pathFinder;

        private Vector3 startPos;

        private float cellSize;

        System.Diagnostics.Stopwatch watch;

        [Header("路线寻找次数")]
        public int FindPathExcuteNum = 10;

        // Start is called before the first frame update
        void Start()
        {

            pathFinder = new Mono.PathFinding.PathFinding(10, 10);
            startPos = pathFinder.Grid.GetGridWorldPosition(0, 0);
            cellSize = pathFinder.Grid.CellSize;
            watch = new System.Diagnostics.Stopwatch();
        }

        // Update is called once per frame
        void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                //Debug.Log(Input.mousePosition);
              
                Vector3 endPos = Camera.main.ScreenToWorldPoint(Input.mousePosition + new Vector3(0, 0, 100 - Camera.main.transform.position.z));
                List<Mono.PathFinding.PathNode.PathNode> pathList = null;

                float startTime = Time.realtimeSinceStartup;
                for (int i = 0; i < FindPathExcuteNum;i++ ) { 
                    pathList = pathFinder.FindingPath(startPos, endPos);
                }
                float endTime = Time.realtimeSinceStartup;
                Debug.Log("耗时:"+ (endTime - startTime)*1000 + "ms");

                if (pathList == null)
                {
                    Debug.Log("没有查找到路线");
                    return;
                }
                for (int i = 0; i < pathList.Count-1; i++)
                {
                    Debug.DrawLine(pathFinder.Grid.GetGridWorldPosition(pathList[i].GridX, pathList[i].GridY)+ new Vector3(cellSize/2, cellSize/2,0),pathFinder.Grid.GetGridWorldPosition(pathList[i + 1].GridX, pathList[i + 1].GridY) + new Vector3(cellSize / 2, cellSize / 2, 0), Color.green, 5);
                }
            }

            if (Input.GetMouseButtonDown(1)) {
                Vector3 endPos = Camera.main.ScreenToWorldPoint(Input.mousePosition + new Vector3(0, 0, 100 - Camera.main.transform.position.z));
                pathFinder.Grid.SetBlockAble(endPos,"X",(p) => {
                    p.IsWalkeBlock = true;
                });
            }
            if (Input.GetMouseButtonDown(2)) {
                startPos = Camera.main.ScreenToWorldPoint(Input.mousePosition + new Vector3(0, 0, 100 - Camera.main.transform.position.z));
            }
        }


    }
}

