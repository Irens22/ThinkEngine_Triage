using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AI;

public enum PatientPose { Stand, Sit, LieDown }
public enum TriageColor { white, green, blue, orange, red }
public enum PatientState { arrived, in_triage, waiting, in_visit, needs_therapy, in_therapy, ready_discharge, in_discharge }

[RequireComponent(typeof(AgentMover))]
[RequireComponent(typeof(NavMeshAgent))]
public class PatientAI : MonoBehaviour, IWakeable
{
    private Transform currentAssignedSeat;
    private bool isWalkingToSeat = false;

    [Header("ASP Sensor Variables")]
    public int patientId;
    public TriageColor triageColor;
    public PatientState currentState;
    public string aspTriageColor => triageColor.ToString();
    public string aspCurrentState => currentState.ToString();

    [Header("Components")]
    public Animator animator;
    private NavMeshAgent _agent;
    
    [Header("Time & Worsening Settings")]
    public float simulationTimeMultiplier = 1f; 
    
    private float maxWaitWhite = 480f * 60;
    private float maxWaitGreen = 240f * 60;
    private float maxWaitBlue = 120f * 60;
    private float maxWaitOrange = 30f * 60;
    private float maxWaitRed = 10f * 60; 

    private float currentWaitLimit; 
    private float timeWaiting; 
    private bool isWaitingInCycle = false;

    [Header("UI Reference")]
    public Slider timerSlider; 
    public Image fillImage; 

    private void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
        animator = GetComponentInChildren<Animator>();
        currentWaitLimit = maxWaitWhite; //default
        if(animator != null)
        {
            animator.applyRootMotion = false;
        }
        
        if(_agent != null)
        {
            _agent.updatePosition = true;
            _agent.updateRotation = true;
        }
    }
    
    private void Start()
    {
        if(IdRegistry.Instance != null && !IdRegistry.Instance.Patients.ContainsKey(patientId))
        {
            IdRegistry.Instance.RegisterPatient(patientId, transform);
        }
        GoToWaitingRoom();
    }

    private void Update()
    {
        if(_agent == null) return;
        if(_agent.enabled)
        {
            bool isMoving = _agent.velocity.sqrMagnitude > 0.1f;
            if(animator != null)
            {
                animator.SetBool("isWalking", isMoving);
                if(isMoving)
                {
                    animator.SetBool("isSitting", false);
                    animator.SetBool("isLyingDown", false);
                }
            }
        }

        if(isWaitingInCycle)
        {
            timeWaiting += Time.deltaTime * simulationTimeMultiplier;
            if(timerSlider != null)
            {
                timerSlider.value = timeWaiting/ currentWaitLimit;
            }

            if(timeWaiting >= currentWaitLimit)
            {
                WorsenCondition();
            }
        }

        if(isWalkingToSeat && _agent.hasPath && !_agent.pathPending && _agent.remainingDistance <= _agent.stoppingDistance + 0.1f)
        {
            isWalkingToSeat = false; 
            if(currentAssignedSeat != null)
            {
                AlignAndPose(currentAssignedSeat, PatientPose.Sit);
            }
        }
    }

    public void UpdateState(PatientState newState)
    {
        currentState = newState;
        isWalkingToSeat = false; 
        if(currentAssignedSeat != null)
        {
            WaitingRoomManager.Instance.ReleaseSeat(currentAssignedSeat);
            currentAssignedSeat = null;
        }
        
        if(currentState == PatientState.ready_discharge || currentState == PatientState.in_discharge)
        {
            isWaitingInCycle = false;
            if(timerSlider != null) timerSlider.gameObject.SetActive(false);
        }
        else
        {
            timeWaiting = 0f;
            isWaitingInCycle = true; 
            if(timerSlider != null) timerSlider.gameObject.SetActive(true);
        }
    }

    public void UpdateColor(TriageColor newColor)
    {
        triageColor = newColor;
        
        timeWaiting = 0f; 
        isWaitingInCycle = true;
        
        if(timerSlider != null) timerSlider.gameObject.SetActive(true);

        switch(triageColor)
        {
            case TriageColor.white:
                currentWaitLimit = maxWaitWhite;
                UpdateUIBarColor(new Color(0.85f, 0.85f, 0.85f)); //grigio chiaro (maggiore visibilità rispetto al bianco)
                break;
            case TriageColor.green:
                currentWaitLimit = maxWaitGreen;
                UpdateUIBarColor(Color.green);
                break;
            case TriageColor.blue:
                currentWaitLimit = maxWaitBlue;
                UpdateUIBarColor(Color.cyan); 
                break;
            case TriageColor.orange:
                currentWaitLimit = maxWaitOrange;
                UpdateUIBarColor(new Color(1f, 0.5f, 0f)); 
                break;
            case TriageColor.red:
                currentWaitLimit = maxWaitRed;
                UpdateUIBarColor(Color.red);
                break;
        }
    }

    private void UpdateUIBarColor(Color uiColor)
    {
        if(fillImage != null)
        {
            fillImage.color = uiColor;
        }
    }

    private void WorsenCondition()
    {
        switch(triageColor)
        {
            case TriageColor.white:
                UpdateColor(TriageColor.green);
                break;
            case TriageColor.green:
                UpdateColor(TriageColor.blue);
                break;
            case TriageColor.blue:
                UpdateColor(TriageColor.orange);
                break;
            case TriageColor.orange:
                UpdateColor(TriageColor.red);
                break;
            case TriageColor.red:
                Debug.LogWarning($"[Paziente {patientId}] CODICE ROSSO SCADUTO");
                LeaveHospital();
                break;
        }
    }

    public void LeaveHospital()
    {
        if(IdRegistry.Instance != null)
        {
            IdRegistry.Instance.UnregisterPatient(patientId);
        }
        
        if(currentAssignedSeat != null && WaitingRoomManager.Instance != null)
        {
            WaitingRoomManager.Instance.ReleaseSeat(currentAssignedSeat);
            currentAssignedSeat = null;
        }

        if(IdRegistry.Instance != null && IdRegistry.Instance.exitPoint != null)
        {
            AgentMover mover = GetComponent<AgentMover>();
            if (mover != null)
            {
                mover.desiredX = IdRegistry.Instance.exitPoint.position.x;
                mover.desiredY = IdRegistry.Instance.exitPoint.position.y;
                mover.desiredZ = IdRegistry.Instance.exitPoint.position.z;
            }
            WakeUp();
        }
        else
        {
            Debug.LogWarning($"[PatientAI] Nessun exitPoint trovato");
        }
        
        Destroy(gameObject, 15f); 
    }

    public void AlignAndPose(Transform targetTransform, PatientPose pose)
    {
        if(targetTransform == null) return;
        if(_agent != null && _agent.isActiveAndEnabled && _agent.isOnNavMesh)
        {
            _agent.isStopped = true;
            _agent.updateRotation = false;
            _agent.Warp(targetTransform.position); 
        }
        else
        {
            transform.position = targetTransform.position;
        }
        
        transform.rotation = targetTransform.rotation;

        switch(pose)
        {
            case PatientPose.Sit:
                animator.SetBool("isSitting", true);
                break;
            case PatientPose.LieDown:
                animator.SetBool("isLyingDown", true);
                break;
            case PatientPose.Stand:
                break;
        }
    }

    public void GoToWaitingRoom()
    {
        if(WaitingRoomManager.Instance == null)
        {
            Debug.LogWarning($"[PatientAI] WaitingRoomManager è null");
            return;
        }

        currentAssignedSeat = WaitingRoomManager.Instance.GetRandomAvailableSeat();
        Vector3 targetPos;
        if(currentAssignedSeat != null)
        {
            targetPos = currentAssignedSeat.position;
            isWalkingToSeat = true;
        }
        else
        {
            targetPos = WaitingRoomManager.Instance.GetRandomStandingPoint();
        }

        AgentMover mover = GetComponent<AgentMover>();
        if(mover != null)
        {
            mover.desiredX = targetPos.x;
            mover.desiredY = targetPos.y;
            mover.desiredZ = targetPos.z;
        }
    }

    public void LeaveWaitingRoom()
    {
        if(currentAssignedSeat != null && WaitingRoomManager.Instance != null)
        {
            WaitingRoomManager.Instance.ReleaseSeat(currentAssignedSeat);
            currentAssignedSeat = null;
        }
        
        isWaitingInCycle = false; //blocca il timer durante le cure
        isWalkingToSeat = false;  //elimina il comando di andare a sedersi
        
        if(_agent != null && _agent.isActiveAndEnabled && _agent.isOnNavMesh)
        {
            _agent.isStopped = false; 
            _agent.ResetPath(); 
        }
        WakeUp(); 
    }

    public RoomManager GetRoomByPatientId(int id)
    {
        foreach(var r in IdRegistry.Instance.Rooms)
        {            
            if(r.Value.TryGetComponent(out RoomManager room))
            {
                if(room.currentPatientId == id)
                    return room;
            }
        }
        return null;
    }

    public void WakeUp()
    {
        if(animator != null)
        {
            animator.SetBool("isSitting", false);
            animator.SetBool("isLyingDown", false);
        }
        
        if(_agent != null && _agent.isActiveAndEnabled && _agent.isOnNavMesh)
        {
            _agent.isStopped = false;
            _agent.updateRotation = true;
            _agent.ResetPath(); 
        }
    }

    private void OnDestroy()
    {
        if(currentAssignedSeat != null && WaitingRoomManager.Instance != null)
        {
            WaitingRoomManager.Instance.ReleaseSeat(currentAssignedSeat);
            currentAssignedSeat = null;
        }
        if(IdRegistry.Instance != null)
        {
            IdRegistry.Instance.UnregisterPatient(patientId);
        }
    }
}