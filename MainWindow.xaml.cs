using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace MediaConverter
{
    public partial class MainWindow : Window
    {
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

            TxtLog.AppendText("İşlem başlatılıyor...\n");

            await Task.Run(() => CalistirVeDinle(batDosyaYolu, exeKlasoru));

            BtnBaslat.IsEnabled = true;
            BtnDosyaSec.IsEnabled = true;
            CmbAudio.IsEnabled = true;
            CmbVideo.IsEnabled = true;
            MessageBox.Show("İşlem başarıyla tamamlandı!", "Bilgi", MessageBoxButton.OK, MessageBoxImage.Information);

            secilenDosyaYollari.Clear();
        }

        // --- ASENKRON CMD OKUMA (Kilitlenme Önleyici) ---

        // --- ASENKRON CMD OKUMA (Kilitlenme Önleyici & Temiz Log) ---
        private void CalistirVeDinle(string batYolu, string calismaDizini)
        {
            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = batYolu,
                WorkingDirectory = calismaDizini,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            using (Process process = new Process { StartInfo = psi })
            {
                // 1. KANAL (TEMİZ MESAJLAR): Sadece sizin ".bat" dosyasındaki echo mesajlarınız ekrana basılır.
                process.OutputDataReceived += (s, args) =>
                {
                    if (!string.IsNullOrEmpty(args.Data))
                    {
                        Dispatcher.Invoke(() =>
                        {
                            TxtLog.AppendText(args.Data + "\n");
                            TxtLog.ScrollToEnd();
                        });
                    }
                };

                // 2. KANAL (FFmpeg SPAM): Arka planda donmayı önlemek için okunur AMA ekrana yazdırılmaz.
                process.ErrorDataReceived += (s, args) =>
                {
                    // İçi boş bırakıldı. FFmpeg yazıları ekrandan gizlendi.
                };

                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                process.WaitForExit();
            }
        }
    }
}