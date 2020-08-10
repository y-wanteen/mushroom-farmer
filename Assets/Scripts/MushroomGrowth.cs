using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MushroomGrowth : MonoBehaviour
{
    public Animator animator;

    [SerializeField]
    private float SproutStageLength = 2;
    [SerializeField]
    private float BudStageLength = 2;
 
    private bool SproutStage = false;
    private bool HarvestStage = false;

    // Update is called once per frame
    void Update()
    {
        if (Time.time >= SproutStageLength)
        {
            SproutStage = true;
            animator.SetBool("Bud", SproutStage);
        }

        if (Time.time >= BudStageLength+SproutStageLength)
        {
            if (SproutStage = true)
            {
                HarvestStage = true;
                animator.SetBool("Harvest", HarvestStage);
            }
        }

    }

 
}
