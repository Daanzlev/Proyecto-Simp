using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



/* ----------------- MENU ----------------- */
[CreateAssetMenu(fileName = "Moves", menuName = "Simp/Create new Moves")]



/* ----------------- MOVE BASE ----------------- */
public class MoveBase : ScriptableObject
{


    /* ----------------- ATRIBUTOS ----------------- */
    [SerializeField] string name;
    [TextArea]
    [SerializeField] string description;

    [SerializeField] SimpType type;
    [SerializeField] int power;
    [SerializeField] int accuracy;
    [SerializeField] bool alwaysHits;
    [SerializeField] int pp;
    [SerializeField] int priority;


    [SerializeField] MoveCategory category;
    [SerializeField] MoveEffects effects;
    [SerializeField] List<SecondaryEffects> secondaries;
    [SerializeField] MoveTarget target;



    /* ----------------- GETTERS ----------------- */
    public string Name
    {
        get { return name; }
    }

    public string Description
    {
        get { return description; }
    }

    public SimpType Type
    {
        get { return type; }
    }

    public int Power
    {
        get { return power; }
    }

    public int Accuracy
    {
        get { return accuracy; }
    }

    public bool AlwaysHits
    {
        get { return alwaysHits; }
    }
    public int PP
    {
        get { return pp; }
    }

    public int Priority
    {
        get { return priority; }
    }

    public MoveCategory Category
    {
        get { return category; }
    }

    public MoveEffects Effects
    {
        get { return effects; }
    }

    public List<SecondaryEffects> Secondaries
    {
        get { return secondaries; }
    }
    public MoveTarget Target
    {
        get { return target; }
    }
    
    public static MoveBase Struggle { get; private set; }

    [RuntimeInitializeOnLoadMethod]
    static void Init()
    {
        Struggle = Resources.Load<MoveBase>("Moves/Struggle");
        if (Struggle == null)
            Debug.LogError("❌ No se pudo cargar el movimiento 'Struggle'.");
    }

}



/* ----------------- CATERGORIAS ----------------- */
[System.Serializable]
public class MoveEffects {

    /* ----------------- ATRIBUTOS ----------------- */
    [SerializeField] List<StatBoost> boosts;
    [SerializeField] ConditionID status;
    [SerializeField] ConditionID volatileStatus;


    /* ----------------- ATRIBUTOS ----------------- */
    public List<StatBoost> Boosts {
        get { return boosts; }
    }

    public ConditionID Status {
        get { return status; } 
    }
    public ConditionID VolatileStatus {
        get { return volatileStatus; }
    }
}
/* ----------------- DOBLE EFECTO ----------------- */
[System.Serializable]
public class SecondaryEffects : MoveEffects
{
    [SerializeField] int chance;
    [SerializeField] MoveTarget target;

    public int Chance {
        get { return chance;  }
    }

    public MoveTarget Target {
        get { return target; }
    }
}


/* ----------------- CATERGORIAS ----------------- */
[System.Serializable]
public class StatBoost
{

    /* ----------------- ATRIBUTOS ----------------- */
    public Stat stat;
    public int boost;


}




/* ----------------- CATERGORIAS ----------------- */
public enum MoveCategory {
    Physical,
    Special,
    Status
}



/* ----------------- CATERGORIAS ----------------- */
public enum MoveTarget {
    Enemy,
    Self
}


