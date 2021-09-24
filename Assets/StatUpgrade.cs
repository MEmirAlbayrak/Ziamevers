using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class StatUpgrade : MonoBehaviour
{
    // Start is called before the first frame update
    public Stat playerStat;
    public PlayerController pl;
    public TextMeshProUGUI UpgradePoint;
    void Start()
    {
        playerStat = GameObject.Find("Player").GetComponent<Stat>();
        pl = GameObject.Find("Player").GetComponent<PlayerController>();
    }

    // Update is called once per frame
    void Update()
    {
        UpgradePoint.text =" Upgrade Points "  + playerStat.upgradePoints;
    }
    public void HealthUpgrade()
    { if(playerStat.upgradePoints>0)
        {
            pl.Hp_Slider.maxValue += 10;
            playerStat.upgradePoints--;

        }
    }
    public void DamageUpgrade()
    {
        if (playerStat.upgradePoints > 0)
        {
            playerStat.damage+=20;
            playerStat.upgradePoints--;
            Debug.Log("wqeqweqw");

        }
    }
    
    public void ArmorUpgrade()
    {
        if (playerStat.upgradePoints > 0)
        {
            playerStat.armor += 10;
            playerStat.upgradePoints--;

        }
    }
    public void HealthRegenUpgrade()
    {
        if (playerStat.upgradePoints > 0)
        {
            playerStat.hpRegenerationPerSecond += 4f;
            playerStat.upgradePoints--;

        }
    }
    public void DodgeUpgrade()
    {
        if (playerStat.upgradePoints > 0)
        {
            playerStat.hpRegenerationPerSecond += 4f;
            playerStat.upgradePoints--;

        }

    }
    public void criticalUpgrade()
    {
        if (playerStat.upgradePoints > 0)
        {
            playerStat.criticalChance += 25f;
            playerStat.upgradePoints--;

        }
    }

}
