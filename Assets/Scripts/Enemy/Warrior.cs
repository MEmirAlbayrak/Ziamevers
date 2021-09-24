using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Warrior : MonoBehaviour
{
    PathFind pf;
    Transform plyr;
    public bool doshoot;
    [SerializeField] bool shootOneTime, iswalking, beIdle, oneTime, oneTime2, canAttack,  canMove;
    public string runAnim, attackAnim, idleAnim, dieAnim, curState;
    Animator anim;
    public Transform attackPoint;

    Collider2D[] hitEnemy;
    [SerializeField] LayerMask enemyLayer;
    [SerializeField] float distance;

    Stat st;
    Stat plyStat;

    public float level,
    hp,
    dtcPlayerRange, //dtc = Detection 
    speed,
    damage,
    attackRange,
    attackSpeed;
    public GameObject swordPrefab, helmetPrefab;


    void Start()
    {
        st = GetComponent<Stat>();
        plyStat = GameObject.Find("Player").GetComponent<Stat>();
        canMove = true;
        SetStat();

        oneTime2 = false;
        pf = GetComponent<PathFind>();
        plyr = GameObject.Find("Player").GetComponent<Transform>();
        anim = GetComponent<Animator>();
        pf.enabled = true;
        InvokeRepeating("HowFarThePlayer", 0f, 1f);
    }

    void Update()
    {
        Die();
        if (doshoot)
        {
            shootOneTime = true;
            oneTime = true;

            

            StartCoroutine(Attack());
        }
        if (oneTime)
        {
            oneTime2 = true;
            oneTime = false;

        }
        Animations();
    }

    void SetStat()
    {
        level = plyStat.level;
        hp = level ;
        dtcPlayerRange = 7.5f;
        damage = level * 1.2f;
        attackRange = 1;
        attackSpeed = 1.6f;
        speed = 400;

        st.level = level;
        st.hp = hp;
        st.dtcPlayerRange = dtcPlayerRange;
        st.damage = damage;
        st.attackRange = attackRange;
        st.attackSpeed = attackSpeed;
        st.speed = speed;

    }


    void Animations()
    {

        if (canAttack)
        {
            canMove = false;
            AnimationState(attackAnim);
        }
        if (iswalking)
        {
            AnimationState(runAnim);

        }
        if (beIdle)
        {
            AnimationState(idleAnim);

        }

    }

    void AnimationState(string newState)
    {
        if (curState == newState)
        {
            return;
        }

        anim.Play(newState);

        curState = newState;

    }

    void HowFarThePlayer()
    {


        distance = Vector3.Distance(transform.position, plyr.transform.position);
        if (distance < dtcPlayerRange && canMove)
        {
            pf.enabled = true;
        }
        else
        {
            pf.enabled = false;
        }


        if (distance < attackRange)
        {
            canAttack = true;
        }
        else
        {
            canAttack = false;
        }

    }

    IEnumerator Attack()
    {
        if (shootOneTime)
        {
            yield return new WaitForSeconds(attackSpeed);

            hitEnemy = Physics2D.OverlapCircleAll(attackPoint.position, 1f, enemyLayer);
            
            oneTime = false;
            foreach (Collider2D collison in hitEnemy)
            {
                if (oneTime2)
                {
                    oneTime2 = false;
                    if (collison.gameObject.GetComponent<Stat>() == null)
                    {
                        yield return new WaitForSeconds(0.1f);
                    }
                    collison.gameObject.GetComponent<Stat>().hp -= damage;
                    

                }
            }
            shootOneTime = false;
            yield return new WaitForSeconds(0.5f);
            AnimationState(idleAnim);
            canMove = true;
        }

    }

    void Die()
    {
        if (st.hp <= 0)
        {
            plyStat.experiencePoints += level*2;
           
            int itemSpawnPer = Random.Range(1, 101);

            if (itemSpawnPer < 20)
            {
                int random = Random.Range(0, 2);
                switch (random)
                {
                    case 0:
                        Instantiate(swordPrefab, gameObject.transform.position, Quaternion.identity);
                        break;
                    case 1:
                        Instantiate(helmetPrefab, gameObject.transform.position, Quaternion.identity);
                        break;
                }
            }
            Destroy(gameObject);
        }
       
    }

}
