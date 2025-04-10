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
        }
    };
}
public enum ConditionID
{
    none, psn, brn, slp, par, frz
}
