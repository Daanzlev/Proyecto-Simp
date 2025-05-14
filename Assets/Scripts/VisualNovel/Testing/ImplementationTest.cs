using DIALOGUE;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ImplementationTest : MonoBehaviour
{
    [SerializeField] private TextAsset file;
    
    private GameObject VNsys;

    void Start()
    {
        GameObject obj = GameObject.Find("VN Controller");
        if (obj != null){
            VNsys = obj;
            obj.SetActive(false);
        }
        else{
            Debug.LogWarning("GameObject not found!");
        }
    }

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.K)){
            StartConversation();
        }
    }

    // Update is called once per frame
    void StartConversation()
    {
        if (VNsys != null){
            VNsys.SetActive(true);
            List<string> lines = FileManager.ReadTextAsset(file, false);
            DialogueSystem.instance.Say(lines);
        }
        else{
            Debug.LogWarning("GameObject not found!");
        }
    }
}
