using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemSpawn : MonoBehaviour
{
    // Start is called before the first frame update
    public  GameObject swordPrefab, helmetPrefab;
    public int random;

    Inventory inventory;


    void Start()
    {
        Instantiate(swordPrefab, new Vector3(0f, 3f, 0f), Quaternion.identity);
        Instantiate(helmetPrefab, new Vector3(0f, -3f, 0f), Quaternion.identity);





    }


    // Update is called once per frame
    void Update()
    {

    }
}
