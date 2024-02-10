# Documentazione per il progetto nel complesso

## Struttura delle cartelle

* api-definition: contiene la lista di enpoint per i backend del progetto
* backend: cartella che contiene i 2 backend del progetto; per Applicazione Web: auth, e per Progettazione e implementazione di sistemi software in rete: Api
* database: contiene gli schemi per i backend dei 2 progetti
* envoy: contiene il file di configurazione .yaml di Envoy Proxy per effettuare l'instradamento delle
richieste alle relative applicazioni (frontend, backend api, backend auth)
* fake_oauth_server: cartella che contiene un semplice script nodejs per creare un server http usato nei test del backend per operazioni che richiedono OAuth
* oauth_redirect: cartella che contiene un file di configurazione di Envoy e uno script nodejs per creare un server web usato per effettuare il redirect da un endpoint oauth a localhost. I provider OAuth spesso necessitano di endpoint per i redirect in https, e il server presente in questa cartella viene eseguito su una macchina virtuale in remote con un dominio https collegato per effettuare il redirect a localhost
* test: cartella che contiene i test utili a verificare il corretto funzionamento dei due backend.
* mosquitto: contiene i file di configurazione per il server mosquitto

## Struttura dei test
Nella cartella test si trova un progetto creato in Python utile per la fase di component testing di ciascuno dei due backend. I vari test presenti nella cartella cicd_test_suite/backend/*/routes effettuano quanto segue: inserimento dei dati di test nel database, esecuzioni di chiamate rest al componente da testare, e controllo finale che i risultati ottenuti siano congrui a quanto ci si aspetta. I test per i server di backend necessitano per varie ragioni un database e di un server che emuli un provider oauth. Tale richiesta viene soddisfatta mediante l'ausilio di Container usati per eseguire le rispettive applicazioni in ambienti isolati e facili da gestire.
### Eseguire i test
Per il progetto in Python viene usato Poetry come package manager al posto di Pip, tuttavia rimane possibile eseguire i test senza dover installare Poetry. Per eseguire i test senza Poetry bisogna installare manualmente le dipendenze presenti nel file pyproject.toml tramite Pip o con il package manager a disposizione.<br />
Le dipendenze in questione si trovano nella sezione tool.poetry.dependencies del file pyproject.toml<br />
Per installarle manualmente eseguire il comando sottostante per ciascuna dipendenza
```console
~$ pip install dep_name
```
Per installare le dipendenze con Poetry, eseguire il comando:
```console
~$ poetry install
```
Una volta installate le dipendenze necessarie, i test sono eseguibili con il comando: 
```console
~$ python cicd_test_suite/main.py
```
O, se viene usato Poetry:
```console
~$ poetry run cicd_test_suite/main.py
```
Il progetto, per essere eseguito, necessita oltre alle varie dipendenze, anche di Docker, software usato per la gestione dei container che conterranno le varie parti sotto test.
> :warning: **Docker per eseguire e gestire i container necessita dei permessi di root**. Per evitare di eseguire l'intera applicazione Python con permessi di root potrebbe essere necessario assegnare l'utente al gruppo Docker. Istruzioni presenti al link: [Docker without sudo](https://docs.docker.com/engine/install/linux-postinstall/)


## Struttura del backend Auth
Nella cartella backend/auth si trova il codice sorgente della parte di backend relativa al progetti di Applicazioni Web. Nella cartella src si trova il sorgente del backend, mentre nella cartella test di trova il progetto per lo unit test di alcune parti del sorgente. Come scritto in precedenza il component testing effettivo viene fatto con Docker tramite un programma scritto in Python.
Per eseguire correttamente il software nella cartella src serve creare i/il file:
```
appsettings.<environment>.json
```
Dove **environment** corrisponde al valore di una variabile d'ambiente:
```
ASPNETCORE_ENVIRONMENT=<Development|Testing>
```
Oltre a questo file risulta comunque necessario creare il file:
```
appsettings.json
```
Il contenuto di tali file prevede:
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "Kestrel": {
    "Endpoints": {
      "Http": {
        "Url": "http://0.0.0.0:8000"
      }
    }
  }
}
```
La modifica di Http.Url potrebbe causare funzionamenti anomali quando vengono eseguiti i test di component. Tali anomalie sono causate dal fatto che il programma Python si aspetta che il backend eseguito all'interno del container rimanga in ascolto sulla porta 8000.<br />
Quando viene eseguito il backend auth risulta necessario impostare altri parametri, come la connessione al DB:
```json
{
  "Logging": {},
  "Kestrel": {},
  "database": {
    "host": "ip",
    "port": "port",
    "username": "username",
    "password": "password",
    "database": "database"
  }
}
```


