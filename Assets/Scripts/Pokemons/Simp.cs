using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;


[System.Serializable]
public class Simp {

    /* ----------------- ATRIBUTOS ----------------- */
    [SerializeField] SimpBase _base;
    [SerializeField] int level;



    public SimpBase Base { 
        get { return _base; } 
       // set { _base = value; } 
    }

    public int Level { 
        get { return level; } 
        //set { level = value; } 
    }

    /* ----------------- GETTERS ----------------- */
    public int HP { get; set; }
    public List<Move> Moves { get; set; }

    public Dictionary<Stat, int> Stats { get; private set; }
    public Dictionary<Stat, int> StatBoosts { get; private set; }

    public Queue<string> StatusChanges { get; private set; } = new Queue<string>();


    public int Attack {
        get { return GetStat( Stat.Attack ); }
    }

    public int Defense {
        get { return GetStat(Stat.Defense); }
    }

    public int SpAttack {
        get { return GetStat(Stat.SpAttack); }
    }

    public int SpDefense {
        get { return GetStat(Stat.SpDefense) ; }
    }

    public int Speed {
        get { return GetStat(Stat.Speed); }
    }

    public int MaxHP { get; private set; } 



    /* ----------------- METODOS ----------------- */
    public void Init() {

        //Creación de Moves
        Moves = new List<Move>();

        foreach (var move in Base.LearnableMoves) {

            if (move.Level <= Level) {
                Moves.Add(new Move(move.Base));
            }

            if (Moves.Count >= 4) {
                break;
            }

        }

        CalculateStats();

        HP = MaxHP;

        ResetStatBoost();

    }

    void CalculateStats() {

        Stats = new Dictionary<Stat, int>();

        Stats.Add( Stat.Attack, Mathf.FloorToInt( (Base.Attack * Level)/100f ) + 5 );
        Stats.Add(Stat.Defense, Mathf.FloorToInt( (Base.Defense * Level)/100f ) + 5);
        Stats.Add(Stat.SpAttack, Mathf.FloorToInt( (Base.SpAttack * Level)/100f ) + 5);
        Stats.Add(Stat.SpDefense, Mathf.FloorToInt( (Base.SpDefense * Level)/100f ) + 5);
        Stats.Add(Stat.Speed, Mathf.FloorToInt( (Base.Speed * Level)/100f ) + 5);

        MaxHP = Mathf.FloorToInt( (Base.MaxHP * Level)/100f ) + 10;

    }

    void ResetStatBoost () 
    {
        StatBoosts = new Dictionary<Stat, int>() {
            {Stat.Attack, 0},
            {Stat.Defense, 0},
            {Stat.SpAttack, 0},
            {Stat.SpDefense, 0},
            {Stat.Speed, 0}
        };
    }

    int GetStat(Stat stat) {

        int statVal = Stats[stat];

        // Add Boost
        int boost = StatBoosts[stat];
        float[] boostValues = { 1f, 1.5f, 2f, 2.5f, 3f, 3.5f, 4f };

        if (boost >= 0) {
            statVal = Mathf.FloorToInt( statVal * boostValues[boost] );
        }
        else {
            statVal = Mathf.FloorToInt( statVal / boostValues[-boost] );
        }

        return statVal;

    }

    public void ApplyBoosts( List<StatBoost> statBoosts  ) {

        foreach (var statBoost in statBoosts) {

            Stat stat = statBoost.stat;
            int boost = statBoost.boost;

            StatBoosts[stat] = Mathf.Clamp(StatBoosts[stat] + boost, -6, 6);

            if (boost > 0)
                StatusChanges.Enqueue($"{Base.Name}'s {stat} rose!");
            else
                StatusChanges.Enqueue($"{Base.Name}'s {stat} fell!");
        }

    }

    public DamageDetails TakeDamage(Move move , Simp attacker) {
        
        float critical = 1f;

        if(Random.value * 100f < 6.25f) {
            critical = 2f;
        }

        float type = TypeChart.GetEffectiveness(move.Base.Type, Base.Type1) * TypeChart.GetEffectiveness(move.Base.Type, Base.Type2);

        var damageDetails = new DamageDetails() {
            Type = type,
            Critical = critical,
            Fainted = false
        };

        float attack = (move.Base.Category == MoveCategory.Special) ? attacker.SpAttack : attacker.Attack;
        float defense = (move.Base.Category == MoveCategory.Special) ? SpDefense : Defense;

        float modifiers = Random.Range(0.85f, 1f) *type * critical;
        float a = (2 * attacker.Level + 10) / 250f;      
        float d = a * move.Base.Power * ((float)attack / defense) + 2;
        int damage = Mathf.FloorToInt(d * modifiers);

        HP -= damage;

        if(HP <= 0) {
            HP = 0;
            damageDetails.Fainted = true;
        }

        return damageDetails;

    }

    public Move GetRandomMove() {
        int r = Random.Range(0, Moves.Count);
        return Moves[r];
    }

    public void OnBattleOver()
    {
        ResetStatBoost();
    }
}



public class DamageDetails {

        public bool Fainted {get; set;}
        public float Critical{get; set;}
        public float Type {get; set;}
        
}


