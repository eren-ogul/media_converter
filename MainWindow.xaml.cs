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
        // ObservableCollection: Ekrana anında tepki veren dinamik liste
        private ObservableCollection<string> secilenDosyaYollari = new ObservableCollection<string>();

        public MainWindow()
        {
            InitializeComponent();
            LstDosyalar.ItemsSource = secilenDosyaYollari;
        }

        // 1. Çoklu Dosya Seçme
        private void BtnDosyaSec_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog
            {
                Title = "İşlenecek dosyaları seçin",
                Multiselect = true,
                Filter = "Medya Dosyaları|*.mp4;*.mkv;*.webm;*.avi;*.mov;*.flv;*.mka;*.mp3;*.wav;*.flac;*.ogg;*.m4a|Video Dosyaları|*.mp4;*.mkv;*.webm;*.avi;*.mov;*.flv|Ses Dosyaları|*.mka;*.mp3;*.wav;*.flac;*.ogg;*.m4a"
            };

            if (dialog.ShowDialog() == true)
            {
                foreach (string dosya in dialog.FileNames)
                {
                    if (!secilenDosyaYollari.Contains(dosya)) // Aynı dosyayı 2 kez ekleme
                    {
                        secilenDosyaYollari.Add(dosya);
                    }
                }
            }
        }

        // 2. Çarpı İkonu ile Dosya Silme
        private void BtnDosyaSil_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button tiklananButon && tiklananButon.Tag != null)
            {
                string silinecekDosya = tiklananButon.Tag.ToString() ?? "";
                secilenDosyaYollari.Remove(silinecekDosya);
            }
        }

        // 3. Sonucu Göster
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

        // 4. İşlemi Başlat
        private async void BtnBaslat_Click(object sender, RoutedEventArgs e)
        {
            if (secilenDosyaYollari.Count == 0) // HATA VEREN YER DÜZELTİLDİ (.Count kullanıldı)
            {
                MessageBox.Show("Lütfen işleme sokmak için en az bir dosya seçin!", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (CmbIslem.SelectedItem is ComboBoxItem seciliIslem)
            {
                string secilenBatAdi = seciliIslem.Tag?.ToString() ?? "";
                string exeKlasoru = AppDomain.CurrentDomain.BaseDirectory;
                string batDosyaYolu = Path.Combine(exeKlasoru, secilenBatAdi);

                if (!File.Exists(batDosyaYolu))
                {
                    MessageBox.Show($"'{secilenBatAdi}' dosyası bulunamadı!", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                BtnBaslat.IsEnabled = false;
                BtnDosyaSec.IsEnabled = false;
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
                MessageBox.Show("İşlem başarıyla tamamlandı!", "Bilgi", MessageBoxButton.OK, MessageBoxImage.Information);

                secilenDosyaYollari.Clear(); // HATA VEREN YER DÜZELTİLDİ (Array.Empty yerine Clear kullanıldı)
            }
        }

        // 5. Arka Plan CMD Yöneticisi
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
                process.Start();

                while (!process.StandardOutput.EndOfStream)
                {
                    string? line = process.StandardOutput.ReadLine();
                    Dispatcher.Invoke(() =>
                    {
                        TxtLog.AppendText(line + "\n");
                        TxtLog.ScrollToEnd();
                    });
                }
                process.WaitForExit();
            }
        }
    }
}