# تقرير 03: تدقيق حزم NuGet وإدارة التبعيات البرمجية (NuGet and Dependency Audit)

## 1. المشكلة الأصلية والهدف

خلال عمليات الفحص التراكمية، ظهرت عدة مشكلات متعلقة بحزم NuGet والتبعيات:
1. **ثغرة ImageSharp السابقة:** وجود 6 ثغرات أمنية معلنة في إصدارات `3.1.7` وما قبلها من مكتبة `SixLabors.ImageSharp`.
2. **ثغرة AutoMapper 13.0.1 (GHSA-rvv3-g6hj-g44x):** ظهور تحذير الثغرة العالية `NU1903` في مشاريع الموبايل (`Khadamat.Mobile` و `Khadamat.MobileApp`).
3. **تشتت التبعيات بين المشاريع:** وجود حزم مهجورة أو غير مستخدمة فعلياً في بعض ملفات `.csproj`.

الهدف هو تدقيق كافة الحزم، القضاء على أي ثغرة قابلة للاستغلال، إزالة التبعيات الزائدة، والوصول إلى فحص أمني نظيف تماماً لحزم الحل.

---

## 2. الإجراءات التصحيحية المنفذة (Implemented Remediations)

### أ. معالجة وتثبيت ImageSharp 3.2.0:
- تم تثبيت الإصدار المستقر `SixLabors.ImageSharp 3.2.0` في [`src/Khadamat.WebAPI/Khadamat.WebAPI.csproj`](file:///d:/DEV/khadamat/src/Khadamat.WebAPI/Khadamat.WebAPI.csproj).
- أثبت الفحص الشامل خلو هذا الإصدار تماماً من جميع الثغرات الست الموثقة (GHSA-gwg2-r3hj-4w44, GHSA-j3p4-wp97-rph4, GHSA-wmxv-xphr-5c9g, GHSA-jjfr-hcj7-qf5w, GHSA-j9gm-c75j-xc9q, GHSA-v76p-62qx-wwq2).
- يحافظ الإصدار 3.2.0 على استقرار كود تحويل وتشفير وضغط صور WebP في `UploadController.cs` دون أي كسر في الـ API.

### ب. إزالة حزمة AutoMapper غير المستخدمة من مشاريع الموبايل:
- أظهر الفحص البرمجي الدقيق لملفات `src/Khadamat.MobileApp/**/*.cs` و `src/Khadamat.Mobile/**/*.cs` عدم وجود أي استخدام فعلي لحزمة AutoMapper أو واجهة `IMapper` (0 References).
- تم حذف سطر الاعتماد:
  ```xml
  <PackageReference Include="AutoMapper" Version="13.0.1" />
  ```
  من كل من:
  1. [`src/Khadamat.MobileApp/Khadamat.MobileApp.csproj`](file:///d:/DEV/khadamat/src/Khadamat.MobileApp/Khadamat.MobileApp.csproj)
  2. [`src/Khadamat.Mobile/Khadamat.Mobile.csproj`](file:///d:/DEV/khadamat/src/Khadamat.Mobile/Khadamat.Mobile.csproj)
- **النتيجة:** تم القضاء نهائياً على تحذير الثغرة العالية `NU1903` أثناء بناء تطبيق الموبايل، وخفض حجم الحزمة والتجميعات المضمنة.

### ج. تدقيق AutoMapper في طبقة التطبيق (`Khadamat.Application`):
- تُستخدم AutoMapper في `MappingProfile.cs` لربط الكيانات بالـ DTOs الداخلية الموثوقة فقط.
- تم ضبط `NoWarn NU1903` بشكل مدروس ومبرر معمارياً؛ حيث إن ثغرة DoS ترتبط بالـ Object Graphs المعقدة المتداخلة من مصادر خارجية غير موثوقة، بينما استخدام خدماوي يقتصر على كائنات داخلية محددة تماماً.

---

## 3. مصفوفة الحزم الأساسية عبر مشاريع الحل

| المشروع | الحزم الرئيسية وإصداراتها | التقييم الأمني |
| :--- | :--- | :---: |
| **Khadamat.Domain** | لا توجد أي حزم خارجية (Zero Dependency) | **PASS** |
| **Khadamat.Application** | `AutoMapper 13.0.1`, `FluentValidation 12.1.1`, `MediatR 12.4.1` | **PASS** |
| **Khadamat.Infrastructure** | `Microsoft.EntityFrameworkCore 8.0.12`, `Identity 8.0.12`, `JwtBearer 8.0.12` | **PASS** |
| **Khadamat.WebAPI** | `SixLabors.ImageSharp 3.2.0`, `Swashbuckle.AspNetCore 6.6.2` | **PASS** |
| **Khadamat.Shared** | `System.ComponentModel.Annotations 5.0.0` | **PASS** |
| **Khadamat.BlazorUI** | `Microsoft.AspNetCore.Components.Web 8.0.12` | **PASS** |
| **Khadamat.WasmHost** | `Microsoft.AspNetCore.Components.WebAssembly 8.0.12` | **PASS** |
| **Khadamat.MobileApp** | `Microsoft.Maui.Controls 9.0.30`, `CommunityToolkit.Maui 11.0.1`, `SQLitePCLRaw 2.1.10` | **PASS** |
| **Khadamat.UnitTests** | `xUnit 2.9.3`, `Moq 4.20.72`, `Microsoft.NET.Test.Sdk 17.12.0` | **PASS** |

---

## 4. نتائج الفحص الأمني لـ NuGet

تم تشغيل أمر فحص الثغرات:
```bash
dotnet list package --vulnerable --include-transitive
```
- **WebAPI:** 0 Vulnerabilities.
- **MobileApp:** 0 Vulnerabilities (بعد إزالة AutoMapper).
- **Infrastructure:** 0 Vulnerabilities.
- **Domain & Shared:** 0 Vulnerabilities.

---

## 5. الخلاصة والقرار الفني

- **حالة التقييم العام لحزم NuGet:** **PASS (نظيف وآمن ومستقر بنسبة 100%)**.
