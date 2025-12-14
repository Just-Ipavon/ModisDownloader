
<div align="center">

  <img src="icona.ico" alt="Modis Logo" width="120" />

  <h1>NASA LADSWEB Downloader Pro</h1>
  
  <p>
    <strong>A high-performance native Windows application for automated MODIS data retrieval from NASA EarthData servers.</strong>
    <br>
    <strong>Un'applicazione nativa Windows ad alte prestazioni per il download automatizzato di dati MODIS dai server NASA EarthData.</strong>
  </p>

  <p>
    <a href="https://dotnet.microsoft.com/en-us/download/dotnet/10.0">
      <img src="https://img.shields.io/badge/.NET-10.0-512BD4?style=for-the-badge&logo=dotnet&logoColor=white" alt=".NET 10" />
    </a>
    <a href="#">
      <img src="https://img.shields.io/badge/Platform-Windows%20(x64)-0078D6?style=for-the-badge&logo=windows&logoColor=white" alt="Windows" />
    </a>
    <a href="#">
      <img src="https://img.shields.io/badge/License-MIT-green?style=for-the-badge" alt="License" />
    </a>
  </p>

  <br />
</div>

## Index / Indice

### English Section
1. [Description](#description)
2. [Key Features](#key-features)
3. [Tech Stack](#tech-stack)
4. [Installation and Usage](#installation-and-usage)
5. [Build from Source](#build-from-source)
6. [Security and Privacy](#security-and-privacy)

### Sezione Italiana
1. [Descrizione](#descrizione)
2. [Funzionalità Chiave](#funzionalità-chiave)
3. [Stack Tecnologico](#stack-tecnologico)
4. [Installazione e Primo Avvio](#installazione-e-primo-avvio)
5. [Compilazione da Sorgente](#compilazione-da-sorgente)
6. [Sicurezza e Privacy](#sicurezza-e-privacy)

---

<a name="description"></a>
## Description

**NasaDownloader** is a modern solution developed in C# (Windows Forms) designed to replace legacy scripts based on `wget` and C++. The software interfaces directly with NASA LADSWEB JSON APIs to download satellite products (such as `MYD03`, `MYD021KM`, `MYD35_L2`) efficiently and in parallel.

The application is engineered to handle massive scientific data downloads, enabling direct saving to **local network NAS** (UNC paths) or local directories, autonomously managing authentication via Bearer Token.

<a name="key-features"></a>
## Key Features

* **Native & Dependency-Free:** No longer requires `wget.exe`. Uses `System.Net.Http` for direct and fast connections.
* **Async Multithreading:** Downloads files simultaneously (parallel processing by hour) without blocking the user interface.
* **Flexible Selection:**
    * **Single Day:** Download a specific date.
    * **Day Range:** Download an interval (e.g., May 10th to May 20th).
    * **Month Range:** Automatically download entire months/years.
* **JSON API Integration:** Verifies file existence on the NASA server before downloading, preventing 404 errors and empty downloads.
* **Dual Save Mode:** Native support for saving to network UNC paths (`\\NAS...`) or Local Desktop.
* **Token Security:** The NASA Token is securely saved in `%AppData%`.
* **Hour Filter:** Ability to download only specific time slots (e.g., from 03:00 to 15:00).

<a name="tech-stack"></a>
## Tech Stack

* **Framework:** .NET 10 (Windows Forms)
* **Networking:** `HttpClient` with `Async/Await` management
* **Data Parsing:** `System.Text.Json` for NASA directory analysis
* **Configuration:** Persistent storage via `AppData/Roaming`

<a name="installation-and-usage"></a>
## Installation and Usage

### Prerequisites
1. An active **NASA EarthData** account.
2. A generated **Token** (Profile -> App Keys -> Generate Token).
3. Operating System: Windows 10/11 (x64).

### How to use
1. Download the latest release from the **Releases** section (or compile from source).
2. Run `NasaDownloader.exe`.
3. On the first run, enter your **Token** (it will be saved automatically for future sessions).
4. Select the **Database** (e.g., `MYD03`) and **Archive** (e.g., `61`).
5. Choose the download mode (Tab: Single Day, Range, or Month).
6. Click **START DOWNLOAD**.

> **Note:** Files will be automatically organized into subfolders named `Month_Year` (e.g., `May_2025`).

<a name="build-from-source"></a>
## Build from Source

If you want to modify the code or compile the EXE yourself:

1. Clone the repository:
   ```bash
   git clone [https://github.com/YourName/ModisDownloader.git](https://github.com/YourName/ModisDownloader.git)
   cd NasaDownloader

2.  Compile the project generating a single Self-Contained EXE:

    ```bash
    dotnet publish -c Release -r win-x64 --self-contained -p:PublishSingleFile=true -p:EnableCompressionInSingleFile=true

3.  You will find the executable in:
    `bin/Release/net10.0-windows/win-x64/publish/`

<a name="security-and-privacy"></a>

## Security and Privacy

The `.gitignore` file is configured to exclude sensitive files from the repository.

  * The `user_token.txt` file (if used in debug) is not uploaded.
  * The real token is saved in the secure Windows user path:
    `C:\Users\%USERNAME%\AppData\Roaming\NasaDownloader\settings.txt`

-----

-----

<a name="descrizione"></a>

## Descrizione

**NasaDownloader** è una soluzione moderna sviluppata in C\# (Windows Forms) per sostituire i vecchi script basati su `wget` e C++. Il software si interfaccia direttamente con le API JSON di NASA LADSWEB per scaricare prodotti satellitari (come `MYD03`, `MYD021KM`, `MYD35_L2`) in modo efficiente e parallelo.

L'applicazione è progettata per gestire il download massivo di dati scientifici, permettendo il salvataggio diretto su **NAS di rete locale** o su cartelle locali, gestendo autonomamente l'autenticazione tramite Token Bearer.

<a name="funzionalità-chiave"></a>

## Funzionalità Chiave

  * **Nativo & Senza Dipendenze:** Non richiede più `wget.exe`. Utilizza `System.Net.Http` per connessioni dirette e veloci.
  * **Multithreading Asincrono:** Scarica file simultaneamente (parallelo per ore) senza bloccare l'interfaccia utente.
  * **Selezione Flessibile:**
      * **Giorno Singolo:** Scarica una data specifica.
      * **Range Giorni:** Scarica un intervallo (es. dal 10 al 20 Maggio).
      * **Range Mesi:** Scarica interi mesi/anni automaticamente.
  * **Integrazione API JSON:** Verifica l'esistenza dei file sul server NASA prima del download, prevenendo errori 404 e download vuoti.
  * **Dual Save Mode:** Supporto nativo per salvataggio su percorsi di rete UNC (`\\NAS...`) o Locale (Desktop).
  * **Sicurezza Token:** Il Token NASA viene salvato in modo sicuro in `%AppData%`.
  * **Filtro Orario:** Possibilità di scaricare solo fasce orarie specifiche (es. dalle 03:00 alle 15:00).

<a name="stack-tecnologico"></a>

## Stack Tecnologico

  * **Framework:** .NET 10 (Windows Forms)
  * **Networking:** `HttpClient` con gestione `Async/Await`
  * **Data Parsing:** `System.Text.Json` per l'analisi delle directory NASA
  * **Configurazione:** Gestione persistente tramite `AppData/Roaming`

<a name="installazione-e-primo-avvio"></a>

## Installazione e Primo Avvio

### Prerequisiti

1.  Un account **NASA EarthData** attivo.
2.  Un **Token** generato (Profile -\> App Keys -\> Generate Token).
3.  Sistema Operativo Windows 10/11 (x64).

### Come usare

1.  Scarica l'ultima release dalla sezione **Releases** (o compila il codice sorgente).
2.  Avvia `NasaDownloader.exe`.
3.  Al primo avvio, inserisci il tuo **Token** (verrà salvato automaticamente per le sessioni future).
4.  Seleziona il **Database** (es. `MYD03`) e l'**Archivio** (es. `61`).
5.  Scegli la modalità di download (Tab: Giorno, Range o Mese).
6.  Clicca su **AVVIA DOWNLOAD**.

> **Nota:** I file verranno organizzati automaticamente in sottocartelle `Mese_Anno` (es. `Maggio_2025`).

<a name="compilazione-da-sorgente"></a>

## Compilazione da Sorgente

Se vuoi modificare il codice o compilare l'EXE autonomamente:

1.  Clona la repository:

    ```bash
    git clone [https://github.com/TuoNome/ModisDownloader.git](https://github.com/TuoNome/ModisDownloader.git)
    cd NasaDownloader
    ```

2.  Compila il progetto generando un singolo file EXE (Self-Contained):

    ```bash
    dotnet publish -c Release -r win-x64 --self-contained -p:PublishSingleFile=true -p:EnableCompressionInSingleFile=true
    ```

3.  Troverai l'eseguibile in:
    `bin/Release/net10.0-windows/win-x64/publish/`

<a name="sicurezza-e-privacy"></a>

## Sicurezza e Privacy

Il file `.gitignore` è configurato per escludere file sensibili dalla repository.

  * Il file `user_token.txt` (se usato in debug) non viene caricato.
  * Il token reale viene salvato nel percorso utente sicuro di Windows:
    `C:\Users\%USERNAME%\AppData\Roaming\ModisDownloader\settings.txt`

-----

<div align="center"\>
<sub\>Sviluppato per automatizzare l'acquisizione dati MODIS/VIIRS. Non affiliato ufficialmente con la NASA.</sub\>
</div\>
