# Documentazione per il progetto nel complesso

Progetto a cura di:<br />
Andrea Barchietto<br />
Filippo Checchia

### Note generali
Questo progetto &egrave; stato sviluppato principalmente in ambiente Linux (Ubuntu) con versione SDK 8.0 di DotNet

## Struttura delle cartelle

* api-definition: contiene la lista di endpoint per i backend del progetto
* backend: cartella che contiene i 2 backend del progetto; per Applicazione Web: auth, e per Progettazione e implementazione di sistemi software in rete: api
* database: contiene gli schemi per i backend dei 2 progetti
* envoy: contiene il file di configurazione .yaml di Envoy Proxy per effettuare l'instradamento delle
richieste alle relative applicazioni (frontend, backend api, backend auth)
* fake_oauth_server: cartella che contiene un semplice script nodejs per creare un server http usato nei test del backend per operazioni che richiedono OAuth
* oauth_redirect: cartella che contiene un file di configurazione di Envoy e uno script nodejs per creare un server web usato per il redirect da un endpoint approvato oauth a localhost. I provider OAuth spesso necessitano di endpoint per i redirect in https, e il server presente in questa cartella viene eseguito su una macchina virtuale in remoto con un dominio https. Successivamente verr&agrave; fatto il redirect a lcoalhost
* test: cartella che contiene i test utili a verificare il corretto funzionamento dei due backend.
* mosquitto: contiene i file di configurazione per il server mosquitto

## Struttura dei test
Nella cartella test si trova un progetto creato in Python utile per la fase di component testing di ciascuno dei due backend. I vari test presenti nella cartella cicd_test_suite/backend/*/routes effettuano quanto segue: inserimento dei dati di test nel database, esecuzioni di chiamate rest al componente da testare, e controllo finale che i risultati ottenuti siano congrui a quanto ci si aspetta. I test per i server di backend necessitano per varie ragioni di un database e di un server che emuli un provider oauth. Tale richiesta viene soddisfatta mediante l'ausilio di Container usati per eseguire le rispettive applicazioni in ambienti isolati e facili da gestire.
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
~$ poetry run python3 cicd_test_suite/main.py
```
Una volta avviata l'applicazione, gli step da eseguire sono i seguenti:
1. Setup del network tra container
2. Setup dei container
3. Esecuzione dei test
Segliere dal menu del programma l'opzione in base allo step che si sta per eseguire
Il progetto, per essere eseguito, necessita oltre alle varie dipendenze, anche di Docker, software usato per la gestione dei container che conterranno le varie parti sotto test.
> ATTENZIONE: **Docker per eseguire e gestire i container necessita dei permessi di root**. Per evitare di eseguire l'intera applicazione Python con permessi di root potrebbe essere necessario assegnare l'utente al gruppo Docker. Istruzioni presenti al link: [Docker without sudo](https://docs.docker.com/engine/install/linux-postinstall/)

Per ottimizzare i tempi di creazione del container del backend di Applicazioni Web &egrave; stata creata un'opzione apposita nel menu di avvio. Tale creazione &egrave; gi&agrave; inclusa quando viene fatto il setup completo.

I test sono stati sviluppati in ambienti Linux, cos&igrave; come il backend dell'applicazione pertanto, i container fanno uso di immagini basate su sistema Linux (nei Dockerfile), quindi eseguire i test in ambiente Windows potrebbe non essere possibile (se non si fa uso di WSL2).

### Approvazione automatica degli utenti
Siccome per questo progetto &egrave; previsto come unico sistema di autenticazione quello basato su OpenId, per poter testare le funzionalit&agrave; dell'applicazione quando non esiste ancora alcun utente registrato, bisogna seguire i seguenti step:
1. Accedere alla pagina web come un normale utente ed eseguire l'accesso con il provider desiderato.
2. Compilare la richiesta di adesione al servizio
3. Eseguire lo script Python presente nella cartella: test/cicd_test_suite/backend/auth/authorize.py
4. Inserire le informazioni richieste dallo script Python, ovvero: il token jwt con il quale si &egrave; eseguito l'accesso alla pagina web, e il nome del provider che &egrave; stato utilizzato (esempio: google, facebook)

## Come inserire nuovi provider nel database
Nel file database/auth/init_data.sql sono inseriti 2 esempi che rappresentano il modo di registrare provider da usare per l'applicazione. Lista del necessario: un nome (a libera scelta), l'uri per il link a /.well-known/openid-configuration e la lista di aud (audience) accettati dall'applicazione. In questo modo ogni qualvolta il backend ricever&agrave; un token jwt da parte di un utente sar&agrave; in grado di validarlo in modo autonomo.


## Struttura del backend Auth
Nella cartella backend/auth si trova il codice sorgente della parte di backend relativa al progetto di Applicazioni Web. Nella cartella src si trova il sorgente del backend, mentre nella cartella test di trova il progetto per lo unit test di alcune parti del sorgente. Come scritto in precedenza, il component testing effettivo viene fatto con Docker tramite un programma scritto in Python.
Per eseguire correttamente il software nella cartella src serve creare i file:
```
appsettings.<environment>.json
```
Dove \<environment\> corrisponde al valore di una variabile d'ambiente:
```
ASPNETCORE_ENVIRONMENT=<Development|Testing|...>
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
La modifica di Http.Url potrebbe causare funzionamenti anomali quando vengono eseguiti i test di componente. Tali anomalie sono causate dal fatto che il programma Python si aspetta che il backend eseguito all'interno del container rimanga in ascolto sulla porta 8000.<br />
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
  },
  "local_issuer": "domain"
}
```
Il parametro local_issuer corrisponde al campo iss per i token generati dal backend auth per il backend api del progetto di Progettazione e Implementazione di sistemi software in rete.
Per eseguire il software in ambiente linux con annessa la variabile d'ambiente desiderata, eseguire il comando:
```console
export ASPNETCORE_ENVIRONMENT="Development" dotnet run|watch|build
```

> NOTA per APPLICAZIONI WEB: per testare l'interazione tra frontend e backend non &egrave; necessario fare uso di Envoy Proxy, poich&egrave; il backend &egrave; uno solo. Tale proxy &egrave; stata inserita per instradare le richieste tra i due backend usando un solo dominio da parte del frontend. Quindi, &egrave; possibile eseguire frontend e backend/auth senza particolari accorgimenti.

### Alcune note sulla struttura del backend per Applicazioni Web
Il sistema di autenticazione prevede l'utilizzo di OAuth con OpenId. Siccome lo standard OpenId prevede alcune garanzie per quanto riguarda l'autenticazione si &egrave; scelto di usare come identificativo dell'utente il claim "sub" in coppia con "iss". Perch&egrave; scegliere "sub" e "iss" al posto del campo "email" unito a "email_verified" ? Per il fatto che tutti gli altri dati ad eccezione di "sub" e "iss" possono variare su richiesta dell'utente, motivo per cui possono essere ritenuti inaffidabili.<br />

Il backend, una volta che viene eseguito, lancia due thread:
1. il web server che gestisce le chiamate rest
2. una routine che, dopo aver effettuare una query al DB per ottenere la lista dei provider supportati, si occupa di mantenere aggiornate le chiavi RSA fornite dai provider supportati per verificare l'integrit&agrave; dei token forniti dagli utenti. Nel DB viene salvato per ogni provider supportato il link alla configurazione openid. Per ogni link verr&agrave; effettuata una chiamata rest per ottenere le informazioni quali "issuer" e "jwks" (endpoint per ottenere le chiavi per verificare i token JWT)

Per quanto riguarda la generazione del token jwt per l'accesso alle api, tale processo avviene senza che l'utente ne sia a conoscenza. Quando l'utente cercher&agrave; di accedere a pagine protette che fanno riferimento al server api, sar&agrave; premura del frontend effettuare una chiamata al backend auth per richiedere un token jwt di accesso, e sar&agrave; premura del backend auth effettuare un controllo nel database per verificare se l'utente abbia o meno il permesso, consultando i periodi di accesso consentiti registrati nella tabella api_acl.

## Note sul progetto

L'intero progetto si basa sul disaccoppiare la parte di autenticazione e autorizzazione dalla parte funzionale. Per questo motivo esistono due backend, uno per gestire la creazione degli utenti e la gestione degli accessi alla parte relativa alla gestione dei campi, e uno per la parte di gestione effettiva dei campi e delle risorse delle aziende.<br />
Server backend:
1. Auth: gestisce la registrazione degli utenti, l'accesso alle api, le informazioni delle aziende quali: nome, indirizzo, recapiti, (...), e le informazioni personali degli utenti comprese le informazioni di login
2. Api: non ancora implementato, fa riferimento alla parte di progetto per Progettazione e implementazione di sistemi software in rete.


