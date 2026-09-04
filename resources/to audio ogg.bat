@echo off
set "PATH=%~dp0;%PATH%"
setlocal enabledelayedexpansion
set sayac=1

if not exist "input" mkdir "input"
if not exist "output" mkdir "output"
if not exist "input\input_old" mkdir "input\input_old"

cls
echo ==================================================
echo       EVRENSEL SES DONUSTURME (.ogg)
echo ==================================================
echo.

for %%i in ("input\*.mp4" "input\*.mkv" "input\*.webm" "input\*.avi" "input\*.mov" "input\*.flv" "input\*.mka" "input\*.mp3" "input\*.wav" "input\*.flac" "input\*.ogg" "input\*.m4a") do (
    echo --------------------------------------------------
    echo Isleniyor: "%%~nxi"
    
    :: OGG için Vorbis motoru kullanılarak çok yüksek kalitede (q 7) sıkıştırılır.
    ffmpeg -i "%%i" -vn -c:a libvorbis -q:a 7 "output\%%~ni_!sayac!(audio).ogg"
    
    move "%%i" "input\input_old\" >nul
    echo Tasindi  : "%%~nxi" -^> input_old klasorune
    set /a sayac+=1
)
echo.
echo [BASARILI] OGG Donusturme islemleri tamamlandi!
