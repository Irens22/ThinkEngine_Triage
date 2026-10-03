using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    public AgentSpawner spawner;
    int docCount, nurseCount, patCount;

    [Header("ThinkEngine")]
    public GameObject plannerBrainHost; 

    [Header("Main Menu UI")]
    public GameObject mainMenuPanel;

    [Header("In Game UI")]
    public GameObject topBarPanel;
    public GameObject infoPanel;
    public GameObject cameraPanel;
    public GameObject inGamePanel;
    public GameObject consoleView;
    
    [Header("Counters MainMenu UI")]
    public TextMeshProUGUI doctorCMainTxt;
    public TextMeshProUGUI nurseCMainTxt;
    public TextMeshProUGUI patientCMainTxt;

    [Header("Counters InGameMenu UI")]
    public TextMeshProUGUI doctorCounterTxt;
    public TextMeshProUGUI nurseCounterTxt;
    public TextMeshProUGUI patientCounterTxt;

    private void Start()
    {
        ResetCounters();
        mainMenuPanel.SetActive(true);
        topBarPanel.SetActive(false);
        inGamePanel.SetActive(false);
        infoPanel.SetActive(false);
        cameraPanel.SetActive(false);
        consoleView.SetActive(false);
        
    }

    public void StartSimulation()
    {
        for(int i = 0; i < docCount; i++) spawner.SpawnDoctor();
        for(int i = 0; i < nurseCount; i++) spawner.SpawnNurse();
        for(int i = 0; i < patCount; i++) spawner.SpawnPatient(true);

        UpdateAllTexts();
        mainMenuPanel.SetActive(false);
        topBarPanel.SetActive(true);
        consoleView.SetActive(true);

        if(plannerBrainHost != null)
        {
            plannerBrainHost.SetActive(true);
        }
    }

    public void ToggleInfoPanel()
    {
        inGamePanel.SetActive(false);
        cameraPanel.SetActive(false);
        infoPanel.SetActive(!infoPanel.activeSelf);
    }

    public void ToggleCameraPanel()
    {
        inGamePanel.SetActive(false);
        infoPanel.SetActive(false);
        cameraPanel.SetActive(!cameraPanel.activeSelf);
    }

    public void ToggleInGameMenu()
    {
        infoPanel.SetActive(false);
        cameraPanel.SetActive(false);
        inGamePanel.SetActive(!inGamePanel.activeSelf);
    }
    
    public void ReadCDoctor() 
    {
        doctorCMainTxt.text = docCount.ToString();
        doctorCounterTxt.text = docCount.ToString(); 
    }
    public void ReadCNurse() 
    { 
        nurseCMainTxt.text = nurseCount.ToString();
        nurseCounterTxt.text = nurseCount.ToString(); 
    }
    public void ReadCPatient() 
    { 
        patientCMainTxt.text = patCount.ToString();
        patientCounterTxt.text = patCount.ToString(); 
    }

    private void UpdateAllTexts()
    {
        ReadCDoctor();
        ReadCNurse();
        ReadCPatient();
    }

    //metodi per UI counters
    public void AddDoctor()
    {
        docCount++;
        ReadCDoctor();
    } 

    public void AddDoctorInGame()
    {
        docCount++;
        ReadCDoctor();
        spawner.SpawnDoctor();
    }

    public void RemoveDoctor()
    {
        if(docCount > 0)
        {
            docCount--;
            ReadCDoctor();
        }
    } 

    public void RemoveDoctorInGame()
    {
        if(docCount > 0)
        {
            docCount--;
            ReadCDoctor();
            spawner.RemoveLastStaff("Doctor");
        }
    }
    
    public void AddNurse()
    {
        nurseCount++;
        ReadCNurse();
    }  

    public void AddNurseInGame()
    {
        nurseCount++;
        ReadCNurse();
        spawner.SpawnNurse();
    }

    public void RemoveNurse()
    {
        if(nurseCount > 0)
        {
            nurseCount--;
            ReadCNurse();
        }
    } 

    public void RemoveNurseInGame()
    {
        if(nurseCount > 0)
        {
            nurseCount--;
            ReadCNurse();
            spawner.RemoveLastStaff("Nurse");
        }
    }

    public void AddPatient()
    {
        patCount++;
        ReadCPatient();
    }

    public void RemovePatient()
    {
        if(patCount > 0)
        {
            patCount--;
            ReadCPatient();
        }
    }

    //metodi spawn inGame per i pazienti (in base al color button)
    public void SpawnRedPatient() {
        spawner.SpawnPatient(false, TriageColor.red);
        patCount++;
        ReadCPatient();
    } 
    public void SpawnOrangePatient() {
        spawner.SpawnPatient(false, TriageColor.orange);
        patCount++;
        ReadCPatient();
    }    
    public void SpawnBluePatient() {
        spawner.SpawnPatient(false, TriageColor.blue);
        patCount++;
        ReadCPatient();
    } 
    public void SpawnGreenPatient() {
        spawner.SpawnPatient(false, TriageColor.green);
        patCount++;
        ReadCPatient();
    } 
    public void SpawnWhitePatient() {
        spawner.SpawnPatient(false, TriageColor.white);
        patCount++;
        ReadCPatient();
    } 

    //metodi di pulizia e stop
    public void StopSimulation()
    {
        if(plannerBrainHost != null)
        {
            plannerBrainHost.SetActive(false);
        }

        spawner.DespawnAll();
        ResetCounters();
        
        topBarPanel.SetActive(false);
        inGamePanel.SetActive(false);
        infoPanel.SetActive(false);
        cameraPanel.SetActive(false);
        mainMenuPanel.SetActive(true);

        CameraController camController = Camera.main.GetComponent<CameraController>();
    
        if(camController != null)
        {
            camController.ResetCamera();
        }
    }

    private void ResetCounters()
    {
        docCount = 0;
        nurseCount = 0;
        patCount = 0;
        UpdateAllTexts();
    }

    public void Quit(){ Application.Quit(); }
}