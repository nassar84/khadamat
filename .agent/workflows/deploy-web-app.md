---
description: كيفية عمل Publish ورفع الموقع (WebAPI + Blazor WASM)
---

هذا الدليل يوضح الخطوات اللازمة لإنتاج نسخة نهائية من المشروع (API وواجهة المستخدم) ورفعها على سيرفر الاستضافة.

### **1. الخطوة الأولى: بناء تطبيق الموبايل (Android APK) وتحديثه**
دائماً وقبل نشر الموقع، يجب بناء أحدث ملف APK حتى يكون متوفراً للتحميل الفوري من الموقع:
```powershell
dotnet publish src/Khadamat.MobileApp/Khadamat.MobileApp.csproj -f net8.0-android -c Release
```
* ملف الـ APK الموقع (Signed) الجديد ينتج في المجلد التالي:
  `src/Khadamat.MobileApp/bin/Release/net8.0-android/android-arm64/publish/com.nassar84.khadamat-Signed.apk`
* يتم نسخ هذا الملف فوراً إلى:
  `D:\maged\Khadamat\wwwroot\downloads\khadamat.apk`
  وإلى:
  `src/Khadamat.WebAPI/wwwroot/downloads\khadamat.apk`

---

### **2. الخطوة الثانية: عمل الـ Publish للموقع كاملاً (WebAPI + Blazor WASM)**
يجب استخدام مشروع الـ **WebAPI** كنقطة انطلاق للـ Publish، لأنه مهيأ للعمل كـ (Hosted Model) يحتوي على الـ API والـ Blazor معاً:
```powershell
dotnet publish src/Khadamat.WebAPI/Khadamat.WebAPI.csproj -c Release -o D:\maged\Khadamat
```
*   `-c Release`: لضمان بناء نسخة محسنة وسريعة.
*   `-o D:\maged\Khadamat`: المجلد المستهدف للرفع على السيرفر (Production).

---

### **3. الخطوة الثالثة: التأكد من وجود ملف الـ APK وتاريخه**
بعد انتهاء نشر الموقع، تأكد من وجود ملف الـ APK وفحص تاريخه ووقته عبر:
```powershell
Get-Item D:\maged\Khadamat\wwwroot\downloads\khadamat.apk | Select-Object Name, Length, LastWriteTime, CreationTime
```

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
5.  **إعدادات الـ Application Pool (SmarterASP)**:
    اذهب إلى **Website Management** -> **Application Pools**، واضغط **Actions** بجانب الموقع:
    - ✅ تأكد من اختيار **.Net Core (No Managed Code)**.
    - ✅ تأكد من اختيار **64-bit mode** (ضروري لـ .NET 8).
    - ✅ اضغط **Stop Pool** ثم **Start Pool** لتفعيل التغييرات.

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
