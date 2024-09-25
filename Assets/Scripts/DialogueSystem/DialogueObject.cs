using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Dialogue", menuName = "Dialogue/Create new Dialogue")]
public class DialogueObject : ScriptableObject
{
    public string Name;
    public string Text;
    public DialogueObject[] Next;
}
