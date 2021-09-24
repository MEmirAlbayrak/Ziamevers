using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Pathfinding;

public class PlayerController : MonoBehaviour
{
    /*Animasyon Listesi:
     * Archer:
     * ArcIdle +
     * ArcRun +
     * ArcShoot + 
     * ArcRoll -
     * ArcHit +
     * ArcDie -
     * 
     * Swordman:
     * SwIdle +
     * SwRun +
     * SwA1 + 
     * SwA2 - 
     * SwA3 -
     * SwA4 -
     * SwRoll -  
     * SwHit + 
     * SwDie -
     */

    #region Movement
    float speed , nextDash;
    Rigidbody2D rb;
    #endregion

    #region Attack
    //Sword
    Collider2D[] hitEnemy;
    bool isSword, oneTime;
    public Transform attackPoint;
    float attackRange, damage, nextAttack;
    [SerializeField] LayerMask enemyLayer;
    public int attack;

    //Arrow
    [SerializeField] Transform bowTip;
    [SerializeField] GameObject Arrow;
    Vector2 diraction;
    float angle;


    #endregion

    #region Anim
    Animator anim;
    string curState;
    public bool afterAnim;
    #endregion

    Stat st;
    float curHp;
    public bool itemTouched;
    public itemScript Is;
    public bool itemGrabbed;
    public Canvas Invcv,statcv;
    public Slider Hp_Slider, Exp_Slider;
    float nextLevelExp, curLevel;
    public Canvas Esc;



    void Start()
    {
        statcv.enabled=false;
        afterAnim = false;
        st = GetComponent<Stat>();
        Invcv.enabled = false;
        nextLevelExp = 100;
        Exp_Slider.maxValue = nextLevelExp;
        attack = 0;
        curHp = st.hp;
        Hp_Slider.maxValue = st.hp;
        st.level = 20;
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        speed = 7f;
        attackRange = 20;
        enemyLayer = 10;
        curLevel = st.level;
        Esc.enabled = false;

    }

    void Update()
    {
        Movement();
        Attack();
        Animations();
        Dash();
        grabItem();
        OpenInventory();
        SliderUpt();
        LevelUp();
        openStat();
        Escmenu();
    }
    void grabItem()
    {
        if (itemTouched)
        {
            if (Input.GetKeyDown(KeyCode.E))
            {

                Inventory.instance.AddItem(Is);

                SpriteRenderer sp = Is.gameObject.GetComponent<SpriteRenderer>();
                itemGrabbed = true;
                StartCoroutine(Is.DestroySelf());
                
            }
        }
    }
    void OpenInventory()
    {
        if(Input.GetKeyDown(KeyCode.I))
        {
            if(Invcv.enabled)
            {
                Invcv.enabled = false;
                Time.timeScale = 1;
            }
            else
            {
                Invcv.enabled = true;
                Time.timeScale = 0;
            }
        }
       
    }
    void openStat()
    {
        if(Input.GetKeyDown(KeyCode.Tab))
        {
            if (statcv.enabled)
            {
                statcv.enabled = false;
                Time.timeScale = 1;
            }
            else
            {
                statcv.enabled = true;
                Time.timeScale = 0;
            }
        }
    }

    void SliderUpt()
    {
        Hp_Slider.value = st.hp;
        Exp_Slider.value = st.experiencePoints;
    }

    void LevelUp()
    {
        if (nextLevelExp<st.experiencePoints)
        {
            st.level+=1;
            st.upgradePoints += 5;
            st.experiencePoints -= nextLevelExp;
            nextLevelExp += st.level * 1.5f;
            Exp_Slider.maxValue = nextLevelExp;
            st.hp = Hp_Slider.maxValue;
        }
    }

    void Dash()
    {
        if (Input.GetKeyDown(KeyCode.LeftShift) && Time.time > nextDash)
        {
            nextDash = Time.time + 2;

            float h = Input.GetAxisRaw("Horizontal");
            float v = Input.GetAxisRaw("Vertical");
            transform.position = new Vector3(transform.position.x + h, transform.position.y + v, transform.position.z);
        }

       

    }

    void Movement()
    {

        rb.velocity = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")) * speed;

        if (Input.GetAxisRaw("Horizontal") > .1f)
        {
            rb.transform.localScale = new Vector3(.5f, .5f, 1f);
        }
        if (Input.GetAxisRaw("Horizontal") < -.1f)
        {

            rb.transform.localScale = new Vector3(-.5f, .5f, 1f);
        }
    }

    void Attack()
    {

        //Change Weapon
        if (Input.GetKeyDown(KeyCode.Q))
        {

            if (isSword)
            {
                isSword = false;
            }
            else
            {
                isSword = true;
            }
        }

        if (!isSword)
        {
            diraction = Camera.main.ScreenToWorldPoint(Input.mousePosition) - transform.position;
            angle = Mathf.Atan2(diraction.y, diraction.x) * Mathf.Rad2Deg;
            bowTip.transform.rotation = Quaternion.Euler(0f, 0f, angle - 90);

        }

        //Attack/Shoot
        if (Input.GetMouseButtonDown(0) && Time.time > nextAttack)
        {
            nextAttack = Time.time + 0.37f;

            if (isSword)
            {
                Debug.Log("ddd");
                oneTime = true;
                int rnd = Random.Range(1, 5);
                AnimationState("swA" + rnd);


            }
            else
            {
                AnimationState("ArcShoot");
                oneTime = true;
                //StartAnimation
            }
        }

        if (isSword && attack == 1)
        {
            
            SwordDamage();
        }
        if (!isSword && attack == 1)
        {
            ArrowDamage();
        }


    }

    void Animations()
    {
        if (rb.velocity == new Vector2(0, 0))
        {
            if (isSword)
            {

                AnimationState("swIdle");
            }
            else
            {
                AnimationState("ArcIdle");

            }
        }
        else
        {
            if (isSword)
            {
                AnimationState("SwRun");
            }
            else
            {
                AnimationState("ArcRun");
            }
        }

        if (curHp > st.hp)
        {
            if (isSword)
            {
                AnimationState("swHit");
            }
            else
            {
                AnimationState("ArcHit");

            }
            
            curHp = st.hp;
        }

        if (afterAnim)
        {
            if (isSword)
            {
                Debug.Log("swIDle");
                anim.Play("swIdle");
                afterAnim = false;
            }
            else
            {
                Debug.Log("arcIDle");
                anim.Play("ArcIdle");
                afterAnim = false;

            }

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

    public void SwordDamage()
    {
        hitEnemy = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayer);
        Debug.Log("SwordDamage: " + oneTime);
        foreach (Collider2D collison in hitEnemy)
        {
            
            Debug.Log("ssss");
            if (oneTime)
            {

                

                collison.GetComponent<Stat>().hp -= st.swordDamage;
                oneTime = false;
                Debug.Log("GiveSwordDamage");
                
                

            }
        }
        
    }

    public void ArrowDamage()
    {
        if (oneTime)
        {
            GameObject throwingArrow = Instantiate(Arrow, bowTip.position, bowTip.rotation);
            throwingArrow.GetComponent<Rigidbody2D>().velocity = bowTip.up * 15f;
            oneTime = false;
        }
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Item"))
        {
            itemTouched = true;
            Is = collision.gameObject.GetComponent<itemScript>();
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Item"))
        {
            itemTouched = false;
            itemGrabbed = false;
            Is = null;
        }
    }
    public void Escmenu()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (Esc.enabled==true)
            {
                Esc.enabled = false;
                Time.timeScale = 1;

            }
            else
            {
                Esc.enabled = true;
                Time.timeScale = 0;

            }
        }
    }


}
