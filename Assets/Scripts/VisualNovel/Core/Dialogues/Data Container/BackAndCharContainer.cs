using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

// Esto es la caja que contiene al objeto del backfround y del personaje para que se puedan modificar
namespace DIALOGUE{
[System.Serializable]
public class BackAndCharContainer
{
    [SerializeField] private Image character; //If there is more than one this would have to be implemented as a list? Check back on this
    [SerializeField] private RawImage background;

    public void ShowBackGround(bool shown){
        background.gameObject.SetActive(shown);
    }

    public void ShowCharacter(bool shown){
        character.gameObject.SetActive(shown);
    }
}
}
