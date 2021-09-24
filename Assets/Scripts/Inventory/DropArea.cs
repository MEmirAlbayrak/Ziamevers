using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DropArea : MonoBehaviour
{
    public List<DropCon> DropCons = new List<DropCon>();
    public event Action<DraggableComponent> OnDropHandler;
    public bool Accepts(DraggableComponent draggable)
    {
        return DropCons.TrueForAll(cond => cond.Check(draggable));
    }
    public void Drop(DraggableComponent draggable)
    {
        OnDropHandler?.Invoke(draggable);
        draggable.transform.position = transform.position;
    }
}
