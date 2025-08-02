using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections.Generic;

public class ChoiceManager : MonoBehaviour
{
    public static ChoiceManager instance;

    [SerializeField] private GameObject buttonPrefab;
    [SerializeField] private GameObject buttonParent;

    private Action<int> onChoiceSelected;

    private void Awake()
    {
        instance = this;
        buttonParent.SetActive(false);
    }

    public void ShowChoices(List<string> choices, Action<int> callback)
    {
        buttonParent.SetActive(true);
        ClearChoices();

        onChoiceSelected = callback;

        for (int i = 0; i < choices.Count; i++)
        {
            int index = i;
            GameObject btnObj = Instantiate(buttonPrefab, buttonParent.transform);
            btnObj.GetComponentInChildren<TMP_Text>().text = choices[i];
            btnObj.GetComponent<Button>().onClick.AddListener(() => MakeChoice(index));
        }
    }

    public void ClearChoices()
    {
        foreach (Transform child in buttonParent.transform)
        {
            Destroy(child.gameObject);
        }
    }

    private void MakeChoice(int choiceIndex)
    {
        gameObject.SetActive(false);
        onChoiceSelected?.Invoke(choiceIndex);
    }

    public void hide()
    {
        buttonParent.SetActive(false);
    }
}
