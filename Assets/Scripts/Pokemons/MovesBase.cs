using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Moves", menuName = "Simp/Create new Moves")]
public class MoveBase : ScriptableObject
{
    [SerializeField] string name;
    [TextArea]
    [SerializeField] string description;

    [SerializeField] SimpType type;
    [SerializeField] int power;
    [SerializeField] int accuracy;
    [SerializeField] int pp;
    
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
    
    public int PP
    {
        get { return pp; }
    }
    public bool IsSpecial
    {
        get { 
            if ( type == SimpType.Fire || type == SimpType.Water 
            || type == SimpType.Grass || type == SimpType.Electric || type == SimpType.Dragon)
            
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }    
}
