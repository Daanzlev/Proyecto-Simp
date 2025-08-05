using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

// This container is speciffically for the name of the speaker in the dialogue box
// It has its own section so it can be hidden, and if no name is given to the speaker it will hide by default

namespace DIALOGUE{
[System.Serializable]
public class NameContainer
{
    [SerializeField] private GameObject root;
    [SerializeField] private TextMeshProUGUI nameText;

    public void Show(string nameToShow = ""){
        root.SetActive(true);

        if(nameToShow != string.Empty){
            nameText.text = nameToShow;
        }
    }

    public void Hide(){
        root.SetActive(false);
    }
}
}