using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public enum StaffType { doctor, nurse }

[RequireComponent(typeof(AgentMover))]
[RequireComponent(typeof(NavMeshAgent))]
public class StaffAI : MonoBehaviour, IWakeable
{
    [Header("ASP Sensor Variables")]
    public int staffId;
    public StaffType staffType;
    public string aspStaffType => staffType.ToString();
    public string aspStatus;
    public int currentPatientId = -1;

    [Header("Components")]
    public Animator animator;
    private NavMeshAgent _agent;

    [Header("Staff Room Wander Settings")]
    public float wanderRadius = 4f; 
    public float minWaitTime = 3f;  
    public float maxWaitTime = 10f; 
    
    private Coroutine wanderRoutine;

    private void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
        animator = GetComponentInChildren<Animator>();
        aspStatus = "idle";

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
        if(staffType == StaffType.doctor && IdRegistry.Instance != null && !IdRegistry.Instance.Doctors.ContainsKey(staffId))
        {
            IdRegistry.Instance.RegisterDoctor(staffId, transform);
        }
        else if(staffType == StaffType.nurse && IdRegistry.Instance != null && !IdRegistry.Instance.Nurses.ContainsKey(staffId))
        {
            IdRegistry.Instance.RegisterNurse(staffId, transform);
        }
        GoToStaffRoom();
    }

    private void Update()
    {
        if(animator == null || _agent == null || !_agent.isActiveAndEnabled) return;
        bool isMoving = _agent.velocity.sqrMagnitude > 0.1f;
        animator.SetBool("isWalking", isMoving);
    }

    public void AssignTask(int patientId)
    {
        StopWandering(); 
        WakeUp();
        aspStatus = "busy";
        currentPatientId = patientId;
    }

    public void ClearTask()
    {
        aspStatus = "idle";
        currentPatientId = -1;
    }
    
    public void GoToStaffRoom()
    {
        StopWandering();
        WakeUp();
        if(IdRegistry.Instance != null && IdRegistry.Instance.staffRoomPoint != null)
        {
            wanderRoutine = StartCoroutine(ReturnAndWander(IdRegistry.Instance.staffRoomPoint));
        }
    }

    private IEnumerator ReturnAndWander(Transform roomCenter)
    {        
        Vector3 target = roomCenter.position;
        AgentMover mover = GetComponent<AgentMover>();
        if(mover != null)
        {
            mover.desiredX = target.x;
            mover.desiredY = target.y;
            mover.desiredZ = target.z;
        }

        float timeout = 10f;
        while(!HasArrivedRobust() && timeout > 0)
        {
            timeout -= Time.deltaTime;
            yield return null; 
        }

        while(true)
        {
            Vector2 circle = Random.insideUnitCircle * wanderRadius;
            Vector3 randomDirection = roomCenter.position + new Vector3(circle.x, 0f, circle.y);
            
            NavMeshHit hit;
            if(NavMesh.SamplePosition(randomDirection, out hit, wanderRadius, NavMesh.AllAreas))
            {
                if(mover != null)
                {
                    mover.desiredX = hit.position.x;
                    mover.desiredY = hit.position.y;
                    mover.desiredZ = hit.position.z;
                }
            }
            float waitTime = Random.Range(minWaitTime, maxWaitTime);
            yield return new WaitForSeconds(waitTime);
        }
    }

    public void StopWandering()
    {
        if(wanderRoutine != null)
        {
            StopCoroutine(wanderRoutine);
            wanderRoutine = null;
        }
        
        if(_agent != null && _agent.isActiveAndEnabled && _agent.isOnNavMesh)
        {
            _agent.ResetPath();
        }
    }

    public void WakeUp()
    {
        if(_agent != null && _agent.isActiveAndEnabled && _agent.isOnNavMesh)
        {
            _agent.isStopped = false; 
            _agent.ResetPath(); 
        }
        else if(_agent != null && !_agent.enabled) 
        {
            _agent.enabled = true;
        }
    }

    private bool HasArrivedRobust()
    {
        if(_agent == null || !_agent.isActiveAndEnabled || !_agent.isOnNavMesh) return true;
        if(!_agent.pathPending)
        {
            if(_agent.hasPath)
            {
                return _agent.remainingDistance <= _agent.stoppingDistance + 0.2f;
            }
            else
            {
                AgentMover mover = GetComponent<AgentMover>();
                if(mover != null)
                {
                    Vector2 currentXZ = new Vector2(transform.position.x, transform.position.z);
                    Vector2 desiredXZ = new Vector2(mover.desiredX, mover.desiredZ);
                    
                    if(Vector2.Distance(currentXZ, desiredXZ) <= _agent.stoppingDistance + 0.5f)
                    {
                        return true;
                    }
                }
            }
        }
        return false;
    }

    private void OnDestroy()
    {
        if(IdRegistry.Instance != null)
        {
            if(staffType == StaffType.doctor)
                IdRegistry.Instance.UnregisterDoctor(staffId);
            else if(staffType == StaffType.nurse)
                IdRegistry.Instance.UnregisterNurse(staffId);
        }
    }
}