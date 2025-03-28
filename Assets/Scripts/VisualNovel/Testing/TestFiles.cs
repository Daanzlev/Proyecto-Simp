using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TESTING{
public class TestFiles : MonoBehaviour
{
    //private string fileName = "testFile.txt";// load file directly
    //private string fileName = "testFile";// load from unity Assets/Resources
    [SerializeField] private TextAsset fileName; // as serialzied field, this is probably the best

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(Run());
    }

    IEnumerator Run(){
        List<string> lines = FileManager.ReadTextAsset(fileName, false);

        foreach(string line in lines){
            Debug.Log(line);
        }
        yield return null;
    }
}
}