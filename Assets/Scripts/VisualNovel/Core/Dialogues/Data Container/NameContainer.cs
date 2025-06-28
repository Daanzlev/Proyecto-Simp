using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

// Esto es la caja que contiene el texto de nombre en la pantalla, parte del dialogue container
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