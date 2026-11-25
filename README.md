# ThinkEngine Triage
> **Gestione centralizzata delle risorse ospedaliere con Answer Set Programming: simulazione 3D in Unity**

Progetto di Tesi di Laurea Triennale in Informatica  
**Università della Calabria** — Dipartimento di Matematica e Informatica (DeMaCS)  
**Candidata:** Irene Rosanna Covelli  
**Relatori:** Prof. Giovambattista Ianni, Dott.ssa Denise Angilica  
**Anno Accademico:** 2026/2027  

---

## Descrizione del progetto

**ThinkEngine_Triage** è un simulatore multi-agente tridimensionale sviluppato in **Unity** che modella i flussi di lavoro, le code e la presa in carico clinica all'interno di un Pronto Soccorso. 

Il cuore decisionale del sistema è affidato a un motore di **Intelligenza Artificiale Dichiarativa**: tramite il framework ibrido **ThinkEngine**, il simulatore si interfaccia in tempo reale con il solver **DLV2**, il quale risolve problemi combinatori di allocazione ottima delle risorse sanitarie (medici, infermieri e sale cliniche) formulati in **Answer Set Programming (ASP)** secondo il paradigma *Guess, Check, Optimize*.

---

## Funzionalità principali

- **Protocollo di Triage:** modellazione della classificazione a cinque codici colore (Bianco, Verde, Blu, Arancione, Rosso) conforme alle linee guida nazionali del triage intraospedaliero.
- **Deperimento clinico dinamico:** monitoraggio temporale attivo dello stato di salute dei pazienti in attesa; in caso di ritardi prolungati, il quadro patologico peggiora forzando l'escalation al codice di gravità superiore.
- **Prelazione per le emergenze (preemption):** in condizioni di saturazione delle risorse, l'ingresso di un Codice Rosso determina l'interruzione cautelativa di procedure non urgenti in corso, liberando istantaneamente il personale e la sala rianimazione tramite un pattern di *rollback* di stato.
- **Concorrenza parallela e asincrona:** sincronizzazione non-bloccante tra il Game Loop di Unity e il ciclo deliberativo di ThinkEngine mediante l'adozione di *Coroutine* e disaccoppiamento dello stato (*State.READY*).
- **Navigazione autonoma (NavMesh):** Pathfinding e local avoidance dinamica per tutti gli agenti sanitari e i pazienti nello spazio.
- **Interfaccia spaziale e Trasparenza:** 
  - HUD World-Space sopra i pazienti con tecnica *Billboard* per la visualizzazione dei timer di attesa.
  - Console di Log in-game che traduce i predicati formali di ASP in messaggi esplicativi in linguaggio naturale.
  - Pannello di iniezione a runtime per simulare stress-test e scenari di maxi-emergenza.

---

## Architettura e Tecnologie

1. **Livello Esecutivo ed Embodiment:**
   - **Engine:** Unity (C#)
   - **Pathfinding:** Unity NavMesh A*
   - **UI:** uGUI (Screen Space Overlay & World Space)
2. **Livello di Traduzione e Middleware:**
   - **Framework:** [ThinkEngine](https://github.com/DeMaCS-UNICAL/ThinkEngine)
   - **Tracciamento:** `IdRegistry` (Mappatura *O(1)* tra GameObject e simboli logici)
3. **Livello Deliberativo:**
   - **Linguaggio:** Answer Set Programming (ASP)
   - **Solver:** DLV2 (esecuzione asincrona su thread separato)

---

## Come eseguire il progetto

1. **Prerequisiti:**
   - Unity (versione LTS compatibile con ThinkEngine).
   - Git installato sul proprio ambiente.
2. **Clonazione della repository:**
   ```bash
   git clone https://github.com/Irens22/ThinkEngine_Triage.git
   ```
3. **Apertura del Progetto:**
   - Avviare **Unity Hub** e selezionare *Open* $\rightarrow$ selezionare la cartella clonata `ThinkEngine_Triage`.
4. **Esecuzione:**
   - Aprire la scena principale da `Assets/Scenes/`.
   - Premere il tasto **Play** nell'editor di Unity.
   - Utilizzare il pannello laterale per iniettare pazienti o variare l'organico dello staff a runtime.

---
