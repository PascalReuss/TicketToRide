using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuScript : MonoBehaviour
{
    //public List<Toggle> toggle;
    //public Toggle recordCasesToggle;
    //public Toggle recordCasesCompleteToggle;
    //public Toggle trainToggle;
    //public ToggleGroup projectSelectionToggle;
    //Toggle[] projectToggles;
    //public TMPro.TMP_Dropdown dropdown;
    string mode;
    public List<Button> isAiButtons;
    public List<GameObject> playernames;
    public List<GameObject> dropdowns;
    public GameObject players;
    public List<ToggleGroup> AIselectionToggle;
    public Toggle recordCasesToggle;
    int playercount;

    public void OnClickStartGame()
    {
        for (int i = 0; i < 5; i++)
        {
            if (players.transform.GetChild(i).gameObject.activeInHierarchy)
                playercount++;
        }

        PlayerPrefs.SetInt("playercount", playercount);

        for (int i = 0; i < playercount; i++)
        {
            //string name = "playerIsAI" + i;
            //if (isAiButtons[i].GetComponent<ToggleButton>().isAI)
            //    PlayerPrefs.SetInt(name, 1);
            //else
            //    PlayerPrefs.SetInt(name, 0);

            PlayerPrefs.SetString("playername" + i, playernames[i].GetComponent<TMP_InputField>().text);
            PlayerPrefs.SetString("playercolor" + i, dropdowns[i].transform.GetComponent<Dropdownexample>().selectedColor);

            Toggle selectedAI = AIselectionToggle[i].ActiveToggles().FirstOrDefault();
            PlayerPrefs.SetString("selectedAI" + i, selectedAI.name);
        }

        if (recordCasesToggle.isOn)
            PlayerPrefs.SetInt("recordingCases", 1);
        else
            PlayerPrefs.SetInt("recordingCases", 0);

        //if (recordCasesCompleteToggle.isOn)
        //    PlayerPrefs.SetInt("recordCasesComplete", 1);
        //else
        //    PlayerPrefs.SetInt("recordCasesComplete", 0);

        //if (trainToggle.isOn)
        //    PlayerPrefs.SetInt("trainRandom", 1);
        //else
        //    PlayerPrefs.SetInt("trainRandom", 0);

        //Toggle selectedProject = projectSelectionToggle.ActiveToggles().FirstOrDefault();
        //PlayerPrefs.SetString("situationType", selectedProject.GetComponentInChildren<TextMeshProUGUI>().text);

        //index = dropdown.value;
        //mode = dropdown.options[dropdown.value].text;

        PlayerPrefs.SetString("playerMode", mode);

        SceneManager.LoadScene(0);
    }

    private void Start()
    {
        recordCasesToggle.isOn = false;
    }

    private void Update()
    {
        //bool interactable = true;
        //for (int i = 0; i < 5; i++)
        //{
        //    if (AIselectionToggle[i].ActiveToggles().Count() != 0)
        //    {
        //        if (AIselectionToggle[i].ActiveToggles().FirstOrDefault().name != "Human")
        //        {
        //            recordCasesToggle.interactable = false;
        //            recordCasesToggle.isOn = false;
        //            interactable = false;
        //        }
        //    }
        //}
        //if (interactable)
        //    recordCasesToggle.interactable = true;
    }

    public void Quit()
    {
        Application.Quit();
        EditorApplication.ExecuteMenuItem("Edit/Play");
    }
}
