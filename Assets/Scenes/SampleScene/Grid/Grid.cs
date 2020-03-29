using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace PathFind.Grid
{
    /// <summary>
    /// 网格
    /// </summary>
    public class Grid<T>
    {

        public event EventHandler<OnGridObjectChangedEventArgs> OnGridObjectChanged;
        public class OnGridObjectChangedEventArgs : EventArgs
        {
            public int x;
            public int y;
        }

        /// <summary>
        /// 每个网格大小
        /// </summary>
        public float CellSize;

        /// <summary>
        /// 网格起点
        /// </summary>
        private Vector3 originalPos;

        private T[,] gridArray;

        private TextMesh[,] debugTexhMeshArray;

        public int Width { set; get; }

        public int Height { set; get; }

        public Grid(int width, int height, float cellSize, Vector3 originalPos, Func<Grid<T>, int, int, T> createGridObject)
        {
            this.Width = width;
            this.Height = height;
            this.originalPos = originalPos;
            gridArray = new T[width, height];
            debugTexhMeshArray = new TextMesh[width, height];
            this.CellSize = cellSize;

            for (int x = 0; x < gridArray.GetLength(0); x++)
            {
                for (int y = 0; y < gridArray.GetLength(1); y++)
                {
                    gridArray[x, y] = createGridObject(this, x, y);
                }
            }

            ///调试开关
            bool IsDebugVisual = true;

            if (IsDebugVisual)
            {
                ////开始创造网格
                for (int x = 0; x < width; x++)
                {
                    for (int y = 0; y < height; y++)
                    {
                        debugTexhMeshArray[x, y] = Until.CreatTextMesh(gridArray[x, y].ToString(), null, GetGridWorldPosition(x, y) + new Vector3(cellSize, cellSize) / 2, 30, Color.white);
                        Debug.DrawLine(GetGridWorldPosition(x, y), GetGridWorldPosition(x, y + 1), Color.white, 1000f);
                        Debug.DrawLine(GetGridWorldPosition(x, y), GetGridWorldPosition(x + 1, y), Color.white, 1000f);
                    }
                }
                Debug.DrawLine(GetGridWorldPosition(0, height), GetGridWorldPosition(width, height), Color.white, 1000f);
                Debug.DrawLine(GetGridWorldPosition(width, 0), GetGridWorldPosition(width, height), Color.white, 1000f);
                OnGridObjectChanged += (object sender, OnGridObjectChangedEventArgs eventArgs) =>
                {
                    debugTexhMeshArray[eventArgs.x, eventArgs.y].text = gridArray[eventArgs.x, eventArgs.y]?.ToString();
                };

            }
        }

        /// <summary>
        /// 获取当前格子的位置
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public Vector3 GetGridWorldPosition(int x, int y)
        {
            return new Vector3(x, y) * CellSize + originalPos;
        }

        /// <summary>
        /// 设置格子的值
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public void SetValue(int x, int y, T value)
        {
            if (x >= 0 && y >= 0 && x < Width && y < Height)
            {
                gridArray[x, y] = value;
                debugTexhMeshArray[x, y].text = value.ToString();
                if (OnGridObjectChanged != null) OnGridObjectChanged(this, new OnGridObjectChangedEventArgs { x = x, y = y });
            }
        }

        public void SetValue(Vector3 pos, T value)
        {
            int x;
            int y;
            GetGridXY(pos, out x, out y);
            if (x >= 0 && y >= 0)
            {
                SetValue(x, y, value);
            }
        }

        /// <summary>
        /// 获取格子的值
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public T GetValue(int x, int y)
        {
            if (x >= 0 && y >= 0 && x < Width && y < Height)
                return gridArray[x, y];
            else
                return default(T);
        }

        public T GetValue(Vector3 pos) {
            int x;
            int y;
            GetGridXY(pos, out x, out y);
            if (x >= 0 && y >= 0)
            {
                return gridArray[x, y];
            }
            else
                return default(T);
        }

        //
        public void SetBlockAble(Vector3 pos,string txt,Action<T> action) {
            int x;
            int y;
            GetGridXY(pos, out x, out y);
            if (x >= 0 && y >= 0 && x < Width && y < Height)
            {
                debugTexhMeshArray[x, y].text = txt ?? (debugTexhMeshArray[x, y].text = gridArray[x, y].ToString());
                action(gridArray[x, y]);
            }
        }


        /// <summary>
        /// 根据当前选中位置，获取选中的格子位置信息
        /// </summary>
        /// <param name="worldPosition"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public void GetGridXY(Vector3 worldPosition, out int x, out int y)
        {
            x = Mathf.FloorToInt((worldPosition.x - originalPos.x) / CellSize);
            y = Mathf.FloorToInt((worldPosition.y - originalPos.y) / CellSize);
        }

        public void TriggerGridValueChanged(int x, int y)
        {
            if (OnGridObjectChanged != null) OnGridObjectChanged(this, new OnGridObjectChangedEventArgs { x = x, y = y });
        }
    }

}
