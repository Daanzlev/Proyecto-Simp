using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



/* ----------------- MENU ----------------- */
[CreateAssetMenu(fileName = "Moves", menuName = "Simp/Create new Moves")]



/* ----------------- MOVE BASE ----------------- */
public class MoveBase : ScriptableObject {

    /* ----------------- ATRIBUTOS ----------------- */
    [SerializeField] string name;
    [TextArea]
    [SerializeField] string description;

    [SerializeField] SimpType type;
    [SerializeField] int power;
    [SerializeField] int accuracy;
    [SerializeField] int pp;
    [SerializeField] MoveCategory category;
    [SerializeField] MoveEffects effects;
    [SerializeField] MoveTarget target;



    /* ----------------- GETTERS ----------------- */
    public string Name {
        get { return name; }
    }

    public string Description {
        get { return description; }
    }

    public SimpType Type {
        get { return type; }
    }

    public int Power {
        get { return power; }
    }

    public int Accuracy {
        get { return accuracy; }
    }
    
    public int PP {
        get { return pp; }
    }

    public MoveCategory Category {
        get { return category; }
    }

    public MoveEffects Effects {
        get { return effects; }
    }

    public MoveTarget Target {
        get { return target; }
    }

}



/* ----------------- CATERGORIAS ----------------- */
[System.Serializable]
public class MoveEffects {

    /* ----------------- ATRIBUTOS ----------------- */
    [SerializeField] List<StatBoost> boosts;



    /* ----------------- ATRIBUTOS ----------------- */
    public List<StatBoost> Boosts {
        get { return boosts; }
    }

}



/* ----------------- CATERGORIAS ----------------- */
[System.Serializable]
public class StatBoost {

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


