using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattleHud : MonoBehaviour
{
    [SerializeField] Text nameText;
    [SerializeField] Text levelText;
    [SerializeField] Text statusText;    
    [SerializeField] HPBar hpBar;


    [SerializeField] Color psnColor;
    [SerializeField] Color brnColor;
    [SerializeField] Color slpColor;
    [SerializeField] Color parColor;
    [SerializeField] Color frzColor;

    Simp _simp;
    Dictionary<ConditionID, Color> statusColors;


    public void SetData(Simp simp)
    {
        _simp = simp;

        nameText.text= simp.Base.Name;
        levelText.text= "Lvl " + simp.Level;
        hpBar.SetHP((float) simp.HP / simp.MaxHP);

        statusColors = new Dictionary<ConditionID, Color>()
        {
            {ConditionID.psn, psnColor },
            {ConditionID.brn, brnColor },
            {ConditionID.slp, slpColor },
            {ConditionID.par, parColor },
            {ConditionID.frz, frzColor },
        };
        

        SetStatusText();
        _simp.OnStatusChanged += SetStatusText;
    }
    void SetStatusText()
    {
        
       if (_simp.Status == null)
        {
            statusText.text = "";
        }
        //No esta entrando al else, no se porque
        else
        {
            statusText.text = "Hola";
            statusText.text = _simp.Status.Id.ToString().ToUpper();
            statusText.color = statusColors[_simp.Status.Id];
        }
    }

    public IEnumerator UpdateHP()
    {
        if (_simp.HpChanged)
        {
            yield return hpBar.SetHPSmooth((float)_simp.HP / _simp.MaxHP);
            _simp.HpChanged = false;
        }

    }
}
