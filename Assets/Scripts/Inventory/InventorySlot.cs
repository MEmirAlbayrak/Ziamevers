using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventorySlot : MonoBehaviour
{
    // Start is called before the first frame update
    itemScript item;
    public Image icon;

    public void Add(itemScript newItem)
    {
        item = newItem;
        icon.sprite = item.icon;
        icon.enabled = true;

    }
    public void ClearSlot()
    {
        item = null;
        icon.sprite = null;
    }
}
