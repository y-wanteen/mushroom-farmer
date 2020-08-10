using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using System.Linq;

public class PlayerMovement : MonoBehaviour
{
    [Header("Player Movement")]
    public float moveSpeed = 5f;

    private float timeDownX = 0.0f;
    private float timeDownY = 0.0f;

    private float xVel;
    private float yVel;

    private new Transform transform;
    private Vector3 direction;
    private Vector3 nextTile;
    
    Coroutine moveToNextTile;

    [Header("Sprite Animations")]
    private SpriteAnimator animator;

    [Header("Tilemaps to Check")]
    [SerializeField] Tilemap wallTiles;

    // Start is called before the first frame update
    void Start()
    {
        transform = PlayerManager.Instance.transform;
        animator = PlayerManager.Instance.animator;
    }

    private void Update()
    {
        if (GameManager.Instance.gameStarted)
        {

            xVel = (int)Input.GetAxisRaw("Horizontal");
            yVel = (int)Input.GetAxisRaw("Vertical");

            if (xVel == 0 && yVel == 0)
            {
                animator.SetIdleAnim(true);
                PlayerManager.Instance.isPlayerIdle = true;
            }
            else if (xVel != 0 || yVel != 0)
            {
                animator.SetIdleAnim(false);
                PlayerManager.Instance.isPlayerIdle = false;

                if (Mathf.Abs(xVel) > 0)
                {
                    direction = new Vector3(xVel, 0f, 0f);
                    // animator.HorizontalAnimInputCheck(xVel);

                }
                else if (Mathf.Abs(yVel) > 0)
                {
                    direction = new Vector3(0f, yVel, 0f);
                    // animator.VerticalAnimInputCheck(yVel);
                }
                //transform.position += movement * Time.deltaTime * moveSpeed; //Old method

                //currentTile = transform.position;
                nextTile = transform.position + direction;

                //Debug.Log(GetTile(wallTiles, nextTile));
                //Debug.Log(nextTile);


                //Debug.Log(IsOccupiedByShroom(nextTile));
                if (!PlayerManager.Instance.isPlayerMoving && !IsBlockedTile(nextTile))
                    moveToNextTile = StartCoroutine(MoveToTile(nextTile));
                else
                    animator.AnimInputCheck(direction);
            }
        }
    }


    public void MushroomFollow()
    {
        Vector3 target;
        for (int i = 0; i < PlayerManager.Instance.mushroomFollowers.Count; i++)
        {
            if (i == 0)
            {
                target = PlayerManager.Instance.CurrentTile;
            }
            else
            {
                target = PlayerManager.Instance.mushroomFollowers[i - 1].CurrentTile;
            }
            
            PlayerManager.Instance.mushroomFollowers[i].FollowToTile(target);
        }
    }

    private IEnumerator MoveToTile(Vector3 target)
    {
        PlayerManager.Instance.isPlayerMoving = true;

        animator.AnimInputCheck(direction);

        PlayerManager.Instance.UpdateLastTile();
        PlayerManager.Instance.CurrentTile = transform.position;

        MushroomFollow();
        
        float sqrRemainingDistance = (transform.position - target).sqrMagnitude;
        while (sqrRemainingDistance > float.Epsilon)
        {
            transform.position = Vector3.MoveTowards(transform.position, target, Time.deltaTime * moveSpeed);
            sqrRemainingDistance = (transform.position - target).sqrMagnitude;
            //Debug.Log(sqrRemainingDistance);
            //if (sqrRemainingDistance == 1)
            //{
            //    moveToNextTile = null;
            //    isMoving = false;
            //    yield break;
            //}
            yield return null;
        }

        moveToNextTile = null;

        PlayerManager.Instance.CurrentTile = transform.position;
        PlayerManager.Instance.isPlayerMoving = false;
    }

    private bool IsBlockedTile(Vector3 cellWorldPos)
    {
        return GetTile(wallTiles, cellWorldPos) != null;
    }

    private bool IsOccupiedByShroom(Vector3 cellWorldPos)
    {
        bool occupied = false;

        for (int i = 0; i < PlayerManager.Instance.mushroomFollowers.Count; i++)
        {
            if (PlayerManager.Instance.mushroomFollowers[i].transform.position == cellWorldPos)
            {
                Debug.Log($"{PlayerManager.Instance.mushroomFollowers[i]} shroom friend is here don't squish'em!");
                occupied = true;
                break;
            }
        }

        return occupied;
    }

    TileBase GetTile(Tilemap tilemap, Vector3 cellWorldPos)
    {
        return tilemap.GetTile(tilemap.WorldToCell(cellWorldPos));
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Mushroom"))
        {
            //Debug.Log($"Ate shroom friend {collision.gameObject.name} ):");

            //mushrooms.Remove(collision.gameObject.GetComponent<Mushroom>());
            //collision.gameObject.SetActive(false);
        }
    }

}
