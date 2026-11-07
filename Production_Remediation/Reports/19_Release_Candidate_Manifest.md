# تقرير 19: وثيقة بيان الإصدار النهائي (Release Candidate Manifest)

## 1. بيانات الإصدار والالتزام المصدري (Release & Commit Metadata)

- **اسم المنظومة والتطبيق:** خدماوي (Khadamawy)
- **معرّف الحزمة (Package ID / ApplicationId):** `com.nassar84.khadamawy`
- **إصدار التطبيق (Version / VersionCode):** Version `1.0.0` / VersionCode `1`
- **الالتزام المصدري المعتمد (Base Commit Hash):** `90bf7ac8`
- **تاريخ إصدار البيان (Manifest Release Date):** 2026-11-07
- **بيئة البناء والتحقق المعتمدة:** Windows Server 64-bit / .NET SDK 9.0.320 & .NET SDK 10.0.403 LTS

---

## 2. جدول أطر العمل المستهدفة للمشاريع (Target Framework Matrix)

| المشروع (Project) | إطار العمل المستهدف (TFM) | حالة البناء (Build) | الغرض والدور في المنظومة |
| :--- | :---: | :---: | :--- |
| **Khadamat.Domain** | `net8.0` | **PASS** (0 Errors) | الكيانات الأساسية، قواعد الأعمال، الكائنات القيمة (Core Entities) |
| **Khadamat.Application** | `net8.0` | **PASS** (0 Errors) | حالات الاستخدام، MediatR Handlers، التحقق والتخزين المؤقت |
| **Khadamat.Infrastructure** | `net8.0` | **PASS** (0 Errors) | EF Core، الهوية Identity، تشفير المفاتيح، والخدمات السحابية |
| **Khadamat.WebAPI** | `net8.0` | **PASS** (0 Errors) | واجهة البرمجة الخلفية للإنتاج، رؤوس الأمان، ومصادقة JWT |
| **Khadamat.Shared** | `net8.0` | **PASS** (0 Errors) | عقود DTOs المشتركة، النماذج، والرسائل بين الأنظمة |
| **Khadamat.BlazorUI** | `net8.0` | **PASS** (0 Errors) | واجهات Blazor الموحدة لمستخدمي الويب والموبايل الداخلي |
| **Khadamat.WasmHost** | `net8.0` | **PASS** (0 Errors) | مضيف WebAssembly للوحة التحكم وواجهات الإدارة |
| **Khadamat.MobileApp** | `net9.0-android`<br>`net8.0-windows10.0.19041.0`<br>`(net9.0-ios جاهز)` | **PASS** (0 Errors) | تطبيق الهواتف الإنتاجي الرئيسي المجهز لمتاجر التطبيقات |
| **Khadamat.Mobile** | `net8.0-android` | **PASS** (0 Errors) | مشروع تجريبي أولي (Legacy Sandbox) محفوظ دون نشر إنتاجي |
| **Khadamat.UnitTests** | `net9.0` | **PASS** (0 Errors) | حزمة الاختبارات الشاملة (55 اختبار وحدة ناجح بنسبة 100%) |

---

## 3. محركات البناء وحزم العمل (SDKs & Workloads Specification)

1. **محرك البناء الأساسي المثبت بالنظام (Host SDK):**
   - **الإصدار:** `.NET SDK 9.0.320`
   - **المسار:** `C:\Program Files\dotnet\sdk\9.0.320`
2. **محرك البناء الموسع لـ .NET 10 LTS (Standalone LTS SDK):**
   - **الإصدار:** `.NET SDK 10.0.403 LTS`
   - **المسار:** `D:\dotnet10\sdk\10.0.403`
   - **محرك MSBuild:** الإصدار `18.9.15`
   - **أوقات التشغيل المعتمدة:** `Microsoft.NETCore.App 10.0.14` و `Microsoft.AspNetCore.App 10.0.14`
3. **حزم العمل المثبتة لـ .NET 10 (Installed Workloads):**
   - `android`: الإصدار `36.1.118` (جاهز بالكامل لاستهداف Android API 36 / Android 16)
   - `ios`: الإصدار `27.1.10018`
   - `maccatalyst`: الإصدار `27.0.10742`
   - `maui-windows`: الإصدار `10.0.110`
   - `maui-android`: الإصدار `10.0.110`
   - `wasm-tools-net6`: الإصدار `10.0.114`

---

## 4. جدول حزم NuGet الأساسية وحالتها الأمنية

| اسم الحزمة (Package Name) | الإصدار المثبت | حالة الثغرات المعلنة (CVEs) | القرار المعماري والملاحظات |
| :--- | :---: | :---: | :--- |
| **SixLabors.ImageSharp** | `3.2.0` | **0 Vulnerabilities (Clean)** | تمت ترقيتها لمعالجة جميع التحذيرات الأمنية السابقة GHSA بنجاح |
| **AutoMapper** | `13.0.1` | **0 Vulnerabilities (Clean)** | منظمة في Backend، ومستبعدة بالكامل من تطبيق الموبايل لتحسين الأداء |
| **Microsoft.EntityFrameworkCore.SqlServer** | `8.0.12` | **0 Vulnerabilities (Clean)** | إصدار الصيانة المستقر المتوافق كلياً مع .NET 8.0 LTS |
| **Microsoft.AspNetCore.Authentication.JwtBearer** | `8.0.12` | **0 Vulnerabilities (Clean)** | متوافق تماماً مع `JwtKeyManager` ونظام التحقق من مفاتيح `kid` |
| **Microsoft.IdentityModel.Tokens** | `8.3.0` | **0 Vulnerabilities (Clean)** | دعم متقدم لتسجيل واسترجاع مفاتيح التوقيع المتعددة |
| **Microsoft.Maui.Controls** | `9.0.30` | **0 Vulnerabilities (Clean)** | المحرك المستقر المعتمد لتطبيق الموبايل الإنتاجي |
| **MediatR** | `12.4.1` | **0 Vulnerabilities (Clean)** | فصل طبقات التطبيق وحالات الاستخدام (CQRS Pattern) |
| **FluentValidation** | `11.11.0` | **0 Vulnerabilities (Clean)** | التحقق البرمجي التلقائي الصارم من صحة المدخلات |

---

## 5. نتائج الاختبارات المخبرية (Automated Verification & Unit Tests)

- **إجمالي عدد اختبارات الوحدة:** 55 اختباراً
- **الاختبارات الناجحة (Passed):** 55 (بنسبة 100%)
- **الاختبارات الفاشلة (Failed):** 0
- **الاختبارات المتجاهلة (Skipped):** 0
- **تغطية أمان مفاتيح JWT المحدثة:**
  - `Test1_ActiveKey_SignsAndValidatesSuccessfully` $\rightarrow$ **PASS**
  - `Test2_MultipleKeys_DifferentKid_ValidatesCorrectly` $\rightarrow$ **PASS**
  - `Test3_RetiredKey_FailsValidation` $\rightarrow$ **PASS**
  - `Test4_GracePeriod_ValidatesBeforeExpiration` $\rightarrow$ **PASS**
  - `Test5_ExpiredGracePeriod_FailsValidation` $\rightarrow$ **PASS**
  - `Test6_LegacyToken_WithoutKid_ValidatesDuringTransition` $\rightarrow$ **PASS**
  - `Test7_AddKey_SetsActive_RetiresPrevious` $\rightarrow$ **PASS**
  - `Test8_LegacyToken_WithoutKid_AfterCutoff_FailsValidation` $\rightarrow$ **PASS (قطع التوكنات القديمة)**
  - `Test9_AlgorithmAllowList_Enforces_HmacSha256` $\rightarrow$ **PASS (حظر خلط الخوارزميات)**

---

## 6. مواصفات حزمة أندرويد (Android Release Package Manifest)

| الخاصية (Attribute) | القيمة المقاسة مخبرياً (Measured Value) | التقييم الفني الصارم |
| :--- | :--- | :---: |
| **نوع الحزمة (Package Type)** | Android App Bundle (`.aab`) | **PASS** |
| **المسار الكامل** | `src/Khadamat.MobileApp/bin/Release/net9.0-android/com.nassar84.khadamawy-Signed.aab` | **PASS** |
| **حجم الحزمة الفعلي** | `54,922,423 bytes` (~52.38 MB) | **PASS** |
| **بصمة التجزئة الرقمية (SHA-256)** | `CAAC5773FC35AD1C442F37D801695C49E3F4EE8A62610B43EDCD976B18430A7B` | **PASS** |
| **التحقق من صحة الحزمة (Bundletool)** | `bundletool validate --bundle=...` بنجاح تام | **PASS** |
| **محاذاة الذاكرة (16 KB Page Alignment)** | فحص `zipalign -c -P 16 4` اجتاز بنجاح لجميع ملفات SO الثنائية | **PASS** |
| **محاذاة مقاطع ELF الثنائية** | فحص `LOAD` segments في مكتبات `arm64-v8a` و `x86_64` محاذاة لـ `0x4000` | **PASS** |
| **شهادة التوقيع الحالية (Signing Certificate)** | `CN=Android Debug, O=Android, C=US`<br>SHA-256: `2E:05:22:84:AA:C1:8B:0A:98:8B:E0:EC:37:1D:E5:CC:64:FF:E9:3B:5E:FF:62:A8:6B:E0:B3:A3:9E:62:AF:BA` | **BLOCKED (Credentials)** |

> [!CAUTION]
> **شرط حاسم لمتجر Google Play:**
> الحزمة الحالية موقعة محلياً بمفتاح Debug Keystore لتأكيد نجاح خط التجميع والاختبار. لا يجوز رفعها إلى Google Play Console بهذه الشهادة. يجب استخدام مفتاح الإنتاج (Upload Key Keystore) الخاص بحساب المطور المعتمد لإعادة التوقيع النهائي.

---

## 7. مواصفات منصة Apple iOS (iOS Manifest)

| المحور | الوضع الراهن | التقييم |
| :--- | :--- | :---: |
| **معرّف الحزمة (`CFBundleIdentifier`)** | مطابق للإنتاج `com.nassar84.khadamawy` | **PASS** |
| **سياسة أمان النقل (`ATS - App Transport Security`)** | لا توجد أي استثناءات غير آمنة (`NSAllowsArbitraryLoads = false`) | **PASS** |
| **أذونات واستحقاقات النظام (`Entitlements.plist`)** | تم تجهيز Keychain Sharing و Apple Push Notifications (APNs) | **PASS** |
| **إنتاج حزمة التطبيق الموقعة (`.ipa Archive`)** | خط البناء مؤتمت؛ يتطلب خادم بناء macOS مع بيئة Xcode والشهادات الرسمية | **BLOCKED (Hardware)** |

---

## 8. حالة ادعاءات الأداء السابقة (Performance Claims Status)

تطبيقاً لمعايير النزاهة الهندسية المعتمدة:

- **ادعاء "خفض الباندويث بنسبة 70%":** $\rightarrow$ **NOT VERIFIED** (مقارنة نظرية لصيغ WebP؛ لم يُقس باختبار شبكي واقعي على السحابة).
- **ادعاء "خفض استهلاك الذاكرة بنسبة 35%":** $\rightarrow$ **NOT VERIFIED** (أثر إيجابي معروف لـ `AsNoTracking`؛ لم يُقس بـ Memory Profiler تحت ضغط).
- **ادعاء "تحسين وقت الإقلاع بنسبة 40%":** $\rightarrow$ **NOT VERIFIED** (ميزة نظرية لتفعيل AOT؛ يتطلب جهازاً فيزيائياً و Startup Tracing).

---

## 9. قائمة الموانع التشغيلية والإجراءات المتبقية (External Blockers & Action Items)

| الإجراء المطلوب (Required Action) | الجهة المسؤولة | الأثر التشغيلي إذا لم يُنفّذ |
| :--- | :---: | :--- |
| **تزويد مفتاح التوقيع الإنتاجي (Production Upload Key)** | مالك التطبيق / DevOps | لن يقبل Google Play الحزمة بشهادة الـ Debug الحالية |
| **إجراء تدوير أسرار السحابة الحية (Cloud Secret Rotation)** | مهندس السحابة / DevOps | بقاء المفاتيح القديمة (المكشوفة تاريخياً في Git) نشطة في خوادم الإنتاج |
| **تسجيل مفتاح الترخيص المجاني لـ ImageSharp** | مالك المشروع | قد تظهر شروط الترخيص أو تحذيرات عند الاستخدام التجاري المكثف |
| **تشغيل خط بناء iOS على بيئة macOS** | فريق DevOps | عدم توفر حزمة `.ipa` للرفع المباشر على TestFlight / App Store |

---

## 10. القرار النهائي لبوابة الإطلاق (Final Release Gate Verdict)

- **الكود المصدري والبناء الداخلي:** **PASS 100% (جاهز ومكتمل وخالٍ من الثغرات والأخطاء)**.
- **التجهيز الإنتاجي للمتاجر (Store Submission Readiness):** **CONDITIONAL (مشروط باستلام الـ Upload Key وتدوير الأسرار السحابية)**.
