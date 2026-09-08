using UnityEngine;
using ThinkEngine.Planning;

public class ActionInterrupt : Action
{
    public int patientId { get; set; }

    public override State Prerequisite()
    {
        if(IdRegistry.Instance != null && IdRegistry.Instance.Patients.ContainsKey(patientId))
        {
            return State.READY;
        }
        return State.ABORT;
    }

    public override void Do()
    {
        if(IdRegistry.Instance == null || !IdRegistry.Instance.Patients.ContainsKey(patientId))
        {
            return;
        }

        PatientAI patient = IdRegistry.Instance.Patients[patientId].GetComponent<PatientAI>();

        if(patient != null)
        {
            patient.StopAllCoroutines();
        }

        if(IdRegistry.Instance.Doctors != null)
        {
            foreach(var d in IdRegistry.Instance.Doctors)
            {
                StaffAI staff = d.Value.GetComponent<StaffAI>();
                if(staff != null && staff.currentPatientId == patientId)
                {
                    staff.ClearTask();
                    staff.GoToStaffRoom();
                }
            }
        }

        if(IdRegistry.Instance.Nurses != null)
        {
            foreach(var n in IdRegistry.Instance.Nurses)
            {
                StaffAI staff = n.Value.GetComponent<StaffAI>();
                if(staff != null && staff.currentPatientId == patientId)
                {
                    staff.ClearTask();
                    staff.GoToStaffRoom();
                }
            }
        }

        RoomManager room = patient.GetRoomByPatientId(patientId);
        if(room != null)
        {
            room.ClearRoom();
        }

        if(patient != null)
        {
            switch(patient.currentState)
            {
                case PatientState.in_triage:
                    patient.UpdateState(PatientState.arrived);
                    break;
                case PatientState.in_visit:
                    patient.UpdateState(PatientState.waiting);
                    break;
                case PatientState.in_therapy:
                    patient.UpdateState(PatientState.needs_therapy);
                    break;
            }

            patient.WakeUp(); 
            patient.GoToWaitingRoom();
        }
    }

    public override State Done()
    {
        return State.READY;
    }
}