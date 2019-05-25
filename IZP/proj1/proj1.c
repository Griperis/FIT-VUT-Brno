/*
**  Projekt 1 - Prace s textem
**  Verze 2.0
**  Autor:  Zdenek Dolezal
**  Login:  xdolez82
**  Fakulta informacnich technologii VUT v Brne
**  Datum: 16. 10. 2017
*/

/*  Program nebo funkce vraci hodnotu 0 pokud se spravne provedly.
**  Pokud nastala v programu chyba, program vraci ERROR (-2)
*/
#define MAX_ADRESS_LENGTH 100 //maximalni delka adresy 100 znaku + znak konce retezce
#define ALLOWED_CHARS 126     //pocet povolenych znaku - ASCII printable znaky
#define NUMBER_OF_ADRESSES 42 //maximalni pocet adres
#define ERROR -2              //Chybovy stav zadefinovany jako -2 {-1 je EOF}

#include <stdio.h>
#include <string.h>
#include <ctype.h>
#include <stdbool.h>

//Funkce cistici pole
void clearArray(char *arr)
{
    for (int i = 0; arr[i] != 0; i++){
        arr[i] = 0;
    }

}
//Funkce, ktera tiskne enabled znaky
void printEnabled(char *arr)
{
    printf("Enable: ");
    for (int i = 0; i < ALLOWED_CHARS; i++){
        if (arr[i] != 0){
          printf("%c", arr[i]);
        }
    }
    printf("\n");
}
//Funkce prevadejici adresu do docasne adresy
void toTemporary(char *arr, char *temp)
{
    clearArray(temp);
    for (int i = 0; arr[i] != 0; i++){
        temp[i] = arr[i];
    }
}
//Funkce nacte zvetsi kazde pismeno a ulozi ho do pole current_adress
int getLine(char *arr)
{
    int c = 0;          //jeden znak
    int c_count = 0;    //pocet znaku
    clearArray(arr);
    while ((c = getchar()) != '\n'){
        if (isprint(c) && c_count < MAX_ADRESS_LENGTH){
            arr[c_count] = toupper(c);
            c_count++;
        }else if (c == EOF){
            return EOF;

        }else{
            fprintf(stderr, "ERROR: Not supported format in <file.txt (only printable characters and <100 chars)\n");
            return ERROR;
        }
    }
    return 0;
}

//Funkce, ktera zkontroluje argument a v pripade ze je v poradku ho prevede na velka pismena a ulozi
int checkArg(char *arg, char *arr)
{
    for (int i = 0; arg[i] != 0; i++){
        if (isprint(arg[i])){
            arr[i] = toupper(arg[i]);
        }else{
          fprintf(stderr, "ERROR: Not printable argument\n");
          return ERROR;
        }
    }
    if (strlen(arg) > MAX_ADRESS_LENGTH){
        fprintf(stderr, "ERROR: Input argument is too long (maximum of 100 characters)\n");
        return ERROR;
    }
    return 0;
}

//Funkce ktera porovnava radek s argumentem a uklada enabled znaky
int checkLine(char *arr, char *input_arg, char *enabled, int arg_length, char *temporary_adress)
{
    int eqc_count = 0;    //pocet shodnych znaku
    int en_c = 0;         //pocet povolenych znaku
    for (int i = 0; i < arg_length+1; i++){   //+1 kvuli znaku konce retezce
        if (input_arg[i] == arr[i]){
            eqc_count++;
        }else if (eqc_count == arg_length){
            enabled[arr[i]-' '] = arr[i];    //odecitame znak mezery, abychom pro serazene ASCII znaky dostali indexy od 0
            en_c++;
            toTemporary(arr, temporary_adress);
        }
    }
    return en_c;
}

int main(int argc, char *argv[])
{
    //deklarace
    char current_adress[MAX_ADRESS_LENGTH+1] = {0};   //pole obsahujici adresu na radku, max znaku + znak konce stringu
    char enabled[ALLOWED_CHARS] = {0};                //pole povolenych znaku
    char input_arg[MAX_ADRESS_LENGTH+1] = {0};        //pole do ktereho se nasledne uklada vstupni argument zvetseny na velka pismena
    char temporary_adress[MAX_ADRESS_LENGTH+1] = {0}; //"docasna" adresa pro ulozeni pripadneho nalezeneho retezce

    int adress_count = 0;   //pocet adres
    int enabled_count = 0;  //pocet povolenych znaku
    int arg_length = 0;     //delka argumentu
    int gl_return = 0;      //kontrola getLine funkce

    bool found = false;     //kontrola zda byl nalezen shodny string(pro pridat ze nejaka adresa je podretezcem jine)
    //kod
    //vstupni podminka pro pocet argumentu
    if (argc < 1 && argc > 2){
        fprintf(stderr, "ERROR: Wrong arguments format - prefix<file.txt expected\n");
        return ERROR;
    }
    //pro dva argumenty
    if (argc == 2){
        arg_length = strlen(argv[1]);
        if (checkArg(argv[1],input_arg) == ERROR){
            return ERROR;
        }
    }
    //cyklus nacita ze stdin radek po radku, dokud nedostane EOF nebo chybovy stav
    while((gl_return = getLine(current_adress)) != EOF){
        if (gl_return == ERROR){
          return ERROR;
        }
        //Pro 1 argument(vytiskne prvni znaky adres (kazdy pouze jednou)
        if (argc == 1){
            enabled_count += checkLine(current_adress, input_arg, enabled, arg_length, temporary_adress);
        //Pro 2 argumenty(zkontroluje zda se argument a radek nerovnaji a pote zkontroluje radek)
        }else if (argc == 2){
            if (strcmp(current_adress, input_arg) == 0){
                printf("Found: %s\n", current_adress);
                found = true;
            }
            enabled_count += checkLine(current_adress, input_arg, enabled, arg_length, temporary_adress);
        }
        adress_count++;
        //Kontrola zda pocet adres neni vysi nez povoleny
        if(adress_count > NUMBER_OF_ADRESSES){
          fprintf(stderr, "ERROR: Number of adresses exceeded\n");
          return ERROR;
        }
    }
    //Vyhodnoceni prubehu programu
    //enabled
    if (enabled_count > 1){
        printEnabled(enabled);
        return 0;
    //found
    }else if (enabled_count == 1 || found == true){
        printf("Found: %s\n", temporary_adress);
        return 0;
    //not found
    }else if (enabled_count == 0 && found == false){
        printf("Not found\n");
        return 0;
    }
}
