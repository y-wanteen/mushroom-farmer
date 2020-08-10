using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static MushroomFarmerData;

/// <summary>
/// Sprite Basic Locomotion Animator
/// </summary>
public class SpriteAnimator : MonoBehaviour
{
    [Header("Sprites and Animation")]
    [SerializeField] Animator animator;
    [SerializeField] SpriteRenderer spriteRenderer;

    public Direction spriteFacingDirection;
    [SerializeField] bool isSideIdleLeft = true;
    
    private static readonly int HASH_IS_WALKING_SIDE = Animator.StringToHash("isWalkingSide");
    private static readonly int HASH_IS_WALKING_UP = Animator.StringToHash("isWalkingUp");
    private static readonly int HASH_IS_WALKING_DOWN = Animator.StringToHash("isWalkingDown");
    private static readonly int HASH_IS_IDLE_SIDE = Animator.StringToHash("isIdleSide");
    private static readonly int HASH_IS_IDLE_UP = Animator.StringToHash("isIdleUp");
    private static readonly int HASH_IS_IDLE_DOWN = Animator.StringToHash("isIdleDown");

    
    public void AnimInputCheck(Vector3 direction)
    {
        bool sideMovement = Mathf.Abs(direction.x) > Mathf.Abs(direction.y);
        
        switch(sideMovement)
        {
            case true:
                HorizontalAnimInputCheck(direction.x);
                break;
            case false:
                VerticalAnimInputCheck(direction.y);
                break;
        }
    }

    public void HorizontalAnimInputCheck(float val)
    {
        animator.SetBool(HASH_IS_WALKING_UP, false);
        animator.SetBool(HASH_IS_WALKING_DOWN, false);
        animator.SetBool(HASH_IS_WALKING_SIDE, true);

        if (val > 0)
        {
            spriteRenderer.flipX = isSideIdleLeft;
            spriteFacingDirection = Direction.Right;
        }
        else
        {
            spriteRenderer.flipX = !isSideIdleLeft;
            spriteFacingDirection = Direction.Left;
        }
    }

    public void VerticalAnimInputCheck(float val)
    {
        if (val > 0)
        {
            animator.SetBool(HASH_IS_WALKING_UP, true);
            animator.SetBool(HASH_IS_WALKING_DOWN, false);
            animator.SetBool(HASH_IS_WALKING_SIDE, false);

            spriteFacingDirection = Direction.Up;
        }
        else
        {
            animator.SetBool(HASH_IS_WALKING_UP, false);
            animator.SetBool(HASH_IS_WALKING_DOWN, true);
            animator.SetBool(HASH_IS_WALKING_SIDE, false);

            spriteFacingDirection = Direction.Down;
        }
    }

    public void SetIdleAnim(bool val)
    {
        switch(val)
        {
            case true:
                if (animator.GetBool(HASH_IS_WALKING_SIDE))
                {
                    animator.SetBool(HASH_IS_IDLE_SIDE, true);
                    animator.SetBool(HASH_IS_WALKING_SIDE, false);
                }
                else if (animator.GetBool(HASH_IS_WALKING_UP))
                {
                    animator.SetBool(HASH_IS_IDLE_UP, true);
                    animator.SetBool(HASH_IS_WALKING_UP, false);
                }
                else if (animator.GetBool(HASH_IS_WALKING_DOWN))
                {
                    animator.SetBool(HASH_IS_IDLE_DOWN, true);
                    animator.SetBool(HASH_IS_WALKING_DOWN, false);
                }
                break;
            case false:
                animator.SetBool(HASH_IS_IDLE_SIDE, false);
                animator.SetBool(HASH_IS_IDLE_UP, false);
                animator.SetBool(HASH_IS_IDLE_DOWN, false);
            break;
        }
    }
}
