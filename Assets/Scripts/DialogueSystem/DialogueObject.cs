using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Dialogue", menuName = "Dialogue/Create new Dialogue")]
public class DialogueObject : ScriptableObject
{
    public string Name;
    public string Text;
    public string Title;
    public Sprite_Side Side;
    public Dialogue_Expresion Expresion;
    public Dialogue_Animation Action;
    public DialogueObject[] Next = new DialogueObject[1];
}