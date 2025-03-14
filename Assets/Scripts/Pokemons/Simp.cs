using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class Simp 
{
    [SerializeField] SimpBase _base;
    [SerializeField] int level;

    public SimpBase Base 
    { 
        get { return _base; } 
       // set { _base = value; } 
    }

    public int Level 
    { 
        get { return level; } 
        //set { level = value; } 
    }


    public int HP { get; set; }
    public List<Move> Moves { get; set; }

    public void Init()
    {
        HP = MaxHP;

        //Creación de Moves
        Moves= new List<Move>();
        foreach(var move in Base.LearnableMoves)
        {
            if(move.Level <= Level)
            {
                Moves.Add(new Move(move.Base));
            }

            if(Moves.Count >= 4)
            {
                break;
            }
        }

    }
    
    public int Attack
    {
        get { return Mathf.FloorToInt((Base.Attack * Level) / 100f) + 5; }
    }

    public int Defense
    {
        get { return Mathf.FloorToInt((Base.Defense * Level) / 100f) + 5; }
    }

    public int SpAttack
    {
        get { return Mathf.FloorToInt((Base.SpAttack * Level) / 100f) + 5; }
    }

    public int SpDefense
    {
        get { return Mathf.FloorToInt((Base.SpDefense * Level) / 100f) + 5; }
    }

    public int Speed
    {
        get { return Mathf.FloorToInt((Base.Speed * Level) / 100f) + 5; }
    }

    public int MaxHP
    {
        get { return Mathf.FloorToInt((Base.MaxHP * Level) / 100f) + 10; }
    }

    public DamageDetails TakeDamage(Move move , Simp attacker)
    {
        float critical = 1f;
        if(Random.value * 100f < 6.25f)
        {
            critical = 2f;
        }

        float type = TypeChart.GetEffectiveness(move.Base.Type, Base.Type1) * TypeChart.GetEffectiveness(move.Base.Type, Base.Type2);

        var damageDetails = new DamageDetails()
        {
            Type = type,
            Critical = critical,
            Fainted = false
        };

        float attack = (move.Base.IsSpecial) ? attacker.SpAttack : attacker.Attack;
        float defense = (move.Base.IsSpecial) ? SpDefense : Defense;

        float modifiers = Random.Range(0.85f, 1f) *type * critical;
        float a = (2 * attacker.Level + 10) / 250f;      
        float d = a * move.Base.Power * ((float)attack / defense) + 2;
        int damage = Mathf.FloorToInt(d * modifiers);

        HP -= damage;
        if(HP <= 0)
        {
            HP = 0;
            damageDetails.Fainted = true;
        }
        return damageDetails;
    }
    public Move GetRandomMove()
    {
        int r = Random.Range(0, Moves.Count);
        return Moves[r];
    }


}

public class DamageDetails
{
        public bool Fainted {get; set;}
        public float Critical{get; set;}
        public float Type {get; set;}
        
}
