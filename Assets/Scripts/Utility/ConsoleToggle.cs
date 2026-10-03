using UnityEngine;

public class ConsoleToggle : MonoBehaviour
{
    [Header("La mia Console")]
    public CanvasGroup consoleGroup;
    
    private bool isVisible = true;

    private void Start()
    {
        UpdateVisibility();
    }

    public void Toggle()
    {
        isVisible = !isVisible;
        UpdateVisibility();
    }

    private void UpdateVisibility()
    {
        //se è visibile l'alpha: 1 (opaco), altrimenti 0 (invisibile)
        consoleGroup.alpha = isVisible ? 1f : 0f;
        
        //disabilita l'interazione del mouse quando è invisibile
        consoleGroup.interactable = isVisible;
        consoleGroup.blocksRaycasts = isVisible; 
    }
}