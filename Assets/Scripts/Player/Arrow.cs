using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Arrow : MonoBehaviour
{


    
    public Transform attackPoint;
    float damage;
    [SerializeField] LayerMask enemyLayer;
    Stat st;

    private void Start()
    {
        Destroy(gameObject,1.5f);
        st = GameObject.Find("Player").GetComponent<Stat>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {

        if (collision.gameObject.CompareTag("Hitable"))
        {

            if (collision.GetComponent<Stat>() == null)
            {
                return;
            }
            collision.GetComponent<Stat>().hp -= st.bowDamage;
            Destroy(gameObject);
        }

            


        
    }


}
