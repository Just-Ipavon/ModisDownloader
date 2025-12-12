# NASA LADSWEB Downloader Pro

A native Windows application for automated downloading of MODIS/VIIRS data from NASA EarthData servers.
Applicazione nativa Windows per il download automatizzato di dati MODIS/VIIRS dai server NASA EarthData.

---

## Index / Indice

### English Section
1. [Description](#description)
2. [Key Features](#key-features)
3. [Requirements](#requirements)
4. [Installation and Usage](#installation-and-usage)
5. [Compilation](#compilation)
6. [Security](#security)

### Sezione Italiana
1. [Descrizione](#descrizione)
2. [Funzionalita Principali](#funzionalita-principali)
3. [Requisiti](#requisiti)
4. [Installazione e Uso](#installazione-e-uso)
5. [Compilazione](#compilazione-1)
6. [Sicurezza](#sicurezza)

---

<a name="description"></a>
## Description (English)

NasaDownloader is a modern solution developed in C# (Windows Forms, .NET 10) designed to replace legacy scripts based on wget. The software interfaces directly with NASA LADSWEB JSON APIs to download satellite products (such as MYD03, MYD021KM, MYD35_L2) efficiently and in parallel.

The application allows massive data downloading, supporting both saving to local network NAS (UNC paths) and any custom local folder. It handles authentication autonomously via Bearer Token.

<a name="key-features"></a>
## Key Features

* **Native & No Dependencies:** Does not require wget.exe. Uses System.Net.Http for direct connections.
* **Async Multithreading:** Downloads files simultaneously (parallel hours processing) without blocking the UI.
* **Dual Language:** Real-time interface switching between English and Italian.
* **Flexible Selection:**
    * **Single Day:** Downloads a specific date.
    * **Day Range:** Downloads an interval (e.g., May 10th to May 20th).
    * **Month Range:** Downloads entire months/years automatically.
* **Custom Save Location:** Choose any local folder or use preset NAS network paths.
* **JSON API Integration:** Verifies file existence on NASA servers before downloading, preventing 404 errors.
* **Token Security:** The NASA Token is saved securely in the AppData folder and is never hardcoded.
* **Hour Filter:** Ability to download specific time slots (e.g., from 03:00 to 15:00).

<a name="requirements"></a>
## Requirements

* Windows 10 or Windows 11 (64-bit).
* An active **NASA EarthData** account.
* A generated **Token** (Profile -> App Keys -> Generate Token).

<a name="installation-and-usage"></a>
## Installation and Usage

1.  Download the latest executable from the **Releases** section.
2.  Run `NasaDownloader.exe`.
3.  On the first run, enter your **Token** (it will be saved automatically).
4.  Select the **Database** (e.g., MYD03) and **Archive** (e.g., 61).
5.  Choose the destination: use a NAS preset or select "Local" to browse for a specific folder on your PC.
6.  Select the download mode (Single Day, Range, or Month).
7.  Click **START DOWNLOAD**.

Files will be automatically organized into `Month_Year` subfolders.

<a name="compilation"></a>
## Compilation

To compile the source code yourself:

1.  Clone the repository.
2.  Run the following command in the project directory:
    `dotnet publish -c Release -r win-x64 --self-contained -p:PublishSingleFile=true -p:EnableCompressionInSingleFile=true`
3.  The executable will be located in `bin/Release/net10.0-windows/win-x64/publish/`.

<a name="security"></a>
## Security

The `.gitignore` file is configured to exclude sensitive files from the repository. The real user token is saved in the secure Windows user path:
`C:\Users\%USERNAME%\AppData\Roaming\NasaDownloader\settings.txt`

---
---

<a name="descrizione"></a>
## Descrizione (Italiano)

NasaDownloader e una soluzione moderna sviluppata in C# (Windows Forms, .NET 10) progettata per sostituire i vecchi script basati su wget. Il software si interfaccia direttamente con le API JSON di NASA LADSWEB per scaricare prodotti satellitari (come MYD03, MYD021KM, MYD35_L2) in modo efficiente e parallelo.

L'applicazione permette il download massivo di dati, supportando sia il salvataggio su NAS di rete locale (percorsi UNC) sia su qualsiasi cartella locale personalizzata. Gestisce autonomamente l'autenticazione tramite Token Bearer.

<a name="funzionalita-principali"></a>
## Funzionalita Principali

* **Nativo & Senza Dipendenze:** Non richiede wget.exe. Usa System.Net.Http per connessioni dirette.
* **Multithreading Asincrono:** Scarica file simultaneamente senza bloccare l'interfaccia.
* **Doppia Lingua:** Cambio interfaccia in tempo reale tra Inglese e Italiano.
* **Selezione Flessibile:**
    * **Giorno Singolo:** Scarica una data specifica.
    * **Range Giorni:** Scarica un intervallo di giorni.
    * **Range Mesi:** Scarica interi mesi o anni automaticamente.
* **Salvataggio Personalizzato:** Possibilita di scegliere qualsiasi cartella locale o usare i preset NAS.
* **Integrazione API JSON:** Verifica l'esistenza dei file sul server NASA prima del download.
* **Sicurezza Token:** Il Token NASA viene salvato in modo sicuro nella cartella AppData.
* **Filtro Orario:** Possibilita di scaricare solo fasce orarie specifiche (es. dalle 03:00 alle 15:00).

<a name="requisiti"></a>
## Requisiti

* Windows 10 o Windows 11 (64-bit).
* Un account **NASA EarthData** attivo.
* Un **Token** generato dal sito NASA (Profile -> App Keys -> Generate Token).

<a name="installazione-e-uso"></a>
## Installazione e Uso

1.  Scarica l'ultimo eseguibile dalla sezione **Releases**.
2.  Avvia `NasaDownloader.exe`.
3.  Al primo avvio, inserisci il tuo **Token** (verra salvato automaticamente).
4.  Seleziona il **Database** (es. MYD03) e l'**Archivio** (es. 61).
5.  Scegli la destinazione: usa un preset NAS o seleziona "Locale" e clicca "..." per scegliere una cartella.
6.  Scegli la modalita di download (Giorno, Range o Mese).
7.  Clicca su **AVVIA DOWNLOAD**.

I file verranno organizzati automaticamente in sottocartelle `Mese_Anno`.

<a name="compilazione-1"></a>
## Compilazione

Per compilare il codice sorgente autonomamente:

1.  Clona la repository.
2.  Esegui il seguente comando nella cartella del progetto:
    `dotnet publish -c Release -r win-x64 --self-contained -p:PublishSingleFile=true -p:EnableCompressionInSingleFile=true`
3.  Troverai l'eseguibile in: `bin/Release/net10.0-windows/win-x64/publish/`.

<a name="sicurezza"></a>
## Sicurezza

Il file `.gitignore` esclude i file sensibili dalla repository. Il token reale dell'utente viene salvato nel percorso sicuro di Windows:
`C:\Users\%USERNAME%\AppData\Roaming\NasaDownloader\settings.txt`