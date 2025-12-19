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
    // --- CLASSE PER LE IMPOSTAZIONI UTENTE ---
    public class AppSettings
    {
        public string Token { get; set; } = "";
        public string Archivio { get; set; } = "61"; // NASA Source
        public string TargetArchivio { get; set; } = "Archivio_7"; // Valid only for NAS/Local destination folder name
    }

    // --- GESTORE CONFIGURAZIONE (Salvataggio JSON) ---
    public static class ConfigManager
    {
        private static string FolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "NasaDownloader");
        private static string FilePath = Path.Combine(FolderPath, "settings.json"); // Cambiato estensione in .json

        public static AppSettings Load()
        {
            try
            {
                if (File.Exists(FilePath))
                {
                    string json = File.ReadAllText(FilePath);
                    var settings = JsonSerializer.Deserialize<AppSettings>(json);
                    if (settings != null) return settings;
                }
            }
            catch { /* Ignora errori, usa default */ }
            return new AppSettings();
        }

        public static void Save(string token, string archivio, string targetArchivio)
        {
            try
            {
                var settings = new AppSettings { Token = token, Archivio = archivio, TargetArchivio = targetArchivio };
                string json = JsonSerializer.Serialize(settings);
                
                if (!Directory.Exists(FolderPath)) Directory.CreateDirectory(FolderPath);
                File.WriteAllText(FilePath, json);
            }
            catch { /* Ignora errori */ }
        }
    }

    public class NasaFile { public string name { get; set; } }
    public class NasaResponse { public List<NasaFile> content { get; set; } }

    public partial class Form1 : Form
    {
        // GUI Components
        private TabControl tabControl;
        private TabPage tabSingle, tabRangeDay, tabRangeMonth;
        
        // Date & Numeric Controls
        private DateTimePicker dtpSingle, dtpRangeStart, dtpRangeEnd, dtpMonthStart, dtpMonthEnd;
        private NumericUpDown numDoySingle, numYearSingle; 
        private NumericUpDown numDoyStart, numYearStart;
        private NumericUpDown numDoyEnd, numYearEnd;
        private NumericUpDown numMonthIdxStart, numYearIdxStart, numMonthIdxEnd, numYearIdxEnd;
        
        private TextBox txtToken, txtArchivio, txtTargetArchivio, txtLocalPath;
        private CheckedListBox clbDatabases;
        private RadioButton rbNas1, rbNas2, rbLocal;
        private Button btnDownload, btnLang, btnBrowseLocal;
        private RichTextBox rtbLog;
        private NumericUpDown numHourStart, numHourEnd;
        
        // Labels & Groups
        private Label lblToken, lblDb, lblArch, lblTargetArch, lblFromDay, lblToDay, lblFromMonth, lblToMonth, lblDaySingle, lblStartHour, lblEndHour;
        private GroupBox grpNas, grpOre;

        const string BaseUrl = "https://ladsweb.modaps.eosdis.nasa.gov/archive/allData"; // Tolto il '61' hardcoded qui perché ora è dinamico

        private static readonly HttpClient client = new HttpClient();
        private bool isEnglish = false;
        private bool isUpdatingDate = false;

        public Form1()
        {
            InitializeGui();
            
            // --- CARICAMENTO IMPOSTAZIONI ---
            var settings = ConfigManager.Load();
            txtToken.Text = settings.Token;
            txtArchivio.Text = settings.Archivio; 
            txtTargetArchivio.Text = settings.TargetArchivio;
            
            rbLocal.Checked = true;
            UpdateLanguage();
        }

        private void InitializeGui()
        {
            this.Text = "NASA LADSWEB Downloader Pro v2.2";
            this.Size = new Size(660, 900);
            this.StartPosition = FormStartPosition.CenterScreen;

            btnLang = new Button() { Text = "EN", Location = new Point(570, 10), Size = new Size(50, 25), BackColor = Color.LightGray };
            btnLang.Click += (s, e) => { isEnglish = !isEnglish; UpdateLanguage(); };
            this.Controls.Add(btnLang);

            int y = 40;

            // Token
            lblToken = CreateLabel("Token:", 20, y);
            txtToken = new TextBox() { Location = new Point(20, y + 20), Width = 600 };
            this.Controls.Add(txtToken);
            y += 50;

            // Database
            lblDb = CreateLabel("Database (Seleziona uno o più):", 20, y);
            clbDatabases = new CheckedListBox() { Location = new Point(20, y + 20), Width = 200, Height = 60, CheckOnClick = true };
            clbDatabases.Items.AddRange(new object[] { "MYD03", "MYD021KM", "MYD35_L2" });
            clbDatabases.SetItemChecked(0, true);
            this.Controls.Add(clbDatabases);

            // Archivio NASA (Source)
            lblArch = CreateLabel("Archivio NASA:", 240, y);
            txtArchivio = new TextBox() { Text = "61", Location = new Point(240, y + 20), Width = 80 };
            this.Controls.Add(txtArchivio);

            // Archivio Target (Destination Folder)
            lblTargetArch = CreateLabel("Cartella Destinazione:", 350, y);
            txtTargetArchivio = new TextBox() { Text = "Archivio_7", Location = new Point(350, y + 20), Width = 150 };
            this.Controls.Add(txtTargetArchivio);
            y += 90;

            // Destinazione
            grpNas = new GroupBox() { Text = "Destinazione Salvataggio", Location = new Point(20, y), Size = new Size(600, 100) };
            rbNas1 = new RadioButton() { Text = "NAS29F79B", Location = new Point(10, 25), Width = 100 };
            rbNas2 = new RadioButton() { Text = "NASFA8369", Location = new Point(120, 25), Width = 100 };
            rbLocal = new RadioButton() { Text = "Locale (Scegli cartella)", Location = new Point(230, 25), Width = 180, ForeColor = Color.Blue, Font = new Font(this.Font, FontStyle.Bold) };
            rbLocal.CheckedChanged += (s, e) => { txtLocalPath.Enabled = btnBrowseLocal.Enabled = rbLocal.Checked; };

            txtLocalPath = new TextBox() { Location = new Point(10, 60), Width = 490, ReadOnly = true, Text = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "NasaDownload") };
            btnBrowseLocal = new Button() { Text = "...", Location = new Point(510, 58), Width = 40 };
            btnBrowseLocal.Click += BtnBrowseLocal_Click;

            grpNas.Controls.Add(rbNas1); grpNas.Controls.Add(rbNas2); grpNas.Controls.Add(rbLocal);
            grpNas.Controls.Add(txtLocalPath); grpNas.Controls.Add(btnBrowseLocal);
            this.Controls.Add(grpNas);
            y += 110;

            // --- TABS ---
            tabControl = new TabControl() { Location = new Point(20, y), Size = new Size(600, 140) }; 
            
            // TAB 1: Single
            tabSingle = new TabPage("Giorno Singolo");
            lblDaySingle = CreateLabel("Giorno (Data / DOY / Anno):", 10, 10, tabSingle);
            dtpSingle = new DateTimePicker() { Location = new Point(10, 30), Width = 200 };
            numDoySingle = CreateDoyNumeric(220, 30, tabSingle);
            numYearSingle = CreateYearNumeric(290, 30, tabSingle);
            SyncFullDateControls(dtpSingle, numDoySingle, numYearSingle);
            tabSingle.Controls.Add(dtpSingle);
            tabControl.TabPages.Add(tabSingle);

            // TAB 2: Range Days
            tabRangeDay = new TabPage("Range Giorni");
            lblFromDay = CreateLabel("Da:", 10, 10, tabRangeDay);
            dtpRangeStart = new DateTimePicker() { Location = new Point(10, 30), Width = 200 };
            numDoyStart = CreateDoyNumeric(220, 30, tabRangeDay);
            numYearStart = CreateYearNumeric(290, 30, tabRangeDay);
            SyncFullDateControls(dtpRangeStart, numDoyStart, numYearStart);

            lblToDay = CreateLabel("A:", 10, 60, tabRangeDay);
            dtpRangeEnd = new DateTimePicker() { Location = new Point(10, 80), Width = 200 };
            numDoyEnd = CreateDoyNumeric(220, 80, tabRangeDay);
            numYearEnd = CreateYearNumeric(290, 80, tabRangeDay);
            SyncFullDateControls(dtpRangeEnd, numDoyEnd, numYearEnd);

            tabRangeDay.Controls.Add(dtpRangeStart); tabRangeDay.Controls.Add(dtpRangeEnd);
            tabControl.TabPages.Add(tabRangeDay);

            // TAB 3: Month Range
            tabRangeMonth = new TabPage("Range Mesi");
            lblFromMonth = CreateLabel("Da (Mese / Anno):", 10, 10, tabRangeMonth);
            dtpMonthStart = new DateTimePicker() { Location = new Point(10, 30), Width = 150, CustomFormat = "MM/yyyy", Format = DateTimePickerFormat.Custom };
            numMonthIdxStart = CreateMonthNumeric(170, 30, tabRangeMonth);
            numYearIdxStart = CreateYearNumeric(220, 30, tabRangeMonth);
            SyncMonthControls(dtpMonthStart, numMonthIdxStart, numYearIdxStart);

            lblToMonth = CreateLabel("A (Mese / Anno):", 10, 60, tabRangeMonth);
            dtpMonthEnd = new DateTimePicker() { Location = new Point(10, 80), Width = 150, CustomFormat = "MM/yyyy", Format = DateTimePickerFormat.Custom };
            numMonthIdxEnd = CreateMonthNumeric(170, 80, tabRangeMonth);
            numYearIdxEnd = CreateYearNumeric(220, 80, tabRangeMonth);
            SyncMonthControls(dtpMonthEnd, numMonthIdxEnd, numYearIdxEnd);

            tabRangeMonth.Controls.Add(dtpMonthStart); tabRangeMonth.Controls.Add(dtpMonthEnd);
            tabControl.TabPages.Add(tabRangeMonth);
            
            this.Controls.Add(tabControl);
            y += 150;

            // Filtro Ore
            grpOre = new GroupBox() { Text = "Filtro Ore (0-23)", Location = new Point(20, y), Size = new Size(600, 70) };
            lblStartHour = CreateLabel("Inizio:", 30, 25, grpOre); 
            numHourStart = new NumericUpDown() { Location = new Point(90, 22), Width = 60, Minimum = 0, Maximum = 23, Value = 3 };
            lblEndHour = CreateLabel("Fine:", 280, 25, grpOre); 
            numHourEnd = new NumericUpDown() { Location = new Point(340, 22), Width = 60, Minimum = 0, Maximum = 23, Value = 15 };
            grpOre.Controls.Add(numHourStart); grpOre.Controls.Add(numHourEnd);
            this.Controls.Add(grpOre);
            y += 80;

            btnDownload = new Button() { Text = "AVVIA DOWNLOAD", Location = new Point(20, y), Size = new Size(600, 40), BackColor = Color.LightSkyBlue, Font = new Font(this.Font, FontStyle.Bold) };
            btnDownload.Click += BtnDownload_Click;
            this.Controls.Add(btnDownload);
            y += 50;

            rtbLog = new RichTextBox() { Location = new Point(20, y), Size = new Size(600, 200), ReadOnly = true, BackColor = Color.Black, ForeColor = Color.Lime };
            this.Controls.Add(rtbLog);
        }

        // --- HELPER & SYNC ---
        private Label CreateLabel(string text, int x, int y, Control parent = null)
        {
            Label l = new Label() { Text = text, Location = new Point(x, y), AutoSize = true };
            if (parent == null) this.Controls.Add(l); else parent.Controls.Add(l);
            return l;
        }

        private NumericUpDown CreateDoyNumeric(int x, int y, Control parent) {
            var num = new NumericUpDown() { Location = new Point(x, y), Width = 60, Minimum = 1, Maximum = 366 };
            parent.Controls.Add(num); return num;
        }
        private NumericUpDown CreateMonthNumeric(int x, int y, Control parent) {
            var num = new NumericUpDown() { Location = new Point(x, y), Width = 45, Minimum = 1, Maximum = 12 };
            parent.Controls.Add(num); return num;
        }
        private NumericUpDown CreateYearNumeric(int x, int y, Control parent) {
            var num = new NumericUpDown() { Location = new Point(x, y), Width = 60, Minimum = 1990, Maximum = 2050, Value = DateTime.Now.Year };
            parent.Controls.Add(num); return num;
        }

        private void SyncFullDateControls(DateTimePicker dtp, NumericUpDown numDoy, NumericUpDown numYear)
        {
            numDoy.Value = dtp.Value.DayOfYear;
            numYear.Value = dtp.Value.Year;
            dtp.ValueChanged += (s, e) => {
                if (isUpdatingDate) return;
                isUpdatingDate = true;
                numDoy.Value = dtp.Value.DayOfYear;
                numYear.Value = dtp.Value.Year;
                isUpdatingDate = false;
            };
            EventHandler updateDateFromNums = (s, e) => {
                if (isUpdatingDate) return;
                isUpdatingDate = true;
                try {
                    int y = (int)numYear.Value;
                    int d = (int)numDoy.Value;
                    dtp.Value = new DateTime(y, 1, 1).AddDays(d - 1);
                } catch { }
                isUpdatingDate = false;
            };
            numDoy.ValueChanged += updateDateFromNums;
            numYear.ValueChanged += updateDateFromNums;
        }

        private void SyncMonthControls(DateTimePicker dtp, NumericUpDown monthNum, NumericUpDown yearNum)
        {
            monthNum.Value = dtp.Value.Month;
            yearNum.Value = dtp.Value.Year;
            dtp.ValueChanged += (s, e) => {
                if (isUpdatingDate) return;
                isUpdatingDate = true;
                monthNum.Value = dtp.Value.Month;
                yearNum.Value = dtp.Value.Year;
                isUpdatingDate = false;
            };
            EventHandler updateDate = (s, e) => {
                if (isUpdatingDate) return;
                isUpdatingDate = true;
                try { dtp.Value = new DateTime((int)yearNum.Value, (int)monthNum.Value, 1); } catch { }
                isUpdatingDate = false;
            };
            monthNum.ValueChanged += updateDate;
            yearNum.ValueChanged += updateDate;
        }

        // --- LINGUA ---
        private void UpdateLanguage()
        {
            btnLang.Text = isEnglish ? "IT" : "EN";
            if (isEnglish)
            {
                lblToken.Text = "Token (Auto-saved):";
                lblDb.Text = "Database (Select one or more):";
                lblArch.Text = "NASA Archive (Source):";
                lblTargetArch.Text = "Dest Folder Name:";
                grpNas.Text = "Save Destination";
                rbLocal.Text = "Local (Choose Folder)";
                tabSingle.Text = "Single Day";
                tabRangeDay.Text = "Day Range";
                tabRangeMonth.Text = "Month Range";
                lblDaySingle.Text = "Day (Date / DOY / Year):";
                lblFromDay.Text = "From:"; lblToDay.Text = "To:";
                lblFromMonth.Text = "From (Month / Year):"; lblToMonth.Text = "To (Month / Year):";
                grpOre.Text = "Hour Filter (0-23)";
                lblStartHour.Text = "Start:"; lblEndHour.Text = "End:";
                btnDownload.Text = "START DOWNLOAD";
            }
            else
            {
                lblToken.Text = "Token (Salvato in automatico):";
                lblDb.Text = "Database (Seleziona uno o più):";
                lblArch.Text = "Archivio NASA (Sorgente):";
                lblTargetArch.Text = "Nome Cartella Destinazione:";
                grpNas.Text = "Destinazione Salvataggio";
                rbLocal.Text = "Locale (Scegli Cartella)";
                tabSingle.Text = "Giorno Singolo";
                tabRangeDay.Text = "Range Giorni";
                tabRangeMonth.Text = "Range Mesi";
                lblDaySingle.Text = "Giorno (Data / DOY / Anno):";
                lblFromDay.Text = "Da:"; lblToDay.Text = "A:";
                lblFromMonth.Text = "Da (Mese / Anno):"; lblToMonth.Text = "A (Mese / Anno):";
                grpOre.Text = "Filtro Ore (0-23)";
                lblStartHour.Text = "Inizio:"; lblEndHour.Text = "Fine:";
                btnDownload.Text = "AVVIA DOWNLOAD";
            }
        }

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

        // --- DOWNLOAD ---
        private async void BtnDownload_Click(object sender, EventArgs e)
        {
            if (clbDatabases.CheckedItems.Count == 0)
            {
                MessageBox.Show(isEnglish ? "Please select at least one database." : "Seleziona almeno un database.", "Info");
                return;
            }

            // --- SALVATAGGIO IMPOSTAZIONI ---
            ConfigManager.Save(txtToken.Text, txtArchivio.Text, txtTargetArchivio.Text);

            btnDownload.Enabled = false;
            rtbLog.Clear();
            Log(isEnglish ? "Starting process..." : "Avvio processo...");

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", txtToken.Text);

            try
            {
                DateTime startDate, endDate;
                if (tabControl.SelectedTab == tabSingle) { 
                    startDate = dtpSingle.Value.Date; endDate = startDate; 
                }
                else if (tabControl.SelectedTab == tabRangeDay) { 
                    startDate = dtpRangeStart.Value.Date; endDate = dtpRangeEnd.Value.Date; 
                }
                else { 
                    startDate = new DateTime(dtpMonthStart.Value.Year, dtpMonthStart.Value.Month, 1);
                    endDate = new DateTime(dtpMonthEnd.Value.Year, dtpMonthEnd.Value.Month, 1).AddMonths(1).AddDays(-1);
                }

                if (startDate > endDate) throw new Exception(isEnglish ? "Start Date > End Date" : "Data Inizio > Data Fine");

                string archivioUrl = txtArchivio.Text;        // Es: 61
                string archivioTarget = txtTargetArchivio.Text; // Es: Archivio_7

                string nasName = rbNas1.Checked ? "NAS29F79B" : "NASFA8369";
                int hStart = (int)numHourStart.Value;
                int hEnd = (int)numHourEnd.Value;

                string basePath = rbLocal.Checked ? txtLocalPath.Text : Path.Combine($@"\\{nasName}", archivioTarget, "Modis");
                if (!Directory.Exists(basePath)) Directory.CreateDirectory(basePath);

                // Loop DBs
                foreach (var dbItem in clbDatabases.CheckedItems)
                {
                    string db = dbItem.ToString();
                    Log("================================");
                    Log($"DATABASE: {db}");
                    Log("================================");

                    for (DateTime date = startDate; date <= endDate; date = date.AddDays(1))
                    {
                        Log($"--> {(isEnglish ? "Day" : "Giorno")}: {date:dd/MM/yyyy} (DOY: {date.DayOfYear:000})");
                        
                        string urlDir = $"{BaseUrl}/{archivioUrl}/{db}/{date.Year}/{date.DayOfYear:000}";
                        List<NasaFile> files = await GetFileListAsync(urlDir);

                        if (files == null || files.Count == 0) { Log("   [INFO] No files. (API returned 0 items)"); continue; }
                        Log($"   [DEBUG] Files found: {files.Count}. First: {files[0].name}");

                        string nomeMese = char.ToUpper(date.ToString("MMMM")[0]) + date.ToString("MMMM").Substring(1);
                        string targetDir = Path.Combine(basePath, $"{nomeMese}_{date.Year}");
                        if (!Directory.Exists(targetDir)) Directory.CreateDirectory(targetDir);

                        List<Task> tasks = new List<Task>();
                        for (int h = hStart; h < hEnd; h++)
                        {
                            // Iteriamo per tutti i minuti (00, 05, ..., 55)
                            for (int m = 0; m < 60; m += 5)
                            {
                                string timeStr = $".{h:00}{m:00}."; // es: .0000. , .0005. , .1255.
                                
                                // Cerchiamo TUTTI i file che corrispondono a questo orario
                                var matches = files.Where(f => f.name.Contains(timeStr));

                                foreach (var match in matches)
                                {
                                    tasks.Add(DownloadFileAsync($"{urlDir}/{match.name}", Path.Combine(targetDir, match.name)));
                                }
                            }
                        }
                        await Task.WhenAll(tasks);
                    }
                }
                Log("--------------------------------");
                Log(isEnglish ? "ALL COMPLETED!" : "TUTTO COMPLETATO!");
                MessageBox.Show(isEnglish ? "Download Completed!" : "Download Completato!");
            }
            catch (Exception ex) { Log($"ERROR: {ex.Message}"); }
            finally { btnDownload.Enabled = true; }
        }

        private async Task<List<NasaFile>> GetFileListAsync(string url)
        {
            string fullUrl = url + ".json";
            try
            {
                // this.Invoke call to ensure it runs on UI thread if needed, though usually context is captured.
                // But to be safe lets just invoke. 
                // Actually, let's just assume we are on UI context or Log handles it.
                // The Log method checks Invoke? The original Log did not check invoke but used AppendText. 
                // Wait, BtnDownload_Click is the caller. 
                // Let's use Invoke in Log or just modify Log to be safe? 
                // The existing DownloadFileAsync uses Invoke for logging. I should probably use Invoke here too if I log.
                
                var res = await client.GetAsync(fullUrl);
                if (!res.IsSuccessStatusCode) 
                {
                    this.Invoke((MethodInvoker)delegate { Log($"   [API ERR] {res.StatusCode} for {fullUrl}"); });
                    return null;
                }
                string json = await res.Content.ReadAsStringAsync();
                
                // Try format 1
                try {
                    var root = JsonSerializer.Deserialize<NasaResponse>(json);
                    if (root != null && root.content != null) return root.content;
                } catch {}

                // Try format 2
                try {
                   return JsonSerializer.Deserialize<List<NasaFile>>(json);
                } catch {}

                // If we are here, parsing failed
                 this.Invoke((MethodInvoker)delegate { Log($"   [API ERR] JSON Parse failed for {fullUrl}"); });
                 return null;
            }
            catch (Exception ex) { 
                 this.Invoke((MethodInvoker)delegate { Log($"   [API EX] {ex.Message}"); });
                 return null; 
            }
        }

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