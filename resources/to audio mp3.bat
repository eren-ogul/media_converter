@echo off
set "PATH=%~dp0;%PATH%"
setlocal enabledelayedexpansion
set sayac=1

if not exist "input" mkdir "input"
if not exist "output" mkdir "output"
if not exist "input\input_old" mkdir "input\input_old"

cls
echo ==================================================
echo       EVRENSEL SES DONUSTURME (.mp3)
echo ==================================================
echo.
echo [!] "input" klasorundeki dosyalar taraniyor...
echo.

for %%i in ("input\*.mp4" "input\*.mkv" "input\*.webm" "input\*.avi" "input\*.mov" "input\*.flv" "input\*.mka" "input\*.mp3" "input\*.wav" "input\*.flac" "input\*.ogg" "input\*.m4a") do (
    echo --------------------------------------------------
    echo Isleniyor: "%%~nxi"
    
    :: MP3 için mümkün olan en yüksek kalite (320kbps) kullanılarak dönüştürülür.
    ffmpeg -i "%%i" -vn -c:a libmp3lame -b:a 320k "output\%%~ni_!sayac!(audio).mp3"
    
    move "%%i" "input\input_old\" >nul
    echo Tasindi  : "%%~nxi" -^> input_old klasorune
    set /a sayac+=1
)
echo.
echo [BASARILI] MP3 Donusturme islemleri tamamlandi!
