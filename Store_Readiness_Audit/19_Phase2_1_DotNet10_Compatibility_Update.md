# التقرير التاسع عشر: تقييم الجاهزية ومصفوفة التوافق لترقية .NET 10 LTS (Phase 2.1)

**المشروع:** خدماوي (Khadamawy)  
**التاريخ:** 7 نوفمبر 2026  
**المرحلة:** Phase 2.1 – التحقق النهائي من التوافق  
**المسؤول:** Senior .NET Architect  

---

## 1. الخلفية الحرجة: نهاية دعم .NET 8 و .NET 9 (End of Support Deadline)

> [!CRITICAL]
> **حقيقة زمنية حاسمة:**  
> وفقاً لجدول دعم مايكروسوفت الرسمي:
> * **.NET 8 (LTS):** ينتهي دعمه الرسمي في **10 نوفمبر 2026** (بعد 3 أيام من تاريخ اليوم!).
> * **.NET 9 (STS):** ينتهي دعمه الرسمي في **10 نوفمبر 2026**.
> هذا يعني أن البقاء على .NET 8 أو .NET 9 في بيئات الإنتاج سيجعل المشروع يعمل على أطر عمل غير مدعومة أمنياً (Out of Support) بعد 10 نوفمبر 2026، مما يجعل الانتقال إلى **.NET 10 LTS** ضرورة حتمية عاجلة وليست مجرد خيار رفاهية مستقبلي.

---

## 2. فحص الأطر الحالية للمشاريع العشرة (Current Target Frameworks)

| المشروع | نوع المشروع | إطار العمل الفعلي الحالي (TFM) | حالة الدعم بعد 10 نوفمبر 2026 |
| :--- | :--- | :---: | :---: |
| **Khadamat.Domain** | Class Library | `net8.0` | **منتهي الدعم (End of Support)** |
| **Khadamat.Shared** | Class Library | `net8.0` | **منتهي الدعم (End of Support)** |
| **Khadamat.Application** | Class Library | `net8.0` | **منتهي الدعم (End of Support)** |
| **Khadamat.Infrastructure** | Class Library | `net8.0` | **منتهي الدعم (End of Support)** |
| **Khadamat.WebAPI** | ASP.NET Core Web API | `net8.0` | **منتهي الدعم (End of Support)** |
| **Khadamat.BlazorUI** | Blazor WebAssembly Component | `net8.0` | **منتهي الدعم (End of Support)** |
| **Khadamat.WasmHost** | Blazor WebAssembly Host | `net8.0` | **منتهي الدعم (End of Support)** |
| **Khadamat.MobileApp** | .NET MAUI App | `net9.0-android`, `net9.0-ios` | **منتهي الدعم (End of Support)** |
| **Khadamat.Mobile** | Blazor Hybrid (Legacy) | `net8.0-android`, `net8.0-ios`, ... | **منتهي الدعم (End of Support)** |
| **Khadamat.UnitTests** | xUnit Test Suite | `net9.0` | **منتهي الدعم (End of Support)** |

---

## 3. فحص المعوقات الفنية المثبتة بالأدلة (Evidence-Based Blockers)

تم إجراء فحص للبيئة الفعلية وخوادم البناء (`dotnet --info`)، وكشف الفحص الأدلة القاطعة التالية:

### أ. العائق الأول: عدم تثبيت .NET 10 SDK و Workloads على جهاز البناء (Evidence: FAIL)
* **الأمر:** `dotnet --info`
* **النتيجة الفعلية:**
  - `SDKs installed:` `9.0.310`, `9.0.320`. (لا يوجد أي .NET 10 SDK مثبت حالياً على السيرفر/محطة العمل).
  - `Workloads installed:` `android 35.0.105/9.0.100`, `maui 9.0.120/9.0.100`. (لا توجد حزم عمل MAUI 10 أو Android 10).
  - `Runtimes installed:` يتوفر `Microsoft.NETCore.App 10.0.14` و `WindowsDesktop.App 10.0.14`، ولكن حزمة `Microsoft.AspNetCore.App 10.0.x` غير متوفرة!
* **الحكم:** **`FAIL`** – لا يمكن تنفيذ بناء المشاريع على `net10.0` محلياً قبل تثبيت `.NET 10 SDK (v10.0.100+)` وحزم العمل التابعة له.

### ب. العائق الثاني: حزم Firebase و AndroidX لمجتمع .NET (Evidence: WARNING)
* **التحليل الفني:**
  - يعتمد مشروع `Khadamat.MobileApp` على `Xamarin.Firebase.Messaging 123.4.1` و `Xamarin.AndroidX.Collection.Ktx 1.5.0.1`.
  - هذه الحزم تعمل بثبات مع .NET 9، ولكن في .NET 10 تخضع منظومة Bindings إلى الانتقال لـ .NET Android 10 bindings الرسمية الجديدة. الترقية دون اختبار تفاعلي قد تؤدي إلى انهيارات أثناء استقبال الإشعارات (Background Push Notifications).
* **الحكم:** **`WARNING`** – يتطلب اختبارات حية دقيقة على أجهزة أندرويد قبل نشر حزمة الإنتاج.

---

## 4. إمكانية فصل ترقية الـ Backend عن تطبيق الموبايل (Independent Migration)

> [!TIP]
> **استنتاج معماري عالي القيمة:**  
> **نعم، يمكن ترقية مشاريع الخادم والـ Backend والـ WebAPI بشكل مستقل تماماً عن تطبيق الموبايل (PASS).**

* **السبب:**
  - يتواصل تطبيق الموبايل مع الـ Backend حصرياً عبر بروتوكول HTTP REST APIs القياسي (JSON payloads عبر HTTPS).
  - لا توجد أي مشاركة ثنائية (Binary DLL coupling) بين تطبيق الموبايل والـ WebAPI أثناء التشغيل.
  - تطبيق الموبايل يستهلك فقط مشاريع `Khadamat.Shared` و `Khadamat.Domain` عبر الكود المصدري.
* **المسار الاستراتيجي المقترح:**
  1. ترقية طبقات الـ Core والـ Backend أولاً: (`Domain`, `Shared`, `Application`, `Infrastructure`, `WebAPI`) إلى `net10.0`.
  2. الاستفادة من ترقية EF Core 10 و ASP.NET Core 10 و Serilog 10 وسد كافة الثغرات الأمنية على السيرفر فوراً.
  3. إبقاء `Khadamat.MobileApp` مؤقتاً على `net9.0-android` حتى استقرار بيئة البناء وتثبيت SDK 10، حيث إن Google Play يقبل تطبيقات مبنية بـ Target SDK 36 حتى لو كانت النواة .NET 9 كما أثبتنا في تقرير الـ AAB.

---

## 5. مصفوفة توافق الحزم مع .NET 10 / MAUI 10

| الحزمة / المكتبة | الإصدار الحالي | التوافق مع .NET 10 | الإصدار المقترح لـ .NET 10 | حالة التوافق البرمجي |
| :--- | :---: | :---: | :---: | :---: |
| **Microsoft.EntityFrameworkCore.SqlServer** | 8.0.0 | **PASS** | `10.0.14` | متوافق تماماً (يتطلب فحص الـ Queries) |
| **Microsoft.AspNetCore.Identity.EntityFrameworkCore** | 8.0.0 | **PASS** | `10.0.14` | متوافق 100% |
| **MediatR** | 12.4.1 | **PASS** | `12.4.1` أو `14.3.0` | متوافق بالكامل |
| **AutoMapper** | 13.0.1 | **PASS** | `13.0.1` أو `16.3.0` | متوافق (مراجعة GHSA-rvv3-g6hj-g44x) |
| **FluentValidation** | 12.1.1 | **PASS** | `12.1.1` أو `12.2.0` | متوافق 100% |
| **Serilog.AspNetCore** | 8.0.3 | **PASS** | `10.0.0` | متوافق ومصمم لـ .NET 10 |
| **SQLitePCLRaw.bundle_green** | 2.1.10 | **PASS** | `2.1.10` / `2.1.11` | متوافق مع .NET 10 و 16 KB |
| **Microsoft.Maui.Controls** | 9.0.120 | **WARNING** | `10.0.120` | يتطلب .NET 10 MAUI Workload |
| **Xamarin.Firebase.Messaging** | 123.4.1 | **WARNING** | `125.1.3` | يتطلب فحص أذونات وتكامل الإشعارات |

---

## 6. أقصر مسار آمن للترقية (Shortest Safe Migration Path)

```
المرحلة 1: تجهيز السيرفر وبيئة البناء (Build Infrastructure)
  └── تحميل وتثبيت .NET 10 SDK الرسمي وحزمة Workload: maui, android, wasm-tools.

المرحلة 2: ترقية مشاريع السيرفر والـ WebAPI (Server-Side Migration)
  ├── ترقية Domain و Shared إلى TargetFramework = net10.0
  ├── ترقية Application و Infrastructure و WebAPI إلى net10.0
  ├── تحديث حزم EF Core و ASP.NET Core إلى 10.0.14 وحزم Serilog إلى 10.0.0
  └── تشغيل الـ WebAPI واجتياز اختبارات الـ Integration والـ Unit Tests.

المرحلة 3: ترقية الـ Blazor Frontend (BlazorUI & WasmHost)
  ├── ترقية Khadamat.BlazorUI و Khadamat.WasmHost إلى net10.0
  └── اختبار عمل الـ WASM والـ PWA في المتصفح.

المرحلة 4: ترقية تطبيق الموبايل (Mobile App Migration)
  ├── تحديث Khadamat.MobileApp إلى net10.0-android و net10.0-ios
  ├── تحديث MAUI Controls إلى 10.0.120
  └── بناء الـ Release مع R8 والتحقق من استقرار الإشعارات والـ SQLite.
```

---

## 7. متطلبات الاستضافة والـ Runtime (Deployment & Hosting Requirements)

1. **خادم الاستضافة للـ WebAPI:**
   - تثبيت حزمة **ASP.NET Core 10.0 Hosting Bundle** على خادم Windows Server / IIS أو تشغيل الحاوية عبر Docker Image `mcr.microsoft.com/dotnet/aspnet:10.0`.
2. **متطلبات بناء Android:**
   - OpenJDK 17 أو 21.
   - Android SDK Build-Tools 35.0.0 / 36.0.0.
   - .NET 10 Android Workload.
3. **متطلبات بناء iOS:**
   - جهاز macOS (Apple Silicon M1/M2/M3) يعمل بـ macOS Sonoma أو Sequoia.
   - Xcode 16+ مع iOS 18/26 SDK.
   - شهادات التوقيع التجاري Distribution Certificate و Provisioning Profile من Apple Developer Program.

---

## 8. التصنيف النهائي لفقرات الجاهزية لـ .NET 10

* **إمكانية ترقية الـ WebAPI والسيرفر مستقلاً:** **`PASS`**
* **توافق حزم EF Core و MediatR و Serilog:** **`PASS`**
* **جاهزية بيئة البناء الحالية لـ .NET 10:** **`FAIL`** (عدم تثبيت .NET 10 SDK حالياً).
* **توافق حزم Firebase و MAUI 10 للموبايل:** **`WARNING`** (تحتاج اختبار تجريبي بعد تثبيت الـ SDK).
