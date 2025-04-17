using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConditionsDB
{

    public static Dictionary<ConditionID, Condition> Conditions { get; private set; } = new Dictionary<ConditionID, Condition>()
    {
        {
            ConditionID.psn,
            new Condition()
            {
                Name = "Poison",
                StartMessage = "has been poisoned",
                OnAfterTurn = (Simp simp) =>
                {
                    simp.UpdateHp(simp.MaxHP / 8);
                    simp.StatusChanges.Enqueue($"{simp.Base.name} hurt itself due to poison");
                }
            }
        },
        {
            ConditionID.brn,
            new Condition()
            {
                Name = "Burn",
                StartMessage = "has been burned",
                OnAfterTurn = (Simp simp) =>
                {
                    simp.UpdateHp(simp.MaxHP / 16);
                    simp.StatusChanges.Enqueue($"{simp.Base.name} hurt itself due to burn");
                }
            }
        },
        {
            ConditionID.par,
            new Condition()
            {
                Name = "Paralyzed",
                StartMessage = "has been paralyzed",
                OnBeforeMove = (Simp simp) =>
                {
                    if (Random.Range(1,5) == 1)
                    {
                        simp.StatusChanges.Enqueue($"{simp.Base.name}'s paralyzed and can't move");
                        return false;
                    }
                    return true;
                }
            }
        },
        {
            ConditionID.frz,
            new Condition()
            {
                Name = "Freeze",
                StartMessage = "has been frozen",
                OnBeforeMove = (Simp simp) =>
                {
                    if (Random.Range(1,5) == 1)
                    {
                        simp.CureStatus();
                        simp.StatusChanges.Enqueue($"{simp.Base.name}'s is not frozen anymore");
                        return true;
                    }
                    return false;
                }
            }
        },
         {
            ConditionID.slp,
            new Condition()
            {
                Name = "Sleep",
                StartMessage = "has fallen asleep",
                OnStart = (Simp simp) =>
                {
                    //Sleep for 1-3 turns
                    simp.StatusTime = Random.Range(1,4);
                    Debug.Log($"Will be asleep for {simp.StatusTime} moves");
                },
                OnBeforeMove = (Simp simp) =>
                {
                    if (simp.StatusTime <= 0)
                    {
                        simp.CureStatus();
                        simp.StatusChanges.Enqueue($"{simp.Base.Name} woke up!");
                        return true;

                    }
                    simp.StatusTime--;
                    simp.StatusChanges.Enqueue($"{simp.Base.Name} is sleeping");
                    return false;

                }
            }
        }
    };
}
public enum ConditionID
{
    none, psn, brn, slp, par, frz
}
