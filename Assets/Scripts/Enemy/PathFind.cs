using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

public class PathFind : MonoBehaviour
{

    Transform target;

    [SerializeField] float speed;
    public float nextWaypointDistance = 3f;


    Path path;
    int currentWaypoint;
    bool reachedEndOfPath = false;
    bool oneTimeR, oneTimeL;
    float localScale;

    Seeker seeker;
    Rigidbody2D rb;

    void Start()
    {
        oneTimeL = true;
        oneTimeR = true;
        seeker = GetComponent<Seeker>();
        rb = GetComponent<Rigidbody2D>();
        target = GameObject.Find("Player").GetComponent<Transform>();
        

        InvokeRepeating("UpdatePath", 0f, .2f);
        speed = 800;
    }


    void UpdatePath()
    {
        if (seeker.IsDone())
        {
            seeker.StartPath(rb.position, new Vector3(target.position.x, target.position.y + 0.2f, 1), OnPathComplate);

        }
    }

    void OnPathComplate(Path p)
    {
        if (!p.error)
        {
            path = p;
            currentWaypoint = 0;
        }
    }

    void FixedUpdate()
    {

        if (path == null)
        {
            return;
        }

        if (currentWaypoint >= path.vectorPath.Count)
        {
            reachedEndOfPath = true;
            return;
        }
        else
        {
            reachedEndOfPath = false;
        }

        Vector2 diraction = ((Vector2)path.vectorPath[currentWaypoint] - rb.position).normalized;

        Vector2 force = diraction * speed * Time.deltaTime;
        rb.AddForce(force);

        float distance = Vector2.Distance(rb.position, path.vectorPath[currentWaypoint]);

        if (distance < nextWaypointDistance)
        {
            currentWaypoint += 1;
        }

        if(target.position.x > transform.position.x)
        {
            if(transform.localScale.x < 0)
            {
               transform.localScale = new Vector3(transform.localScale.x * -1, transform.localScale.y, 1f);
            }
        }
        else
        {
            if(transform.localScale.x > 0)
            {
                transform.localScale = new Vector3(transform.localScale.x * -1, transform.localScale.y, 1f);
            }
        }

    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            rb.constraints = RigidbodyConstraints2D.FreezeAll;
        }

    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            rb.constraints = RigidbodyConstraints2D.None;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        }
    }
}
