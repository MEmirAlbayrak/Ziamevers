using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallSpriteScript : MonoBehaviour
{
    public DungeonManager dm;
    public SpriteRenderer sR;
    public Sprite bottomRightSprite,noneWallSprite,FlatWall,LeftDownWall,LeftWall,DownRightBottomWall,RightWall,LeftCornerWall,downCorner;
   
    // Start is called before the first frame update
    void Start()
    {
        sR = GetComponent<SpriteRenderer>();
        dm = GameObject.Find("Dungeon Manager").GetComponent<DungeonManager>();
        wallHit();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void wallHit()
    {
        Collider2D TopHit = Physics2D.OverlapBox(new Vector2(gameObject.transform.position.x, gameObject.transform.position.y + 1), Vector2.one * 0.8f, 0, dm.WallMask);
        Collider2D RightHit = Physics2D.OverlapBox(new Vector2(gameObject.transform.position.x+1, gameObject.transform.position.y), Vector2.one * 0.8f, 0, dm.WallMask);
        Collider2D DownHit = Physics2D.OverlapBox(new Vector2(gameObject.transform.position.x, gameObject.transform.position.y -1), Vector2.one * 0.8f, 0, dm.WallMask);
        Collider2D LeftHit = Physics2D.OverlapBox(new Vector2(gameObject.transform.position.x-1, gameObject.transform.position.y), Vector2.one * 0.8f, 0, dm.WallMask);

        Collider2D TopFHit = Physics2D.OverlapBox(new Vector2(gameObject.transform.position.x, gameObject.transform.position.y + 1), Vector2.one * 0.8f, 0, dm.FloorMask);
        Collider2D RightFHit = Physics2D.OverlapBox(new Vector2(gameObject.transform.position.x + 1, gameObject.transform.position.y), Vector2.one * 0.8f, 0, dm.FloorMask);
        Collider2D DownFHit = Physics2D.OverlapBox(new Vector2(gameObject.transform.position.x, gameObject.transform.position.y - 1), Vector2.one * 0.8f, 0, dm.FloorMask);
        Collider2D LeftFHit = Physics2D.OverlapBox(new Vector2(gameObject.transform.position.x - 1, gameObject.transform.position.y), Vector2.one * 0.8f, 0, dm.FloorMask);
        if (TopHit && LeftHit && !DownHit && !RightHit && !RightFHit && !DownFHit)
        {
            sR.sprite = bottomRightSprite;
        }
        else if(!TopHit && !LeftHit && !DownHit && !RightHit)
        {
            sR.sprite = noneWallSprite;
        }
        //else if(RightHit && TopHit && !DownHit && !LeftHit)
        //{
        //    sR.sprite = bottomRightSprite;
           
        //}
        else if(RightHit && LeftHit && TopFHit && !DownFHit && !TopHit && !DownHit)
        {
            sR.sprite = FlatWall;
        }
        else if(TopHit && DownFHit && RightFHit && LeftFHit)
        {
            sR.sprite = downCorner;
        }
        else if(TopHit && RightHit && !LeftFHit && !DownFHit)
        {
            sR.sprite = LeftDownWall;
        }
        else if(TopHit && DownHit && RightFHit && !LeftFHit && !LeftHit)
        {
            sR.sprite = LeftWall;
        }
        else if(DownHit && LeftHit && TopFHit && RightFHit)
        {
            sR.sprite = DownRightBottomWall;
        }
        else if(TopHit && DownHit && !RightFHit && LeftFHit && !LeftHit)
        {
            sR.sprite = RightWall;
        }
        else if(DownHit && RightHit && LeftFHit && TopFHit)
        {
            sR.sprite = DownRightBottomWall;
            transform.localScale = new Vector3(transform.localScale.x * -1, transform.localScale.y, 1);
        }
        else if(TopHit && RightHit && LeftFHit && DownFHit)
        {
            sR.sprite = LeftCornerWall;

        }
        else if(TopHit && LeftFHit && RightFHit && DownFHit)
        {
            sR.sprite = LeftCornerWall;
            transform.localScale = new Vector3(transform.localScale.x * -1, transform.localScale.y, 1);
        }
        else if(TopFHit && DownFHit && LeftFHit ||RightFHit && !TopHit && !LeftHit || !RightHit && !DownHit && !TopHit)
        {
            sR.sprite = noneWallSprite;
        }
        
    }
    private void OnDrawGizmos()
    {
        Gizmos.DrawCube(new Vector2(gameObject.transform.position.x+1, gameObject.transform.position.y), Vector2.one * 0.8f);

        Gizmos.color = Color.red;
    }
}
