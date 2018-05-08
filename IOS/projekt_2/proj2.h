/*
Operating Systems
Project 2 - Header file
The Senate Bus Problem
Faculty of information technology BUT
Author: Zdenek Dolezal
Login: xdolez82
*/

/* Brief
Programs purpose is simulating the synchronization problem called
The Senate Bus Problem. Riders are coming to a bus stop and waiting for bus,
bus comes in random intervals and it has desired capacity, only that many riders
can enter it. When bus is at stop, no other riders can come there. They must wait
till the bus departs. Bus ends, when all riders have been transported.

My school project implementation:
There are two main processes created after launch of the program. Bus and
rider generator {which generates riders, who ale also processes].
Synchronizing is inspired of solution from book The little book of semaphores.
One additional semaphore is added to exclude writing in log by more than one
process at once.
Global variables that I use additionaly to global shared memory variables
and semaphores, are more of a simplifier solution,that the code can be
easier to understand.
Result of simulation is in proj2.out file, which is overwritten every time
*/
#include <stdio.h>
#include <sys/types.h>
#include <sys/wait.h>
#include <fcntl.h>
#include <time.h>
#include <unistd.h>
#include <signal.h>
#include <stdlib.h>
#include <sys/shm.h>
#include <sys/ipc.h>
#include <semaphore.h>

/* Defines */
#define LOCKED 0
#define UNLOCKED 1
#define SHM_ERR -1
#define SEM_ERR -1
#define EXIT_SUCCESS 0
#define EXIT_FAILURE 1

/* Semaphores */
#define SEM_NAME1 "sem.xdolez82_ios_mutex"
#define SEM_NAME2 "sem.xdolez82_ios_bus"
#define SEM_NAME3 "sem.xdolez82_ios_boarded"
#define SEM_NAME4 "sem.xdolez82_ios_multiplex"
#define SEM_NAME5 "sem.xdolez82_ios_end"
#define SEM_NAME6 "sem.xdolez82_ios_printMutex"

sem_t *mutex;
sem_t *bus;
sem_t *boarded;
sem_t *multiplex;
sem_t *printMutex;
sem_t *end;

/* Shared memory */
int shmID = 0;
int shmROSID = 0;
int shmRiderCountID = 0;
int shmRidersWaitingID = 0;
int shmBusCapacityID = 0;

int *shmAction = NULL;
int *shmRidersOnStop = NULL;
int *shmRiderCount = NULL;
int *shmRidersWaiting = NULL;
int *shmBusCapacity = NULL;

/* Shared output file */
FILE *outFile;

/* Parameters */
struct parameters {
    int riderMax;   /* Number of riders to create*/
    int capacity;   /* Capacity of the bus */
    int art;        /* Maximum limit for rider generation time*/
    int abt;        /* Maximum limit for bus ride simulation*/
};

struct parameters p;

/*!
   @brief "Parses arguments"
   @param argc - "total count of arguments"
   @param *argv[] - "arguments"
   @param *params - "pointer to a structure handling parameters"
   @return "0 if succesfull or 1 when something went wrong"
*/
int handle_params(int argc, char *argv[]);
/*!
   @brief "Allocation of semaphores"
   @return "0 on success | SEM_ERR when failed"
*/
int get_semaphores(void);
/*!
   @brief "Allocation of memory"
   @return "0 on success  SHM_ERR when failed"
*/
int get_memory(void);

/*!
   @brief "Calls functions to free all allocated resources used"
*/
void free_resources(void);
/*!
   @brief "Frees all allocated memory"
*/
void free_memory(void);
/*!
   @brief "Destroys all semaphores"
*/
void destroy_sem(void);
/*!
   @brief "Creates rider processes"
   @param *genActualPid - "process id of generator"
   @param *ridPid - "process id of rider"
   @param *riderSelfID - "internal integer id of rider"
   @return "Stops generation if next rider could not be forked"
*/
void rider_gen(pid_t *genActualPid, pid_t *ridPid, int *riderSelfID);
/*!
   @brief "Bus process - loops until all riders have boarded"
   @return "if everyone was transported, bus ends"
*/
void bus_process();
/*!
   @brief "Rider process"
   @param *riderSelfID - "internal integer id of rider"
   @return "If rider succesfully finished it is terminated"
*/
void rider(int riderSelfID);
