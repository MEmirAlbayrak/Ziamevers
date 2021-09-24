using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileSpawner : MonoBehaviour
{
    DungeonManager dungMan;
   public  GameObject wall;
    public GameObject floor;
    

    private void Awake()
    {
        dungMan = FindObjectOfType<DungeonManager>();
        floor = Instantiate(dungMan.floorPrefab, transform.position, Quaternion.identity) as GameObject;
        floor.name = dungMan.floorPrefab.name;
        floor.transform.SetParent(dungMan.transform);
        //SHOWS  MAX X POSITION AND MAX Y POSITION//
        if(transform.position.x > dungMan.maxX)
        {
            dungMan.maxX = transform.position.x;
        }
        if(transform.position.x < dungMan.minX)
        {
            dungMan.minX = transform.position.x;
        }
        if(transform.position.y > dungMan.maxY)
        {
            dungMan.maxY = transform.position.y;
        }
        if (transform.position.y < dungMan.minY)
        {
            dungMan.minY = transform.position.y;
        }
        //SHOWS  MAX X POSITION AND MAX Y POSITION//

    }
    void Start()
    {
       
                LayerMask envMask = LayerMask.GetMask("Floor", "Wall");
       
        for (int i = -1; i <= 1; i++)
        {

            for (int j = -1; j <= 1; j++)
            {
                Vector2 targetPos = new Vector2(transform.position.x + i, transform.position.y + j);

                Collider2D hit = Physics2D.OverlapBox(targetPos, Vector2.one*0.8f , 0, envMask);

                //IF COLLIDER IS NOT TOUCHING ANYTHING INSTANTIATE WALL//
                if (!hit)
                {
                    wall = Instantiate(dungMan.wallPrefab, targetPos, Quaternion.identity) as GameObject;
                    wall.name = dungMan.wallPrefab.name;
                    wall.transform.SetParent(dungMan.transform);
                }
                //IF COLLIDER IS NOT TOUCHING ANYTHING INSTANTIATE WALL//
                Debug.Log(hit);

               
            }
        }
        Destroy(gameObject); 

    }
    //DRAWING GIZMOS FOR TILE WE SPAWNED//
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.white;
        Gizmos.DrawCube(transform.position, Vector3.one * 1);
    }
    //DRAWING GIZMOS FOR TILE WE SPAWNED//




}
