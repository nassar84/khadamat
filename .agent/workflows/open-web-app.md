---
description: How to start the Khadamat Web application (Full Stack)
---

To start the Khadamat web application with both the API and the Blazor WebAssembly UI, follow these steps:

### 🚀 Recommended (Windows One-Click)
Run the `start_full_stack.bat` script located in the root directory. This will:
1. Build the entire solution.
2. Open a new terminal for the **WebAPI** (Port 5144).
3. Open a new terminal for the **WasmHost** (Port 5028).

### 🖥️ Manual Startup
If you prefer running manual commands, open **two separate terminals** in the root directory:

#### 1. Start the Backend API (First)
```bash
dotnet run --project src\Khadamat.WebAPI\Khadamat.WebAPI.csproj
```
Wait until the terminal shows: `Now listening on: http://localhost:5144`

#### 2. Start the Frontend UI (Second)
```bash
dotnet run --project src\Khadamat.WasmHost\Khadamat.WasmHost.csproj
```
Wait until the terminal shows: `Now listening on: http://localhost:5028`

### 🔍 Troubleshooting "Stuck on Loading"
If the browser shows "جاري التحميل" (Loading) forever:
- Confirm that the **WebAPI** terminal is running and listening on port 5144.
- Run `dotnet build Khadamat.sln` to refresh static assets.
- Clear browser cache and refresh page (`Ctrl + F5`).
