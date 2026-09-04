<div align="center">

# 🎬 Media Converter

**Windows için hızlı, sade ve FFmpeg tabanlı medya dönüştürme aracı.**

Ses dosyalarını farklı formatlara dönüştürün veya videoları NVIDIA GPU hızlandırmasıyla 480p–1440p arasında yeniden ölçeklendirin.

![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?logo=dotnet&logoColor=white)
![Platform](https://img.shields.io/badge/Platform-Windows-0078D6?logo=windows&logoColor=white)
![UI](https://img.shields.io/badge/UI-WPF-5C2D91)
![FFmpeg](https://img.shields.io/badge/FFmpeg-Powered-007808?logo=ffmpeg&logoColor=white)
![NVIDIA](https://img.shields.io/badge/GPU-NVIDIA%20NVENC-76B900?logo=nvidia&logoColor=white)

</div>

---

## ✨ Özellikler

- 🎵 Video ve ses dosyalarından ses dönüştürme
- 🎞️ 480p, 720p, 1080p ve 1440p video yeniden ölçeklendirme
- ⚡ NVIDIA CUDA / NVENC ile GPU hızlandırmalı video işleme
- 📂 Aynı anda birden fazla dosya seçebilme
- 🧹 Seçilen dosyaları listeden tek tek kaldırabilme
- 📝 İşlem durumunu uygulama içindeki log alanından takip edebilme
- 📁 Çıktı klasörünü uygulama üzerinden doğrudan açabilme
- 🔒 Kaynak dosyalara dokunmadan çalışma kopyaları üzerinden işlem yapma
- 📦 Self-contained Windows yayın desteği

---

## 🎵 Ses Dönüştürme

Uygulama ses işlemleri için aşağıdaki çıktı profillerini sunar:

| Çıktı | Codec / Ayar | Açıklama |
|---|---|---|
| **MKA** | `-c:a copy` | Ses akışını yeniden kodlamadan ayırır |
| **AAC** | AAC · 256 kbps | `.aac` çıktısı |
| **M4A** | AAC · 256 kbps | `.m4a` kapsayıcısı |
| **MP3** | LAME · 320 kbps | Yüksek kaliteli MP3 |
| **OGG** | Vorbis · Quality 7 | Değişken bit oranlı Vorbis |
| **WAV** | PCM S16LE · Stereo | Sıkıştırılmamış PCM ses |

Ses işlemlerinde arayüz üzerinden şu dosya türleri seçilebilir:

```text
MP4, MKV, WEBM, AVI, MOV, FLV,
MKA, MP3, WAV, FLAC, OGG, M4A
```

> **Not:** Mevcut `MKA` profili yalnızca `MP4, MKV, WEBM, AVI, MOV, FLV, MKA, MP3, WAV` dosyalarını tarar. FLAC, OGG ve M4A dosyalarının MKA profiliyle işlenmesi için ilgili batch profilinin genişletilmesi gerekir.

---

## 🎞️ Video Dönüştürme

Video küçültme / yeniden ölçeklendirme profilleri:

| Çözünürlük | H.264 | H.265 / HEVC |
|---|:---:|:---:|
| **480p** | ✅ | ✅ |
| **720p** | ✅ | ✅ |
| **1080p** | ✅ | ✅ |
| **1440p** | ✅ | ✅ |

Video profilleri FFmpeg üzerinden NVIDIA donanım hızlandırmasını kullanır:

```text
CUDA decode
    ↓
scale_cuda
    ↓
h264_nvenc / hevc_nvenc
    ↓
MP4 output
```

Kodlama ayarlarında `CQ 28` kullanılır ve ses akışı yeniden kodlanmadan kopyalanır.

Video işlemlerinde desteklenen giriş uzantıları:

```text
MP4, MKV, WEBM, AVI, MOV, FLV
```

> Video dönüştürme profillerinin çalışması için **CUDA ve NVENC destekli bir NVIDIA ekran kartı** ile uygun NVIDIA sürücüsü gerekir.

---

## ⚙️ Nasıl Çalışır?

Media Converter, seçilen dosyalar üzerinde doğrudan işlem yapmak yerine önce çalışma kopyaları oluşturur.

```text
Seçilen dosyalar
       │
       ▼
    input/
       │
       ▼
Seçilen .bat profili
       │
       ▼
     FFmpeg
       │
       ├────────► output/
       │
       └────────► input/input_old/
```

İş akışı:

1. Kullanıcı bir ses veya video profili seçer.
2. Bir veya daha fazla medya dosyası seçilir.
3. Uygulama seçilen dosyaları `input` klasörüne **kopyalar**.
4. Seçilen profile karşılık gelen `.bat` dosyası çalıştırılır.
5. FFmpeg dönüşümü gerçekleştirir.
6. Oluşturulan dosyalar `output` klasörüne kaydedilir.
7. İşlenen çalışma kopyaları `input/input_old` klasörüne taşınır.
8. **Sonucu Göster** butonu `output` klasörünü Windows Explorer ile açar.

> Kaynak olarak seçtiğiniz orijinal dosyalar taşınmaz veya silinmez. İşlem, uygulamanın çalışma dizinine alınan kopyalar üzerinde gerçekleştirilir.

---

## 🖥️ Sistem Gereksinimleri

### Hazır / Self-contained sürümü kullanmak için

- Windows 10 veya Windows 11
- x64 sistem
- Video dönüştürme için CUDA / NVENC destekli NVIDIA GPU
- Güncel NVIDIA sürücüsü

**Ayrıca .NET Runtime kurmanız gerekmez.**

Proje `self-contained` olarak yayınlandığında gerekli .NET çalışma zamanı dosyaları uygulamayla birlikte gelir.

FFmpeg de proje ile birlikte dağıtıldığı için ayrıca sistem genelinde FFmpeg kurulumu gerekmez.

> [!IMPORTANT]
> Self-contained yayın yalnızca .NET bağımlılığını ortadan kaldırır.  
> Uygulamanın çalışabilmesi için `ffmpeg.exe`, `ffprobe.exe` ve ilgili `.bat` profil dosyalarının da uygulamanın yanında bulunması gerekir. Bu nedenle dağıtım yaparken yalnızca `MediaConverter.exe` dosyasını değil, **publish klasörünün tamamını** paylaşın.

### Kaynak koddan geliştirmek için

- Windows
- [.NET 10 SDK](https://dotnet.microsoft.com/)
- WPF destekli Visual Studio sürümü veya .NET CLI
- Video profillerini test etmek için uyumlu NVIDIA GPU

---

## 🚀 Kurulum

### Kaynak kodu klonlama

```powershell
git clone https://github.com/eren-ogul/media_converter.git
cd media_converter
```

Projeyi derleyin:

```powershell
dotnet restore
dotnet build
```

Çalıştırın:

```powershell
dotnet run
```

Alternatif olarak `MediaConverter.csproj` dosyasını Visual Studio ile açıp doğrudan çalıştırabilirsiniz.

---

## 📦 Self-contained Release Oluşturma

Windows x64 için Release çıktısı:

```powershell
dotnet publish -c Release -r win-x64 --self-contained true
```

Publish çıktısı varsayılan olarak şu dizinde oluşur:

```text
bin\Release\net10.0-windows\win-x64\publish\
```

Bu klasör, uygulamanın çalışması için gerekli .NET runtime dosyalarını içerir.

### Dağıtım

Kullanıcıya **publish klasörünün tamamını** verin.

Örnek yapı:

```text
publish/
├── MediaConverter.exe
├── ffmpeg.exe
├── ffprobe.exe
├── to audio aac.bat
├── to audio m4a.bat
├── to audio mka.bat
├── to audio mp3.bat
├── to audio ogg.bat
├── to audio waw.bat
├── video downscale (480-h264).bat
├── video downscale (480-h265).bat
├── video downscale (720-h264).bat
├── video downscale (720-h265).bat
├── video downscale (1080-h264).bat
├── video downscale (1080-h265).bat
├── video downscale (1440-h264).bat
├── video downscale (1440-h265).bat
└── ... .NET runtime dosyaları
```

> `input`, `output` ve `input/input_old` klasörleri gerektiğinde otomatik olarak oluşturulur.

---

## 📖 Kullanım

1. `MediaConverter.exe` dosyasını çalıştırın.
2. **Ses Formatı Seçimi** veya **Video Çözünürlüğü Seçimi** alanından bir profil belirleyin.
3. **Dosyaları Seç** butonuna tıklayın.
4. Dönüştürülecek bir veya daha fazla dosyayı seçin.
5. Gerekirse listeden istemediğiniz dosyaları kaldırın.
6. **Dönüştür** butonuna tıklayın.
7. İşlem ilerlemesini log alanından takip edin.
8. İşlem tamamlandığında **Sonucu Göster** butonuyla çıktı klasörünü açın.

---

## 🗂️ Proje Yapısı

```text
media_converter/
├── App.xaml
├── App.xaml.cs
├── AssemblyInfo.cs
├── MainWindow.xaml
├── MainWindow.xaml.cs
├── MediaConverter.csproj
│
├── ffmpeg.exe
├── ffprobe.exe
│
├── to audio aac.bat
├── to audio m4a.bat
├── to audio mka.bat
├── to audio mp3.bat
├── to audio ogg.bat
├── to audio waw.bat
│
├── video downscale (480-h264).bat
├── video downscale (480-h265).bat
├── video downscale (720-h264).bat
├── video downscale (720-h265).bat
├── video downscale (1080-h264).bat
├── video downscale (1080-h265).bat
├── video downscale (1440-h264).bat
├── video downscale (1440-h265).bat
│
└── yt_icon_red_digital (1).ico
```

### Temel bileşenler

- **`MainWindow.xaml`** — WPF kullanıcı arayüzü
- **`MainWindow.xaml.cs`** — Dosya seçimi, profil yönetimi, çalışma klasörü hazırlama ve process yönetimi
- **`*.bat`** — FFmpeg dönüşüm profilleri
- **`ffmpeg.exe`** — Medya dönüştürme motoru
- **`ffprobe.exe`** — Medya analiz aracı
- **`MediaConverter.csproj`** — .NET / WPF proje yapılandırması

Proje şu hedef framework'ü kullanır:

```xml
<TargetFramework>net10.0-windows</TargetFramework>
<UseWPF>true</UseWPF>
```

Batch ve executable dosyaları build çıktısına kopyalanacak şekilde proje dosyasında tanımlanmıştır.

---

## ⚠️ Bilinen Sınırlamalar

- Uygulama **Windows'a özeldir**.
- Video profilleri NVIDIA CUDA / NVENC bağımlıdır.
- AMD veya Intel GPU için ayrı video kodlama profilleri mevcut değildir.
- `MKA` batch profili şu anda FLAC, OGG ve M4A girdilerini taramaz.
- WAV profili arayüzde şu anda `WAW` olarak etiketlenmiştir; oluşturulan çıktı yine `.wav` formatındadır.
- Farklı klasörlerden seçilen ancak **aynı dosya adına sahip** dosyalar çalışma alanına kopyalanırken birbiriyle çakışabilir.
- FFmpeg'in desteklediği codec ve donanım özellikleri kullanılan FFmpeg build'ine bağlıdır.

---

## 🛠️ Teknolojiler

- **C#**
- **.NET 10**
- **WPF**
- **FFmpeg / FFprobe**
- **NVIDIA CUDA**
- **NVIDIA NVENC**
- **Windows Batch Scripts**

---

## 🤝 Katkıda Bulunma

Katkılar ve geliştirme önerileri memnuniyetle karşılanır.

```text
1. Repository'yi fork'layın
2. Yeni bir branch oluşturun
3. Değişikliklerinizi yapın
4. Commit oluşturun
5. Branch'inizi push edin
6. Pull Request açın
```

Örnek:

```powershell
git checkout -b feature/yeni-ozellik
git commit -m "Yeni özellik eklendi"
git push origin feature/yeni-ozellik
```

---

## 📜 Lisans ve Üçüncü Taraf Bileşenler

Bu proje medya işleme için **FFmpeg** kullanır. FFmpeg kendi lisans koşullarına tabidir.

Repository'de şu anda ayrı bir `LICENSE` dosyası bulunmadığından, proje için açık kaynak lisansı belirtilmiş değildir. Projeyi açık kaynak lisansı altında dağıtmak istiyorsanız uygun bir `LICENSE` dosyası ekleyebilirsiniz.

FFmpeg dağıtımı yaparken kullandığınız FFmpeg build'inin lisans koşullarını ayrıca kontrol etmeniz önerilir.

---

## 👤 Geliştirici

**Mehmet Eren Oğul**

GitHub: [@eren-ogul](https://github.com/eren-ogul)

---

<div align="center">

**Media Converter** — Basit arayüz, güçlü FFmpeg altyapısı.

⭐ Projeyi faydalı bulduysanız repository'yi yıldızlayabilirsiniz.

</div>
