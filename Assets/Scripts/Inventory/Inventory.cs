using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    // Start is called before the first frame update
    public List<itemScript> itemList = new List<itemScript>(15);
    public static Inventory instance;

    private void Awake()
    {
        instance = this;

    }
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
    public void AddItem(itemScript item)
    {
        if(itemList.Count<15)
        {

        itemList.Add(item);
        }


    }
    public void RemoveItem(itemScript item)
    {
        itemList.Remove(item);
    }
}
