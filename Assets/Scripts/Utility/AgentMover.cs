using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class AgentMover : MonoBehaviour
{
    [Header("Target Coordinates")]
    public float desiredX;
    public float desiredY;
    public float desiredZ;

    [Header("Movement Settings")]
    public float stoppingDistance = 0.5f;

    private NavMeshAgent _agent;
    private Vector3 _currentTarget;
    private IWakeable _linkedAgent; 
    private bool _hasWokenUp = false;

    void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
        _linkedAgent = GetComponent<IWakeable>(); 
        
        if(_agent != null)
        {
            _agent.stoppingDistance = stoppingDistance;
        }

        desiredX = transform.position.x;
        desiredY = transform.position.y;
        desiredZ = transform.position.z;
        _currentTarget = transform.position;
    }

    void Update()
    {
        if(_agent == null) return;

        Vector3 destination = new Vector3(desiredX, desiredY, desiredZ);
        bool destinationChanged = Vector3.Distance(_currentTarget, destination) > 0.1f;

        if(!destinationChanged) return;
        if(!_hasWokenUp && _linkedAgent != null)
        {
            _linkedAgent.WakeUp();
            _hasWokenUp = true;
        }

        if(!_agent.isActiveAndEnabled) 
        {
            _agent.enabled = true;
        }

        if(_agent.isOnNavMesh)
        {
            _currentTarget = destination;
            _hasWokenUp = false;
            
            if(_agent.isStopped)
            {
                _agent.isStopped = false;
            }
            bool result = _agent.SetDestination(_currentTarget);
        }
        else
        {
            NavMeshHit hit;
            if(NavMesh.SamplePosition(transform.position, out hit, 10f, NavMesh.AllAreas))
            {
                _agent.Warp(hit.position);
            }
        }
    }
}