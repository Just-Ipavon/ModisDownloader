using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Drawing;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Linq;

namespace NasaDownloader
{
    // --- GESTORE CONFIGURAZIONE (Salva il token in AppData) ---
    public static class ConfigManager
    {
        // Percorso sicuro: C:\Users\Utente\AppData\Roaming\NasaDownloader\settings.txt
        private static string FolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "NasaDownloader");
        private static string FilePath = Path.Combine(FolderPath, "settings.txt");

        public static string LoadToken(string defaultToken)
        {
            try
            {
                if (File.Exists(FilePath))
                {
                    string savedToken = File.ReadAllText(FilePath).Trim();
                    if (!string.IsNullOrEmpty(savedToken)) return savedToken;
                }
            }
            catch { /* Ignora errori di lettura */ }
            return defaultToken;
        }

        public static void SaveToken(string token)
        {
            try
            {
                if (!Directory.Exists(FolderPath)) Directory.CreateDirectory(FolderPath);
                File.WriteAllText(FilePath, token);
            }
            catch { /* Ignora errori di scrittura */ }
        }
    }

    // --- CLASSI PER IL PARSING JSON ---
    public class NasaFile { public string name { get; set; } }
    public class NasaResponse { public List<NasaFile> content { get; set; } }

    public partial class Form1 : Form
    {
        // --- COMPONENTI GUI ---
        private TabControl tabControl;
        private TabPage tabSingle, tabRangeDay, tabRangeMonth;
        private DateTimePicker dtpSingle, dtpRangeStart, dtpRangeEnd, dtpMonthStart, dtpMonthEnd;
        private TextBox txtToken, txtArchivio, txtLocalPath;
        private ComboBox cmbDatabase;
        private RadioButton rbNas1, rbNas2, rbLocal;
        private Button btnDownload, btnLang, btnBrowseLocal;
        private RichTextBox rtbLog;
        private NumericUpDown numHourStart, numHourEnd;
        private Label lblToken, lblDb, lblArch, lblFromDay, lblToDay, lblFromMonth, lblToMonth, lblDaySingle, lblStartHour, lblEndHour;
        private GroupBox grpNas, grpOre;

        // Costanti e Variabili
        const string BaseUrl = "https://ladsweb.modaps.eosdis.nasa.gov/archive/allData/61";
        string defaultToken = "INSERISCI_IL_TUO_TOKEN_QUI"; // Sostituisci con il token predefinito

        private static readonly HttpClient client = new HttpClient();
        private bool isEnglish = false; // Default: Italiano

        public Form1()
        {
            InitializeGui();
            
            // Carica il token salvato (se esiste)
            txtToken.Text = ConfigManager.LoadToken(defaultToken);
            
            cmbDatabase.SelectedIndex = 0; 
            rbLocal.Checked = true; // Default su Locale per comodità
            
            UpdateLanguage(); // Applica le etichette in Italiano
        }

        private void InitializeGui()
        {
            this.Text = "NASA LADSWEB Downloader Pro";
            this.Size = new Size(620, 850);
            this.StartPosition = FormStartPosition.CenterScreen;

            // Tasto Cambio Lingua (In alto a destra)
            btnLang = new Button() { Text = "EN", Location = new Point(530, 10), Size = new Size(50, 25), BackColor = Color.LightGray };
            btnLang.Click += (s, e) => { isEnglish = !isEnglish; UpdateLanguage(); };
            this.Controls.Add(btnLang);

            int y = 40;

            // Token
            lblToken = CreateLabel("Token:", 20, y);
            txtToken = new TextBox() { Location = new Point(20, y + 20), Width = 560 };
            this.Controls.Add(txtToken);
            y += 50;

            // Database
            lblDb = CreateLabel("Database:", 20, y);
            cmbDatabase = new ComboBox() { Location = new Point(20, y + 20), Width = 200 };
            cmbDatabase.Items.AddRange(new object[] { "MYD03", "MYD021KM", "MYD35_L2" });
            this.Controls.Add(cmbDatabase);

            // Archivio
            lblArch = CreateLabel("Archivio:", 240, y);
            txtArchivio = new TextBox() { Text = "61", Location = new Point(240, y + 20), Width = 80 };
            this.Controls.Add(txtArchivio);
            
            // Gruppo Destinazione (NAS o Locale)
            grpNas = new GroupBox() { Text = "Destinazione Salvataggio", Location = new Point(20, y + 60), Size = new Size(560, 100) };
            
            rbNas1 = new RadioButton() { Text = "NAS29F79B", Location = new Point(10, 25), Width = 100 };
            rbNas2 = new RadioButton() { Text = "NASFA8369", Location = new Point(120, 25), Width = 100 };
            
            // Radio Button Locale + Logica Abilitazione
            rbLocal = new RadioButton() { Text = "Locale (Scegli cartella)", Location = new Point(230, 25), Width = 180, ForeColor = Color.Blue, Font = new Font(this.Font, FontStyle.Bold) };
            rbLocal.CheckedChanged += (s, e) => { 
                txtLocalPath.Enabled = rbLocal.Checked; 
                btnBrowseLocal.Enabled = rbLocal.Checked; 
            };

            // Path Locale e Tasto Sfoglia (...)
            txtLocalPath = new TextBox() { Location = new Point(10, 60), Width = 450, ReadOnly = true, Text = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "NasaDownload") };
            btnBrowseLocal = new Button() { Text = "...", Location = new Point(470, 58), Width = 40 };
            btnBrowseLocal.Click += BtnBrowseLocal_Click;

            grpNas.Controls.Add(rbNas1); grpNas.Controls.Add(rbNas2); grpNas.Controls.Add(rbLocal);
            grpNas.Controls.Add(txtLocalPath); grpNas.Controls.Add(btnBrowseLocal);
            this.Controls.Add(grpNas);
            y += 180;

            // Tabs (Giorno, Range, Mesi)
            tabControl = new TabControl() { Location = new Point(20, y), Size = new Size(560, 120) };
            
            tabSingle = new TabPage("Giorno Singolo");
            lblDaySingle = CreateLabel("Giorno:", 10, 10, tabSingle);
            dtpSingle = new DateTimePicker() { Location = new Point(10, 30), Width = 200 };
            tabSingle.Controls.Add(dtpSingle);
            tabControl.TabPages.Add(tabSingle);

            tabRangeDay = new TabPage("Range Giorni");
            lblFromDay = CreateLabel("Da:", 10, 10, tabRangeDay); dtpRangeStart = new DateTimePicker() { Location = new Point(10, 30), Width = 200 };
            lblToDay = CreateLabel("A:", 230, 10, tabRangeDay); dtpRangeEnd = new DateTimePicker() { Location = new Point(230, 30), Width = 200 };
            tabRangeDay.Controls.Add(dtpRangeStart); tabRangeDay.Controls.Add(dtpRangeEnd);
            tabControl.TabPages.Add(tabRangeDay);

            tabRangeMonth = new TabPage("Range Mesi");
            lblFromMonth = CreateLabel("Da Mese:", 10, 10, tabRangeMonth); dtpMonthStart = new DateTimePicker() { Location = new Point(10, 30), Width = 200, CustomFormat = "MM/yyyy", Format = DateTimePickerFormat.Custom };
            lblToMonth = CreateLabel("A Mese:", 230, 10, tabRangeMonth); dtpMonthEnd = new DateTimePicker() { Location = new Point(230, 30), Width = 200, CustomFormat = "MM/yyyy", Format = DateTimePickerFormat.Custom };
            tabRangeMonth.Controls.Add(dtpMonthStart); tabRangeMonth.Controls.Add(dtpMonthEnd);
            tabControl.TabPages.Add(tabRangeMonth);
            this.Controls.Add(tabControl);
            y += 130;

            // --- FILTRO ORE (CORRETTO BUG GRAFICO) ---
            grpOre = new GroupBox() { Text = "Filtro Ore (0-23)", Location = new Point(20, y), Size = new Size(560, 70) };
            
            // Blocco Inizio (spostato a X=30)
            lblStartHour = CreateLabel("Inizio:", 30, 25, grpOre); 
            numHourStart = new NumericUpDown() { Location = new Point(90, 22), Width = 60, Minimum = 0, Maximum = 23, Value = 3 };
            
            // Blocco Fine (spostato a X=280 per evitare sovrapposizioni)
            lblEndHour = CreateLabel("Fine:", 280, 25, grpOre); 
            numHourEnd = new NumericUpDown() { Location = new Point(340, 22), Width = 60, Minimum = 0, Maximum = 23, Value = 15 };
            
            grpOre.Controls.Add(numHourStart); grpOre.Controls.Add(numHourEnd);
            this.Controls.Add(grpOre);
            y += 80;

            // Bottone Download
            btnDownload = new Button() { Text = "AVVIA DOWNLOAD", Location = new Point(20, y), Size = new Size(560, 40), BackColor = Color.LightSkyBlue, Font = new Font(this.Font, FontStyle.Bold) };
            btnDownload.Click += BtnDownload_Click;
            this.Controls.Add(btnDownload);
            y += 50;

            // Area Log
            rtbLog = new RichTextBox() { Location = new Point(20, y), Size = new Size(560, 200), ReadOnly = true, BackColor = Color.Black, ForeColor = Color.Lime };
            this.Controls.Add(rtbLog);
        }

        private Label CreateLabel(string text, int x, int y, Control parent = null)
        {
            Label l = new Label() { Text = text, Location = new Point(x, y), AutoSize = true };
            if (parent == null) this.Controls.Add(l); else parent.Controls.Add(l);
            return l;
        }

        // --- GESTIONE LINGUA ---
        private void UpdateLanguage()
        {
            btnLang.Text = isEnglish ? "IT" : "EN"; // Cambia testo tasto
            
            if (isEnglish)
            {
                lblToken.Text = "Token (Auto-saved):";
                lblDb.Text = "Database:";
                lblArch.Text = "Archive:";
                grpNas.Text = "Save Destination";
                rbLocal.Text = "Local (Choose Folder)";
                tabSingle.Text = "Single Day";
                tabRangeDay.Text = "Day Range";
                tabRangeMonth.Text = "Month Range";
                lblDaySingle.Text = "Select Day:";
                lblFromDay.Text = "From:"; lblToDay.Text = "To:";
                lblFromMonth.Text = "From Month:"; lblToMonth.Text = "To Month:";
                grpOre.Text = "Hour Filter (0-23)";
                lblStartHour.Text = "Start:"; lblEndHour.Text = "End:";
                btnDownload.Text = "START DOWNLOAD";
            }
            else
            {
                lblToken.Text = "Token (Salvato in automatico):";
                lblDb.Text = "Database:";
                lblArch.Text = "Archivio:";
                grpNas.Text = "Destinazione Salvataggio";
                rbLocal.Text = "Locale (Scegli Cartella)";
                tabSingle.Text = "Giorno Singolo";
                tabRangeDay.Text = "Range Giorni";
                tabRangeMonth.Text = "Range Mesi";
                lblDaySingle.Text = "Giorno:";
                lblFromDay.Text = "Da:"; lblToDay.Text = "A:";
                lblFromMonth.Text = "Da Mese:"; lblToMonth.Text = "A Mese:";
                grpOre.Text = "Filtro Ore (0-23)";
                lblStartHour.Text = "Inizio:"; lblEndHour.Text = "Fine:";
                btnDownload.Text = "AVVIA DOWNLOAD";
            }
        }

        // --- BROWSER CARTELLA LOCALE ---
        private void BtnBrowseLocal_Click(object sender, EventArgs e)
        {
            using (var fbd = new FolderBrowserDialog())
            {
                fbd.Description = isEnglish ? "Select destination folder" : "Seleziona cartella di destinazione";
                fbd.UseDescriptionForTitle = true;
                if (fbd.ShowDialog() == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                {
                    txtLocalPath.Text = fbd.SelectedPath;
                }
            }
        }

        // --- LOGICA DI DOWNLOAD PRINCIPALE ---
        private async void BtnDownload_Click(object sender, EventArgs e)
        {
            // Salva il token attuale
            ConfigManager.SaveToken(txtToken.Text);

            btnDownload.Enabled = false;
            rtbLog.Clear();
            Log(isEnglish ? "Starting process..." : "Avvio processo...");

            // Imposta header autenticazione
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", txtToken.Text);

            try
            {
                // 1. Calcolo Date
                DateTime startDate, endDate;
                if (tabControl.SelectedTab == tabSingle) { 
                    startDate = dtpSingle.Value.Date; 
                    endDate = startDate; 
                }
                else if (tabControl.SelectedTab == tabRangeDay) { 
                    startDate = dtpRangeStart.Value.Date; 
                    endDate = dtpRangeEnd.Value.Date; 
                }
                else { 
                    // Logica Mesi: Dal 1° del mese Start all'ultimo del mese End
                    startDate = new DateTime(dtpMonthStart.Value.Year, dtpMonthStart.Value.Month, 1);
                    endDate = new DateTime(dtpMonthEnd.Value.Year, dtpMonthEnd.Value.Month, 1).AddMonths(1).AddDays(-1);
                }

                if (startDate > endDate) throw new Exception(isEnglish ? "Start Date must be before End Date" : "Data Inizio deve essere prima della Fine");

                // 2. Lettura Parametri
                string db = cmbDatabase.SelectedItem.ToString();
                string archivio = txtArchivio.Text;
                string nasName = rbNas1.Checked ? "NAS29F79B" : "NASFA8369";
                int hStart = (int)numHourStart.Value;
                int hEnd = (int)numHourEnd.Value;

                // 3. Definizione Percorso Base
                string basePath;
                if (rbLocal.Checked) 
                {
                    basePath = txtLocalPath.Text; // Usa percorso utente
                }
                else 
                {
                    basePath = Path.Combine($@"\\{nasName}", archivio, "Modis"); // Usa percorso NAS
                }

                // Crea cartella base se non esiste
                if (!Directory.Exists(basePath)) Directory.CreateDirectory(basePath);
                Log($"Dest: {basePath}");

                // 4. Ciclo Giorni
                for (DateTime date = startDate; date <= endDate; date = date.AddDays(1))
                {
                    Log($"--> {(isEnglish ? "Day" : "Giorno")}: {date:dd/MM/yyyy}");
                    
                    // URL JSON NASA
                    string urlDir = $"{BaseUrl}/{db}/{date.Year}/{date.DayOfYear:000}";
                    
                    // Ottieni lista file
                    List<NasaFile> files = await GetFileListAsync(urlDir);

                    if (files == null || files.Count == 0) { 
                        Log("   [INFO] No files / Nessun file."); 
                        continue; 
                    }

                    // Sottocartella Mese_Anno (es. Maggio_2025)
                    string nomeMese = char.ToUpper(date.ToString("MMMM")[0]) + date.ToString("MMMM").Substring(1);
                    string targetDir = Path.Combine(basePath, $"{nomeMese}_{date.Year}");
                    if (!Directory.Exists(targetDir)) Directory.CreateDirectory(targetDir);

                    // Filtro Ore e Download Parallelo
                    List<Task> tasks = new List<Task>();
                    for (int h = hStart; h < hEnd; h++)
                    {
                        string hourStr = $".{h:00}"; // Cerca es. ".03"
                        
                        // Trova file che corrisponde all'ora (es. .0300. o .0305.)
                        var match = files.FirstOrDefault(f => f.name.Contains($"{hourStr}00.") || f.name.Contains($"{hourStr}05."));
                        
                        if (match != null) 
                        {
                            string fileUrl = $"{urlDir}/{match.name}";
                            string localFilePath = Path.Combine(targetDir, match.name);
                            tasks.Add(DownloadFileAsync(fileUrl, localFilePath));
                        }
                    }
                    
                    // Aspetta che tutte le ore di questo giorno finiscano
                    await Task.WhenAll(tasks);
                }

                Log("--------------------------------");
                Log(isEnglish ? "COMPLETED!" : "COMPLETATO!");
                MessageBox.Show(isEnglish ? "Download Completed!" : "Download Completato!");
            }
            catch (Exception ex) 
            { 
                Log($"ERROR: {ex.Message}"); 
                MessageBox.Show($"Error: {ex.Message}", "Errore", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally 
            { 
                btnDownload.Enabled = true; 
            }
        }

        // Helper: Scarica JSON e restituisce lista file
        private async Task<List<NasaFile>> GetFileListAsync(string url)
        {
            try
            {
                var res = await client.GetAsync(url + ".json");
                if (!res.IsSuccessStatusCode) return null;
                
                string json = await res.Content.ReadAsStringAsync();
                
                // Tenta di deserializzare il formato { content: [...] }
                var root = JsonSerializer.Deserialize<NasaResponse>(json);
                if (root != null && root.content != null) return root.content;
                
                // Fallback: Tenta lista diretta [...]
                return JsonSerializer.Deserialize<List<NasaFile>>(json);
            }
            catch { return null; }
        }

        // Helper: Scarica singolo file
        private async Task DownloadFileAsync(string url, string path)
        {
            try
            {
                if (File.Exists(path)) { 
                    this.Invoke((MethodInvoker)delegate { Log($"   [SKIP] Exists: {Path.GetFileName(path)}"); }); 
                    return; 
                }
                
                using (var res = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead))
                {
                    res.EnsureSuccessStatusCode();
                    using (var fs = File.Open(path, FileMode.Create))
                    {
                        await (await res.Content.ReadAsStreamAsync()).CopyToAsync(fs);
                    }
                }
                this.Invoke((MethodInvoker)delegate { Log($"   [OK] Downloaded: {Path.GetFileName(path)}"); });
            }
            catch (Exception ex) { 
                this.Invoke((MethodInvoker)delegate { Log($"   [ERR] {ex.Message}"); }); 
            }
        }

        private void Log(string msg) { 
            rtbLog.AppendText($"{DateTime.Now:HH:mm} > {msg}\n"); 
            rtbLog.ScrollToCaret(); 
        }
    }
}