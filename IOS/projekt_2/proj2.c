/*
Operating Systems
Project 2 - C file
The Senate Bus Problem
Faculty of information technology BUT
Author: Zdenek Dolezal
Login: xdolez82
*/
#include "proj2.h"

int main(int argc, char *argv[]){
    /* Initialization */
    if (handle_params(argc, argv)) return EXIT_FAILURE;
    if (get_memory()) return EXIT_FAILURE;
    if (get_semaphores()) return EXIT_FAILURE;

    char *filename = "proj2.out";
    int riderSelfID = 0;
    pid_t genActualPid;
    pid_t genPid;
    pid_t ridPid;
    pid_t busPid;

    outFile = fopen(filename,"w");
    if (outFile == NULL){
        fprintf(stderr, "Error: Failed to open output file\n");
        return EXIT_FAILURE;
    }

    setbuf(outFile,NULL);
    srand(time(0));

    /* Creation of bus and rider generator processes*/
    busPid = fork();
    if (busPid == 0){
        bus_process();
    }else if (busPid < 0){
        free_resources();
        fprintf(stderr, "Error: Failed to fork process for bus\n");
        return EXIT_FAILURE;
    }else{
        genPid = fork();
        if (genPid == 0){
            rider_gen(&genActualPid, &ridPid, &riderSelfID);
        }else if (genPid < 0){
            fprintf(stderr, "Error: Failed to fork riders generator\n");
            kill(busPid, SIGTERM);
            return EXIT_FAILURE;
        }
        if (ridPid == 0 && getpid() != genActualPid){
            rider(riderSelfID);
        }
    }
    waitpid(busPid,NULL, 0);
    waitpid(genPid,NULL, 0);
    free_resources();
    return EXIT_SUCCESS;
}

int get_memory(){
    shmID = shmget(IPC_PRIVATE, sizeof(int), IPC_CREAT | 0666);
    shmROSID = shmget(IPC_PRIVATE, sizeof(int), IPC_CREAT | 0666);
    shmRiderCountID = shmget(IPC_PRIVATE, sizeof(int), IPC_CREAT | 0666);
    shmRidersWaitingID = shmget(IPC_PRIVATE, sizeof(int), IPC_CREAT | 0666);
    shmBusCapacityID = shmget(IPC_PRIVATE, sizeof(int), IPC_CREAT | 0666);
    shmAction = shmat(shmID, NULL, 0);
    shmRidersOnStop = shmat(shmROSID, NULL, 0);
    shmRiderCount = shmat(shmRiderCountID, NULL, 0);
    shmRidersWaiting = shmat(shmRidersWaitingID, NULL, 0);
    shmBusCapacity = shmat(shmBusCapacityID, NULL, 0);

    /* Error handling, if something failed, destroy what was created and end */
    if (shmID == SHM_ERR || shmROSID == SHM_ERR || shmRiderCountID == SHM_ERR ||
        shmRidersWaitingID == SHM_ERR || shmBusCapacityID == SHM_ERR){
        fprintf(stderr, "Error: Could not allocate shared memory IDs\n");
        free_memory();
        return SHM_ERR;

    }else if ( shmAction == NULL || shmRidersOnStop == NULL || shmRiderCount == NULL ||
        shmRidersWaiting == NULL || shmBusCapacity == NULL){
        fprintf(stderr, "Error: Could not allocate shared memory\n");
        free_memory();
        return SHM_ERR;
    }
    (*shmAction) = 1;
    (*shmRiderCount) = 0;
    (*shmRidersOnStop) = 0;
    return 0;
}
int get_semaphores(){
    mutex = sem_open(SEM_NAME1, O_CREAT | O_EXCL, 0666, UNLOCKED);
    bus = sem_open(SEM_NAME2, O_CREAT | O_EXCL, 0666, LOCKED);
    boarded = sem_open(SEM_NAME3, O_CREAT | O_EXCL, 0666, LOCKED);
    multiplex = sem_open(SEM_NAME4, O_CREAT | O_EXCL, 0666,p.capacity);
    end = sem_open(SEM_NAME5, O_CREAT | O_EXCL, 0666, LOCKED);
    printMutex = sem_open(SEM_NAME6, O_CREAT | O_EXCL, 0666, UNLOCKED);

    /* Error handling, if something failed, destroy what was created and end */
    if (mutex == SEM_FAILED || bus == SEM_FAILED || boarded == SEM_FAILED ||
        multiplex == SEM_FAILED || end == SEM_FAILED || printMutex == SEM_FAILED){
            destroy_sem();
            free_memory();
            fprintf(stderr, "Error: Failed to open semaphores\n");
            return SEM_ERR;
    }
    return 0;
}
void destroy_sem(){
    sem_close(mutex);
    sem_close(bus);
    sem_close(boarded);
    sem_close(multiplex);
    sem_close(end);
    sem_close(printMutex);
    sem_unlink(SEM_NAME1);
    sem_unlink(SEM_NAME2);
    sem_unlink(SEM_NAME3);
    sem_unlink(SEM_NAME4);
    sem_unlink(SEM_NAME5);
    sem_unlink(SEM_NAME6);
}
void free_memory(){
    shmctl(shmID, IPC_RMID, NULL);
    shmctl(shmRidersWaitingID, IPC_RMID, NULL);
    shmctl(shmROSID, IPC_RMID, NULL);
    shmctl(shmRiderCountID, IPC_RMID, NULL);
    shmctl(shmBusCapacityID, IPC_RMID, NULL);
}
void rider(int riderSelfID){
    /* Entering stop */
    sem_wait(mutex);
    (*shmRidersOnStop)++;
    sem_wait(printMutex);
    fprintf(outFile,"%d: RID %d: enter: %d\n", (*shmAction),riderSelfID,(*shmRidersOnStop));
    (*shmAction)++;
    sem_post(printMutex);
    sem_post(mutex);
    sem_wait(multiplex);
    sem_wait(bus);
    /* Boarding bus */
    sem_wait(printMutex);
    fprintf(outFile,"%d: RID %d: boarding\n", (*shmAction),riderSelfID);
    (*shmAction)++;
    (*shmBusCapacity)--;
    (*shmRidersOnStop)--;
    (*shmRidersWaiting)--;
    sem_post(printMutex);
    /* Depart bus, if there are no more riders or bus is full*/
    if ((*shmRidersOnStop) == 0 || (*shmBusCapacity == 0)){
        sem_post(boarded);
    }else{
        sem_post(bus);
    }
    /* Finish */
    sem_wait(mutex);
    sem_wait(end);
    sem_wait(printMutex);
    fprintf(outFile,"%d: RID %d: finish\n",(*shmAction),riderSelfID);
    (*shmAction)++;
    sem_post(printMutex);
    sem_post(mutex);
    exit(EXIT_SUCCESS);
}
void bus_process(){
    /* Start*/
    sem_wait(printMutex);
    fprintf(outFile,"%d: BUS: start\n",(*shmAction));
    (*shmAction)++;
    sem_post(printMutex);
    /*  Bus loop, ends when all riders were boarded
        - every round bus capacity is initialized to max
        - when riders board, they decrease the capacity
        - when the capacity is 0 or there are no more riders, bus departs
        - after every loop bus sleeps for random time between 0 and p.abt
    */
    while ((*shmRiderCount) < p.riderMax || (*shmRidersOnStop) > 0 || (*shmRidersWaiting) > 0){
        int n = 0;
        sem_wait(mutex);
        (*shmBusCapacity) = p.capacity;
        /* Arrival */
        sem_wait(printMutex);
        fprintf(outFile,"%d: BUS: arrival\n",(*shmAction));
        (*shmAction)++;
        sem_post(printMutex);
        /* Boarding */
        if ((*shmRidersOnStop) > 0){
            sem_wait(printMutex);
            fprintf(outFile,"%d: BUS: start boarding: %d\n",(*shmAction),(*shmRidersOnStop));
            (*shmAction)++;
            sem_post(printMutex);
            n = ((*shmRidersOnStop) > p.capacity) ? p.capacity : (*shmRidersOnStop);
            for (int i = 0; i < n; ++i) sem_post(multiplex);
            sem_post(bus);
            sem_wait(boarded);
            sem_wait(printMutex);
            fprintf(outFile,"%d: BUS: end boarding: %d\n",(*shmAction),(*shmRidersOnStop));
            (*shmAction)++;
            sem_post(printMutex);
        }
        /* Depart */
        sem_wait(printMutex);
        fprintf(outFile,"%d: BUS: depart\n", (*shmAction));
        (*shmAction)++;
        sem_post(printMutex);
        sem_post(mutex);
        if (p.abt > 0){
            int r = (random() % p.abt) * 1000;
            usleep(r);
        }
        /* End of loop */
        sem_wait(printMutex);
        fprintf(outFile,"%d: BUS: end\n",(*shmAction));
        (*shmAction)++;
        sem_post(printMutex);
        for (int i = 0; i < n; ++i) sem_post(end);
    }
    /* Finish - all riders were transported*/
    sem_wait(printMutex);
    fprintf(outFile,"%d: BUS: finish\n",(*shmAction));
    (*shmAction)++;
    sem_post(printMutex);
    exit(EXIT_SUCCESS);
}
void rider_gen(pid_t *genActualPid, pid_t *ridPid, int *riderSelfID){
    (*genActualPid) = getpid();
    /*  Rider generator loop, ends when all riders were created
        - every cycle forks new process for rider
        - then sleeps for random between 0 and p.art
    */
    for(int i = 1; i <= p.riderMax; ++i){
        if((*genActualPid) == getpid()){
            (*ridPid) = fork();
            if ((*ridPid) == 0){
                /* Rider was generated - Start */
                sem_wait(printMutex);
                (*riderSelfID) = i;
                fprintf(outFile,"%d: RID %d: start\n",(*shmAction),(*riderSelfID));
                (*shmAction)++;
                (*shmRiderCount)++;
                (*shmRidersWaiting)++;
                sem_post(printMutex);
            }else if ((*ridPid) < 0){
                fprintf(stderr, "Error: Could not create rider %d, stopping generator\n", (*shmRiderCount));
                (*shmRiderCount) = p.riderMax;
                exit(EXIT_FAILURE);
            }
            if ((*genActualPid) == getpid()){
                if (p.art > 0){
                    int r = (random() % p.art) * 1000;
                    usleep(r);
                }
            }
        }
    }
    if ((*genActualPid) == getpid()) {
        waitpid((*ridPid),NULL,0);
        exit(EXIT_SUCCESS);
    }
}
void free_resources(){
    free_memory();
    destroy_sem();
    fclose(outFile);
}
int handle_params(int argc, char *argv[]){
    char *errptr;
    if (argc != 5){
        fprintf(stderr, "Error: Invalid number of arguments\n");
        return 1;
    }
    /* Check for correct format, and if the numbers are in right interval */
    p.riderMax = strtol(argv[1],&errptr,10);
    if (*errptr != 0 || p.riderMax <= 0){
        fprintf(stderr, "Error: Invalid format of argument R, R > 0\n");
        return 1;
    }
    p.capacity = strtol(argv[2],&errptr,10);
    if (*errptr != 0 || p.capacity <= 0){
        fprintf(stderr, "Error: Invalid format of argument C, C > 0\n");
        return 1;
    }
    p.art = strtol(argv[3],&errptr,10);
    if (*errptr != 0 || p.art < 0 || p.art > 1000){
        fprintf(stderr, "Error: Invalid format of argument ART, ART >= 0 <= 1000\n");
        return 1;
    }
    p.abt = strtol(argv[4],&errptr,10);
    if (*errptr != 0 || p.abt < 0 || p.abt > 1000){
        fprintf(stderr, "Error: Invalid format of argument ABT, ABT >= 0 <= 1000\n");
        return 1;
    }
    return 0;
}
