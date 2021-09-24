using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RangerEnemy : MonoBehaviour
{


    public Transform gunPos;
    public GameObject projectile;
    [SerializeField]
    public Transform Player;
    Animator anim;
    string curState;
    PathFind pf;
    [SerializeField] float distance;
    bool iswalking, beIdle, shootOneTime;
    public bool doshoot;
    public string runAnim, attackAnim, idleAnim, dieAnim;
    Stat plStat, st;
    public float attackRange,
        level,
        hp,
        speed,
        damage,
        detecPlayerRange,
        attackSpeed;
     public  GameObject swordPrefab, helmetPrefab;
    


    void Start()
    {

       
        pf = GetComponent<PathFind>();
        st = GetComponent<Stat>();
        attackSpeed = 2;
        InvokeRepeating("HowFarThePlayer", 0f, 1f);

        plStat = GameObject.FindGameObjectWithTag("Player").GetComponent<Stat>();
        Player = GameObject.FindGameObjectWithTag("Player").GetComponent<Transform>();
        anim = GetComponent<Animator>();
        setStat();
        

    }


    void Update()
    {
        if (doshoot)
        {
            shootOneTime = true;

            StartCoroutine(ShootProjectileIE());
        }
        Animations();
        Die();
       



    }
    void setStat()
    {
        level = plStat.level;
        hp = level ;
        detecPlayerRange = 17;
        damage = level ;
        attackRange = 10f;
        attackSpeed = 0.01f;
        speed = 400;

        st.level = level;
        st.hp = hp;
        st.dtcPlayerRange = detecPlayerRange;
        st.damage = damage;
        st.attackRange = attackRange;
        st.attackSpeed = attackSpeed;
        st.speed = speed;
    }
    void HowFarThePlayer()
    {

        distance = Vector3.Distance(transform.position, Player.transform.position);


        if (distance < detecPlayerRange && distance > attackRange)
        {
            pf.enabled = true;
            iswalking = true;
            beIdle = false;
        }
        else
        {
            pf.enabled = false;
            iswalking = false;
            if (distance > detecPlayerRange)
            {
                beIdle = true;
            }
            if (distance < attackRange)
            {
                beIdle = false;
            }

        }

        //Attack

    }

    void Animations()
    {
        if (iswalking)
        {
            AnimationState(runAnim);

        }
        else if (beIdle)
        {
            AnimationState(idleAnim);

        }
        else
        {

            AnimationState(attackAnim);


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

    IEnumerator ShootProjectileIE()
    {
        yield return new WaitForSeconds(0.5f);

        if (shootOneTime)
        {
            Debug.Log("DidShoot");
            Instantiate(projectile, gunPos.transform.position, Quaternion.identity);
            shootOneTime = false;
            AnimationState(idleAnim);
        }


        yield return new WaitForSeconds(attackSpeed);

        if (distance < attackRange)
        {

            AnimationState(attackAnim);
        }
    }
    public void Die()
    {
        
        if (st.hp <= 0)
        {
            
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

            plStat.experiencePoints += level;
            Destroy(gameObject);
        }
    }


}



