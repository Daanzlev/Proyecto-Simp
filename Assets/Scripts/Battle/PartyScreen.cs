using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class PartyScreen : MonoBehaviour
{
    [SerializeField] Text messageText;
    //[SerializeField] List<BattleHud> memberHuds;
    PartyMemberUI [] memberSlots;
    List<Simp> simps;

    public void Init()
    {
        memberSlots = GetComponentsInChildren<PartyMemberUI>(true);
    }

    public void SetPartyData (List<Simp>simps)
    {
        this.simps = simps;
        
        for (int i = 0; i < memberSlots.Length; i++)
        {
            if (i < simps.Count)
            {
                memberSlots[i].gameObject.SetActive(true);
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

    public void UpdateMemberSelection(int selMember)
    {
        for (int i = 0; i < simps.Count; i++)
        {
            if (i == selMember)
            {
                memberSlots[i].SetSelected(true);
            }
            else
            {
                memberSlots[i].SetSelected(false);
            }
        }
    }


    public void SetMessageText(string message)
    {
        messageText.text = message;
    }
}
