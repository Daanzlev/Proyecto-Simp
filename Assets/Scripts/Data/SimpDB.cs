using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpDB
{
    static Dictionary<string, SimpBase> simps;

    public static void Init()
    {
        simps = new Dictionary<string, SimpBase>();

        var simpArray = Resources.LoadAll<SimpBase>("");
        foreach (var simp in simpArray)
        {
            if (simps.ContainsKey(simp.Name))
            {
                Debug.LogError($"There are two simps with the name {simp.Name}");
                continue;
            }

            simps[simp.Name] = simp;
        }
    }

    public static SimpBase GetSimpByName(string name)
    {
        if (!simps.ContainsKey(name))
        {
            Debug.LogError($"Simp with name {name} not found in the database");
            return null;
        }

        return simps[name];
    }
}
