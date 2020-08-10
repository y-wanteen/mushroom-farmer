using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using static MushroomFarmerData;

/// <summary>
/// Main Mushroom Class
/// </summary>
public class Mushroom : MonoBehaviour
{
    [HideInInspector] public new Transform transform;

    [Header("Mushroom Growth")]

    public MushroomType type;
    public MushroomSpawn originalSpawner;

    [Header("Growth Stages")]
    public MushroomStage currentShroomStage = MushroomStage.Sprout;

    [SerializeField]
    [Tooltip("Time that shroom stays as a sprout for")]
    [Range(0, 10)]
    private float sproutStageLength = 3;

    [SerializeField]
    [Tooltip("Time that shroom stays as a bud for")]
    [Range(0, 10)]
    private float budStageLength = 5;

    [Header("Growth Needs")]
    public MushroomNeeds currentShroomNeeds = MushroomNeeds.None;

    [SerializeField]
    [Tooltip("Time until shroom starts dying after expressing needs, not including dying animation length")]
    [Range(0, 100)]
    private float waitForNeedsLength = 2;

    private static readonly int HASH_STAGE_BUD = Animator.StringToHash("Bud");
    private static readonly int HASH_STAGE_HARVEST = Animator.StringToHash("Harvest");
    private static readonly int HASH_STAGE_DYING = Animator.StringToHash("isDying");
    
    [SerializeField] GameObject needsFood;
    [SerializeField] GameObject needsLove;
    [SerializeField] GameObject needsWater;

    private bool hasNoNeeds = true;

    Coroutine shroomGrowthStage;
    Coroutine shroomNeedsRoutine;

    BoxCollider2D colliderTrigger;

    [Header("Sprites and Animations")]
    [SerializeField] SpriteAnimator spriteAnimator;
    [SerializeField] Animator animator;

    [Header("Mushroom Movement")]
    [SerializeField] private float moveSpeed = 5f;

    private bool isMoving = false;
    public Vector3 CurrentTile { get; private set; }

    Coroutine moveToNextTile;
    Coroutine waitForIdle;

    [Header("Audio")]
    [SerializeField] AudioSource audioSource;

    [Header("Growth Audio")]
    [SerializeField] AudioClip popClip_Sprout;
    [SerializeField] AudioClip popClip_Bud;
    [SerializeField] AudioClip[] popClip_Harvest;
    [SerializeField] AudioClip deathClip;

    [Header("Needs Audio")]
    [SerializeField] AudioClip needsClip_Food;
    [SerializeField] AudioClip needsClip_Love;
    [SerializeField] AudioClip needsClip_Water;

    private bool canJoinLine = false;

    public bool isTester = false;
   
    private void Awake()
    {
        transform = gameObject.transform;
        
        shroomGrowthStage = StartCoroutine(SproutStage());
        colliderTrigger = GetComponent<BoxCollider2D>();

        StartCoroutine(WaitForGameEnd());
    }

    private void Update()
    {
#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.F))
            FulfillNeeds();
#endif
    }

    private IEnumerator WaitForGameEnd()
    {
        yield return new WaitUntil(() => !GameManager.Instance.gameStarted);

        StopAllCoroutines();
    }

    private IEnumerator SproutStage()
    {
#if UNITY_EDITOR
        Debug.Log("<color=green> ENTERED SPROUT STAGE! </color>");
#endif

        currentShroomStage = MushroomStage.Sprout;

        if (originalSpawner)
            transform.position = originalSpawner.transform.position;
        
        audioSource.PlayOneShot(popClip_Sprout);

        float timer = 0;

        while(true)
        {
            timer += Time.deltaTime;
#if UNITY_EDITOR
          //  Debug.Log($"{gameObject.name} Time as Sprout: {timer}");
#endif
            if (currentShroomNeeds == MushroomNeeds.None)
            {
                if (timer >= sproutStageLength)
                {
                    shroomGrowthStage = StartCoroutine(BudStage());

                    if (shroomNeedsRoutine != null)
                    {
                        StopCoroutine(shroomNeedsRoutine);
                        shroomNeedsRoutine = null;
                        hasNoNeeds = true;
                    }

                    yield break;
                }

                GetRandomNeeds();
                yield return null;
            }

            yield return null;
        }
    }

    private IEnumerator BudStage()
    {
#if UNITY_EDITOR
        Debug.Log("<color=green> ENTERED BUD STAGE! </color>");
#endif

        currentShroomStage = MushroomStage.Bud;

        animator.SetBool(HASH_STAGE_BUD, true);
        audioSource.PlayOneShot(popClip_Bud);

        float timer = 0;

        while (true)
        {
            timer += Time.deltaTime;
#if UNITY_EDITOR
            //Debug.Log($"{gameObject.name} Time as Bud: {timer}");
#endif
            if (currentShroomNeeds == MushroomNeeds.None)
            {
                if (timer >= budStageLength)
                {
                    shroomGrowthStage = StartCoroutine(HarvestShroom());

                    if (shroomNeedsRoutine != null)
                    {
                        StopCoroutine(shroomNeedsRoutine);
                        shroomNeedsRoutine = null;
                        hasNoNeeds = true;
                    }

                    yield break;
                }

                GetRandomNeeds();
                yield return null;
            }
            yield return null;
        }
    }

    private void GetRandomNeeds()
    {
        if (!isTester && hasNoNeeds)
        {

            int needsSomething = Random.Range(0, 11);

            if (needsSomething <= 5)
            {
#if UNITY_EDITOR
                //Debug.Log($"<color=blue>{gameObject.name} doesn't need anything</color>");
#endif
                return;
            }
            else
            {
                int randomNeed = Random.Range(1, 4);

                if (shroomNeedsRoutine == null)
                    shroomNeedsRoutine = StartCoroutine(WaitForNeeds((MushroomNeeds)randomNeed));
#if UNITY_EDITOR
               // Debug.Log($"<color=blue>{gameObject.name} needs {(MushroomNeeds)randomNeed}</color>");
#endif
            }
        }
    }

    private IEnumerator ShowNeeds(MushroomNeeds need)
    {
        switch(need)
        {
            case MushroomNeeds.Food:
                //Show icon here too
                yield return new WaitUntil(() => !audioSource.isPlaying);
                needsFood.SetActive(true);
                audioSource.PlayOneShot(needsClip_Food);
                break;
            case MushroomNeeds.Water:
                yield return new WaitUntil(() => !audioSource.isPlaying);
                needsWater.SetActive(true);
                audioSource.PlayOneShot(needsClip_Water);
                break;
            case MushroomNeeds.Love:
                yield return new WaitUntil(() => !audioSource.isPlaying);
                needsLove.SetActive(true);
                audioSource.PlayOneShot(needsClip_Love);

                break;
        }
    }

    public void FulfillNeeds()
    {
        switch (currentShroomNeeds)
        {
            case MushroomNeeds.Food:
                needsFood.SetActive(false);
                break;
            case MushroomNeeds.Water:
                needsWater.SetActive(false);

                break;
            //case MushroomNeeds.Love:
            //    needsLove.SetActive(false);
            //    break;
        }
        
        if (shroomNeedsRoutine != null)
        {
            StopCoroutine(shroomNeedsRoutine);
            shroomNeedsRoutine = null;
        }

        currentShroomNeeds = MushroomNeeds.None;
        hasNoNeeds = true;
    }

    public void ReceiveLove()
    {
        needsLove.SetActive(false);

        if (shroomNeedsRoutine != null)
        {
            StopCoroutine(shroomNeedsRoutine);
            shroomNeedsRoutine = null;
        }

        currentShroomNeeds = MushroomNeeds.None;
        hasNoNeeds = true;
    }

    private IEnumerator WaitForNeeds(MushroomNeeds newNeed)
    {
        hasNoNeeds = false;

        float timer = 0;

        yield return new WaitForSeconds(1f); //courtesy delay

        currentShroomNeeds = newNeed;
        StartCoroutine(ShowNeeds(currentShroomNeeds));

        while (timer < waitForNeedsLength)
        {
            timer += Time.deltaTime;
#if UNITY_EDITOR
           // Debug.Log($"{gameObject.name} Time until Dying: {timer}");
#endif
            if (currentShroomNeeds == MushroomNeeds.None)
            {
                shroomNeedsRoutine = null;
                hasNoNeeds = true;
                yield break;
            }
            else
            {
                if (timer >= waitForNeedsLength) //& didn't get what was needed
                {
                    animator.SetBool(HASH_STAGE_DYING, true);
                    //Debug.Log("<color=red>!!! DYING</color>");
                    yield break;
                }
            }
            yield return null;
        }
    }
    
    //Called by last frame of Death Animation
    public void ShroomIsDead()
    {
#if UNITY_EDITOR
        Debug.Log("<color=red> SHROOM DEAD ): </color>");
#endif
        currentShroomStage = MushroomStage.Dead;

        audioSource.PlayOneShot(deathClip);

        needsLove.SetActive(false);
        needsWater.SetActive(false);
        needsFood.SetActive(false);

        originalSpawner.BlockSpawning();

        if (shroomGrowthStage != null)
        {
            StopCoroutine(shroomGrowthStage);
            shroomGrowthStage = null;
        }

        if (shroomNeedsRoutine != null)
        {
            StopCoroutine(shroomNeedsRoutine);
            shroomNeedsRoutine = null;
        }
    }

    #region Harvest Stage

    //Set to true in harvest animation
    public void CanJoinLine()
    {
        canJoinLine = true;
    }



    public IEnumerator HarvestShroom()
    {
        currentShroomStage = MushroomStage.Harvest;
        animator.SetBool(HASH_STAGE_HARVEST, true);

        colliderTrigger.enabled = false;

        needsLove.SetActive(false);
        needsWater.SetActive(false);
        needsFood.SetActive(false);

        int randomHarvestClip = Random.Range(0, 2);
        audioSource.PlayOneShot(popClip_Harvest[randomHarvestClip]);

        yield return new WaitUntil(() => canJoinLine);

        //join the line
        StartCoroutine(JoinFollowerLine());

        yield return null;
    }

    public IEnumerator JoinFollowerLine()
    {
        isMoving = true;

        CurrentTile = transform.position;

        Vector3 direction = PlayerManager.Instance.LastTile.position - CurrentTile;

        spriteAnimator.SetIdleAnim(false);
        spriteAnimator.AnimInputCheck(direction);

        float sqrRemainingDistance = (transform.position - PlayerManager.Instance.LastTile.position).sqrMagnitude;

        while (sqrRemainingDistance > float.Epsilon)
        {
            transform.position = Vector3.MoveTowards(transform.position, PlayerManager.Instance.LastTile.position, Time.deltaTime * moveSpeed);
            sqrRemainingDistance = (transform.position - PlayerManager.Instance.LastTile.position).sqrMagnitude;
            yield return null;
        }

        moveToNextTile = null;
        CurrentTile = transform.position;
        isMoving = false;
#if UNITY_EDITOR
        Debug.Log("<color=yellow>JOINED THE LINE</color>");
#endif
        PlayerManager.Instance.mushroomFollowers.Add(this);
        originalSpawner.WaitForSpawn(); //reset the spawner

        yield break;
    }

    #endregion

    #region Movement
    public void FollowToTile(Vector3 target)
    {
        if (moveToNextTile == null)
            moveToNextTile = StartCoroutine(MoveToTile(target));
        else
        {
            StopCoroutine(moveToNextTile);
            moveToNextTile = StartCoroutine(MoveToTile(target));
        }

        if (waitForIdle == null)
            waitForIdle = StartCoroutine(WaitForIdle());
    }

    private IEnumerator MoveToTile(Vector3 target)
    {
        isMoving = true;
        
        CurrentTile = transform.position;

        Vector3 direction = target-CurrentTile;

        spriteAnimator.SetIdleAnim(false);
        spriteAnimator.AnimInputCheck(direction);

        float sqrRemainingDistance = (transform.position - target).sqrMagnitude;

        while (sqrRemainingDistance > float.Epsilon)
        {
            transform.position = Vector3.MoveTowards(transform.position, target, Time.deltaTime * moveSpeed);
            sqrRemainingDistance = (transform.position - target).sqrMagnitude;
            yield return null;
        }

        moveToNextTile = null;
        CurrentTile = transform.position;
        isMoving = false;
    }

    private IEnumerator WaitForIdle()
    {
        yield return new WaitUntil(() => PlayerManager.Instance.isPlayerIdle && !isMoving);
        spriteAnimator.SetIdleAnim(true);
        waitForIdle = null;
        yield break;
    }
#endregion
}
