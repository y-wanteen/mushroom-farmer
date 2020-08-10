using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MushroomFarmerData : MonoBehaviour
{
    public enum MushroomType
    {
        Cerberus = 0, 
        Dwight = 1, 
        Shinji = 2,
        Vlad = 3,
    }

    public enum MushroomStage
    {
        Sprout = 0,
        Bud = 1,
        Harvest = 2,
        Dead = 3,
    }

    public enum MushroomNeeds
    {
        None = 0,
        Water = 1,
        Food = 2,
        Love = 3,
    }

    public enum Direction
    {
        Up = 0,
        Down = 1,
        Left = 2,
        Right = 3,
    }

    public enum Item
    {
        None = 0,
        Food = 1,
        Water = 2,
    }

    public enum InteractableType
    {
        None = 0,
        Food = 1,
        Water = 2,
        Book = 3,
    }

    public enum Button
    {
        ButtonA = 0,
        ButtonB = 1,
    }
}
