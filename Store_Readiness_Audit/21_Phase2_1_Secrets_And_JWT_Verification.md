# التقرير الحادي والعشرون: التحقق الأمني من الأسرار ومفاتيح JWT وبيئة الإنتاج (Phase 2.1)

**المشروع:** خدماوي (Khadamawy)  
**التاريخ:** 7 نوفمبر 2026  
**المرحلة:** Phase 2.1 – التحقق النهائي من الأمان  
**المسؤول:** Senior Application Security Architect  

---

## 1. فحص وتدقيق منظومة تكوين الأسرار (Configuration Security Audit)

تم إجراء تدقيق معماري على ملفات التكوين، شفرة البدء، وتكامل الأسرار:

### أ. التحقق من تحميل `appsettings.Secrets.json` أثناء التطوير
* **الكود المصدري في `src/Khadamat.WebAPI/Program.cs`:**
  ```csharp
  var builder = WebApplication.CreateBuilder(args);
  builder.Configuration.AddJsonFile("appsettings.Secrets.json", optional: true, reloadOnChange: true);
  ```
* **التحقق من ملف `.gitignore`:**
  - تم فحص `.gitignore` في جذر المستودع وتأكيد السطر 28:
    ```gitignore
    appsettings.Secrets.json
    ```
* **النتيجة:**
  - يتم استدعاء الملف بشكل صريح كملف اختياري محلي. في بيئة التطوير، يتواجد الملف محلياً ولا يتم تضمينه أو تتبعه داخل Git.
* **التصنيف:** **`PASS`**

---

### ب. فحص بقاء أي أسرار في الملفات المتتبعة بـ Git (Tracked Files Scan)
* **الفحص:**
  - تم فحص `src/Khadamat.WebAPI/appsettings.json` و `appsettings.Development.json`.
  - تم التأكد من تطهير كافة كلمات المرور الحقيقية ونصوص الاتصال ومفاتيح JWT واستبدالها بنصوص نائبة معيارية (Sanitized Placeholders):
    `Server=YOUR_SERVER_HOST;Database=YOUR_DATABASE_NAME;User Id=YOUR_DB_USER;Password=YOUR_DB_PASSWORD;...`
    `"Secret": "REPLACE_WITH_A_SECURE_RANDOM_KEY_AT_LEAST_32_CHARACTERS"`
* **التصنيف:** **`PASS`**

---

### ج. فحص سجل Git التاريخي (Git History Exposure)
* **التحليل الجنائي:**
  - على الرغم من تنظيف الملف في التعديل الأخير، فإن سجل الـ Commits التاريخي القديم للمستودع لا يزال يحتفظ بالنسخ السابقة التي رُفعت فيها بيانات الاتصال والمفاتيح في الماضي.
  - تعتبر تلك القيم القديمة **مكشوفة حكماً وتستوجب التدوير الفوري** على خوادم الإنتاج.
* **التصنيف:** **`WARNING`** (يتطلب تنفيذ خطة التدوير المستقلة).

---

### د. تكوين بيئة الإنتاج واستخدام متغيرات البيئة (Production Environment Config)
* **آلية التكوين المعمارية الموصى بها في بيئة الإنتاج:**
  - في بيئات الإنتاج (سواء كانت IIS، Windows Service، Docker، أو Azure App Services)، يجب الاعتماد على **Environment Variables** أو **Azure Key Vault / Secret Manager** لتجاوز ملفات الـ JSON نهائياً، وفق التسميات المعيارية لـ ASP.NET Core:
    - `ConnectionStrings__DefaultConnection`
    - `JwtSettings__Secret`
    - `Authentication__Google__ClientSecret`
    - `Authentication__Facebook__AppSecret`
    - `SmtpSettings__Password`
* **التحقق:**
  - آلية `WebApplication.CreateBuilder(args)` الافتراضية في .NET تدعم قراءة متغيرات البيئة تلقائياً وتقديمها بأعلى أولوية تحميل متجاوزة ملفات `appsettings.json`.
* **التصنيف:** **`PASS`**

---

## 2. التحليل المعماري لتدوير مفاتيح JWT دون قطع الجلسات (Zero-Downtime JWT Rotation)

### أ. فحص الوضع البرمجي الحالي (Current Single-Key Architecture)
* **الكود الحالي في `src/Khadamat.Infrastructure/DependencyInjection.cs`:**
  ```csharp
  .AddJwtBearer(options =>
  {
      options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
      {
          ValidateIssuer = true,
          ValidateAudience = true,
          ValidateLifetime = true,
          ValidateIssuerSigningKey = true,
          ValidIssuer = configuration["JwtSettings:Issuer"],
          ValidAudience = configuration["JwtSettings:Audience"],
          IssuerSigningKey = new SymmetricSecurityKey(
              Encoding.UTF8.GetBytes(configuration["JwtSettings:Secret"]!))
      };
  })
  ```
* **المشكلة المعمارية:**
  - يتم التحقق حصرياً عبر مفتاح مفرد (`IssuerSigningKey`).
  - في حال تم تغيير مفتاح `JwtSettings:Secret` على خادم الإنتاج فجأة، فإن **جميع الرموز المميزة (Tokens) الصالحة التي يحملها آلاف المستخدمين النشطين على الموبايل والويب ستفشل في التحقق فوراً (Signature Verification Failed)**.
  - سيؤدي ذلك إلى تسجيل خروج جماعي مفاجئ للمستخدمين وظهور أخطاء `401 Unauthorized` دون سابق إنذار.

### ب. الحل المعماري المعتمد لتدوير المفاتيح بسلاسة (Smooth Key Rotation Pattern)
لتدوير المفتاح دون التسبب في انهيار جلسات المستخدمين:
1. **استخدام `IssuerSigningKeys` (بصيغة الجمع):**
   تعديل إعدادات التحقق لدعم مفتاحين في نفس الوقت:
   - **المفتاح النشط الجديد (Active Signing Key):** يُستخدم لإنشاء وتوقيع الـ Tokens الجديدة عند تسجيل الدخول أو التجديد.
   - **المفتاح السابق المتقاعد (Retiring Key):** يُقبل حصرياً في التحقق من صحة توقيع الـ Tokens القديمة التي لم تنته صلاحيتها بعد.
2. **بروتوكول التدوير التدريجي (Phased Rotation Schedule):**
   - *اليوم 0:* إضافة المفتاح الجديد في مصفوفة المفاتيح المقبولة للتحقق والبدء في توقيع التوكنات الجديدة به.
   - *فترة السماح (Grace Period - 30 يوماً):* يستمر قبول التوكنات القديمة الموقعة بالمفتاح السابق.
   - *اليوم 31:* إزالة المفتاح القديم نهائياً بعد انتهاء صلاحية كافة التوكنات الصادرة سابقاً.

* **التصنيف:** **`WARNING`** (الكود الحالي يدعم مفتاحاً واحداً فقط؛ تغيير المفتاح سيؤدي إلى تسجيل خروج جماعي ما لم يتم إدراج نمط المفاتيح المتعددة).

---

## 3. مصفوفة التحقق والتصنيف النهائي لفقرات الأسرار و JWT

| بند التحقق | الوصف الفني | النتيجة | التصنيف |
| :--- | :--- | :---: | :---: |
| **تحميل `appsettings.Secrets.json`** | استدعاء صريح في Program.cs ومحمي بـ .gitignore | يعمل بسلاسة | **`PASS`** |
| **تطهير الأسرار في السورس كود** | استبدال كلمات المرور في appsettings.json بـ Placeholders | تم التطهير | **`PASS`** |
| **جاهزية الإنتاج لمتغيرات البيئة** | دعم حقن التكوين عبر Server Environment Variables | مدعوم معمارياً | **`PASS`** |
| **أمان سجل Git القديم** | تسرب بيانات اتصال قديمة في تاريخ الـ Commits القديم | مكشوف قديماً | **`WARNING`** |
| **تدوير JWT بدون انقطاع الجلسات** | دعم تدوير المفتاح بسلاسة دون طرد المستخدمين | يتطلب تعدد المفاتيح | **`WARNING`** |
| **حماية أسرار الإنتاج الحية** | عدم طباعة أي قيم حقيقية وعدم تدوير الأسرار أثناء التدقيق | تم الامتثال التام | **`PASS`** |
