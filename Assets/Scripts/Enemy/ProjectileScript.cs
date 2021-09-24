using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileScript : MonoBehaviour
{
    public int speed;
    Rigidbody2D rb;
    public Transform PlayerPos;
    public Stat playerStat;
    Vector2 moveDirection;
    public int dmg;


    void Start()
    {
        dmg = 1; // Test Damage
        PlayerPos = GameObject.Find("Player").GetComponent<Transform>();
        rb = GetComponent<Rigidbody2D>();
        moveDirection = (PlayerPos.transform.position - transform.position).normalized * speed;
        rb.velocity = new Vector2(moveDirection.x, moveDirection.y);
        playerStat = GameObject.Find("Player").GetComponent<Stat>();
        Destroy(gameObject, 1);

    }


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            playerStat.hp -= dmg *(100 / 100 + playerStat.armor);
           
            Destroy(gameObject);
        }
    }
}
