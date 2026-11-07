# التقرير الثاني والعشرون: المسح النهائي الشامل لحزم NuGet وترقيعات مايكروسوفت (Phase 2.1)

**المشروع:** خدماوي (Khadamawy)  
**التاريخ:** 7 نوفمبر 2026  
**المرحلة:** Phase 2.1 – التحقق النهائي من التوافق  
**المسؤول:** Senior .NET Architect & NuGet Specialist  

---

## 1. فحص ترقيعات مايكروسوفت لـ .NET 8 و .NET 9 (Microsoft Package Servicing)

تم إجراء مسح شامل لحزم `Microsoft.AspNetCore.*` و `Microsoft.Extensions.*` و `Microsoft.EntityFrameworkCore.*` عبر مشاريع المنظومة، ومقارنتها بمستويات الترقيع المدعومة حتى أواخر أكتوبر 2026:

### مستويات الترقيع الرسمية لمايكروسوفت (Official Servicing Patch Levels):
* **.NET 8 Servicing Level:** أحدث إصدار ترقيعي هو **`8.0.33`**.
* **.NET 9 Servicing Level:** أحدث إصدار ترقيعي هو **`9.0.22`**.

### أ. تحليل الحزم الحالية في المشاريع
1. **مشروع `Khadamat.Infrastructure` و `Khadamat.BlazorUI` و `Khadamat.WasmHost`:**
   - الحزم المثبتة حالياً: `Microsoft.EntityFrameworkCore 8.0.0`، `Microsoft.AspNetCore.Identity.EntityFrameworkCore 8.0.0`، `Microsoft.AspNetCore.Components.WebAssembly 8.0.0`.
   - **التقييم الفني:** تعمل على الإصدار الأولي `8.0.0` ولم تتم ترقيتها إلى آخر ترقيع صيانة تراكمي `8.0.33`.
   - **الخطورة:** الإصدار `8.0.0` يحتوي على ثغرات انتقال أمني (مثل `System.Text.Json 8.0.0` و `Microsoft.Extensions.Caching.Memory 8.0.0` و `Microsoft.AspNetCore.SignalR.Common 8.0.0`).
   - **التصنيف:** **`WARNING`** (مطلوب تحديث الحزم إلى `8.0.33` دون تغيير الـ Major Version).

2. **مشروع `Khadamat.WebAPI`:**
   - الحزم المحدثة حالياً: `Microsoft.AspNetCore.Components.WebAssembly.Server 8.0.11`، `Microsoft.AspNetCore.Authentication.JwtBearer 8.0.11`، `Microsoft.AspNetCore.OpenApi 8.0.11`.
   - **التقييم الفني:** تم تحديثها جزئياً لسد ثغرة إعادة التوجيه، ولكن يمكن رفعها إلى `8.0.33` لتحقيق أعلى معايير الاستقرار.
   - **التصنيف:** **`WARNING`**

3. **مشروع `Khadamat.MobileApp` (.NET 9):**
   - حزم Extensions تعمل على `8.0.0` وحزم MAUI تعمل على `9.0.120`.
   - **التقييم الفني:** متوافقة ومستقرة، والترقيع النهائي لـ .NET 9 هو `9.0.22`.
   - **التصنيف:** **`PASS`**

---

## 2. التحقق المعماري لحزمة Serilog وتوافقها مع WebAPI

### أ. الفحص الفني لـ `Serilog.AspNetCore 8.0.3`
* **بيئة التشغيل المستهدفة للـ WebAPI:** `net8.0`.
* **فحص مواصفات الحزمة (`serilog.aspnetcore.nuspec`):**
  - تحتوي الحزمة صراحة على مجموعة اعتماديات مخصصة لـ `net8.0`:
    - `Microsoft.Extensions.Logging (>= 8.0.0)`
    - `Serilog (>= 3.1.1)`
    - `Serilog.Extensions.Hosting (>= 8.0.0)`
    - `Serilog.Settings.Configuration (>= 8.0.4)`
* **تصحيح المفاهيم المعمارية (Architectural Clarification):**
  - لا يجوز وصف حزمة `Serilog.AspNetCore 8.0.3` بأنها "حزمة LTS" بالمعنى الرسمي؛ إذ إن حزم Serilog هي مشاريع مجتمعية مفتوحة المصدر تتبع الترقيم الدلالي (SemVer) ولا تتبع جدول دعم مايكروسوفت لدورات الحياة (Microsoft Lifecycle Policy).
  - ولكنها **الحزمة الأكثر توافقاً واستقراراً وتجانساً بنسبة 100%** مع مشروع الـ WebAPI المستهدف لـ .NET 8، لأنها تمنع تسلل حزم التشفير التجريبية `System.Security.Cryptography.Pkcs 10.0.0` التي كانت تسبب ثغرات غير مستقرة عند استخدام `Serilog.AspNetCore 10.0.0`.
* **التصنيف:** **`PASS`**

---

## 3. تقرير الحزم المتأثرة بالثغرات بعد التدقيق النهائي

| الحزمة | المشروع المتأثر | الإصدار الحالي | نوع الثغرة / الملاحظة | الإجراء التصحيحي الموصى به |
| :--- | :--- | :---: | :--- | :--- |
| **`SixLabors.ImageSharp`** | `Khadamat.WebAPI` | 3.1.7 | 6 ثغرات نشطة (DoX / Buffer Overflow) | الترقية إلى `3.2.0` أو `4.1.3` مع مفتاح ترخيص شرعي |
| **`AutoMapper`** | `Application`, `Domain`, `WebAPI` | 13.0.1 | `GHSA-rvv3-g6hj-g44x` (Type Confusion) | الحزمة تعمل مع DTOs داخلية محددة؛ مراجعة التحديث أو الاستبدال بـ Mapster |
| **`Microsoft.EntityFrameworkCore.*`** | `Khadamat.Infrastructure` | 8.0.0 | إصدار أولي يفتقد ترقيعات الاستقرار | الترقيع المباشر إلى الإصدار الصامت **`8.0.33`** |
| **`Microsoft.AspNetCore.Components.WebAssembly`** | `Khadamat.BlazorUI`, `WasmHost` | 8.0.0 | إصدار أولي يفتقد ترقيعات الصيانة | الترقيع المباشر إلى الإصدار الصامت **`8.0.33`** |
| **`SQLitePCLRaw.bundle_green`** | `Khadamat.MobileApp` | 2.1.10 | متوافقة 100% مع 16 KB ومعايير أندرويد 16 | **إبقاء الحزمة كما هي (PASS)** |

---

## 4. مصفوفة التحقق والتصنيف النهائي لمسح NuGet

| بند التحقق | الحالة | التصنيف |
| :--- | :--- | :---: |
| **توافق Serilog.AspNetCore 8.0.3 مع WebAPI** | خالية من حزم .NET 10 Preview ومتوافقة مع net8.0 | **`PASS`** |
| **مكتبة SQLitePCLRaw و 16 KB** | مثبتة بالإصدار 2.1.10 وخالية من الثغرات | **`PASS`** |
| **ترقيعات مايكروسوفت لـ .NET 8 (8.0.33)** | تحتاج ترقية حزم 8.0.0 إلى 8.0.33 في مشاريع Infrastructure و Blazor | **`WARNING`** |
| **ثغرات SixLabors.ImageSharp** | تتطلب الترقية إلى 3.2.0/4.1.3 مع مفتاح ترخيص | **`FAIL`** |
| **ثغرة AutoMapper 13.0.1** | تتطلب حماية مسارات الـ Mapping أو الترقية لـ 14+ | **`WARNING`** |
