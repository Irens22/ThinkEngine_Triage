using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

public class Console : MonoBehaviour
{
    public static Console Instance { get; private set; }

    [Header("UI References")]
    public TextMeshProUGUI consoleText;
    public ScrollRect scrollRect;

    [Header("Settings")]
    public int maxLines = 50; 
    public float duplicateCooldown = 2.0f; 
    
    private Queue<string> logLines = new Queue<string>(); 
    private Dictionary<string, float> messageHistory = new Dictionary<string, float>();

    private void Awake()
    {
        if(Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        consoleText.text = "";
        LogMessage("--- SIMULATION STARTED ---", "#FFFFFF");
    }

    public void LogMessage(string message, string colorHex = "#FFFFFF")
    {
        if(messageHistory.ContainsKey(message))
        {
            if(Time.time - messageHistory[message] < duplicateCooldown)
                return;
        }
        messageHistory[message] = Time.time;

        string formattedMessage = $"<color={colorHex}>{message}</color>";
        logLines.Enqueue(formattedMessage);

        if(logLines.Count > maxLines)
        {
            logLines.Dequeue();
        }

        consoleText.text = string.Join("\n", logLines);
        
        Canvas.ForceUpdateCanvases();
        scrollRect.verticalNormalizedPosition = 0f;
    }

    public void LogPatientAction(string message, TriageColor color)
    {
        string hexColor = "#FFFFFF"; //default

        switch(color)
        {
            case TriageColor.white:
                hexColor = "#D3D3D3";
                break;
            case TriageColor.green:
                hexColor = "#00FF00";
                break;
            case TriageColor.blue:
                hexColor = "#00BFFF";
                break;
            case TriageColor.orange:
                hexColor = "#FFA500";
                break;
            case TriageColor.red:
                hexColor = "#FF3333";
                break;
        }

        LogMessage(message, hexColor);
    }

    public void ClearConsole()
    {
        logLines.Clear();
        consoleText.text = "";
        
        messageHistory.Clear(); 
        LogMessage("--- SIMULATION STARTED ---", "#FFFFFF");
    }
}