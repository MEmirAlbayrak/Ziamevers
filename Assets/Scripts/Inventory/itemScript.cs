using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class itemScript : MonoBehaviour
{
    public float damage,
        level,
        crit,
        speed;
    public Sprite icon;


    public enum Rarity
    {
        Mythic,
        Legendary,
        Epic,
        Rare,
        Common
    }

    public IEnumerator DestroySelf()
    {
        yield return new WaitForSeconds(0.1f);
        gameObject.SetActive(false);
    }
    private void Awake()
    {


    }

    void Start()
    {
        icon = GetComponent<SpriteRenderer>().sprite;
        giveRarity();
        giveStat();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void giveStat()
    {
        if (giveRarity() == Rarity.Common)
        {

            this.damage = Random.Range(5, 7);
            this.crit = 3;
            this.speed = 2;
            this.level = 1;
            Debug.Log("common");

        }
        if (giveRarity() == Rarity.Rare)
        {

            this.damage = Random.Range(10, 13);
            this.crit = 5;
            this.speed = 7;
            this.level = 2;
            Debug.Log("rare");



        }
        if (giveRarity() == Rarity.Epic)
        {

            this.damage = Random.Range(17, 20);
            this.crit = 8;
            this.speed = 4;
            this.level = 3;
            Debug.Log("epic");

        }
        if (giveRarity() == Rarity.Legendary)
        {

            this.damage = Random.Range(23, 27);
            this.crit = 10;
            this.speed = 5;
            this.level = 3;
            Debug.Log("lege");

        }
        if (giveRarity() == Rarity.Mythic)
        {

            this.damage = Random.Range(30, 35);
            this.crit = 15;
            this.speed = 6;
            this.level = 4;
            Debug.Log("mythic");

        }


    }

    public Rarity giveRarity()
    {

        int raritypicker = Random.Range(1, 16);
        switch (raritypicker)
        {



            case 1: return Rarity.Common;

            case 2: return Rarity.Common;

            case 3: return Rarity.Common;

            case 4: return Rarity.Common;

            case 5: return Rarity.Common;

            case 6: return Rarity.Rare;

            case 7: return Rarity.Rare;

            case 8: return Rarity.Rare;

            case 9: return Rarity.Rare;

            case 10: return Rarity.Epic;

            case 11: return Rarity.Epic;

            case 12: return Rarity.Epic;

            case 13: return Rarity.Legendary;

            case 14: return Rarity.Legendary;

            case 15: return Rarity.Mythic;



            default: return Rarity.Common;







        }
    }

}
