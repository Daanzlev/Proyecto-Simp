using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewCharacter", menuName = "Dialogue/Create new Character")]
public class DialogueCharacter : ScriptableObject
{
    public string Name = "New Character";
    public List<DialogueExpressionSprite> Sprites;

    public Sprite Get(Dialogue_Expresion expresion)
    {
        foreach (DialogueExpressionSprite currentExpresion in Sprites)
        {
            if (currentExpresion.Expresion == expresion)
                return currentExpresion.Sprite;
        }
        return null;
    }
}

[System.Serializable]
public class DialogueExpressionSprite
{
    public Dialogue_Expresion Expresion;
    public Sprite Sprite;
}

[CreateAssetMenu(fileName = "NewIndex", menuName = "Dialogue/Create new Index")]
public class DialogueCharacterIndex : ScriptableObject
{
    public DialogueCharacter[] Characters;

    public DialogueCharacter Get(string name)
    {
        foreach (DialogueCharacter Character in Characters)
        {
            if (Character.Name == name)
                return Character;
        }
        return null;
    }
}