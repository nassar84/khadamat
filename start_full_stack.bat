@echo off
set "ROOT_DIR=%~dp0"
echo.
echo ============================================================
echo [1/3] Building solution...
echo ============================================================
dotnet build "%ROOT_DIR%Khadamat.sln" -c Debug

if %ERRORLEVEL% NEQ 0 (
    echo [ERROR] Build failed. Please check the logs.
    pause
    exit /b %ERRORLEVEL%
)

echo.
echo ============================================================
echo [2/3] Starting Backend API (Port 5144)...
echo ============================================================
start "Khadamat Web API" cmd /k "cd /d %ROOT_DIR% && dotnet run --project src\Khadamat.WebAPI\Khadamat.WebAPI.csproj --no-build"

echo Waiting for API to start...
timeout /t 5 /nobreak > nul

echo.
echo ============================================================
echo [3/3] Starting Blazor WasmHost (Port 5028)...
echo ============================================================
start "Khadamat WasmHost" cmd /k "cd /d %ROOT_DIR% && dotnet run --project src\Khadamat.WasmHost\Khadamat.WasmHost.csproj --no-build --urls http://localhost:5028"

echo.
echo ============================================================
echo DONE: Web App should be opening at http://localhost:5028
echo ============================================================
echo.
echo Press any key to exit this window (API and Wasm terminals will stay open).
pause > nul
