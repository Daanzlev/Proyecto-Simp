using DG.Tweening;
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
    [SerializeField] GameObject expBar;

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
        SetLevel();
        hpBar.SetHP((float) simp.HP / simp.MaxHP);
        SetExp();

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

    public void SetLevel()
    {
        levelText.text = "Lvl " + _simp.Level;
    }

    public void SetExp()
    {
        if (expBar == null) return; 

        float normalizedExp = GetNormalizedExp();
        expBar.transform.localScale = new Vector3 (normalizedExp, 1, 1);
    }
    public IEnumerator SetExpSmooth(bool reset=false)
    {
        if (expBar == null) yield break;

        if (reset)
            expBar.transform.localScale = new Vector3(0, 1, 1);

        float normalizedExp = GetNormalizedExp();
        yield return expBar.transform.DOScaleX(normalizedExp, 1.5f).WaitForCompletion();
    }
    float GetNormalizedExp()
    {
        int currLevelExp = _simp.Base.GetExpForLevel(_simp.Level);
        int nextLevelExp = _simp.Base.GetExpForLevel(_simp.Level + 1);

        float normalizedExp = (float)(_simp.Exp - currLevelExp) / (nextLevelExp - currLevelExp);
        return Mathf.Clamp01(normalizedExp);
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
