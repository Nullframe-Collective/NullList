@echo off
setlocal

set "TARGET={{TARGET}}"
set "NEW={{NEW}}"
set "GAME={{GAME}}"

echo Waiting for game to close...

:WAIT_GAME

tasklist /FI "IMAGENAME eq %GAME%.exe" 2>NUL | find /I "%GAME%.exe" >NUL

if not errorlevel 1 (
    timeout /t 2 /nobreak >NUL
    goto WAIT_GAME
)

echo Game closed.

if not exist "%NEW%" (
    echo Update file not found.
    goto CLEANUP
)

echo Removing old DLL...

del /F /Q "%TARGET%" >NUL 2>&1

if exist "%TARGET%" (
    echo Failed to remove old DLL.
    goto CLEANUP
)

echo Installing new DLL...

move /Y "%NEW%" "%TARGET%" >NUL 2>&1

if not exist "%TARGET%" (
    echo Failed to install new DLL.
    goto CLEANUP
)

echo Update installed successfully.

:CLEANUP

del /F /Q "%~f0" >NUL 2>&1

exit /b 0
