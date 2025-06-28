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

        private Sprite[] multipleCharSprites = {};

        public void ShowBackGround(bool shown) {
            background.gameObject.SetActive(shown);
        }

        public void ShowCharacter(bool shown) {
            character.gameObject.SetActive(shown);
        }

        public void ChangeBackground(Texture newBackground)
        {
            background.texture = newBackground;
        }

        public void ChangeCharacter(Sprite newCharacter)
        {
            character.sprite = newCharacter;
        }


        //If more sprites are used in a conversation, use the following to set the multiple sprites
        public void setCharacterSprites(Sprite[] spriteSet)
        {
            multipleCharSprites = spriteSet;
            ChangeCharacter(multipleCharSprites[0]);
        }

        public void changeCharIndex(int index)
        {
            if(multipleCharSprites.Length == 0)
            {
                Debug.Log("No sprite set given yet");
                return;
            }
            try
            {
                ChangeCharacter(multipleCharSprites[index]);
            }
            catch
            {
                Debug.Log("Something wrong when selecting Sprite, probably index issue");
            }
        }
}
}
