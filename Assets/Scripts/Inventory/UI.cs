using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI : MonoBehaviour
{
    // Start is called before the first frame update
    Inventory inventory;
    public Transform itemsParent;
    public InventorySlot[] slots;
    PlayerController pl;
    public ItemStat[] inventoryImages;
    void Start()
    {
        inventory = Inventory.instance;
        slots = itemsParent.GetComponentsInChildren<InventorySlot>();
        pl = GameObject.Find("Player").GetComponent<PlayerController>();

    }

    // Update is called once per frame
    void Update()
    {



        if (pl.itemGrabbed)
        {
            for (int i = 0; i < slots.Length; i++)
            {
                if (inventory.itemList.Count > 0)
                {
                    Debug.Log(":DD0");
                    slots[i].Add(inventory.itemList[i]);
                }
                if (inventory.itemList.Count > 0)
                {

                    inventoryImages[i].damage = Inventory.instance.itemList[i].damage;
                    inventoryImages[i].level = Inventory.instance.itemList[i].level;
                    inventoryImages[i].criticalChance = Inventory.instance.itemList[i].crit;
                    inventoryImages[i].speed = Inventory.instance.itemList[i].speed;
                }
            }

        }



    }
}
