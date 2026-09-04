@echo off
set "PATH=%~dp0;%PATH%"
setlocal enabledelayedexpansion
set sayac=1

:: Gerekli klasörleri otomatik oluşturur
if not exist "input" mkdir "input"
if not exist "output" mkdir "output"
if not exist "input\input_old" mkdir "input\input_old"

cls
echo ==================================================
echo     EVRENSEL SES DONUSTURME .m4a
echo ==================================================
echo.
echo [!] "input" klasorundeki medya dosyalari taraniyor...
echo.

:: Hem video hem de farklı ses formatlarını (mka, mp3, wav vb.) tek seferde tarar
for %%i in ("input\*.mp4" "input\*.mkv" "input\*.webm" "input\*.avi" "input\*.mov" "input\*.flv" "input\*.mka" "input\*.mp3" "input\*.wav" "input\*.flac" "input\*.ogg") do (
    echo --------------------------------------------------
    echo Isleniyor: "%%~nxi"
    
    :: FFmpeg ile yüksek kalitede (256 kbps) M4A'ya dönüştür
    ffmpeg -i "%%i" -vn -c:a aac -b:a 256k "output\%%~ni_!sayac!(converted).m4a"
    
    :: İşlemi biten orijinal dosyayı input_old klasörüne taşı
    move "%%i" "input\input_old\" >nul
    echo Tasindi  : "%%~nxi" -^> input_old klasorune
    
    set /a sayac+=1
)

echo.
echo ==================================================
echo [BASARILI] Tum donusturme islemleri tamamlandi! 
echo Ciktilar "output" klasorune kaydedildi.
echo ==================================================
