using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class SimpParty : MonoBehaviour
{
    [SerializeField] List<Simp> simps;

    public List<Simp> Simps {
        get { return simps; }
    }

    private void Start ()
    {
        foreach (var simp in simps)
        {
            simp.Init();
        }
    }

    public Simp GetHealthySimp()
    {
       return simps.Where(x => x.HP > 0).FirstOrDefault();
    }
}
