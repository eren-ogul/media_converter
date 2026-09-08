using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace MediaConverter
{
    public partial class MainWindow : Window
    {

        private Task RunBatFileWithProgressAsync(string batFileName)
        {
            var tcs = new TaskCompletionSource<bool>();

            // "input" klasöründeki dosya sayısını bul (input_old klasörünü sayma)
            string inputDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "input");
            int totalFiles = Directory.Exists(inputDir) ? Directory.GetFiles(inputDir).Length : 0;
            int currentFileIndex = 0;
            TimeSpan totalDuration = TimeSpan.Zero;
            string currentFileName = "";

            string batPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "resources", batFileName);

            Process process = new Process();
            process.StartInfo.FileName = batPath;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true; // "echo Isleniyor:" yazılarını okumak için
            process.StartInfo.RedirectStandardError = true;  // ffmpeg'in yüzde loglarını okumak için
            process.StartInfo.CreateNoWindow = true;         // Siyah ekranı gizle

            // 1. KISIM: .bat dosyasının kendi yazdığı yazıları (echo) dinle
            process.OutputDataReceived += (sender, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data) && e.Data.Contains("Isleniyor:"))
                {
                    currentFileIndex++;
                    totalDuration = TimeSpan.Zero;

                    // Dosya adını temizle (Örn: Isleniyor: "video.mp4" -> video.mp4)
                    string newFileName = e.Data.Replace("Isleniyor:", "").Trim(' ', '"');

                    Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        // Varsa eski dosyanın bittiğini yazdır
                        if (!string.IsNullOrEmpty(currentFileName))
                        {
                            TxtLog.AppendText($"[✓] Biten dosya: {currentFileName}\n\n");
                        }

                        // Yeni dosyaya geç ve başladığını yazdır
                        currentFileName = newFileName;
                        TxtLog.AppendText($"[►] Başlanan dosya: {currentFileName}...\n");
                        TxtLog.ScrollToEnd();

                        TxtFileCount.Text = $"{currentFileIndex} / {totalFiles}";
                        PbProgress.Value = 0;
                        TxtPercentage.Text = "%0";
                    }));
                }
            };

            // 2. KISIM: FFmpeg'in süre/progress çıktılarını dinle
            process.ErrorDataReceived += (sender, e) =>
            {
                if (string.IsNullOrEmpty(e.Data)) return;

                // Özel Hata 22 (10-bit HDR / H264 Uyuşmazlığı) Kontrolü
                if (e.Data.Contains("-22") || e.Data.Contains("Invalid argument"))
                {
                    Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        TxtLog.AppendText($"\n[!] HATA: Bu video yüksek renk derinliğine (10-bit HDR) sahip olduğu için H.264 kodlayıcı ile işlenemedi.\n");
                        TxtLog.AppendText($"[!] Çözüm: Lütfen video çözünürlüğü seçerken H.264 yerine H.265 (HEVC) seçeneklerini kullanın.\n\n");
                        TxtLog.ScrollToEnd();
                    }));
                }

                //Eğer FFmpeg hata(Error) verirse bunu log ekranına yazdıralım
                else if (e.Data.Contains("Error") || e.Data.Contains("fatal") || e.Data.Contains("Invalid"))
                {
                    string hataMesaji = e.Data;
                    Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        TxtLog.AppendText($"[!] FFmpeg Uyarı/Hata: {hataMesaji}\n");
                        TxtLog.ScrollToEnd();
                    }));
                }

                // Toplam süreyi yakala
                if (totalDuration == TimeSpan.Zero && e.Data.Contains("Duration:"))
                {
                    var match = Regex.Match(e.Data, @"Duration: (\d{2}:\d{2}:\d{2}\.\d+)");
                    if (match.Success)
                    {
                        TimeSpan.TryParse(match.Groups[1].Value, CultureInfo.InvariantCulture, out totalDuration);
                    }
                }

                // Anlık süreyi yakala ve yüzge hesapla
                if (totalDuration != TimeSpan.Zero && e.Data.Contains("time="))
                {
                    var match = Regex.Match(e.Data, @"time=(\d{2}:\d{2}:\d{2}\.\d+)");
                    if (match.Success)
                    {
                        if (TimeSpan.TryParse(match.Groups[1].Value, CultureInfo.InvariantCulture, out TimeSpan currentTime))
                        {
                            double percentage = (currentTime.TotalSeconds / totalDuration.TotalSeconds) * 100;

                            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                            {
                                if (percentage > 100) percentage = 100;
                                PbProgress.Value = percentage;
                                TxtPercentage.Text = $"%{percentage:F0}";
                            }));
                        }
                    }
                }
            };

            process.EnableRaisingEvents = true;
            process.Exited += (sender, e) =>
            {
                // İşlem tamamen bitince UI'ı son kez güncelle
                Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    // En son biten dosyayı da bitti olarak ekrana yazdır
                    if (!string.IsNullOrEmpty(currentFileName))
                    {
                        TxtLog.AppendText($"[✓] Biten dosya: {currentFileName}\n\n");
                    }

                    PbProgress.Value = 100;
                    TxtPercentage.Text = "%100";
                }));

                tcs.SetResult(true);
                process.Dispose();
            };

            process.Start();
            process.BeginOutputReadLine(); // Echo dinlemeyi başlat
            process.BeginErrorReadLine();  // FFmpeg dinlemeyi başlat

            return tcs.Task;
        }

        private ObservableCollection<string> secilenDosyaYollari = new ObservableCollection<string>();
        private bool isUpdatingCombo = false; // Sonsuz döngüyü önleyen kilit

        public MainWindow()
        {
            InitializeComponent();
            LstDosyalar.ItemsSource = secilenDosyaYollari;
        }

        // --- MENÜ KİLİTLEME VE BUTON AKTİVASYONU ---

        private void CmbAudio_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (isUpdatingCombo) return;

            if (CmbAudio.SelectedIndex != -1)
            {
                isUpdatingCombo = true;
                CmbVideo.SelectedIndex = -1; // Video seçimini sıfırla
                isUpdatingCombo = false;

                BtnDosyaSec.IsEnabled = true;
                secilenDosyaYollari.Clear(); // Menü değiştiğinde eski dosyaları temizle (Uyumsuzluğu önler)
            }
            else if (CmbVideo.SelectedIndex == -1)
            {
                BtnDosyaSec.IsEnabled = false;
            }
        }

        private void CmbVideo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (isUpdatingCombo) return;

            if (CmbVideo.SelectedIndex != -1)
            {
                isUpdatingCombo = true;
                CmbAudio.SelectedIndex = -1; // Ses seçimini sıfırla
                isUpdatingCombo = false;

                BtnDosyaSec.IsEnabled = true;
                secilenDosyaYollari.Clear(); // Menü değiştiğinde eski dosyaları temizle (Uyumsuzluğu önler)
            }
            else if (CmbAudio.SelectedIndex == -1)
            {
                BtnDosyaSec.IsEnabled = false;
            }
        }

        // --- DİNAMİK FİLTRELİ DOSYA SEÇİMİ ---

        private void BtnDosyaSec_Click(object sender, RoutedEventArgs e)
        {
            string aktifFiltre = "Tüm Dosyalar|*.*";

            // Hangi menünün açık olduğuna göre filtre belirle
            if (CmbAudio.SelectedIndex != -1)
            {
                aktifFiltre = "Medya Dosyaları|*.mp4;*.mkv;*.webm;*.avi;*.mov;*.flv;*.mka;*.mp3;*.wav;*.flac;*.ogg;*.m4a|Video Dosyaları|*.mp4;*.mkv;*.webm;*.avi;*.mov;*.flv|Ses Dosyaları|*.mka;*.mp3;*.wav;*.flac;*.ogg;*.m4a";
            }
            else if (CmbVideo.SelectedIndex != -1)
            {
                aktifFiltre = "Video Dosyaları|*.mp4;*.mkv;*.webm;*.avi;*.mov;*.flv";
            }

            OpenFileDialog dialog = new OpenFileDialog
            {
                Title = "İşlenecek medya dosyalarını seçin",
                Multiselect = true,
                Filter = aktifFiltre
            };

            if (dialog.ShowDialog() == true)
            {
                foreach (string dosya in dialog.FileNames)
                {
                    if (!secilenDosyaYollari.Contains(dosya))
                    {
                        secilenDosyaYollari.Add(dosya);
                    }
                }
            }
        }

        // --- SİLME VE GÖSTERME İŞLEMLERİ ---

        private void BtnDosyaSil_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button tiklananButon && tiklananButon.Tag != null)
            {
                string silinecekDosya = tiklananButon.Tag.ToString() ?? "";
                secilenDosyaYollari.Remove(silinecekDosya);
            }
        }

        private void BtnSonucuGoster_Click(object sender, RoutedEventArgs e)
        {
            string exeKlasoru = AppDomain.CurrentDomain.BaseDirectory;
            string outputDizin = Path.Combine(exeKlasoru, "output");

            if (Directory.Exists(outputDizin))
            {
                Process.Start("explorer.exe", outputDizin);
            }
            else
            {
                MessageBox.Show("Henüz bir 'output' klasörü oluşmamış.", "Bilgi", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        // --- İŞLEMİ BAŞLATMA VE HANGİ BAT'IN ÇALIŞACAĞINI BULMA ---

        private async void BtnBaslat_Click(object sender, RoutedEventArgs e)
        {
            if (secilenDosyaYollari.Count == 0)
            {
                MessageBox.Show("Lütfen işleme sokmak için en az bir dosya seçin!", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string secilenBatAdi = "";

            // Hangi menüde seçim yapıldığını bul
            if (CmbAudio.SelectedIndex != -1 && CmbAudio.SelectedItem is ComboBoxItem audioItem)
            {
                secilenBatAdi = audioItem.Tag?.ToString() ?? "";
            }
            else if (CmbVideo.SelectedIndex != -1 && CmbVideo.SelectedItem is ComboBoxItem videoItem)
            {
                secilenBatAdi = videoItem.Tag?.ToString() ?? "";
            }

            if (string.IsNullOrEmpty(secilenBatAdi)) return;

            string exeKlasoru = AppDomain.CurrentDomain.BaseDirectory;
            string batDosyaYolu = Path.Combine(exeKlasoru, "resources", secilenBatAdi);

            if (!File.Exists(batDosyaYolu))
            {
                MessageBox.Show($"'{secilenBatAdi}' dosyası bulunamadı!", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            BtnBaslat.IsEnabled = false;
            BtnDosyaSec.IsEnabled = false;
            CmbAudio.IsEnabled = false;
            CmbVideo.IsEnabled = false;
            TxtLog.Text = "Dosyalar çalışma alanına hazırlanıyor...\n";

            string inputKlasoru = Path.Combine(exeKlasoru, "input");
            Directory.CreateDirectory(inputKlasoru);

            foreach (string dosya in secilenDosyaYollari)
            {
                string dosyaAdi = Path.GetFileName(dosya);
                string hedefYol = Path.Combine(inputKlasoru, dosyaAdi);

                try
                {
                    File.Copy(dosya, hedefYol, true);
                }
                catch (Exception ex)
                {
                    TxtLog.AppendText($"Hata: {dosyaAdi} kopyalanamadı. ({ex.Message})\n");
                }
            }

            TxtLog.AppendText($"----------------------------------\n");
            TxtLog.AppendText($"Toplam {secilenDosyaYollari.Count} adet dosya işlenecek.\n");
            TxtLog.AppendText($"----------------------------------\n\n");
            TxtLog.ScrollToEnd();


            // --- ÇAKIŞMA ÖNLEME KODU ---
            string outputKlasoru = Path.Combine(exeKlasoru, "output");
            Directory.CreateDirectory(outputKlasoru);

            if (Directory.Exists(outputKlasoru))
            {
                // Hangi .bat dosyası çalışıyorsa çözünürlüğünü yakala
                string hedefCozunurluk = "";
                if (secilenBatAdi.Contains("480")) hedefCozunurluk = "480p";
                else if (secilenBatAdi.Contains("720")) hedefCozunurluk = "720p";
                else if (secilenBatAdi.Contains("1080")) hedefCozunurluk = "1080p";
                else if (secilenBatAdi.Contains("1440")) hedefCozunurluk = "1440p";


                // Video menüsü seçildiyse arayacağımız uzantı kesinlikle ".mp4" olmalı.
                // Eğer ses menüsü seçildiyse çıktı uzantısını bat adından tahmin et.
                string hedefUzanti = ".mp4";
                if (CmbAudio.SelectedIndex != -1)
                {
                    if (secilenBatAdi.Contains("mp3")) hedefUzanti = ".mp3";
                    else if (secilenBatAdi.Contains("mka")) hedefUzanti = ".mka";
                    else if (secilenBatAdi.Contains("m4a")) hedefUzanti = ".m4a";
                    else if (secilenBatAdi.Contains("aac")) hedefUzanti = ".aac";
                    else if (secilenBatAdi.Contains("ogg")) hedefUzanti = ".ogg";
                    else if (secilenBatAdi.Contains("wav") || secilenBatAdi.Contains("waw")) hedefUzanti = ".wav";
                    else hedefUzanti = ".*"; // Bilinmeyen ses formatıysa genel ara
                }

                foreach (string dosya in secilenDosyaYollari)
                {
                    string dosyaAdiSensiz = Path.GetFileNameWithoutExtension(dosya);

                    // Sadece o anki dosyanın, ilgili çözünürlükteki ve uzantıdaki çıktısını hedefler.
                    // Output klasöründeki .mkv gibi diğer orijinal/farklı formatlı dosyalara dokunmaz!
                    string aramaDeseni = string.IsNullOrEmpty(hedefCozunurluk)
                        ? $"{dosyaAdiSensiz}_*{hedefUzanti}"
                        : $"{dosyaAdiSensiz}_*({hedefCozunurluk}){hedefUzanti}";

                    string[] eskiUrunler = Directory.GetFiles(outputKlasoru, aramaDeseni);
                    foreach (var eski in eskiUrunler)
                    {
                        try
                        {
                            Microsoft.VisualBasic.FileIO.FileSystem.DeleteFile(
                                eski,
                                Microsoft.VisualBasic.FileIO.UIOption.OnlyErrorDialogs,
                                Microsoft.VisualBasic.FileIO.RecycleOption.SendToRecycleBin
                            );
                        }
                        catch { }
                    }
                }
                // ---------------------------

                // Motoru Çalıştır


                await RunBatFileWithProgressAsync(secilenBatAdi);

                BtnBaslat.IsEnabled = true;
                BtnDosyaSec.IsEnabled = true;
                CmbAudio.IsEnabled = true;
                CmbVideo.IsEnabled = true;
                TxtLog.AppendText($"----------------------------------\n");
                TxtLog.AppendText("İŞLEMLER Tamamlandı!\n");
                TxtLog.ScrollToEnd();
                secilenDosyaYollari.Clear();
            }

            // --- ASENKRON CMD OKUMA (Kilitlenme Önleyici) ---

        }
    }
}