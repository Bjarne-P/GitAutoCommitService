@echo off
:: Check if the script is running as administrator
net session >nul 2>&1
if %ERRORLEVEL% NEQ 0 (
    echo Requesting Administrator privileges...
    powershell -Command "Start-Process '%~f0' -Verb RunAs"
    exit /b
)

:: Interactive input of values
set /p EXE_PATH=Please enter the path to the service executable (e.g., C:\Path\To\GitAutoCommitService.exe): 
if not exist "%EXE_PATH%" (
    echo ERROR: The specified path does not exist. Please make sure the file is present.
    timeout /t 15
    exit /b 1
)

set /p SERVICE_USER=Please enter the user under which the service should run (e.g., .\Username): 
if "%SERVICE_USER%"=="" (
    echo ERROR: No username provided.
    timeout /t 15
    exit /b 1
)

set /p SERVICE_PASSWORD=Please enter the password for the user: 
if "%SERVICE_PASSWORD%"=="" (
    echo ERROR: No password provided.
    timeout /t 15
    exit /b 1
)

:: Service information
SET SERVICE_NAME=GitAutoCommitService
SET DISPLAY_NAME=Git Auto Commit Service
SET SERVICE_DESCRIPTION=Automatically commits and pushes changes to a Git repository every 5 minutes

:: Check if sc.exe is available
where sc >nul 2>nul
IF %ERRORLEVEL% NEQ 0 (
    echo ERROR: 'sc.exe' is not available. Please ensure it is in the PATH.
    timeout /t 15
    exit /b 1
)

:: Install the service
echo Installing service %SERVICE_NAME%...
sc create "%SERVICE_NAME%" binPath= "\"%EXE_PATH%\"" DisplayName= "%DISPLAY_NAME%" start= auto obj= "%SERVICE_USER%" password= "%SERVICE_PASSWORD%"
IF %ERRORLEVEL% NEQ 0 (
    echo ERROR: Failed to create the service %SERVICE_NAME%.
    timeout /t 15
    exit /b 1
)

:: Set the service description
sc description "%SERVICE_NAME%" "%SERVICE_DESCRIPTION%"
IF %ERRORLEVEL% NEQ 0 (
    echo ERROR: Failed to set the description for the service %SERVICE_NAME%.
    timeout /t 15
    exit /b 1
)

echo Service %SERVICE_NAME% installed successfully.
echo You can start the service with: sc start %SERVICE_NAME%

:: Keep the script open for 15 seconds
timeout /t 15
exit /b 0
