@echo off
set "PATH=%~dp0bin;%PATH%"
setlocal enabledelayedexpansion
set sayac=1

:: Gerekli klasörleri otomatik oluşturur (eğer yoksa)
if not exist "input" mkdir "input"
if not exist "output" mkdir "output"
if not exist "input\input_old" mkdir "input\input_old"

cls
echo ==================================================
echo       SES AYIKLAMA ARACI (All formats to .mka)
echo ==================================================
echo.
echo [!] "input" klasorundeki dosyalar taraniyor...
echo.

:: "input" klasöründeki mp4'leri tarar
for %%i in ("input\*.mp4" "input\*.mkv" "input\*.webm" "input\*.avi" "input\*.mov" "input\*.flv") do (
    echo Isleniyor: "%%~nxi"
    
    :: %%~ni komutu dosyanın orijinal adını uzantısız olarak alır
    ffmpeg -i "%%i" -vn -c:a copy "output\%%~ni_!sayac!(only audio).mka"

    :: 2. ADIM: İşlem biter bitmez orijinal videoyu input_old içine taşı
    move "%%i" "input\input_old\" >nul
    echo Tasindi  : "%%~nxi" -^> input_old klasorune
    
    set /a sayac+=1
)

echo.
echo ==================================================
echo [BASARILI] Islem tamamlandi! 
echo Dosyalar "output" klasorune kaydedildi.
echo ==================================================
