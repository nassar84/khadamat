# تقرير 29: الخطة التنفيذية المعمارية للترقية إلى .NET 10 LTS (Phase 2.2)

## 1. ملخص تنفيذي (Executive Summary)

تم إعداد هذه الخطة المعمارية التنفيذية المفصلة للانتقال المنسق لمنظومة "خدماوي" بمشاريعها العشرة إلى إطار العمل الموحد **.NET 10 LTS (Long Term Support)**.

تعد هذه الخطة مرجعاً هندسياً مستقلاً يوضح خطوات الترقية، الحزم المستهدفة، متطلبات بيئة البناء والتطوير، استراتيجيات إدارة المخاطر، وخطة التراجع (Rollback Strategy).

> [!NOTE]
> **نطاق الالتزام في هذه المرحلة:**
> تم وضع وتوثيق هذه الخطة الهندسية بالكامل دون تعديل الأطر المستهدفة الفعلية (`TargetFramework`) للمشاريع في المرحلة الحالية، وذلك لضمان استقرار حزم وبيئات البناء الحالية حتى اكتمال كافة مراحل التدقيق والاعتماد.

| قطاع الحل | المشاريع المشمولة | الإطار الحالي | الإطار المستهدف في .NET 10 | درجة التعقيد والمخاطر |
| :--- | :--- | :--- | :--- | :--- |
| **Backend & Core** | Domain, Application, Infrastructure, WebAPI | `net8.0` | `net10.0` | منخفضة (Low) |
| **Shared Libs** | Khadamat.Shared | `net8.0` | `net10.0` | منخفضة جداً (Minimal) |
| **Blazor Web** | Khadamat.BlazorUI, Khadamat.WasmHost | `net8.0` | `net10.0` | متوسطة (Moderate) |
| **MAUI Mobile** | Khadamat.MobileApp, Khadamat.Mobile | `net9.0-android`, `net9.0-ios` | `net10.0-android`, `net10.0-ios` | متوسطة إلى مرتفعة (Workloads) |
| **Quality & Tests** | Khadamat.UnitTests | `net8.0` | `net10.0` | منخفضة (Low) |

---

## 2. متطلبات بيئة البناء والتطوير المسبقة (Prerequisites)

قبل بدء الترقية الفعلية إلى .NET 10، يجب التأكد من توفر الأدوات التالية على خوادم البناء وأجهزة المطورين:
1. **.NET 10 SDK:** تثبيت الإصدار المستقر `10.0.100` أو أعلى.
2. **أدوات عمل الموبايل (Workloads):**
   ```bash
   dotnet workload install maui-android maui-ios maui wasm-tools
   ```
3. **Android SDK:**
   - تثبيت منصة `platforms;android-36` و `build-tools;36.0.0` أو أعلى.
   - التحقق من توافق `Microsoft.Android.Sdk` مع API 36 دون إطلاق تحذير `XA1008`.
4. **Apple Developer Environment (iOS):**
   - تثبيت Xcode 26 مع iOS 26 SDK على أجهزة macOS المخصصة للبناء السحابي أو المحلي.

---

## 3. خطة الترقية المرحلية المنفصلة (Phased Migration Roadmap)

### المرحلة الأولى: ترقية النواة والطبقات المشتركة (Domain & Shared)
1. تعديل ملفات `.csproj` لكل من `Khadamat.Domain` و `Khadamat.Shared`:
   ```xml
   <TargetFramework>net10.0</TargetFramework>
   ```
2. التحقق من عدم وجود أي تبعيات قديمة أو اعتمادات غير متوافقة.
3. تجميع واختبار سلامة تجميع المكتبات المشتركة.

### المرحلة الثانية: ترقية طبقات التطبيق والبنية التحتية والـ Backend
1. تعديل `Khadamat.Application`:
   - ترقية `MediatR` إلى الإصدار المتوافق مع .NET 10.
   - ترقية `FluentValidation` إلى أحدث إصدار مستقر.
2. تعديل `Khadamat.Infrastructure`:
   - ترقية كافة حزم `Microsoft.EntityFrameworkCore.*` إلى `10.0.x`.
   - ترقية `Microsoft.AspNetCore.Identity.EntityFrameworkCore` إلى `10.0.x`.
   - التحقق من تكامل `JwtKeyManager` مع مكتبات `Microsoft.IdentityModel.Tokens` المحدثة.
3. تعديل `Khadamat.WebAPI`:
   - ترقية حزم ASP.NET Core إلى `10.0.x`.
   - فحص توافق `Swashbuckle.AspNetCore` أو الانتقال إلى `Microsoft.AspNetCore.OpenApi` الأصلي المدمج في .NET 10.
   - التحقق من تشغيل وعمل `SixLabors.ImageSharp 3.2.0` أو تقييم الانتقال لـ `4.x` بعد استقرار واجهات الـ WebP.
4. تعديل وتشغيل اختبارات الوحدة `Khadamat.UnitTests` للتأكد من نجاح كافة الاختبارات الـ 53 تحت بيئة .NET 10.

### المرحلة الثالثة: ترقية واجهات الويب (Blazor Components & WebAssembly)
1. تعديل `Khadamat.BlazorUI` و `Khadamat.WasmHost` إلى `net10.0`.
2. تحديث حزم `Microsoft.AspNetCore.Components.WebAssembly` إلى `10.0.x`.
3. التحقق من توافق مكتبة الرسوميات وعناصر واجهة المستخدم.
4. اختبار عمليات النشر الثنائية (AOT Compilation لـ WebAssembly).

### المرحلة الرابعة: ترقية تطبيق الموبايل (.NET MAUI 10)
1. تعديل `Khadamat.MobileApp.csproj`:
   ```xml
   <TargetFrameworks>net10.0-android;net10.0-ios</TargetFrameworks>
   ```
2. ترقية حزم `Microsoft.Maui.Controls` و `Microsoft.Maui.Controls.Compatibility` إلى إصدار `10.0.x`.
3. ترقية `CommunityToolkit.Maui` و `CommunityToolkit.Mvvm` إلى الإصدارات المتوافقة مع MAUI 10.
4. التحقق من حزمة `SQLitePCLRaw.bundle_green` وتوافق `libe_sqlite3.so` مع محاذاة 16 KB في بيئة .NET 10.
5. بناء واختبار تطبيق أندرويد Release AAB والتأكد من نجاح التوقيع ومحاذاة الـ ZIP عبر `zipalign`.
6. بناء واختبار تطبيق iOS Release IPA والتأكد من مطابقة `Entitlements.plist` وشهادات Apple.

---

## 4. مصفوفة توافق الحزم والبدائل المعتمدة في .NET 10

| الحزمة الحالية | الإصدار الحالي | الإصدار المستهدف في .NET 10 | ملاحظات التوافق والمعالجة |
| :--- | :--- | :--- | :--- |
| **Microsoft.EntityFrameworkCore** | `8.0.12` | `10.0.0` | تحديث مباشر، لا توجد كسور رئيسية في الاستعلامات الأساسية |
| **MediatR** | `12.4.1` | `12.x / 13.x` | متوافقة تماماً مع حقن الاعتمادية في .NET 10 |
| **FluentValidation** | `11.11.0` | أحدث إصدار مستقر | متوافقة تماماً |
| **SixLabors.ImageSharp** | `3.2.0` | `3.2.0 / 4.x` | `3.2.0` تعمل على `net10.0` عبر `netstandard2.1` |
| **CommunityToolkit.Maui** | `11.0.1` | الإصدار المتوافق مع MAUI 10 | متابعة إطلاق نسخة MAUI 10 الرسمية |
| **SQLitePCLRaw.bundle_green** | `2.1.10` | `2.1.10+` | مدعومة بالكامل وتضمن محاذاة 16 KB |

---

## 5. خطة إدارة المخاطر والتراجع السريع (Rollback Strategy)

1. **العزل في فرع مستقل (Branch Isolation):**
   - يتم تنفيذ كامل خطوات الترقية داخل فرع منفصل مخصص باسم:
     `feature/dotnet10-lts-migration`
2. **بوابات الجودة (Quality Gates):**
   - لا يتم دمج الفرع في `main` إلا بعد:
     - نجاح البناء بنسبة 100% لكافة المشاريع العشرة (0 Errors).
     - اجتياز جميع اختبارات الوحدة وتوسيعها لتشمل كافة خدمات الـ API والمصادقة.
     - اجتياز فحص `bundletool validate` و `zipalign -c -P 16` بنجاح لحزمة أندرويد.
     - التحقق الفعلي من إقلاع وتشغيل التطبيق على محاكي Android 15/16 وجهاز iOS حقيقي.
3. **إجراءات التراجع الفوري:**
   - في حال حدوث أي عائق غير قابل للحل في حزم الموبايل (مثل تأخر توافق بعض مكاتب الطرف الثالث في MAUI 10)، يمكن إعادة الـ Backend والموبايل فوراً إلى خط الأساس المستقر (`net8.0` للـ Backend و `net9.0` للموبايل) عبر Git دون أي تأثير على بيانات الإنتاج.

---

## 6. الخلاصة والقرار الفني (Pass/Fail)

- **حالة خطة الانتقال إلى .NET 10:** **PASS (الخطة مكتملة ومعتمدة معمارياً)**
- **مستوى الجاهزية:** الحل جاهز للانتقال المنظم عند فتح نافذة الترقية المعتمدة من إدارة المشروع.
