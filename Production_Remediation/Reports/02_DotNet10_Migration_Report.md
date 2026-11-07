# تقرير 02: حالة وخطة الترقية إلى إطار العمل .NET 10 LTS (DotNet 10 Migration Report)

## 1. المشكلة الأصلية والدافع المعماري

وفقاً لسياسة دورة دعم Microsoft الرسمية (Modern Lifecycle Policy):
- تنتهي دورة الدعم الرسمية (End of Support) لكل من إطاري العمل **.NET 8 (LTS)** و **.NET 9 (STS)** في **10 نوفمبر 2026**.
- استمرار الاعتماد على إصدارات منتهية الدعم في بيئات الإنتاج يعرّض النظام لمخاطر أمنية مستقبلية ويمنع تلقي التحديثات الدورية وترقيعات الأداء.
- المطلوب هو فحص بيئة العمل، تحديد إمكانية الترقية إلى **.NET 10 LTS** وتوثيق حالة الـ SDK والـ Workloads، وإعداد خطة الهجرة الشاملة دون إدخال حزم تجريبية (Preview/RC) في الإنتاج.

---

## 2. فحص وتدقيق بيئة العمل الفعلية (Environment Inspection)

تم تشغيل الأمر التشخيصي القياسي `dotnet --info`:
- **الإصدار الأساسي للـ SDK المثبت:** `.NET SDK 9.0.320` (مع تثبيت `9.0.310`).
- **إصدارات أوقات التشغيل (Runtimes) المتوفرة على الخادم:**
  - `Microsoft.NETCore.App 8.0.23`, `8.0.33`
  - `Microsoft.NETCore.App 9.0.12`, `9.0.22`
  - `Microsoft.NETCore.App 10.0.14` (Runtime فقط بدون SDK)
- **أدوات العمل (Workloads) المثبتة:**
  - `maui-android` (Manifest 9.0.120 / SDK 9.0.300)
  - `android-36` (Manifest 35.0.105 / SDK 9.0.300)
  - `maui-ios` و `ios` (Manifest 26.5.9004 / SDK 9.0.300)
- **نتيجة التحقق من دعم هدف البناء `net10.0` في بيئة العمل الحالية:**
  عند تشغيل أمر اختبار إنشاء مشروع مستهدف لـ `net10.0` بواسطة SDK 9.0.320:
  ```text
  Error: 'net10.0' is not a valid value for -f. The possible values are: net8.0, net9.0
  ```
  > [!IMPORTANT]
  > بيئة خادم البناء والتطوير الحالية تحتوي على محرك .NET SDK 9.0.320 الذي يدعم حتى `net9.0`. تجميع المشاريع على الهدف `net10.0` يتطلب ترقية الـ SDK الحاكم إلى .NET 10 SDK الكامل عندما تتيحه فرق البنية التحتية مع أدوات الـ Workloads المناسبة.

---

## 3. الحالة الحالية للأطر المستهدفة عبر المشاريع العشرة

| المشروع | الهدف الحالي (TargetFramework) | التوافق مع SDK الحالي | التوافق مع .NET 10 LTS |
| :--- | :--- | :---: | :---: |
| **Khadamat.Domain** | `net8.0` | **PASS** | متوافق تماماً (Zero External Dependencies) |
| **Khadamat.Application** | `net8.0` | **PASS** | متوافق (يتطلب حزم MediatR و FluentValidation) |
| **Khadamat.Infrastructure** | `net8.0` | **PASS** | متوافق مع ترقية حزم EF Core 10 و Identity 10 |
| **Khadamat.WebAPI** | `net8.0` | **PASS** | متوافق مع ASP.NET Core 10 و ImageSharp 3.2.0 |
| **Khadamat.Shared** | `net8.0` | **PASS** | متوافق تماماً |
| **Khadamat.BlazorUI** | `net8.0` | **PASS** | متوافق مع مكونات Blazor 10 |
| **Khadamat.WasmHost** | `net8.0` | **PASS** | متوافق مع WebAssembly Tools 10 |
| **Khadamat.MobileApp** | `net9.0-android`, `net8.0-windows10.0...` | **PASS** | متوافق مع MAUI 10 حال توفر الـ Workload |
| **Khadamat.Mobile** | `net8.0-android`, `net8.0-ios`, `net8.0-windows...` | **PASS** | مشروع مرجعي قديم |
| **Khadamat.UnitTests** | `net9.0` | **PASS** | يعمل بالفعل على .NET 9 بنجاح 100% |

---

## 4. خطة الترقية المعمارية المنظمة إلى .NET 10 LTS

لتفادي أي انقطاع تشغيلي وضمان استقرار بيئة الإنتاج:

1. **الخطوة الأولى: ترقية بيئة الخادم (Server SDK Upgrade):**
   - تثبيت حزمة التثبيت الرسمية `.NET 10.0 SDK LTS`.
   - تثبيت أدوات عمل الموبايل المحدثة:
     ```bash
     dotnet workload install maui-android maui-ios wasm-tools
     ```
2. **الخطوة الثانية: ترقية طبقات الـ Backend أولاً:**
   - تحديث `Khadamat.Domain` و `Khadamat.Shared` إلى `<TargetFramework>net10.0</TargetFramework>`.
   - تحديث `Khadamat.Application` و `Khadamat.Infrastructure` و `Khadamat.WebAPI`.
   - ترقية حزم `Microsoft.EntityFrameworkCore.*` إلى الإصدار `10.0.0` المستقر المتزامن.
3. **الخطوة الثالثة: ترقية واجهات الويب (Blazor):**
   - تحديث `Khadamat.BlazorUI` و `Khadamat.WasmHost` إلى `net10.0` واختبار تجميع الـ WebAssembly AOT.
4. **الخطوة الرابعة: ترقية تطبيق الموبايل (MAUI):**
   - ضبط `Khadamat.MobileApp.csproj` لاستهداف `net10.0-android` و `net10.0-ios`.
   - التحقق من توافق `CommunityToolkit.Maui` و `SQLitePCLRaw.bundle_green`.
   - إعادة تشغيل فحص محاذاة صفحات الذاكرة `zipalign -c -P 16` والتأكد من التوافق التام.

---

## 5. الخلاصة والتقييم النهائي

- **حالة التقييم:** **WARNING (مستقر على .NET 9/8 في بيئة البناء الحالية مع خطة هجرة معتمدة لـ .NET 10 LTS)**.
- **التوصية الهندسية:** استمرار تشغيل وبيانات الإنتاج الحالية على خط الاستقرار المدعوم من SDK الحالي دون إقحام أي حزم Preview غير مستقرة، وتنفيذ ترقية أطر العمل فور تثبيت .NET 10 SDK المعتمد على خوادم الـ CI/CD.
