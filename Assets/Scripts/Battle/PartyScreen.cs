using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class PartyScreen : MonoBehaviour
{
    [SerializeField] Text messageText;
    //[SerializeField] List<BattleHud> memberHuds;
    PartyMemberUI [] memberSlots;

    public void Init()
    {
        memberSlots = GetComponentsInChildren<PartyMemberUI>();
    }

    public void SetPartyData (List<Simp>simps)
    {
        for (int i = 0; i < memberSlots.Length; i++)
        {
            if (i < simps.Count)
            {
                //memberSlots[i].gameObject.SetActive(true);
                memberSlots[i].SetData(simps[i]);
            }
            else
            {
                memberSlots[i].gameObject.SetActive(false);
            }

            messageText.text = "Choose your Simp";
        }
    }

}
