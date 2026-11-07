# تقرير 02: التدقيق الشامل لحزم NuGet والتبعيات والتوافق الأمني

**تاريخ التدقيق:** 7 نوفمبر 2026  
**المراجع:** Senior .NET Architect  
**الهدف:** فحص شجرة الحزم والتبعيات المباشرة والانتقالية، وتحديد الثغرات الأمنية والتعارضات بين الإصدارات.

---

## 1. ملخص حالة التبعيات والحزم

تم إجراء مسح فني لكافة ملفات `.csproj` مع تشغيل أوامر الفحص الأمني الرسمية:
`dotnet list package --vulnerable --include-transitive`

### جدول التبعيات المباشرة عبر المشاريع:

| المشروع | الحزمة المباشرة (Package) | الإصدار الحالي | التوافق مع TFM | ملاحظات وتنبيهات أمنية |
| :--- | :--- | :--- | :--- | :--- |
| **Khadamat.Domain** | لا توجد حزم خارجية | - | سليم | نقي برمجياً (Pure POCOs) وفق قواعد المعمارية النظيفة. |
| **Khadamat.Application** | MediatR | 12.4.1 | متوافق (`net8.0`) | إصدار مستقر وحديث متوافق تماماً. |
| **Khadamat.Application** | AutoMapper | 13.0.1 | متوافق (`net8.0`) | **تحذير أمني عالي الخطورة (CVE / GHSA-7vhp-ff52-4528)**. |
| **Khadamat.Application** | FluentValidation.DependencyInjectionExtensions | 11.11.0 | متوافق (`net8.0`) | مستقر ومتوافق. |
| **Khadamat.Infrastructure**| Microsoft.EntityFrameworkCore.SqlServer | 8.0.11 | متوافق (`net8.0`) | مستقر ومتوافق مع LTS. |
| **Khadamat.Infrastructure**| Microsoft.AspNetCore.Authentication.JwtBearer| 8.0.11 | متوافق (`net8.0`) | مستقر ومتوافق مع LTS. |
| **Khadamat.Infrastructure**| FirebaseAdmin | 3.1.0 | متوافق (`net8.0`) | مستقر لإرسال إشعارات الخادم. |
| **Khadamat.Infrastructure**| MailKit | 4.8.0 | متوافق (`net8.0`) | مستقر ومتوافق. |
| **Khadamat.WebAPI** | Serilog.AspNetCore | **10.0.0** | **تعارض إصدار!** | **حزمة .NET 10 مثبتة على مشروع net8.0!** تسحب حزم `System.Security.Cryptography.Pkcs 10.0.0`. |
| **Khadamat.WebAPI** | SixLabors.ImageSharp | **2.1.9** | متوافق (`net8.0`) | **3 ثغرات أمنية حرجة وعالية الخطورة (DDoS & Memory Corruption)**. |
| **Khadamat.MobileApp** | Microsoft.Maui.Controls | (حسب SDK) | `net9.0-android` / `net8.0-ios` | انقسام مستهدفات MAUI بين أندرويد وiOS. |
| **Khadamat.MobileApp** | CommunityToolkit.Maui | **8.0.1** | **تعارض إصدار!** | مصممة ومبنية لـ **.NET 8** بينما هدف أندرويد هو **net9.0-android**. |
| **Khadamat.MobileApp** | Xamarin.Firebase.Messaging | 123.4.0 | متوافق (Android) | تسبب تضارب فئات مع AndroidX.Collection.Ktx. |
| **Khadamat.MobileApp** | Xamarin.AndroidX.Collection.Ktx | 1.4.0.5 | متوافق (Android) | تحتوي على تكرار لفئة `ArrayMapKt` مع مكتبة Firebase. |

---

## 2. الثغرات الأمنية المكتشفة في شجرة الحزم (Security Vulnerabilities)

أظهر الفحص الأمني وجود 6 ثغرات أمنية معلنة (High Severity Vulnerabilities) في الحزم المباشرة والانتقالية:

### 1) ثغرة AutoMapper (الإصدار الحالي: 13.0.1)
- **المعرف الأمني:** `GHSA-7vhp-ff52-4528` (High Severity).
- **نوع الثغرة:** ارتباك في الأنواع وتجاوز قيود التحقق وإمكانية حجب الخدمة (Type Confusion & Denial of Service).
- **التوصية:** الانتقال إلى أحدث ترقيع أو استبدال تحويلات DTOs المعقدة بتحويلات صريحة (Manual Mapping) إذا تطلب الأمر.

### 2) ثغرات مكتبة معالجة الصور SixLabors.ImageSharp (الإصدار الحالي: 2.1.9)
- **المعرفات الأمنية:**
  - `GHSA-5xvp-cf8q-36hc` (High): تجاوز حدود الذاكرة (Out-of-bounds Read/Write).
  - `GHSA-rx35-8575-hw9j` (High): انهيار الخدمة نتيجة استهلاك المعالج والذاكرة غير المنضبط (Denial of Service).
  - `GHSA-65x7-c272-7v7v` (High): ثغرة في معالجة مصفوفات الألوان وتشفير PNG.
- **التأثير:** بما أن المستخدمين يرفعون صور الهوية والشهادات وصور الخدمات، فإن استغلال هذه الثغرة عبر رفع صورة ملغومة قد يؤدي لتوقف خادم WebAPI بالكامل.
- **التوصية:** الترقية الفورية للحزمة إلى الإصدار `3.1.5` أو أحدث، وضبط معالجة الصور بأمان.

### 3) ثغرة System.Text.Json (الإصدار الانتقالي الحالي: 8.0.0)
- **المعرف الأمني:** `GHSA-8g4q-xg66-9fp4` (High Severity).
- **التأثير:** حجب الخدمة عند تمرير تسلسلات JSON غير منتهية العمق.
- **التوصية:** رفع الإصدار المباشر إلى `8.0.5` أو أحدث لسد الثغرة في كافة مشاريع الحل.

### 4) ثغرات Microsoft.Data.SqlClient (الإصدار الحالي: 5.1.1)
- **المعرف الأمني:** `GHSA-859w-ff3v-4v85` و `GHSA-98g6-xh36-x2p7`.
- **التوصية:** ترقية تبعية EF Core ومزود SQL Client إلى الإصدار `5.1.5` أو `5.2.2`.

---

## 3. تعارضات الإصدارات غير السليمة (Version Conflicts & Anomalies)

### أ) شذوذ حزمة `Serilog.AspNetCore 10.0.0` في WebAPI:
- مشروع `Khadamat.WebAPI` يستهدف `.NET 8.0`.
- تم تثبيت حزمة `Serilog.AspNetCore` بالإصدار `10.0.0` (إصدار مستقبلي تجريبي يتبع .NET 10 Preview).
- نتج عن ذلك سحب حزم ثنائية انتقالية مبنية لـ .NET 10 مثل `System.Security.Cryptography.Pkcs 10.0.0`.
- **التوصية:** خفض الإصدار (Downgrade) فوراً إلى الإصدار المستقر المخصص لـ .NET 8 وهو `Serilog.AspNetCore 8.0.3`.

### ب) تعارض حزمة `CommunityToolkit.Maui` في MobileApp:
- مشروع `Khadamat.MobileApp` يستهدف لنظام أندرويد: `net9.0-android`.
- الحزمة المثبتة هي `CommunityToolkit.Maui 8.0.1` (مخصصة ومترجمة لـ .NET 8).
- حزم `CommunityToolkit.Maui` المتوافقة مع .NET 9 هي إصدارات `9.x.x` (مثل `9.1.0` أو `9.1.1`).
- الاستمرار بهذا التضارب يؤدي إلى تحذيرات تجميعية وتعارضات في الـ Bindings الخاصة بـ Android Native Libraries.
- **التوصية:** ترقية الحزمة إلى `9.1.0` لمطابقة .NET 9 أو توحيد أهداف التطبيق على .NET 8 LTS.

### ج) تضارب فئة `ArrayMapKt` وتعطيل الـ R8:
- وجود `Xamarin.AndroidX.Collection.Ktx 1.4.0.5` مع `Xamarin.Firebase.Messaging 123.4.0` أدى إلى خطأ:
  `Type androidx.collection.ArrayMapKt is defined multiple times`
- تم حل المشكلة سابقاً عبر تعطيل ضاغط الأكواد كلياً: `<UseR8>false</UseR8>`.
- **التقييم الفني:** تعطيل R8 يمثل ممارسة غير مقبولة لتطبيقات الإنتاج على Google Play لأنه يضاعف حجم ملف الـ AAB/APK ويمنع فحص الأكواد الميتة.
- **الحل الصحيح:** إما استبعاد المكتبة الزائدة `Xamarin.AndroidX.Collection.Ktx` (لأن Firebase Messaging تسحب داخلياً ما تحتاجه)، أو استخدام أحدث إصدارات AndroidX المتوافقة مع .NET 9.

---

## 4. تقييم توافق MediatR و AutoMapper و FluentValidation

1. **MediatR (12.4.1):** يعمل بكفاءة تامة ومتوافق مع معمارية CQRS المطبقة في مجلدات `Features/` بمشروع `Khadamat.Application`. لا يمثل أي عائق.
2. **FluentValidation (11.11.0):** متوافق بنسبة 100% مع أنظمة حقن التبعيات (DI) وخطوط معالجة MediatR (Pipeline Behaviors).
3. **AutoMapper (13.0.1):** من ناحية التوافق الوظيفي يعمل بسلاسة، ولكن يلزم معالجة التحذير الأمني كما وُضح أعلاه.
