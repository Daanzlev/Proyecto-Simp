using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

// This container has the sprite or sprites of the character as well as the background image, including the functions to interact with it
namespace DIALOGUE{
    [System.Serializable]
    public class BackAndCharContainer
    {
        [SerializeField] private Image character;
        [SerializeField] private RawImage background;

        private Sprite[] multipleCharSprites = {}; // Used in case there are multiple sprites in a conversation

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

        // This unction is for changing the currently shown sprite if a list has been set
        public void changeCharIndex(int index)
        {
            // Make sure there is a sprite list
            if (multipleCharSprites.Length == 0)
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
