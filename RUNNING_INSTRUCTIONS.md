# 🚀 How to Run Khadamat Application

To successfully run the full application (Frontend + Backend), you need to run **two separate commands** in two separate terminals.

## 1️⃣ Start the Backend API (First)
This is required for:
- Database creation & seeding
- User authentication (Login/Register)
- Data fetching (Services, Categories, etc.)

```bash
cd E:\MVC\khadamat
dotnet run --project src\Khadamat.WebAPI\Khadamat.WebAPI.csproj
```
✅ **Wait** until you see: `Now listening on: http://localhost:5144`

---

## 2️⃣ Start the Frontend UI (Second)
This is the user interface you interact with in the browser.

```bash
cd E:\MVC\khadamat
dotnet run --project src\Khadamat.WasmHost\Khadamat.WasmHost.csproj
```
✅ **Wait** until you see: `Now listening on: http://localhost:5028`

---

## 🔑 Login Credentials

Once both are running, open **http://localhost:5028** and login with:

| Role | Username | Password |
|------|----------|----------|
| **Super Admin** | `SuperAdmin` | `Admin@123` |
| **System Admin** | `Admin` | `Admin@123` |
| **User** | `RegularUser` | `Admin@123` |
