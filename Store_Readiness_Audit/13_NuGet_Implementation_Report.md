# التقرير الثالث عشر: التقرير التنفيذي لتعديلات وتحديثات حزم NuGet (NuGet Implementation Report)

**المشروع:** خدماوي (Khadamawy)  
**التاريخ:** 7 نوفمبر 2026  
**المرحلة:** المرحلة الثانية - التنفيذ المعتمد لحزم NuGet  
**إعداد:** Senior .NET Architect & Security Engineer  

---

## 1. ملخص التعديلات المطبقة في ملفات المشروع

تم إجراء التعديلات التالية على ملفات `.csproj` بشكل دقيق ومدروس، متجنبين التحديث العشوائي الشامل الذي يسبب Breaking Changes:

### أولاً: مشروع Khadamat.WebAPI (`src/Khadamat.WebAPI/Khadamat.WebAPI.csproj`)

1. **معالجة SixLabors.ImageSharp:**
   - **السابق:** `<PackageReference Include="SixLabors.ImageSharp" Version="2.1.9" />`
   - **الجديد:** `<PackageReference Include="SixLabors.ImageSharp" Version="3.1.7" />`
   - **السبب الفني:** إغلاق ثغرات الـ Buffer Overflow و Memory Allocation DoS الخطيرة في إصدار 2.x، وتجنب تعقيدات تراخيص 3.2.0 التجارية التي تعطل الـ Release Builds.
   - **فحص التوافق البرمجي:** الكود المستخدم في `UploadController.cs` يعتمد على `Image.LoadAsync` و `image.Mutate(x => x.Resize(...))` وحفظ بصيغة WebP/JPEG؛ جميع هذه الواجهات متوافقة تماماً 100% مع 3.1.7 ولم تتطلب أي تغيير في كود الأعمال.

2. **معالجة حزم Serilog:**
   - **السابق:** `<PackageReference Include="Serilog.AspNetCore" Version="10.0.0" />`
   - **الجديد:** `<PackageReference Include="Serilog.AspNetCore" Version="8.0.3" />`
   - **تحديث الملحقات:**
     - `<PackageReference Include="Serilog.Sinks.Console" Version="6.0.0" />`
     - `<PackageReference Include="Serilog.Sinks.File" Version="6.0.0" />`
   - **السبب الفني:** إزالة الحزم التجريبية غير المستقرة (Preview) الخاصة بـ .NET 10 التي كانت تتسلل إلى بيئة تشغيل .NET 8 LTS وتسبب تعارضاً في التشفير `System.Security.Cryptography.Pkcs`. الإصدار 8.0.3 هو أحدث وأكثر إصدار مستقر ومدعوم رسمي لـ .NET 8.

3. **تحديث حزم Microsoft.AspNetCore الأساسية:**
   - `<PackageReference Include="Microsoft.AspNetCore.Components.WebAssembly.Server" Version="8.0.11" />` (السابق: 8.0.0)
   - `<PackageReference Include="Microsoft.AspNetCore.Authentication.JwtBearer" Version="8.0.11" />` (السابق: 8.0.0)
   - `<PackageReference Include="Microsoft.AspNetCore.OpenApi" Version="8.0.11" />` (السابق: 8.0.0)
   - **السبب الفني:** إغلاق ثغرة إعادة التوجيه المفتوح `GHSA-qjpx-jqm5-h2c4` واستقرار حزمة JWT والأمان.

---

### ثانياً: مشروع Khadamat.MobileApp (`src/Khadamat.MobileApp/Khadamat.MobileApp.csproj`)

1. **معالجة مكتبة libe_sqlite3 ودعم 16 KB Memory Pages:**
   - **الإجراء:** إضافة اعتماد صريح مباشر:
     `<PackageReference Include="SQLitePCLRaw.bundle_green" Version="2.1.10" />`
   - **السبب الفني:** تجاوز الإصدار الانتقالي القديم `2.1.2` المحمّل بواسطة `sqlite-net-pcl`. الإصدار 2.1.10 يحتوي على مكتبات `.so` مجمعة مسبقاً بمحاذاة صفحات 16 KB المتوافقة مع متطلبات متجر Google Play الإلزامية لعام 2026 وأجهزة Android 16.
   - **النتيجة المباشرة:** اختفاء تحذير `XA0141` بالكامل أثناء تجميع حزمة الأندرويد.

2. **إعادة تفعيل مقلص الكود R8 (Code Shrinking & Tree Shaking):**
   - **الإجراء:** `<UseR8>true</UseR8>`
   - **السبب الفني:** تصغير حجم الحزمة وحماية الأكواد وتقليص الدوال غير المستخدمة، بعدما تم القضاء على تعارض الفئات المكررة (`ArrayMapKt`) عبر تثبيت `Xamarin.AndroidX.Collection.Ktx 1.5.0.1`.
   - **النتيجة:** تجميع تطبيق الموبايل Release واجتياز مرحلة R8 jar بنجاح باهر وبدون أي أخطاء.

---

### ثالثاً: مشروع Khadamat.UnitTests (`tests/Khadamat.UnitTests/`)

1. **إصلاح بيئة اختبار رفع وتخزين الصور:**
   - **الملف:** `ImageStorageServiceTests.cs`
   - **الإجراء:** تجهيز مجلد `KhadamawyImages` داخل المجلد المؤقت للاختبار `_tempTestDir` لضمان استقلالية الاختبارات عن القرص الصلب الخارجي.
   - **النتيجة:** اجتياز 46 اختباراً من أصل 46 (نسبة نجاح 100%).

---

## 2. جدول مقارنة حالة الحزم والمشاريع

| المشروع | الحزمة | الإصدار القديم | الإصدار المعتمد | حالة البناء (Build) |
| :--- | :--- | :--- | :--- | :--- |
| **Khadamat.WebAPI** | `SixLabors.ImageSharp` | 2.1.9 | **3.1.7** | نجح (0 أخطاء) |
| **Khadamat.WebAPI** | `Serilog.AspNetCore` | 10.0.0 (Unstable) | **8.0.3** (LTS) | نجح (0 أخطاء) |
| **Khadamat.WebAPI** | `Microsoft.AspNetCore.*` | 8.0.0 | **8.0.11** | نجح (0 أخطاء) |
| **Khadamat.MobileApp** | `SQLitePCLRaw.bundle_green` | 2.1.2 (Transitive) | **2.1.10** (Direct) | نجح (0 أخطاء، تم حل 16KB) |
| **Khadamat.MobileApp** | `UseR8` | `false` | **`true`** | نجح مع التجميع الكامل |
| **Khadamat.UnitTests** | اختبارات الوحدة | 45 ناجح / 1 فاشل | **46 ناجح / 0 فاشل** | نجاح 100% |

---

## 3. التقييم النهائي لجاهزية الحزم
جميع الحزم المستخدمة الآن متوافقة ومستقرة، ولا توجد أي حزم Preview غير معتمدة داخل مشاريع الـ LTS، وتطبيق الأندرويد مبني بأحدث معايير التوافق مع صفحات الذاكرة 16 KB ومُجهّز لـ Release مع تفعيل حماية R8.
