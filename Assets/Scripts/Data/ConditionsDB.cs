using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConditionsDB
{
    public static void Init()
    {
        foreach (var kvp in Conditions)
        {
            var conditionId = kvp.Key;
            var condition = kvp.Value;

            condition.Id = conditionId;
        }
    }

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
        },

        // Volatile Status Conditions
        {
            ConditionID.confusion,
            new Condition()
            {
                Name = "Confusion",
                StartMessage = "has been confused",
                OnStart = (Simp simp) =>
                {
                    // Confused for 1 - 4 turns
                    simp.VolatileStatusTime = Random.Range(1, 5);
                    Debug.Log($"Will be confused for {simp.VolatileStatusTime} moves");
                },
                OnBeforeMove = (Simp simp) =>
                {
                    if (simp.VolatileStatusTime <= 0)
                    {
                        simp.CureVolatileStatus();
                        simp.StatusChanges.Enqueue($"{simp.Base.Name} kicked out of confusion!");
                        return true;
                    }
                    simp.VolatileStatusTime--;

                    // 50% chance to do a move
                    if (Random.Range(1, 3) == 1)
                        return true;

                    // Hurt by confusion
                    simp.StatusChanges.Enqueue($"{simp.Base.Name} is confused");
                    simp.UpdateHp(simp.MaxHP / 8);
                    simp.StatusChanges.Enqueue($"It hurt itself due to confusion");
                    return false;
                }
            }
        }
    };
}
public enum ConditionID
{
    none, psn, brn, slp, par, frz, confusion
}
