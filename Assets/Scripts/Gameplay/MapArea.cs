using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapArea : MonoBehaviour
{
    [SerializeField] List<Simp> wildSimps;

    public Simp GetRandomWildSimp()
    {
        var wildSimp = wildSimps[Random.Range(0, wildSimps.Count)];
        wildSimp.Init();
        return wildSimp;
        
    }
}
