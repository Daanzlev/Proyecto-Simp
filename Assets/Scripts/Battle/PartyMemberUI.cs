using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PartyMemberUI : MonoBehaviour
{
   [SerializeField] Text nameText;
    [SerializeField] Text levelText;
    [SerializeField] HPBar hpBar;
    [SerializeField] Color selectedColor;

    Simp _simp;
    public void SetData(Simp simp)
    {
        _simp = simp;
        nameText.text= simp.Base.Name;
        levelText.text= "Lvl " + simp.Level;
        hpBar.SetHP((float) simp.HP / simp.MaxHP);
    }

    public void SetSelected(bool selected)
    {
        if (selected)
        {
            nameText.color = selectedColor;
        }
        else
        {
            nameText.color = Color.black;
        }
    }

}
