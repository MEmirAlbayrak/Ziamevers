using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EquibSlot : MonoBehaviour
{
    protected DropArea DropArea;
    
    
    protected virtual void Awake()
    {
        DropArea = GetComponent<DropArea>() ?? gameObject.AddComponent<DropArea>();
        DropArea.OnDropHandler += OnItemDrop;
    }

    private void OnItemDrop(DraggableComponent draggable)
    {
        draggable.transform.position = transform.position;
        

        
    }
}
