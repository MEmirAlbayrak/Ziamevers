using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponSlot : EquibSlot
{
    protected override void Awake()
    {
        base.Awake();
        DropArea.DropCons.Add(new ÝsWeapon());
    }
}
