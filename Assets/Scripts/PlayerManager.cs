using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static MushroomFarmerData;

public class PlayerManager : MonoBehaviour
{
    private static PlayerManager _instance;
    private static bool _isInstanceNull = true;

    [Header("Player")]
    public bool isPlayerMoving = false;
    public bool isPlayerIdle = true;

    public PlayerMovement playerMovement;
    public SpriteAnimator animator;

    public Vector3 CurrentTile;
    public Transform LastTile;

    [SerializeField] ContactFilter2D contactFilter;

    [Header("Mushroom Followers")]
    public List<Mushroom> mushroomFollowers = new List<Mushroom>();

    [Header("Mushroom Needs")]
    [SerializeField] SpriteRenderer playerIcon;

    [SerializeField] Sprite foodSprite;
    [SerializeField] Sprite loveSprite;
    [SerializeField] Sprite waterSprite;

    public Item heldItem = Item.None;

    public GameObject hitObject;
    Collider2D lastHitCollider;

    Coroutine giveLoveRoutine = null;

    [Header("Audio")]
    [SerializeField] AudioSource audioSource;
    [SerializeField] AudioClip grabbedClip_Food;
    [SerializeField] AudioClip grabbedClip_Water;

    [SerializeField] AudioClip giveClip_Food;
    [SerializeField] AudioClip giveClip_Love;
    [SerializeField] AudioClip giveClip_Water;

    EnvironmentInteractable interactable;
    Mushroom shroom;
    
    public static PlayerManager Instance
    {
        get
        {
            if (_isInstanceNull)
            {
                return null;
            }
            else
            {
                return _instance;
            }
        }

        set
        {
            _instance = value;
            _isInstanceNull = false;
        }
    }

    [HideInInspector] public new Transform transform;

    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }
        else
        {
            Instance = null;
            Destroy(this);
        }

        transform = gameObject.transform;
        LastTile.SetParent(null);

        playerIcon.sprite = null;
    }

    private void Start()
    {
        StartCoroutine(GetPlayerInput());
    }

    public void UpdateLastTile()
    {
        if (mushroomFollowers.Count == 0)
            LastTile.position = CurrentTile;
        else
            LastTile.position = mushroomFollowers[mushroomFollowers.Count - 1].CurrentTile;
    }
    
    private IEnumerator GetPlayerInput()
    {
        yield return new WaitUntil(() => GameManager.Instance.gameStarted);

        while(true)
        {
#if UNITY_EDITOR
           // Debug.DrawRay(transform.position, RaycastDirection(), Color.green);
#endif
            if (Input.GetKeyDown(KeyCode.Z))
            {
                FireRaycast(Button.ButtonA);
            }

            if (Input.GetKeyDown(KeyCode.X))
            {
                FireRaycast(Button.ButtonB);
            }

            yield return null;
        }
    }

    private void PressedAButton()
    { 
        if (hitObject == null)
            return;
        else
        {
            if (hitObject.GetComponent<EnvironmentInteractable>())
            {
                interactable = hitObject.GetComponent<EnvironmentInteractable>();
                InteractableHandler(interactable);
            }
            else if (hitObject.GetComponent<Mushroom>())
            {
                shroom = hitObject.GetComponent<Mushroom>();
                MushroomInteractionHandler(shroom);
            }
        }
    }

    private void PressedBButton()
    {
        if (hitObject == null)
            return;
        else if (hitObject.GetComponent<Mushroom>())
        {
            shroom = hitObject.GetComponent<Mushroom>();
            giveLoveRoutine = StartCoroutine(GiveLove(shroom));
        }
    }

    private void InteractableHandler(EnvironmentInteractable interactable)
    {
        switch(interactable.type)
        {
            case InteractableType.Book:
#if UNITY_EDITOR
                Debug.Log("open the book");
#endif
                break;
            case InteractableType.Water:

                playerIcon.sprite = waterSprite;
                audioSource.PlayOneShot(grabbedClip_Water);
                heldItem = Item.Water;

                break;
            case InteractableType.Food:

                playerIcon.sprite = foodSprite;
                audioSource.PlayOneShot(grabbedClip_Food);
                heldItem = Item.Food;
                
                break;
        }

        interactable = null;

    }

    private void MushroomInteractionHandler(Mushroom shroom)
    {
        switch (shroom.currentShroomNeeds)
        {
            case MushroomNeeds.Food:

                if (heldItem == Item.Food)
                {
                    shroom.FulfillNeeds();

                    audioSource.PlayOneShot(giveClip_Food);
                    playerIcon.sprite = null;
                    heldItem = Item.None;
                }
                break;
            case MushroomNeeds.Water:

                if (heldItem == Item.Water)
                {
                    shroom.FulfillNeeds();

                    audioSource.PlayOneShot(giveClip_Water);
                    playerIcon.sprite = null;
                    heldItem = Item.None;
                }
                break;
            case MushroomNeeds.None:
                break;
        }

        shroom = null;
    }
    
    private IEnumerator GiveLove(Mushroom shroom)
    {
        Sprite previousSprite = playerIcon.sprite;
        audioSource.PlayOneShot(giveClip_Love);
        playerIcon.sprite = loveSprite;

        if (shroom.currentShroomNeeds == MushroomNeeds.Love)
        {
            //Debug.Log($"Gave {shroom} love");
            shroom.ReceiveLove();
        }

        yield return new WaitForSeconds(0.5f);

        if (previousSprite == loveSprite)
            playerIcon.sprite = null;
        else
            playerIcon.sprite = previousSprite;

        shroom = null;

        giveLoveRoutine = null;
        yield break;
    }

    
    private void FireRaycast(Button buttonPressed)
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, RaycastDirection(), 1f);
#if UNITY_EDITOR
        Debug.DrawRay(transform.position, RaycastDirection(), Color.red);
        Debug.Log("HIT: " + hit.collider);
#endif

        if (hit.collider != lastHitCollider)
        {
            if (hit.collider == null)
                hitObject = null;
            else
                hitObject = hit.collider.gameObject;
            lastHitCollider = hit.collider;
        }

        if (buttonPressed == Button.ButtonA)
            PressedAButton();
        else if (buttonPressed == Button.ButtonB)
            PressedBButton();
    }

    public Vector2 RaycastDirection()
    {
        Vector2 raycastDirection = -Vector2.up;

        switch(animator.spriteFacingDirection)
        {
            case Direction.Up:
                raycastDirection = Vector2.up;
                break;
            case Direction.Down:
                raycastDirection = -Vector2.up;
                break;
            case Direction.Left:
                raycastDirection = -Vector2.right;
                break;
            case Direction.Right:
                raycastDirection = Vector2.right;
                break;
        }

        return raycastDirection;
    }

}
