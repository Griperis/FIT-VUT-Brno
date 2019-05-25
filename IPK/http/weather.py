#!/usr/bin/env python3

# IPK Projekt 1 - OpenWeatherMap klient
# Autor: Zdenek Dolezal
# Login: xdolez82
# Fakulta informacnich technologii VUT v Brne

import socket
import sys
import json

DEC_PLACES = 2
STATUS_OK = 200
STATUS_PARTIAL = 203

def parse_response(response):
    # Zpracovani odpovedi ze serveru a zpracovani jsonem
    response = response.decode()
    response = response.split('\r\n')
    status = response[0].split(' ')[1]
    if (int(status) == STATUS_OK):
        # Ziskani dat z odpovedi (za poslednim \r\n)
        data = json.loads(response[-1])
    elif (int(status) == STATUS_PARTIAL):
        print("Warning: Data can be incomplete, server returned status: " + status)
        data = json.loads(response[-1]);
    else:
        # Pokus o zjisteni zparvy od serveru
        error_msg = "n/a"
        try:
            error_msg = json.loads(response[-1])['message']
        finally:
            print("ERROR: Server returned status: " + status + "\nWith message: " + error_msg)
            sys.exit()
    return data


def print_data(data):
    # Vypis dat ziskanych z openweathermap api na vystup, v pripade nedostupnych dat je vlozeno n/a

    # Nazev a lokace(stat) mesta
    if 'name' in data:
        out = "City: " + data['name']
        if 'sys' in data and 'country' in data['sys']:
             out += " [" + data['sys']['country'] + "]"
        print(out)

    # Vyuziti zachyceni vyjimky pro lepsi citelnost pri zjisteni popisu pocasi
    try:
        print("Weather: " + data['weather'][0]['description'])
    except:
        print("Weather: n/a")

    # Hlavni cast dat o pocasi
    if 'main' in data:
        main_data = data['main']
        if 'temp' in main_data:
            print("Temperature: " + str(round(main_data['temp'], DEC_PLACES)) + "°C")
        else:
            print("Temperature: n/a °C")

        if 'pressure' in main_data:
            print("Pressure: " + str(main_data['pressure']) + " hPa")
        else:
            print("Pressure: n/a hPa")

        if 'humidity' in main_data:
            print("Humidity: " + str(main_data['humidity']) + " %")
        else:
            print("Humidity: n/a %")

    # Data o vetru
    if 'wind' in data:
        if 'speed' in data['wind']:
            print("Wind speed: " + str(data['wind']['speed']) + " m/s")
        else:
            print("Wind speed: n/a")
        if 'deg' in data['wind']:
            print("Wind direction: " + str(round(data['wind']['deg'], DEC_PLACES)) + "°")
        else:
            print("Wind direction: n/a")


if __name__ == "__main__":
    # Kontrola argumentu
    if len(sys.argv) == 3:
        api_key = sys.argv[1]
        city = sys.argv[2]
    else:
        print("ERROR: Invalid number of arguments")
        sys.exit()

    request = "/data/2.5/weather?q=" + city.lower() + "&units=metric&APPID=" + api_key
    host = "api.openweathermap.org"
    port = 80

    # Spojeni socketu a zaslani pozadavku na server
    with socket.socket(socket.AF_INET, socket.SOCK_STREAM) as sck:
        msg = "GET " + request + " HTTP/1.1\r\nHost: " + host + "\r\nConnection: close \r\n\r\n"

        response = ''
        # Osetreni kdyby se nejaka z operaci se socketem nepovedla
        try:
            sck.connect((host, port))
            sck.send(str.encode(msg))
            response = sck.recv(4096)
        except socket.error as exc:
            print("ERROR: Socket communication failed. Error: " + str(exc))
            sys.exit()

        print_data(parse_response(response))
