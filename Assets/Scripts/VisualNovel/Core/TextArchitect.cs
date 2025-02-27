using System.Collections;
using UnityEngine;
using TMPro;

public class TextArchitect
{
    // Could use Text mesh pro ui or world, no matter which it get assigned to tmpro
    private TextMeshProUGUI tmpro_ui;
    private TextMeshPro tmpro_world;
    public TMP_Text tmpro => tmpro_ui != null ? tmpro_ui : tmpro_world;

    // reference to text being built and current text in the text mesh pro
    public string currentText => tmpro.text;
    public string targetText {get; private set;} = "";
    public string preText {get; private set;} = "";
    private int preTextLength = 0;

    public string fullTargetText => preText + targetText;

    // DIfferent ways the text can appear
    public enum BuildMethod {instant, typewriter, fade}
    public BuildMethod buildMethod = BuildMethod.typewriter;

    //Color
    public Color textColor {get {return tmpro.color;} set {tmpro.color = value;}}

    // Text Speed
    public float speed {get{return baseSpeed * speedMultiplier;} set{speedMultiplier = value;}}
    private const float baseSpeed = 1;
    private float speedMultiplier = 1;

    // Sped Up
    public int charactersPerCycle {get {return speed <= 2 ? characterMultiplier : speed <= 2.5f ? characterMultiplier * 2 : characterMultiplier * 3;}}
    private int characterMultiplier = 1;
    public bool hurryUp = false;

    // Constructors
    public TextArchitect(TextMeshProUGUI tmpro_ui){
        this.tmpro_ui = tmpro_ui;
    }
    public TextArchitect(TextMeshPro tmpro_world){
        this.tmpro_world = tmpro_world;
    }
    // The text works by "building" or "appending" to the text

    // Coroutine to start building text
    public Coroutine Build(string text){
        preText = "";
        targetText = text;

        Stop();

        buildProcess = tmpro.StartCoroutine(Building());
        return buildProcess;
    }

    // same as build but start with text
    public Coroutine Append(string text){
        preText = tmpro.text;
        targetText = text;

        Stop(); 

        buildProcess = tmpro.StartCoroutine(Building());
        return buildProcess; 
    }

    //Stops a currently running build
    private Coroutine buildProcess = null;
    public bool isBuilding => buildProcess != null;
    public void Stop(){
        if (!isBuilding){
            return;
        }
        tmpro.StopCoroutine(buildProcess);
        buildProcess = null;
    }

    // Building process
    IEnumerator Building(){
        Prepare();

        //Switch depending on how text appearing
        switch(buildMethod){
            case BuildMethod.typewriter:
            yield return Build_Typewriter();
            break;

            case BuildMethod.fade:
            yield return Build_Fade();
            break;
        }


    }
    //What to do once build is done
    private void OnComplete(){
        buildProcess = null;
        hurryUp = false;
    }

    public void ForceComplete(){
        switch(buildMethod){
            case BuildMethod.typewriter:
                tmpro.maxVisibleCharacters = tmpro.textInfo.characterCount;
            break;
            case BuildMethod.fade:
            break;
        }

        Stop();
        OnComplete();
    }

    //prepare for building
    private void Prepare(){
        switch(buildMethod){
            case BuildMethod.instant:
                Prepare_Instant();
            break;
            case BuildMethod.typewriter:
                Prepare_Typewriter();
            break;
            case BuildMethod.fade:
                Prepare_Fade();
            break;
        }
    }

    //prepare for the different methods
    private void Prepare_Instant(){
        tmpro.color = tmpro.color;
        tmpro.text = fullTargetText;
        tmpro.ForceMeshUpdate();
        tmpro.maxVisibleCharacters = tmpro.textInfo.characterCount;
    }
    private void Prepare_Typewriter(){
        tmpro.color = tmpro.color;
        tmpro.maxVisibleCharacters = 0;
        tmpro.text = preText;

        if(preText != ""){
            tmpro.ForceMeshUpdate();
            tmpro.maxVisibleCharacters = tmpro.textInfo.characterCount;
        }

        tmpro.text += targetText;
        tmpro.ForceMeshUpdate();
    }
    private void Prepare_Fade(){
        
    }

    // Function for how to build on the different build modes
    private IEnumerator Build_Typewriter(){
        while(tmpro.maxVisibleCharacters < tmpro.textInfo.characterCount){
            tmpro.maxVisibleCharacters += hurryUp ? charactersPerCycle * 5 : charactersPerCycle;
            yield return new WaitForSeconds(0.015f / speed);
        }
        OnComplete();
    }
    private IEnumerator Build_Fade(){
        yield return null;
    }
}