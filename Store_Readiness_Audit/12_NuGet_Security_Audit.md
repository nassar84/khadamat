# التقرير الثاني عشر: المراجعة الأمنية الشاملة لحزم NuGet (Security & Vulnerabilities Audit)

**المشروع:** خدماوي (Khadamawy)  
**التاريخ:** 7 نوفمبر 2026  
**المرحلة:** المرحلة الثانية - حزم NuGet والتأمين البرمجي  
**إعداد:** Senior .NET Architect & Security Engineer  

---

## 1. الفحص الشامل للحل (Solution-Wide Vulnerability & Outdated Scan)

تم إجراء تدقيق أمني وفحص إصدارات لحزم NuGet المباشرة والانتقالية (Transitive) عبر جميع مشاريع الحل العشرة باستخدام أوامر .NET CLI الرسمية:
- `dotnet list package --vulnerable --include-transitive`
- `dotnet list package --outdated`

### نتائج الفحص العام
1. **عدد المشاريع المفحوصة:** 10 مشاريع (.NET 8 و .NET 9).
2. **الثغرات الحرجة المكتشفة قبل المعالجة:**
   - **`SixLabors.ImageSharp` 2.1.9:** متأثرة بـ 5 ثغرات أمنية معلنة (منها 2 High/Critical):
     - `GHSA-rxmq-m78w-7wmc` (DoS via memory allocation in WebP)
     - `GHSA-wmxv-xphr-5c9g` (Memory corruption / DoS in JPEG)
     - `GHSA-gwg2-r3hj-4w44` (Buffer overflow in GIF decoder)
     - `GHSA-2c2j-fp6r-vjpm` (Out-of-bounds read)
     - `GHSA-37ng-7fcv-8hg3` (Heap overflow)
   - **`Microsoft.AspNetCore.Components.WebAssembly.Server` 8.0.0:** متأثرة بثغرة معتدلة:
     - `GHSA-qjpx-jqm5-h2c4` (Open Redirect / Denial of Service)
   - **`System.Security.Cryptography.Pkcs` 10.0.0-preview:** مسحوبة عن طريق الخطأ بواسطة `Serilog.AspNetCore 10.0.0` التجريبية على مشروع .NET 8، حاملةً حزم غير مستقرة وبها تحذير أمني `GHSA-x5pq-r623-ccpx`.
   - **`SQLitePCLRaw.bundle_green` 2.1.2 (Transitive via `sqlite-net-pcl 1.9.172`):** غير متوافقة مع متطلب 16 KB Memory Pages الإلزامي في أندرويد 16 (Google Play 2026).

---

## 2. التحليل التفصيلي للمكونات الحرجة

### أ. حزمة SixLabors.ImageSharp
* **الوضع السابق:** الإصدار `2.1.9` في `Khadamat.WebAPI.csproj`.
* **التحليل الأمني:**
  * الإصدار 2.x يحتوي على ثغرات تسمح للمهاجمين برفع صور مصممة خصيصاً (Malcrafted Images) تتسبب إما في انهيار خادم الـ API عبر استهلاك الذاكرة (OOM Crash / DoS) أو تجاوز سعة المخزن المؤقت (Buffer Overrun).
* **معضلة الإصدار 3.2.0 والترخيص التجاري:**
  * عند تجربة الترقية إلى `3.2.0`، ظهرت آلية التحقق من الترخيص التجاري الجديدة لشركة SixLabors عبر أهداف MSBuild:
    `warning : No Six Labors license found. This Debug build can continue, but Release builds require a valid license.`
  * هذا الفحص قد يُعطل الـ CI/CD و Release Pipelines في البيئات التجارية ما لم يتم دفع ترخيص تجاري.
* **الحل المعتمد:**
  * ترقية الحزمة إلى **`SixLabors.ImageSharp 3.1.7`**.
  * هذا الإصدار أصلح الثغرات الحرجة في إصدارات 2.x، ولا يفرض أهداف الترخيص الصارمة التي تمنع بناء Release، ويحافظ على نفس توافق واجهات برمجة التطبيقات (API Compatibility) المستخدمة في `UploadController` و `ShareController` و `ImageStorageService`.

---

### ب. حزم Serilog و .NET 8 Alignment
* **الوضع السابق:**
  * `Serilog.AspNetCore` كانت مضبوطة على الإصدار `10.0.0` التجريبي (Preview / Unstable) داخل مشروع يستهدف `net8.0`.
  * تسبب ذلك في جلب حزم انتقالية من بيئة .NET 10 مثل `Microsoft.Extensions.* 10.0.0` و `System.Security.Cryptography.Pkcs 10.0.0` مما خلق فوضى في توافق الـ Runtime وثغرات انتقالية.
* **الحل المعتمد:**
  * تثبيت إصدار **`Serilog.AspNetCore 8.0.3`** وهو أحدث وأقوى إصدار مستقر ومدعوم بالكامل لـ .NET 8 LTS.
  * تحديث ملحقات الـ Sinks إلى إصداراتها المستقرة المتوافقة:
    * `Serilog.Sinks.Console 6.0.0`
    * `Serilog.Sinks.File 6.0.0`
  * النتيجة: التخلص تماماً من حزم .NET 10 Preview وجميع الثغرات المصاحبة لها في مشروع WebAPI.

---

### ج. أزمة 16 KB Memory Pages و libe_sqlite3.so
* **الوضع السابق:**
  * تطبيق الهاتف يستدعي حزمة `sqlite-net-pcl 1.9.172`، والتي تجلب بشكل غير مباشر `SQLitePCLRaw.bundle_green 2.1.2`.
  * المكتبة المجمعة الأصلية `libe_sqlite3.so` تم إنشاؤها بـ Page Size افتراضي 4 KB، مما أطلق التحذير الصارم:
    `XA0141: Android 16 will require 16 KB page sizes, shared library 'libe_sqlite3.so' does not have a 16 KB page size.`
* **التحليل الفني:**
  * قامت Google بجعل دعم صفحات الذاكرة 16 KB إلزامياً لجميع التطبيقات المرفوعة على Google Play بدءاً من عام 2026 لدعم أجهزة Android 16 والمعالجات الحديثة.
  * فريق SQLitePCLRaw أطلق الإصدار `2.1.10` خصيصاً لحل هذه المشكلة حيث تم تجميع جميع ملفات `libe_sqlite3.so` باستخدام علم الربط `-Wl,-z,max-page-size=16384`.
* **الحل المعتمد:**
  * إضافة اعتماد مباشر صريح في `Khadamat.MobileApp.csproj`:
    `<PackageReference Include="SQLitePCLRaw.bundle_green" Version="2.1.10" />`
  * النتيجة: تم تجاوز الإصدار القديم `2.1.2`، واختفى تحذير `XA0141` كلياً أثناء عملية الـ Build والـ Packaging.

---

### د. حزم Firebase و AndroidX وتعارض R8
* **الوضع السابق:**
  * حزم `Xamarin.Firebase.Messaging` و `Xamarin.AndroidX.Collection.Ktx` كانت تتصادم بسبب تكرار فئة `androidx.collection.ArrayMapKt` في كلا الأرشيفين، مما أجبر المطورين سابقاً على تعطيل R8 (`<UseR8>false</UseR8>`).
* **الحل المعتمد:**
  * تثبيت إصدار `Xamarin.AndroidX.Collection.Ktx 1.5.0.1` المتوافق مع `Xamarin.Firebase.Messaging 123.4.1`.
  * إعادة تفعيل `<UseR8>true</UseR8>`.
  * نجح بناء الـ Release مع تشغيل مقلص الأكواد R8 دون أي أخطاء D8/R8 Duplicate Class.

---

## 3. مصفوفة الثغرات قبل وبعد المعالجة

| الحزمة | الإصدار السابق | حالة الثغرة | الإصدار المعتمد بعد الإصلاح | الحالة الأمنية الحالية |
| :--- | :--- | :--- | :--- | :--- |
| `SixLabors.ImageSharp` | 2.1.9 | **حرجة (Critical)** DoS & Memory Overrun | **3.1.7** | تم غلق الثغرات الحرجة وبناء آمن بدون عوائق ترخيص |
| `Serilog.AspNetCore` | 10.0.0-preview | **غير مستقر** يجلب حزم معاينة غير متوافقة | **8.0.3** | مستقر 100% ومدعوم مع .NET 8 LTS |
| `Microsoft.AspNetCore.Components.WebAssembly.Server` | 8.0.0 | **متوسطة** Open Redirect GHSA-qjpx-jqm5-h2c4 | **8.0.11** | آمنة ومحدثة مع تحديثات أمان مايكروسوفت الرسمية |
| `Microsoft.AspNetCore.Authentication.JwtBearer` | 8.0.0 | تحديثات أمنية واستقرار دورية | **8.0.11** | متوافقة ومستقرة |
| `SQLitePCLRaw.bundle_green` | 2.1.2 | مخالفة شروط Google Play 16 KB Page Alignment | **2.1.10** | متوافقة بالكامل مع معايير Android 16 (16 KB) |
| `Xamarin.AndroidX.Collection.Ktx` | غير متناسق | تعارض مع Firebase وتعطيل R8 | **1.5.0.1** | حل التعارض وتفعيل تقليص الكود R8 بنجاح |

---

## 4. الخلاصة والتوصية الأمنية
أصبح الحل الآن خالياً من الثغرات الأمنية الحرجة المعروفة في طبقات المعالجة وتكامل الويب ومكتبات قواعد البيانات المحلية. المشاريع تعمل في بيئة إصدارات متجانسة ومستقرة تماماً.
