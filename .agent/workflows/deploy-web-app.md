---
description: كيفية عمل Publish ورفع الموقع (WebAPI + Blazor WASM)
---

هذا الدليل يوضح الخطوات اللازمة لإنتاج نسخة نهائية من المشروع (API وواجهة المستخدم والـ APK) ورفعها على سيرفر الاستضافة.

> [!TIP]
> **الطريقة الأسهل والأسرع (الموصى بها):**
> تشغيل سكريبت النشر الموحد في الطرفية (PowerShell):
> ```powershell
> .\publish_all.ps1
> ```
> يقوم هذا السكريبت تلقائياً بـ:
> 1. تنظيف أي نسخ قديمة من الـ APK وحذف الملفات الزائدة.
> 2. بناء ملف APK واحد فقط محدّث بتاريخ ولحظة البناء الحالية.
> 3. نشر الـ WebAPI مع Blazor WASM إلى مجلد `D:\maged\Khadamat`.
> 4. التأكد الصارم من وجود **ملف APK واحد فقط** (`khadamat.apk`) بتاريخ وتوقيت حديث داخل مجلد التحميل `downloads/`.

---

### **1. الخطوة الأولى (يدوياً): بناء تطبيق الموبايل (Android APK) وتحديثه**
دائماً وقبل نشر الموقع، يتم بناء أحدث ملف APK والتأكد من تنظيف النسخ القديمة:
```powershell
# حذف أي ملفات APK قديمة للتأكد من نشر ملف واحد فقط
Get-ChildItem -Path "src/Khadamat.MobileApp/bin/Release/net8.0-android" -Recurse -Filter "*.apk" -ErrorAction SilentlyContinue | Remove-Item -Force
Get-ChildItem -Path "src/Khadamat.WebAPI/wwwroot/downloads" -Filter "*.apk" -ErrorAction SilentlyContinue | Remove-Item -Force

dotnet publish src/Khadamat.MobileApp/Khadamat.MobileApp.csproj -f net8.0-android -c Release
```
* ملف الـ APK الموقع (Signed) الجديد ينتج في:
  `src/Khadamat.MobileApp/bin/Release/net8.0-android/android-arm64/publish/com.nassar84.khadamat-Signed.apk`
* يتم نسخ هذا الملف باسم واحد فقط (`khadamat.apk`) وتحديث وقته وتاريخه:
  `src/Khadamat.WebAPI/wwwroot/downloads/khadamat.apk`
  وإلى:
  `D:\maged\Khadamat\wwwroot\downloads/khadamat.apk`
* **ملاحظة هامة:** يُمنع نشر أكثر من ملف APK واحد في مجلد `downloads/`.

---

### **2. الخطوة الثانية: عمل الـ Publish للموقع كاملاً (WebAPI + Blazor WASM)**
يجب استخدام مشروع الـ **WebAPI** كنقطة انطلاق للـ Publish، لأنه مهيأ للعمل كـ (Hosted Model) يحتوي على الـ API والـ Blazor معاً:
```powershell
dotnet publish src/Khadamat.WebAPI/Khadamat.WebAPI.csproj -c Release -r win-x64 --self-contained true -o D:\maged\Khadamat
```
*   `-c Release`: لضمان بناء نسخة محسنة وسريعة.
*   `-r win-x64 --self-contained true`: **(هام جداً)** تضمين Runtime الخاص بـ .NET 8 داخل الحزمة لتفادي خطأ `HTTP 500.31 - Failed to load ASP.NET Core runtime` في حال كان السيرفر لا يحتوي على .NET 8 مثبت مسبقاً.
*   `-o D:\maged\Khadamat`: المجلد المستهدف للرفع على السيرفر (Production).

---

### **3. الخطوة الثالثة: التأكد من وجود ملف APK واحد فقط وتاريخه**
بعد انتهاء نشر الموقع، تأكد من وجود ملف الـ APK الوحيد وفحص تاريخه ووقته عبر:
```powershell
Get-ChildItem -Path "D:\maged\Khadamat\wwwroot\downloads" -Filter "*.apk" | Select-Object Name, Length, LastWriteTime, CreationTime
```
يجب أن يظهر ملف واحد فقط وهو `khadamat.apk` بتاريخ ووقت النشر الحالي.

---

### **2. الخطوة الثانية: فحص الملفات الناتجة**
بعد انتهاء الأمر، ستجد في المجلد الناتج (`D:\maged\Khadamat`) الملفات التالية وهي الضرورية للرفع:
- **Khadamat.WebAPI.exe** و **Khadamat.WebAPI.dll**: (المحرك الأساسي).
- **wwwroot**: وهو المجلد الأهم، يحتوي على:
  - ملف `index.html` الخاص بـ Blazor.
  - مجلد `_framework` (الملفات البرمجية للـ WASM).
  - مجلدات الصور والأيقونات.
- **appsettings.json**: إعدادات السيرفر وقواعد البيانات.

---

### **3. الخطوة الثالثة: الرفع على السيرفر (Hosting)**
إذا كنت تستخدم لوحة تحكم مثل **SmarterASP.net** أو **Plesk**:

1.  **ضغط الملفات**: قم بضغط محتويات مجلد `D:\maged\Khadamat` (وليس المجلد نفسه) في ملف واحد بصيغة `.zip`.
2.  **لوحة التحكم**: اذهب إلى **File Manager** الخاص بموقعك على الإنترنت.
3.  **الحذف (اختياري)**: احذف أي ملفات قديمة موجودة في المجلد الرئيسي للموقع (`/`).
4.  **الرفع وفك الضغط**: ارفع ملف الـ `.zip` ثم اختر **Unzip** أو **Extract**.
5.  **إعدادات الـ Application Pool و web.config (SmarterASP)**:
    - ✅ التأكد من أن ملف `web.config` يحتوي على:
      `<aspNetCore processPath=".\Khadamat.WebAPI.exe" arguments="" stdoutLogEnabled="false" stdoutLogFile=".\logs\stdout" hostingModel="OutOfProcess" />`
      *(وضع `OutOfProcess` ضروري جداً لأن سيرفرات SmarterASP المشتركة تعمل بنظام 32-bit افتراضياً لتفادي خطأ HTTP 500.32)*.
    - ✅ في لوحة تحكم SmarterASP: اذهب إلى **Website Management** -> **Application Pools**، واضغط **Stop Pool** ثم **Start Pool** بعد أي تعديل.

---

### **4. حل المشاكل الشائعة (Troubleshooting)**

إذا ظهرت رسالة **"حدث خطأ غير متوقع"** عند فتح الموقع:
1.  **مشكلة الـ CSS**: تأكد أن `index.html` يطلب `Khadamat.WasmHost.styles.css` (وليس BlazorUI).
2.  **مشكلة الـ JSON**: تأكد أن ملف `appsettings.json` **لا يبدأ** بأي تعليقات أو علامة `#`. يجب أن يبدأ بـ `{`.
3.  **مشكلة الـ MIME**: تأكد أن ملف `web.config` المرفوع يحتوي على تعريفات `.wasm` و `.dll` و `.apk`.
4.  **الكاش**: اضغط **Ctrl + F5** في المتصفح لتحديث الملفات القديمة.

---

### **ملاحظات هامة للعمل:**
- **دعم MIME Types**: تأكد من أن السيرفر يدعم امتدادات مثل `.wasm` و `.dat` و `.json`.
- **رابط الـ API**: بما أن المشروع الآن (Hosted)، فإن الـ Blazor سيتصل تلقائياً بنفس الدومين الخاص بالموقع.
- **قواعد البيانات**: تأكد أن سلسلة الاتصال (ConnectionString) في ملف `appsettings.json` المرفوع تشير إلى قاعدة البيانات الحية وليس المحلية.

---

### **5. حماية الصور الدائمة للمستخدمين والخدمات (Permanent Image Storage):**

لحماية صور المستخدمين والخدمات والإعلانات المرفوعة على السيرفر من الحذف عند كل عملية نشر:

**الفكرة الأساسية:**
يتم تخزين الصور في مجلد `uploads` داخل مجلد الـ subdomain نفسه، ولكن عملية النشر تحذف كل شيء **ماعدا** هذا المجلد.

**هيكل المجلدات على السيرفر:**
```
كادر مجلد khadamawy/
├── Khadamat.WebAPI.exe          ← يُحذف ويُستبدل عند كل نشر
├── wwwroot/                     ← يُحذف ويُستبدل عند كل نشر
│   └── downloads/
├── appsettings.json             ← يُحذف ويُستبدل (انتبه لإعداده!)
└── KhadamawyImages/             ← 🔒 محمي لا يُحذف أبداً
    ├── profiles/
    ├── services/
    ├── ads/
    └── ...
```

1. **الإعداد على السيرفر (`appsettings.json`):**
   - بعد كل نشر، تأكد من وجود هذا الإعداد في `appsettings.json` على السيرفر:
     ```json
     "Uploads": {
       "ImagesPath": "<المسار الكامل لمجلد khadamawy>\\uploads"
     }
     ```
   - **مثال:** إذا كان المجلد في `D:\hosting\khadamawy` فيكون:
     ```json
     "Uploads": {
       "ImagesPath": "D:\\hosting\\khadamawy\\KhadamawyImages"
     }
     ```

2. **طريقة الرفع الآمنة على السيرفر (بدلاً من حذف كل شيء):**
   - ❌ **لا تحذف مجلد `KhadamawyImages`** عند الرفع.
   - ✅ احذف كل شيء **ماعدا** مجلد `KhadamawyImages` ثم ارفع الملفات الجديدة.
   - في File Manager: احذف الملفات والمجلدات الأخرى يدوياً، ثم ارفع ملف الـ zip وافضضه.

3. **خطة نقل الصور الحالية (Migration Steps - أول مرة فقط):**
   - **الخطوة أ:** من File Manager، أنشئ مجلداً جديداً باسم `KhadamawyImages` داخل مجلد الموقع `khadamawy/`.
   - **الخطوة ب:** انسخ محتويات مجلد `wwwroot/Images` الحالي إلى مجلد `KhadamawyImages/`.
   - **الخطوة ج:** عدّل `appsettings.Secrets.json` على السيرفر ليشير إلى مجلد `KhadamawyImages`.
   - **الخطوة د:** أعد تشغيل التطبيق (Stop / Start Pool).
   - **النتيجة:** الصور محمية داخل `KhadamawyImages/` ولن تُحذف في أي عملية نشر مستقبلية.

> [!WARNING]
> بعد كل عملية نشر، تأكد من أن `appsettings.json` على السيرفر يحتوي على مسار `ImagesPath` الصحيح.
> يمكن إنشاء ملف `appsettings.Secrets.json` على السيرفر فقط ليحتوي على هذا الإعداد تلقائياً دون الحاجة لتعديله بعد كل نشر.
> يوجد قالب جاهز في `server-config/appsettings.Secrets.json.template` كمرجع.

