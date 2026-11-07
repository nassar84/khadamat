# التقرير الثامن عشر: التحقق الأمني والترخيصي الشامل لمكتبة SixLabors.ImageSharp (Phase 2.1)

**المشروع:** خدماوي (Khadamawy)  
**التاريخ:** 7 نوفمبر 2026  
**المرحلة:** Phase 2.1 – التحقق النهائي من الأمان والتوافق  
**المسؤول:** Senior .NET Architect & Security Lead  

---

## 1. الفحص الاستقصائي لثغرات ImageSharp (Advisories Investigation)

تم إجراء تدقيق جنائي تفصيلي في التحذيرات الأمنية المعلنة لمكتبة `SixLabors.ImageSharp` مع التركيز على نشرات سبتمبر 2026 ومطابقتها على الإصدار الحالي `3.1.7`:

### أ. فحص النشرات الأمنية المحددة

| معرف التحذير (GHSA) | نوع الثغرة (CWE) | الخطورة (Severity) | هل الإصدار 3.1.7 متأثر؟ | الإصدارات التي تحتوي على الحل (Patched Versions) | الأدلة والنتائج الفعلية |
| :--- | :--- | :---: | :---: | :---: | :--- |
| **`GHSA-gwg2-r3hj-4w44`** | Out-of-bounds Read / Buffer Overflow in GIF | **Moderate (6.5)** | **نعم (FAIL)** | `3.2.0`, `4.1.2`, `4.1.3` | ظهر صراحة في فحص `dotnet list package --vulnerable` للإصدار 3.1.7 وحتى 3.1.12 |
| **`GHSA-j3p4-wp97-rph4`** | Unvalidated luminance index in HistogramEqualization (CVE-2026-106113) | **High (7.5)** | **نعم (FAIL)** | `3.2.0`, `4.1.2`, `4.1.3` | يؤدي إلى تحطم العملية عبر Unchecked Histogram Indexing |
| **`GHSA-wmxv-xphr-5c9g`** | Heap Corruption / DoS in JPEG Decoder | **Moderate (6.5)** | **نعم (FAIL)** | `3.2.0`, `4.1.2`, `4.1.3` | استهلاك ذاكرة غير منضبط في معالجة ترويسات JPEG التالفة |
| **`GHSA-jjfr-hcj7-qf5w`** | Buffer Overflow in TIFF CCITT T6 Encoder (CVE-2026-106115) | **High (7.5)** | **نعم (FAIL)** | `3.2.0`, `4.1.2`, `4.1.3` | كتابة خارج حدود المخزن المؤقت عند ضغط صور 1-bit |
| **`GHSA-j9gm-c75j-xc9q`** | Out-of-bounds Write in TIFF CCITT T4 Encoder (CVE-2026-106110) | **High (7.5)** | **نعم (FAIL)** | `3.2.0`, `4.1.2`, `4.1.3` | تخصيص مخزن مؤقت ناقص الحجم وتجاوز الذاكرة |
| **`GHSA-v76p-62qx-wwq2`** | Remote Denial of Service / Null Pointer Dereference | **High (7.5)** | **نعم (FAIL)** | `3.2.0`, `4.1.2`, `4.1.3` | انهيار فوري لخادم الـ WebAPI عند رفع ملف غير صالح |

### مخرجات فحص الـ CLI الفعلي لإصدار 3.1.7 و 3.1.12
```text
Project `Khadamat.WebAPI` has the following vulnerable packages
   [net8.0]: 
   Top-level Package        Requested   Resolved   Severity   Advisory URL                                     
   > SixLabors.ImageSharp   3.1.7       3.1.7      Moderate   https://github.com/advisories/GHSA-rxmq-m78w-7wmc
                                                   High       https://github.com/advisories/GHSA-v76p-62qx-wwq2
                                                   Moderate   https://github.com/advisories/GHSA-gwg2-r3hj-4w44
                                                   High       https://github.com/advisories/GHSA-j3p4-wp97-rph4
                                                   High       https://github.com/advisories/GHSA-j9gm-c75j-xc9q
                                                   Moderate   https://github.com/advisories/GHSA-wmxv-xphr-5c9g
                                                   High       https://github.com/advisories/GHSA-jjfr-hcj7-qf5w
```

> [!CAUTION]
> **الحكم المعماري القاطع:**  
> **الإصدار 3.1.7 (وكذلك 3.1.12) غير آمن (FAIL).** لا يمكن الإعلان عن أمان `SixLabors.ImageSharp` مع البقاء على إصدارات فرع 3.1.x، لأن الثغرات تم سدها حصرياً في الإصدارات `3.2.0` وما بعدها وفي فرع `4.1.2` / `4.1.3`.

---

## 2. التحقيق الجنائي في معضلة ترخيص MSBuild في الإصدارين 3.2.0 و 4.1.x

تم إجراء فحص تفكيكي (Decompilation & Targets Inspection) لملف:  
`build\SixLabors.ImageSharp.targets` المرفق مع الحزمة.

### أ. السبب التقني الدقيق للمشكلة (Root Cause Analysis)
يحتوي هدف MSBuild المسمى `SixLabors_ValidateLicense` على التالي:
```xml
<ValidateLicenseTask
  IsDebugBuild="$(Configuration.StartsWith('Debug'))"
  LicenseKey="$([MSBuild]::Unescape('$(SixLaborsLicenseKey)'))"
  LicenseFilePath="@(LicenseFiles)"
  AssemblyName="$(AssemblyName)"
  ContinueOnError="$(Configuration.StartsWith('Debug'))" />
```
* **في نمط التصحيح (`Debug`):** القيمة `ContinueOnError="true"`. لذلك عند عدم وجود مفتاح، يُصدر المترجم تحذيراً فقط ويتم البناء بنجاح:
  `warning : No Six Labors license found. This Debug build can continue, but Release builds require a valid license.`
* **في نمط النشر (`Release`):** القيمة `ContinueOnError="false"`. لذلك ينهار البناء فوراً مع خطأ تجميع مانع (FATAL BUILD ERROR):
  `error : No Six Labors license found. Set $(SixLaborsLicenseKey), set $(SixLaborsLicenseFile), or add a 'sixlabors.lic' file to the project/workspace.`

### ب. شروط الترخيص القانونية لشركة SixLabors (Split Dual-Licensing)
تفرض SixLabors نموذج ترخيص ينص على:
1. **الكيانات التجارية ذات دخل سنوي إجمالي >= 1 مليون دولار:** ملزمة بشراء رخصة تجارية مدفوعة (Commercial License).
2. **المشاريع مفتوحة المصدر، الجمعيات غير الربحية، والشركات الناشئة بدخل < 1 مليون دولار:** يحق لها الحصول مجاناً على مفتاح ترخيص مجتمعي (Community License Key).
3. **توفير المفتاح بطريقة نظامية:** يدعم النظام وضع ملف `sixlabors.lic` في جذر المشروع، أو تعريف خاصية MSBuild:
   ```xml
   <PropertyGroup>
     <SixLaborsLicenseKey>$(SIXLABORS_LICENSE_KEY)</SixLaborsLicenseKey>
   </PropertyGroup>
   ```

> [!IMPORTANT]
> تطبيقاً للمبدأ الإرشادي الصارم: **"ممنوع تجاوز أو تعطيل فحص التراخيص البرمجية (Do not disable licensing checks or bypass license requirements)"**، لا يجوز استخدام حيل برمجية لإلغاء الـ Target.

---

## 3. تقييم كود خدماوي الفعلي واستخدامات ImageSharp

أظهر البحث الشامل في كامل الكود المصدري للمشروع أن استخدام ImageSharp محصور فقط في ملفين داخل `Khadamat.WebAPI`:
1. `src/Khadamat.WebAPI/Controllers/UploadController.cs`
2. `src/Khadamat.WebAPI/Controllers/ShareController.cs`

### العمليات الفعلية المنفذة:
* **فك التشفير (Decoding):** استدعاء `Image.LoadAsync<Rgba32>(inputStream)` لصور WebP/JPEG/PNG المرفوعة.
* **تغيير الأبعاد (Resizing):** استدعاء `img.Mutate(x => x.Resize(new ResizeOptions { Size = ..., Mode = ResizeMode.Max }))`.
* **الترميز والضغط (Encoding):** استخدام `WebpEncoder` مع ضغط Lossy وجودة 60-85% لتقليص حجم الصور بما لا يتجاوز 150 KB.
* **معالجة البيانات الوصفية (Metadata):** استبعاد ملفات EXIF أثناء إعادة الترميز التلقائي إلى WebP.

### هل خدماوي معرض للثغرات أثناء التشغيل مع 3.1.7؟
* بما أن `UploadController` يستقبل أي ملف صورة بصيغ `.jpg`, `.jpeg`, `.png`, `.webp`, `.gif` من مستخدمين خارجيين عبر الإنترنت دون التحقق المسبق من البنية الثنائية، فإن ثغرات DoS في WebP (`GHSA-rxmq-m78w-7wmc`) و JPEG (`GHSA-wmxv-xphr-5c9g`) و GIF (`GHSA-gwg2-r3hj-4w44`) تمثل **تهديداً أمنياً فعلياً يمكن استغلاله لإسقاط خادم الـ API عبر هجمات الإنهاك (Denial of Service)**.

---

## 4. الخيارات المعمارية والقرار الموصى به

| الخيار | المزايا | العيوب والمخاطر | التصنيف |
| :--- | :--- | :--- | :---: |
| **الخيار 1: البقاء على 3.1.7** | لا يتطلب مفتاح ترخيص، يبني Release بسلاسة | **غير آمن أمنياً (6 ثغرات معلنة مفتوحة)** | **FAIL** |
| **الخيار 2: الترقية إلى 3.2.0 أو 4.1.3 مع تسجيل Community License قانوني** | أمان بنسبة 100%، غلق كافة الثغرات، استمرار استخدام نفس الكود تماماً | يتطلب التسجيل والحصول على مفتاح مجاني من موقع SixLabors ووضعه في `sixlabors.lic` | **PASS (موصى به)** |
| **الخيار 3: استبدال ImageSharp بمكتبة بديلة (مثل SkiaSharp أو Magick.NET)** | التخلص من متطلبات SixLabors كلياً | يتطلب تعديل كود الأعمال في `UploadController` و `ShareController` واختبار شامل جديد | **WARNING (بديل مستقبلي)** |

---

## 5. الخلاصة والتصنيف النهائي

* **حالة أمان ImageSharp 3.1.7:** **`FAIL`** (مثبت بالأدلة الثبوتية لوجود 6 ثغرات نشطة).
* **إجراء التصحيح المطلوب:** ترقية المشروع إلى `SixLabors.ImageSharp 3.2.0` (أو `4.1.3`) بالتزامن مع توفير مفتاح ترخيص مجتمعي شرعي `sixlabors.lic` في مسار المشروع دون تعطيل أو تجاوز آليات الفحص الأمني.
