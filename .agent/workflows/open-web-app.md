---
description: How to start the Khadamat Web application (Full Stack)
---

To start the Khadamat web application with both the API and the Blazor WebAssembly UI, follow these steps:

### 🚀 Recommended (Windows One-Click)
Run the `start_full_stack.bat` script located in the root directory. This will:
1. Build the entire solution.
2. Open a new terminal for the **WebAPI** (Port 5144).
3. Open a new terminal for the **WasmHost** (Port 5028).

### 🖥️ Manual Startup (New Hosted Model)
You only need to start the **WebAPI** project now, as it hosts the frontend automatically:

#### 1. Start the Unified Web App
Open a terminal in the root directory:
```bash
dotnet run --project src\Khadamat.WebAPI\Khadamat.WebAPI.csproj
```
Wait until the terminal shows: `Now listening on: http://localhost:5144`

#### 2. Open the Site
Navigate to: [http://localhost:5144](http://localhost:5144)
(Note: The old port 5028 is no longer used for local startup in this model).

### 🔍 Troubleshooting "Stuck on Loading"
If the browser shows "جاري التحميل" (Loading) forever:
- Confirm that the **WebAPI** terminal is running and listening on port 5144.
- Run `dotnet build Khadamat.sln` to refresh static assets.
- Clear browser cache and refresh page (`Ctrl + F5`).
