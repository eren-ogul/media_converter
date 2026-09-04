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
echo     VIDEO COZUNURLUK DUSURUCU (1440p - %%100 GPU)
echo ==================================================
echo.
echo [!] "input" klasorundeki videolar taraniyor...
echo.

for %%i in ("input\*.mp4" "input\*.mkv" "input\*.webm" "input\*.avi" "input\*.mov" "input\*.flv") do (
    echo --------------------------------------------------
    echo Isleniyor: "%%~nxi"
    
    ffmpeg -hwaccel cuda -hwaccel_output_format cuda -i "%%i" -vf "scale_cuda=-2:1440" -c:v h264_nvenc -cq 28 -c:a copy "output\%%~ni_!sayac!(1440p).mp4"
    
    move "%%i" "input\input_old\" >nul
    echo Tasindi  : "%%~nxi" -^> input_old klasorune
    
    set /a sayac+=1
)

echo.
echo ==================================================
echo [BASARILI] Tum donusturme islemleri tamamlandi! 
echo Kucultulen videolar "output" klasorune kaydedildi.
echo ==================================================
