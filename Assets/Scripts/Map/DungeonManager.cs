using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;
public enum DungeonType { Caverns, Rooms, Hall }

public class DungeonManager : MonoBehaviour
{
    public float minX, maxX, minY, maxY;
    public GameObject floorPrefab, wallPrefab, tilespawnerPrefab, ExitPrefab;
    [SerializeField, Range(50, 5000)]
    public int totalfloorCount;
    List<Vector3> floorList = new List<Vector3>();
    public GameObject[] itemListPrefab, EnemyListPrefab;
    Collider2D floorHit;
    public LayerMask FloorMask, WallMask;
    [SerializeField, Range(0, 101)]
    int itemSpawnPercentage;
    [SerializeField, Range(0, 101)]
    int EnemySpawnPercentage;
    Collider2D TopHit;
    Collider2D RightHit;
    Collider2D DownHit;
    Collider2D LeftHit;
    public DungeonType dungeonType;
    public AstarPath pathfinder;
    GridGraph gg;
    public TileSpawner ts;
    SpriteRenderer wallsprite;
    public Sprite bottomRightWall;



    void Start()
    {
        pathfinder = GameObject.Find("A*").GetComponent<AstarPath>();

        
        FloorMask = LayerMask.GetMask("Floor");

        WallMask = LayerMask.GetMask("Wall");
        // IF WE CHOOSE CAVERN IT SPAWNS RANDOM CAVERNS (AREAS) //    
        switch (dungeonType)
        {
            case DungeonType.Caverns:
                DrawTile();
                break;
            case DungeonType.Rooms:
                DrawRooms();
                break;
        }
        //IF WE CHOOSE ROOM IT SPAWNS RANDOM ROOMS AND HALLWAYS//
    }
    Vector3 RandomDirection()
    {
        //CHOOSE A RANDOM DIRECTION//
        switch (Random.Range(1, 5))
        {
            case 1: return Vector3.up;
            case 2: return Vector3.right;
            case 3: return Vector3.left;
            case 4: return Vector3.down;

        }
        //CHOOSE A RANDOM DIRECTION//
        return Vector3.zero;

    }



    public void DrawTile()
    {
        Vector3 curPos = Vector3.zero;
        floorList.Add(curPos);
        // IF THE GIVEN NUMBER IS BIGGER THAN FLOOR COUNT, CONTINUE SPAWNING FLOORS WITH RANDOM DIRECTIONS//
        while (floorList.Count < totalfloorCount)
        {
            curPos += RandomDirection();


            if (!floorList.Contains(curPos))
            {

                floorList.Add(curPos);

            }
        }
        // IF THE GIVEN NUMBER IS BIGGER THAN FLOOR COUNT, CONTINUE SPAWNING FLOORS WITH RANDOM DIRECTIONS//


        StartCoroutine(Delayer());

    }
    public void DrawRooms()
    {

        Vector3 curPos = Vector3.zero;
        floorList.Add(curPos);

        //IF THE GIVEN NUMBER IS BIGGER THAN FLOOR COUNT , CONTINUE TO SPAWN HALLWAYS WITH GIVEN  LENGTH TO WALK//
        while (floorList.Count < totalfloorCount)
        {
            Vector3 walkDirection = RandomDirection();
            int walkLength = Random.Range(9, 18);
            Collider2D floorleftHit = Physics2D.OverlapBox(new Vector2(-1, 0), Vector2.one * 0.8f, 0, WallMask);
            Collider2D floorrightHit = Physics2D.OverlapBox(new Vector2(1, 0), Vector2.one * 0.8f, 0, WallMask);

            for (int i = 0; i < walkLength; i++)
            {

                if (!floorList.Contains(curPos + walkDirection))
                {
                    floorList.Add(curPos + walkDirection);
                }
                curPos += walkDirection;
                if (walkDirection == Vector3.up)
                {
                    floorList.Add(curPos + walkDirection + Vector3.right);
                }
                if (walkDirection == Vector3.right)
                {
                    floorList.Add(curPos + walkDirection + Vector3.up);
                }
                if (walkDirection == Vector3.left)
                {
                    floorList.Add(curPos + walkDirection + Vector3.up);
                }
                if (walkDirection == Vector3.down)
                {
                    floorList.Add(curPos + walkDirection + Vector3.right);
                }


            }

            //IF THE GIVEN NUMBER IS BIGGER THAN FLOOR COUNT , CONTINUE TO SPAWN HALLWAYS WITH GIVEN  LENGTH TO WALK//

            //SPAWN ROOMS WITH GIVEN WIDTH AND HEIGHT//

            int width = Random.Range(1, 5);
            int height = Random.Range(1, 5);
            for (int w = -width; w <= width; w++)
            {
                for (int h = -height; h <= height; h++)
                {
                    Vector3 offset = new Vector3(w, h, 0);
                    if (!floorList.Contains(curPos + offset))

                    {

                        floorList.Add(curPos + offset);

                    }
                }
            }
            //SPAWN ROOMS WITH GIVEN WIDTH AND HEIGHT//

        }



        StartCoroutine(Delayer());

    }
  

    IEnumerator Delayer()
    {

        //INSTANTIATE TILE SPAWNER//
        for (int i = 0; i < floorList.Count; i++)
        {
            GameObject goTileSpawner = Instantiate(tilespawnerPrefab, floorList[i], Quaternion.identity) as GameObject;
            goTileSpawner.name = tilespawnerPrefab.name;
            goTileSpawner.transform.SetParent(transform);
        }
        //INSTANTIATE TILE SPAWNER//

        //WAIT UNTILL TILESPAWNERS ARE GONE AND MAP IS GENERATED//
        while (FindObjectsOfType<TileSpawner>().Length > 0)
        {
            yield return null;
        }
        //THIS PART IS FOR OPTIMIZATION//
        //WAIT UNTILL TILESPAWNERS ARE GONE AND MAP IS GENERATED//



        gg = AstarPath.active.data.gridGraph;
        gg.center = new Vector3((maxX + minX) / 2, (maxY + minY) / 2, 0);

        /*
        gg.width = System.Math.Abs((int)minX) + System.Math.Abs((int)maxX);
        gg.depth = System.Math.Abs((int)minY)+ System.Math.Abs((int)maxY);
        */

        pathfinder.Scan();
        

        int count = 0;

        //THIS IS FOR CALCULATING THE WALLS AND THEIR SIDES//
        for (int x = (int)minX - 2; x <= (int)maxX + 2; x++)
        {
            for (int y = (int)minY - 2; y <= (int)maxY + 2; y++)
            {
                floorHit = Physics2D.OverlapBox(new Vector2(x, y), Vector2.one * 0.8f, 0, FloorMask);
                if (floorHit)
                {

                    if (!Vector2.Equals(floorHit.transform.position, floorList[floorList.Count - 1]))
                    {
                        TopHit = Physics2D.OverlapBox(new Vector2(x, y + 1), Vector2.one * 0.8f, 0, WallMask);
                        RightHit = Physics2D.OverlapBox(new Vector2(x + 1, y), Vector2.one * 0.8f, 0, WallMask);
                        LeftHit = Physics2D.OverlapBox(new Vector2(x - 1, y), Vector2.one * 0.8f, 0, WallMask);
                        DownHit = Physics2D.OverlapBox(new Vector2(x, y - 1), Vector2.one * 0.8f, 0, WallMask);

                        //ınstantiateItems(TopHit, RightHit, DownHit, LeftHit);
                        InstantiateEnemies(TopHit, RightHit, DownHit, LeftHit);
                         


                    }
                }
            }
        }
        ////THIS IS FOR CALCULATING THE WALLS AND THEIR SIDES//

        ///INSTANTIATE DOOR TO THE LAST FLOOR SPAWNED//
        InstantiateDoorway();
        ///INSTANTIATE DOOR TO THE LAST FLOOR SPAWNED//


        
    }

    /*void InstantiateItems(Collider2D TopHit, Collider2D RightHit, Collider2D DownHit, Collider2D LeftHit)
    {   //INSTANTIATE ITEMS AND ENEMIES WITH GIVEN PERCENTAGE TO OPEN SIDES OF WALLS//
        if (TopHit || LeftHit || RightHit || DownHit && !(TopHit && DownHit) && !(LeftHit && RightHit))
        {
            int roll = Random.Range(0, 101);
            if (roll <= itemSpawnPercentage)
            {

                int randomItem = Random.Range(0, itemListPrefab.Length);
                GameObject goItem = Instantiate(itemListPrefab[randomItem], floorHit.transform.position, Quaternion.identity) as GameObject;
                goItem.name = itemListPrefab[randomItem].name;
                goItem.transform.SetParent(floorHit.transform);
            }
        }
        //INSTANTIATE ITEMS AND ENEMIES WITH GIVEN PERCENTAGE TO OPEN SIDES OF WALLS//
    }*/
    void InstantiateDoorway()
    {
        Vector3 doorPos = floorList[floorList.Count - 1];
        GameObject door = Instantiate(ExitPrefab, doorPos, Quaternion.identity) as GameObject;
        door.name = ExitPrefab.name;
        door.transform.SetParent(transform);
    }



    void InstantiateEnemies(Collider2D TopHit, Collider2D RightHit, Collider2D DownHit, Collider2D LeftHit)
    {
        if (!TopHit && !RightHit && !LeftHit && !DownHit)
        {

            int roll = Random.Range(0, 101);
            if (roll <= EnemySpawnPercentage)
            {


                int randomEnemy = Random.Range(0, EnemyListPrefab.Length);
                GameObject goEnemy = Instantiate(EnemyListPrefab[randomEnemy], floorHit.transform.position, Quaternion.identity) as GameObject;
                goEnemy.name = EnemyListPrefab[randomEnemy].name;
                goEnemy.transform.SetParent(floorHit.transform);
            }
        }
    }


}
