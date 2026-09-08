% Priorità codice colore
priority("red", 5).
priority("orange", 4).
priority("blue", 3).
priority("green", 2).
priority("white", 1).

% Ponte sensore HUB centralizzato
staff(ID, Type, Status, Patient) :- 
    staff_staffId(_, _, ListIndex, ID),
    staff_aspStaffType(_, _, ListIndex, Type),
    staff_aspStatus(_, _, ListIndex, Status),
    staff_currentPatientId(_, _, ListIndex, Patient).

room(ID, Type, Status, Patient) :- 
    room_roomId(_, _, ListIndex, ID),
    room_aspRoomType(_, _, ListIndex, Type),
    room_aspStatus(_, _, ListIndex, Status),
    room_currentPatientId(_, _, ListIndex, Patient).

patient(ID, Color, State) :- 
    patient_patientId(_, _, ListIndex, ID),
    patient_aspTriageColor(_, _, ListIndex, Color),
    patient_aspCurrentState(_, _, ListIndex, State).

% Prelazione (interruzioni)
% Un paziente non rosso attualmente in cura può essere interrotto.
% Il paziente interrotto libera la stanza e torna in sala d'attesa in attesa di nuove risorse.
% I pazienti rossi non sono mai interrompibili.

interruptible(P) :- staff(_, _, "busy", P), patient(P, Color, _), Color != "red".
{ doInterrupt(P) } :- interruptible(P).

% Risorse disponibili
% Una risorsa è disponibile se idle oppure liberata da un'interruzione.
available_doctor(D) :- staff(D, "doctor", "idle", _).
available_doctor(D) :- staff(D, "doctor", "busy", P), doInterrupt(P).

available_nurse(N) :- staff(N, "nurse", "idle", _).
available_nurse(N) :- staff(N, "nurse", "busy", P), doInterrupt(P).

available_room(R, Type) :- room(R, Type, "idle", _).
available_room(R, Type) :- room(R, Type, "busy", P), doInterrupt(P).

% Protezione pazienti attivi
% Chi è già in cura e non subisce interruzioni non deve ricevere nuove azioni.
already_served(P) :- staff(_, _, "busy", P), not doInterrupt(P).

% GUESS 
{ doTriage(P, N, R) : available_nurse(N), available_room(R, "triage") } <= 1 :- patient(P, _, "arrived"), not already_served(P).

{ doVisit(P, D, R) : available_doctor(D), available_room(R, "exam") } <= 1 :- patient(P, Color, "waiting"), Color != "red", not already_served(P).
{ doCoopVisit(P, D, N, R) : available_doctor(D), available_nurse(N), available_room(R, "resus") } <= 1 :- patient(P, "red", "waiting"), not already_served(P).

{ doTherapy(P, N) : available_nurse(N) } <= 1 :- patient(P, Color, "needs_therapy"), Color != "red", not already_served(P).
{ doCoopTherapy(P, D, N) : available_doctor(D), available_nurse(N) } <= 1 :- patient(P, "red", "needs_therapy"), not already_served(P).

{ doDischarge(P, D) : available_doctor(D) } <= 1 :- patient(P, Color, "ready_discharge"), Color != "red", not already_served(P).
{ doCoopDischarge(P, D, N) : available_doctor(D), available_nurse(N) } <= 1 :- patient(P, "red", "ready_discharge"), not already_served(P).

% PREDICATI AUSILIARI
action_type(P, "triage") :- doTriage(P, _, _).
action_type(P, "visit") :- doVisit(P, _, _).
action_type(P, "coop_visit") :- doCoopVisit(P, _, _, _).
action_type(P, "therapy") :- doTherapy(P, _).
action_type(P, "coop_therapy") :- doCoopTherapy(P, _, _).
action_type(P, "discharge") :- doDischarge(P, _).
action_type(P, "coop_discharge") :- doCoopDischarge(P, _, _).

resource_use(doctor, D, P) :- doVisit(P, D, _).
resource_use(doctor, D, P) :- doCoopVisit(P, D, _, _).
resource_use(doctor, D, P) :- doDischarge(P, D).
resource_use(doctor, D, P) :- doCoopTherapy(P, D, _).
resource_use(doctor, D, P) :- doCoopDischarge(P, D, _).

resource_use(nurse, N, P) :- doTriage(P, N, _).
resource_use(nurse, N, P) :- doCoopVisit(P, _, N, _).
resource_use(nurse, N, P) :- doTherapy(P, N).
resource_use(nurse, N, P) :- doCoopTherapy(P, _, N).
resource_use(nurse, N, P) :- doCoopDischarge(P, _, N).

resource_use(room, R, P) :- doTriage(P, _, R).
resource_use(room, R, P) :- doVisit(P, _, R).
resource_use(room, R, P) :- doCoopVisit(P, _, _, R).

% CHECK
% Un paziente può avere al massimo un'azione per ciclo.
:- patient(P, _, _), #count { Type : action_type(P, Type) } > 1.

% Niente risorse condivise tra pazienti diversi
:- resource_use(Type, ID, P1), resource_use(Type, ID, P2), P1 != P2.

% Un paziente già in cura (non interrotto) non riceve nuove azioni.
:- action_type(P, _), already_served(P).

% Un'interruzione è valida solo se almeno una risorsa liberata viene riutilizzata.
resource_reused(P) :- doInterrupt(P), staff(D, "doctor", "busy", P), resource_use(doctor, D, _).
resource_reused(P) :- doInterrupt(P), staff(N, "nurse", "busy", P), resource_use(nurse, N, _).
resource_reused(P) :- doInterrupt(P), room(R, Type, "busy", P), resource_use(room, R, _).
:- doInterrupt(P), not resource_reused(P).

% WEAKLY CONSTRAINTS
% Un paziente è assistito se ha un'azione nuova o è già in cura.
attended(P) :- action_type(P, _).
attended(P) :- already_served(P).

% Penalizza l'inattività in base alla gravità del codice colore.
:~ patient(P, Color, _), priority(Color, Level), not attended(P). [1@Level, P]

% A parità di priorità, minimizza la somma degli ID assistiti.
:~ attended(P). [P@0, P]

% MAPPING PER LO SCHEDULER
applyAction(P, "ActionTriage") :- doTriage(P, N, R).
actionArgument(P, "patientId", P) :- doTriage(P, N, R).
actionArgument(P, "nurseId", N) :- doTriage(P, N, R).
actionArgument(P, "roomId", R) :- doTriage(P, N, R).

applyAction(P, "ActionVisit") :- doVisit(P, D, R).
actionArgument(P, "patientId", P) :- doVisit(P, D, R).
actionArgument(P, "doctorId", D) :- doVisit(P, D, R).
actionArgument(P, "roomId", R) :- doVisit(P, D, R).

applyAction(P, "ActionCoopVisit") :- doCoopVisit(P, D, N, R).
actionArgument(P, "patientId", P) :- doCoopVisit(P, D, N, R).
actionArgument(P, "doctorId", D) :- doCoopVisit(P, D, N, R).
actionArgument(P, "nurseId", N) :- doCoopVisit(P, D, N, R).
actionArgument(P, "roomId", R) :- doCoopVisit(P, D, N, R).

applyAction(P, "ActionTherapy") :- doTherapy(P, N).
actionArgument(P, "patientId", P) :- doTherapy(P, N).
actionArgument(P, "nurseId", N) :- doTherapy(P, N).

applyAction(P, "ActionCoopTherapy") :- doCoopTherapy(P, D, N).
actionArgument(P, "patientId", P) :- doCoopTherapy(P, D, N).
actionArgument(P, "doctorId", D) :- doCoopTherapy(P, D, N).
actionArgument(P, "nurseId", N) :- doCoopTherapy(P, D, N).

applyAction(P, "ActionDischarge") :- doDischarge(P, D).
actionArgument(P, "patientId", P) :- doDischarge(P, D).
actionArgument(P, "doctorId", D) :- doDischarge(P, D).

applyAction(P, "ActionCoopDischarge") :- doCoopDischarge(P, D, N).
actionArgument(P, "patientId", P) :- doCoopDischarge(P, D, N).
actionArgument(P, "doctorId", D) :- doCoopDischarge(P, D, N).
actionArgument(P, "nurseId", N) :- doCoopDischarge(P, D, N).

applyAction(P, "ActionInterrupt") :- doInterrupt(P).
actionArgument(P, "patientId", P) :- doInterrupt(P).

#show applyAction/2.
#show actionArgument/3.