# تقرير 01: نظرة عامة على معمارية وهيكل حل مشروع خدماوي (Khadamawy)

**تاريخ التدقيق:** 7 نوفمبر 2026  
**المراجع:** Senior .NET Architect & Mobile Solutions Consultant  
**حالة المشروع:** فحص وتقييم شامل (Audit Mode)

---

## 1. الهيكل العام للحل (Solution Structure)

يتكون حل **Khadamat.sln** من 10 مشاريع تغطي المنظومة بالكامل (Clean Architecture) عبر الويب والخدمات الخلفية وتطبيقات الهواتف الذكية:

| المشروع | المسار | Target Framework | نوع المشروع والدور الأساسي |
| :--- | :--- | :--- | :--- |
| **Khadamat.Domain** | `src/Khadamat.Domain` | `net8.0` | الكيانات الأساسية (Entities)، التعدادات (Enums)، القواعد العامة للأعمال بدون أي تبعيات خارجية. |
| **Khadamat.Application** | `src/Khadamat.Application` | `net8.0` | منطق الأعمال وحالات الاستخدام (CQRS عبر MediatR)، الواجهات (Interfaces)، كائنات نقل البيانات (DTOs)، ونماذج التحقق (FluentValidation). |
| **Khadamat.Infrastructure** | `src/Khadamat.Infrastructure` | `net8.0` | الوصول لقواعد البيانات (EF Core 8 / SQL Server)، خدمات الهوية (ASP.NET Identity)، المصادقة (JWT, Google, Facebook)، الإشعارات (Firebase Admin)، والبريد الإلكتروني (MailKit). |
| **Khadamat.Shared** | `src/Khadamat.Shared` | `net8.0` | النماذج المشتركة، كائنات DTO المتبادلة بين الخادم وواجهات العميل، وثوابت المنظومة. |
| **Khadamat.WebAPI** | `src/Khadamat.WebAPI` | `net8.0` | واجهات برمجة التطبيقات الخلفية RESTful APIs، توثيق Swagger/OpenAPI، مسارات المصادقة، تحميل الصور (ImageSharp)، وإدارة الاشتراكات والطلبات. |
| **Khadamat.BlazorUI** | `src/Khadamat.BlazorUI` | `net8.0` | مكتبة مكونات Razor المشتركة التي تضم كامل واجهة المستخدم، صفحات العرض، كروت الخدمات، إدارة الملف الشخصي، والمحادثات. |
| **Khadamat.WasmHost** | `src/Khadamat.WasmHost` | `net8.0` | تطبيق Blazor WebAssembly المستضاف لتشغيل المنصة عبر متصفحات الويب وأجهزة الحاسوب. |
| **Khadamat.MobileApp** | `src/Khadamat.MobileApp` | `net9.0-android`, `net8.0-ios`, `net8.0-maccatalyst`, `net8.0-windows10.0.19041.0` | **التطبيق الفعلي والأساسي المخصص للنشر (Production Mobile Application)**. يحتوي على تخصيصات المنصات، Firebase Messaging، وأيقونات وشاشات البداية المعتمدة. |
| **Khadamat.Mobile** | `src/Khadamat.Mobile` | `net8.0-android`, `net8.0-ios`, `net8.0-maccatalyst` | **مشروع افتراضي/تجريبي قديم (Template / Legacy)** لم يتم تطويره ومتروك في الحل. |
| **Khadamat.UnitTests** | `tests/Khadamat.UnitTests` | `net9.0` | اختبارات الوحدة باستخدام xUnit وMoq وFluentAssertions. |

---

## 2. تحديد التطبيق الفعلي المخصص للنشر (Target Identification)

تمت المقارنة الدقيقة بين مشروعي الموبايل لتحديد المشروع الرسمي المخصص لرفعه لمتاجري Google Play وApple App Store:

### أ) مشروع `Khadamat.MobileApp` (التطبيق الفعلي للنشر):
- **معرّف الحزمة (Package ID):** `com.nassar84.khadamawy` (معرف رسمي تجاري خاص بالعميل).
- **اسم العرض (Application Title):** `خدماوي` (Khadamawy).
- **إصدار التطبيق:** Version `2.9`، VersionCode / Build `21`.
- **المكونات والخدمات المتقدمة:**
  - يدمج خدمة إشعارات Firebase المخصصة لنظام أندرويد (`KhadamatFirebaseMessagingService.cs`).
  - يحتوي على شاشات البداية (Splash Screens) وأيقونات الهوية البصرية الرسمية (`khadamawy_logo.png`, `splash_logo.svg`).
  - يمتلك ملفات تكوين أمان الشبكة (`network_security_config.xml`).
  - يرتبط مباشرة بمكتبة `Khadamat.BlazorUI` عبر `BlazorWebView` لتقديم تجربة هجينة (Hybrid App).
- **الخلاصة:** هذا هو المشروع المعتمد الوحيد الذي سيتم تجهيزه ونشره.

### ب) مشروع `Khadamat.Mobile` (مشروع مهجور / Legacy Template):
- **معرّف الحزمة:** `com.companyname.khadamat.mobile` (المعرف الافتراضي لقوالب .NET MAUI).
- **الإصدار:** Version `1.0`، Build `1`.
- **المحتوى:** صفحات افتراضية لقالب مايكروسوفت (MainPage مع العداد Counter الافتراضي)، ولا يحتوي على أي ربط بمنظومة خدماوي أو BlazorUI أو خدمات الإشعارات.
- **التوصية المعمارية:** إزالة هذا المشروع من Solution أو استبعاده تماماً من عمليات البناء لتفادي التشويش وتوفير موارد البناء والتجميع.

---

## 3. معمارية التطبيق الهجين (Hybrid Architecture Workflow)

يعتمد تطبيق **Khadamat.MobileApp** على نمط **.NET MAUI Blazor Hybrid**:
1. **غلاف النظام الأصلي (Native Shell):** يُدار بواسطة .NET MAUI ليتحكم في إعدادات المنصة (Android Activity / iOS AppDelegate)، استقبال الإشعارات السحابية (Push Notifications)، وإدارة دورة حياة التطبيق والأذونات.
2. **محرك العرض (Rendering Engine):** يعتمد على `BlazorWebView` لعرض مكونات Razor المشتركة الموجودة في `Khadamat.BlazorUI`.
3. **طبقة الخدمات المشتركة:** يتواصل تطبيق الموبايل مع واجهات برمجة التطبيقات الخلفية `Khadamat.WebAPI` عبر عملاء HTTP و SignalR لتلقي التحديثات اللحظية وإدارة المحتوى.

---

## 4. انقسام أطر العمل المستهدفة (Target Frameworks Discrepancy)

كشف الفحص عن وجود عدم اتساق جوهري في استهداف أطر العمل (.NET TFMs):
1. مشاريع الواجهة الخلفية وقواعد البيانات والمكونات المشتركة (`Domain`, `Application`, `Infrastructure`, `WebAPI`, `BlazorUI`, `WasmHost`) تستهدف **.NET 8.0** (إصدار الدعم طويل الأمد LTS).
2. مشروع الموبايل `Khadamat.MobileApp` يستهدف **`net9.0-android`** لنظام أندرويد، بينما يستهدف **`net8.0-ios`** لنظام آبل iOS.
3. مشروع الاختبارات `Khadamat.UnitTests` يستهدف **.NET 9.0**.

> [!WARNING]
> التباين في الهدف لنظام أندرويد (`net9.0-android`) بينما بقية المكتبات في الحل مبنية على .NET 8 يسبب سحب حزم ثنائية متضاربة مثل حزم `CommunityToolkit.Maui 8.0.1` المبنية خصيصاً لـ .NET 8 وحزم ملحقات مايكروسوفت. سيتم تفصيل سبل توحيد أو مواءمة هذا التباين في التقارير اللاحقة.
