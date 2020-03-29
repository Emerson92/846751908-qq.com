using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace PathFinding.Test
{
    public class GridTestScript : MonoBehaviour
    {

        private PathFind.Grid.Grid<int> grid;

        // Start is called before the first frame update
        void Start()
        {
            grid = new PathFind.Grid.Grid<int>(4, 4, 5, new Vector3(-10f, -10f, 10),null);
        }

        // Update is called once per frame
        void Update()
        {
            if (Input.GetMouseButton(0))
            {
                ///采用物理方式
                /***
                RaycastHit hit;
                Ray screenRay = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(screenRay, out hit)) {
                    grid.SetValue(hit.point,56);
                }
                ***/
                //Debug.Log(Input.mousePosition);
                grid.SetValue(Camera.main.ScreenToWorldPoint(Input.mousePosition + new Vector3(0, 0, 10 - Camera.main.transform.position.z)), 56);
            }
        }
    }

}
