/*
*  Project 2
*  Version: 2.1
*  Author: Zdenek Dolezal
*  Login: xdolez82
*  Faculty of information technology BUT
*  Date: 17. 11. 2017
*/

//Program returns EXIT_SUCCESS if everything went correctly
//Program return EXIT_FAILURE if something went wrong

//Macros
#define TAYLOR_ITERATIONS 13      //Max iterations for --tan
#define ITERATIONS 9              //Number of iterations needed to get angle accurate to 10 decimal places
#define IMPLICIT_HEIGHT 1.5       //Implicit height of measuring device
#define PI 3.14159265358979323846 //PI number
#define MAX_ANGLE 1.4             //Maximum angle input for --tan argument
#define MIN_ANGLE 0               //Minimum angle input for --tan argument
#define MAX_DEV_HEIGHT 100        //maximal device height
#define MIN_DEV_HEIGHT 0          //minimal device height
#define ERROR_STATE fprintf(stderr, "ERROR: Wrong input, use argument --help\n")  //Defined output for errors

//Libraries
#include <stdio.h>
#include <math.h>
#include <string.h>
#include <stdlib.h>

//Approximation of tangent via taylor`s polynom
double taylor_tan(double x, unsigned int n)
{
    double tay_tan = 0.;
    double power_x = x;

    //Taylor polynome numerators
    const double numerator[] =
    {1, 1, 2, 17, 62, 1382, 21844, 929569, 6404582, 443861162,
    18888466084, 113927491862, 58870668456604
    };

    //Taylor polynome denominators
    const double denominator[] =
    {1, 3, 15, 315, 2835, 155925, 6081075,
    638512875, 10854718875, 1856156927625,
    194896477400625, 49308808782358125, 3698160658676859375
    };
    for (unsigned int i = 0; i < n; i++){
        tay_tan += (numerator[i]*power_x)/(denominator[i]);
        power_x *= x * x;     //x power 3
    }
    return tay_tan;
}
//approximation of tangent via continued fraction
double cfrac_tan(double x, unsigned int n)
{
    double cf_tan = 0.;
    double a = 1;
    double b;
    for (; n > 0; n--)
    {
        b = (n*2-1)/x;
        cf_tan = a / (b - cf_tan);
    }
    return cf_tan;
}
//Makes absolute value from number
double abs_value(double x)
{
    if (x >= 0){
        return x;
    }else{
        return x *= -1;
    }
}
//Calculating height
double calc_height(double d, double c, double beta)
{
    return cfrac_tan(beta, ITERATIONS)*d+c;
}
//Calculating distance
double calc_distance(double c, double alpha)
{
    return c/cfrac_tan(alpha,ITERATIONS);

}
//Geting input angle into interval <-PI/2, PI/2> so program can calculate correctly
double norm_angle(double angle)
{
    char was_negative;

    if (angle < 0){
        angle *=-1;
        was_negative = 1;
    }else if (angle > 0)
        was_negative = 0;

    while(angle > PI/2){
        angle -= PI;
    }
    if (was_negative == 1)
        return angle *= -1;
    return angle;
}
/*Prints tangent from math.h, taylor and continued fraction methods and their ab
* solute errors for iterations from N to M (exclude math.h method - it stays the same)
* in format |Iteration|Math_tangent|Taylor_tang|Taylor_math_abs_err|CF_tan|cf_math_abs_err
*/
void print_tan(double angle, int N, int M)
{
    double math_tan;    //tangent from math.h
    double tay_tan;     //taylor polynom tangent
    double cf_tan;      //continued fraction tangent
    double n_angle;     //normalized angle
    n_angle = norm_angle(angle);
    math_tan = tan(n_angle);
    for (int i = N; i <= M; i++){
        tay_tan = taylor_tan(n_angle, i);
        cf_tan = cfrac_tan(n_angle, i);
        printf("%d %e %e %e %e %e\n",i ,math_tan, tay_tan, abs_value(tay_tan - math_tan), cf_tan, abs_value(cf_tan - math_tan));
    }
}
//Function printing help
void print_help(void)
{
    printf("Help:\n"
            "~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~\n"
            "Program has two main uses:\n"
            "\n"
            "(1)  |--tan A N M| compares accuracy of tangent approximation between C math.h library,\n"
            "                   Taylor polynom and continued fraction methods\n"
            "\n"
            "     Arguments:  A     -  angle in radians,\n"
            "                 N, M  -  where 0 < N <= M <= 13 are phases of iteration (limited \n"
            "                          to M by taylor's polynome complexity)\n"
            "\n"
            "(2)  |[-c X] -m A [B]| measures and calculates object distance and height\n"
            "\n"
            "     Arguments:  -c X  -  <optional> sets height of measuring device X: 0 < X <= 100\n"
            "                 -m A  -  angle in radians A: 0 < A < PI/2\n"
            "                   B   -  <optional> second angle in radians B: 0 < A < PI/2\n"
            "\n"
            "Disclaimer:  If not a number is at angle input, program takes it as a 0 or undefined\n"
            "             behavior may happen\n"
            "\n"
            "Author:  Zdenek Dolezal                                                Login:  xdolez82\n"
            "~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~\n"
    );
}
/* Function that looks at arguments and if it gets correct parameter, then it
*  checks if all other parameters are in correct format. When every parameter
*  is in correct format (strtod/strtol error returns 0), then it checks if
*  any of the parameters are in incorrect format (limit values) or if they
*  are in correct interval. Then it does action depending of the parameters.
*  If something went wrong, function returns EXIT_FAILURE, otherwise if
*  everything went correctly returns EXIT_SUCCESS
*/
int get_args(int argc, char **argv)
{
    double A, B;                   //Angles alpha and beta
    double c = IMPLICIT_HEIGHT;    //implicit height of device
    double d;                      //calculated distance
    unsigned long N, M;            //iterations interval
    char *errptr;                  //error pointer for strtod
    //input check for help | --help
    if (strcmp(argv[1],"--help") == 0 && argc == 2){
        print_help();
        return EXIT_SUCCESS;
    //input check for tangent | --tan
  }else if (strcmp(argv[1],"--tan") == 0 && argc == 5){
        A = strtod(argv[2],&errptr);
        if (*errptr != 0){
            ERROR_STATE;
            return EXIT_FAILURE;
        }
        //for strtoul base is 10
        N = strtoul(argv[3],&errptr,10);
        if (*errptr != 0){
            ERROR_STATE;
            return EXIT_FAILURE;
        }
        M = strtoul(argv[4],&errptr,10);
        if (*errptr != 0){
            ERROR_STATE;
            return EXIT_FAILURE;
        }
        if (N > 0 && N <= M && M <= TAYLOR_ITERATIONS){
            print_tan(A, N, M);
            return EXIT_SUCCESS;
        }else{
            ERROR_STATE;
            return EXIT_FAILURE;
        }
     //input check for measuring with one angle | -m A
    }else if (strcmp(argv[1],"-m") == 0 && argc == 3){
        A = strtod(argv[2],&errptr);
        if (*errptr != 0){
            ERROR_STATE;
            return EXIT_FAILURE;
        }
        //checking if angle is in correct format
        if (A > MIN_ANGLE && A <= MAX_ANGLE){
            printf("%.10e\n",calc_distance(c, A));
            return EXIT_SUCCESS;
        }else{
            ERROR_STATE;
            return EXIT_FAILURE;
        }
    //input check for measuring with two angles | -m A B
    }else if (strcmp(argv[1],"-m") == 0 && argc == 4){
        A = strtod(argv[2],&errptr);
        if (*errptr != 0){
            ERROR_STATE;
            return EXIT_FAILURE;
        }
        B = strtod(argv[3],&errptr);
        if (*errptr != 0){
            ERROR_STATE;
            return EXIT_FAILURE;
        }
        if (A > MIN_ANGLE && A <= MAX_ANGLE && B > MIN_ANGLE && B <= MAX_ANGLE){
            d = calc_distance(c, A);
            printf("%.10e\n",d);
            printf("%.10e\n", calc_height(d, c, B));
            return EXIT_SUCCESS;
        }else{
            ERROR_STATE;
            return EXIT_FAILURE;
        }
    //input check for measuring with one angle and different device height | -c X -m A
  }else if (strcmp(argv[1],"-c") == 0 && argc == 5 && strcmp(argv[3],"-m") == 0){
        c = strtod(argv[2],&errptr);
        if (*errptr != 0){
            ERROR_STATE;
            return EXIT_FAILURE;
        }
        A = strtod(argv[4],&errptr);
        if (*errptr != 0){
            ERROR_STATE;
            return EXIT_FAILURE;
        }
        if (A > MIN_ANGLE && A <= MAX_ANGLE && c > MIN_DEV_HEIGHT && c <= MAX_DEV_HEIGHT){
            printf("%.10e\n",calc_distance(c, A));
            return EXIT_SUCCESS;
        }else{
            ERROR_STATE;
            return EXIT_FAILURE;
        }
    //input check for measuring with two angles and different device height | -c X -m A B
    }else if (strcmp(argv[1],"-c") == 0 && argc == 6 && strcmp(argv[3],"-m") == 0){
        c = strtod(argv[2],&errptr);
        if (*errptr != 0){
            ERROR_STATE;
            return EXIT_FAILURE;
        }
        A = strtod(argv[4],&errptr);
        if (*errptr != 0){
            ERROR_STATE;
            return EXIT_FAILURE;
        }
        B = strtod(argv[5],&errptr);
        if (*errptr != 0){
            ERROR_STATE;
            return EXIT_FAILURE;
        }
        if (A > MIN_ANGLE && A <= MAX_ANGLE && B > MIN_ANGLE && B <= MAX_ANGLE && c > MIN_DEV_HEIGHT && c <= MAX_DEV_HEIGHT){
            d = calc_distance(c, A);
            printf("%.10e\n",d);
            printf("%.10e\n", calc_height(d, c, B));
            return EXIT_SUCCESS;
        }else{
            ERROR_STATE;
            return EXIT_FAILURE;
        }
    }else{
        ERROR_STATE;
        return EXIT_FAILURE;
    }
}

int main(int argc, char *argv[])
{
    //Check if any argument is inputted
    if (argc > 1){
        if (EXIT_FAILURE == get_args(argc, argv)){
            return EXIT_FAILURE;
            }
        return EXIT_SUCCESS;
    }else{
        ERROR_STATE;
        return EXIT_FAILURE;
    }
}
