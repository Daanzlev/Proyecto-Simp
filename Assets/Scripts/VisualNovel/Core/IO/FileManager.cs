using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class FileManager
{
    // Theres 2 of this because in unity editor files are different than in final build ?
    // pretty standard boilerplate for reading text into strings
    public static List<string> ReadTextFile(string filePath, bool includeBlankLines = true){
        // in case path is absolute or not
        if (!filePath.StartsWith('/')){
            filePath = FilePath.root + filePath;
        }

        //read lines into strings
        List<string> lines = new List<string>();
        try{
            using (StreamReader sr = new StreamReader(filePath))
            {
                while(!sr.EndOfStream)
                {
                    string line = sr.ReadLine();
                    if(includeBlankLines || !string.IsNullOrWhiteSpace(line)){
                        lines.Add(line);
                    }
                }
            }

        }catch(FileNotFoundException ex){
            Debug.LogError($"File not found: {ex.FileName}");
        }

        return lines;
    }

    //same as above but as a unity resource
    // all this works with the assets in Assets/Resources/ or passed in as a serialized field, otherwise wont work
    // also remember that when using this, the file path need no extensions ie: .txt
    public static List<string> ReadTextAsset(string filePath, bool includeBlankLines = true){
        TextAsset asset = Resources.Load<TextAsset>(filePath);
        if(asset == null){
            Debug.LogError($"Asset not found: {filePath}");
            return null;
        }

         return ReadTextAsset(asset, includeBlankLines);

    }

    public static List<string> ReadTextAsset(TextAsset asset, bool includeBlankLines = true){
        List<string> lines = new List<string>();

        using(StringReader sr = new StringReader(asset.text)){
            while(sr.Peek() > -1){
                string line = sr.ReadLine();
                if(includeBlankLines || !string.IsNullOrWhiteSpace(line)){
                    lines.Add(line);
                }
            }
        }
        return lines;
    }
}
