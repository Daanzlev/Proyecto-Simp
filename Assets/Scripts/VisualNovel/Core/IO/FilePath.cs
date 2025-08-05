using UnityEngine;

// Honestly this could maybe be kept in the FileManager scritp as it is only used there

public class FilePath
{
    // this is the root of where data is kept, adjust as needed for game, should work regardless of unity or compiled
    public static readonly string root = $"{Application.dataPath}/Scripts/VisualNovel/Dialogues/";
}
