# التقرير الخامس عشر: خطة ودراسة الانتقال إلى .NET 10 LTS و MAUI 10 (Migration Feasibility Plan)

**المشروع:** خدماوي (Khadamawy)  
**التاريخ:** 7 نوفمبر 2026  
**المرحلة:** المرحلة الثانية - دراسة الجدوى المعمارية للإصدارات  
**إعداد:** Senior .NET Architect  

---

## 1. الخلفية ودوافع الدراسة

يستهدف مشروع خدماوي حالياً:
- **مشاريع Backend و Shared:** إطار العمل **.NET 8 (LTS)**.
- **تطبيقات Mobile (Khadamat.MobileApp & Khadamat.Mobile):** إطار العمل **.NET 9 (Current / STS)**.

مع الإعلان عن .NET 10 LTS و MAUI 10 والدعم الموسع لصفحات الذاكرة 16 KB والـ Android API 36، طُرح خيار توحيد المنظومة بالكامل على **.NET 10 LTS**. تم إعداد هذه الدراسة لتقييم جدوى ومخاطر هذا الانتقال، ووضع مصفوفة توافق تفصيلية.

---

## 2. مصفوفة توافق المشاريع العشرة مع .NET 10

| المشروع | الإطار الحالي | الإطار المقترح | توافق الحزم الأساسية | مستوى المخاطرة البرمجية |
| :--- | :--- | :--- | :--- | :--- |
| **Khadamat.Domain** | `net8.0` | `net10.0` | 100% متوافق (Pure C# Entities & Value Objects) | منخفض جداً |
| **Khadamat.Shared** | `net8.0` | `net10.0` | 100% متوافق (DTOs, Enums, Constants) | منخفض جداً |
| **Khadamat.Application** | `net8.0` | `net10.0` | MediatR 12.4.1 و FluentValidation 11.9.0 متوافقان مع .NET 10 | منخفض |
| **Khadamat.Infrastructure** | `net8.0` | `net10.0` | EF Core 8 -> EF Core 10 يتطلب فحص استعلامات SQL و Breaking Changes | متوسط |
| **Khadamat.WebAPI** | `net8.0` | `net10.0` | ASP.NET Core 10، Serilog 10، ImageSharp 3.1.7 | متوسط (يتطلب اختبار شامل) |
| **Khadamat.BlazorUI** | `net8.0` | `net10.0` | Blazor Components و Razor Syntax متوافق | منخفض |
| **Khadamat.WasmHost** | `net8.0` | `net10.0` | Microsoft.AspNetCore.Components.WebAssembly 10 | متوسط |
| **Khadamat.MobileApp** | `net9.0-android`, `net9.0-ios` | `net10.0-android`, `net10.0-ios` | MAUI 10، CommunityToolkit 10، Firebase | **مرتفع حالياً** (انتظار استقرار Workload و Bindings) |
| **Khadamat.Mobile** | `net9.0` | `net10.0` | Blazor Hybrid | متوسط |
| **Khadamat.UnitTests** | `net8.0` | `net10.0` | xUnit 2.8.0، Moq 4.20.70 | منخفض جداً |

---

## 3. تحليل الفوائد والتحديات (Pros vs. Cons & Risks)

### أ. الفوائد المتوقعة من الترقية إلى .NET 10 LTS
1. **دعم رسمي طويل الأمد (LTS - 3 Years):** توحيد كافة مشاريع المنظومة (Web, Mobile, Backend) على إصدار واحد طويل الدعم، مما يلغي التباين الحالي بين .NET 8 و .NET 9.
2. **دعم أصلي متكامل لـ Android API 36 و 16 KB:** بيئة .NET 10 Android Workload مصممة من الأساس لتدعم API 36 وتوليد مكتبات 16 KB افتراضياً دون الحاجة إلى تكوينات خاصة، مما يحل تحذير `XA1008` نهائياً.
3. **تحسينات الأداء واستغلال الذاكرة في MAUI:** قفزة كبيرة في سرعة بدء التشغيل (Cold Startup Time) واستجابة Blazor Hybrid وتحسينات في تقليص الكود Native AOT.

### ب. المخاطر والعوائق الفنية الراهنة (Current Blockers & Risks)
1. **استقرار حزم Firebase و AndroidX لمجتمع .NET:**
   - حزم الـ Bindings الخاصة بـ AndroidX و Google Play Services و Firebase التي يقدمها مجتمع دوت نت لا تزال في مراحل تجريبية أو تحديثية مع أطر عمل .NET 10 الجديدة، مما قد يسبب انهيارات غير متوقعة أثناء التشغيل (Runtime Crashes) على أجهزة مستخدمي الإنتاج.
2. **استقرار أداة التجميع والرابط (R8 & ProGuard):**
   - تغيير قواعد الـ Trimming و Native AOT في .NET 10 قد يتطلب إعادة كتابة ملفات التكوين لتجنب استبعاد فئات حيوية في انعكاسات MediatR أو AutoMapper أو JSON Serialization.
3. **الجدول الزمني للنشر على المتاجر:**
   - تطبيق خدماوي جاهز الآن للبناء والنشر الفوري على بيئة .NET 8 / .NET 9 الحالية بعد معالجة ثغرات الأمان و 16 KB. إدخال ترقية TFM كاملة في هذه اللحظة سيعيد المشروع إلى نقطة الصفر في دورات اختبار الاستقرار (QA Regression Testing).

---

## 4. التوصية المعمارية الصريحة (Architectural Verdict)

> [!IMPORTANT]
> **التوصية:** الإبقاء على الوضع المستقر الحالي (.NET 8 LTS للـ WebAPI و .NET 9 للـ MobileApp) لإتمام عمليات النشر والتسجيل على Google Play و Apple App Store فوراً.
> **تأجيل ترقية الحل إلى .NET 10** إلى مرحلة ما بعد الإطلاق التجاري المستقر، بعد صدور أول تحديث صيانة استقراري (Service Release - เช่น .NET 10.0.2) واستقرار حزم Firebase الرسمية.

---

## 5. خارطة طريق الانتقال المستقبلي (Step-by-Step Migration Roadmap)

عند اتخاذ قرار الانتقال لاحقاً، يتم تنفيذ المراحل الأربع التالية بالتتابع:

```
[ المرحلة الأولى: تجهيز البنية التحتية ] 
  ├── تثبيت .NET 10 SDK النهائي وحزم Workload (android, ios, maui)
  └── ترقية global.json لتعيين إصدار SDK المعتمد

[ المرحلة الثانية: ترقية الطبقات المشتركة و الـ Backend ]
  ├── تعديل TargetFramework في Domain, Shared, Application إلى net10.0
  ├── ترقية Khadamat.Infrastructure و Khadamat.WebAPI إلى net10.0
  ├── تحديث حزم Microsoft.EntityFrameworkCore.* و ASP.NET Core إلى 10.0.x
  └── تشغيل اختبارات الوحدة والتأكد من توافق استعلامات EF Core

[ المرحلة الثالثة: ترقية مشاريع MAUI و الموبايل ]
  ├── ترقية Khadamat.MobileApp إلى net10.0-android و net10.0-ios
  ├── تحديث Microsoft.Maui.Controls و CommunityToolkit.Maui إلى 10.x
  ├── فحص توافق حزم Firebase و Google Play Services
  └── اختبار بناء Release وإنشاء الحزم (AAB و IPA)

[ المرحلة الرابعة: الاختبار الشامل والاعتماد ]
  ├── اختبار Smoke Tests على أجهزة أندرويد حقيقية (Android 14, 15, 16)
  └── اختبار دورة حياة التطبيق على أجهزة iOS حقيقية عبر TestFlight
```
