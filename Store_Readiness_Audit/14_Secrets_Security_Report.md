# التقرير الرابع عشر: تدقيق الأسرار وحماية المفاتيح وخطة التدوير (Secrets & Keys Security Report)

**المشروع:** خدماوي (Khadamawy)  
**التاريخ:** 7 نوفمبر 2026  
**المرحلة:** المرحلة الثانية - أمن البيانات الحساسة والمفاتيح  
**إعداد:** Senior .NET Architect & Security Engineer  

---

## 1. فحص الملفات والإعدادات الحساسة

أجري تدقيق أمني عميق على إعدادات التكوين لجميع المشاريع، لا سيما:
- `src/Khadamat.WebAPI/appsettings.json`
- `src/Khadamat.WebAPI/appsettings.Development.json`
- `src/Khadamat.WebAPI/appsettings.Secrets.json`
- `src/Khadamat.MobileApp/Resources/Raw/` (Firebase Google Services)
- ملفات الـ Repository وسجل Git التاريخي.

### نتائج الفحص
1. **تسرب نصوص الاتصال بقاعدة البيانات في `appsettings.json` المتابع بـ Git:**
   - احتوى الملف على نص اتصال صريح بقاعدة بيانات الإنتاج السحابية المستضافة على مزود استضافة خارجي، متضمناً اسم الخادم واسم المستخدم وكلمة مرور حقيقية.
2. **تسرب مفتاح التوقيع الخاص بـ JWT (JWT Secret Key):**
   - احتوى الملف على مفتاح افتراضي مكشوف في السورس كود.
3. **وجود ملف `appsettings.Secrets.json`:**
   - تم التحقق من وجوده محلياً في خادم التطوير واستبعاده بالكامل في ملف `.gitignore`.
   - آلية استدعائه البرمجية مفعلة في `Program.cs` عبر:
     ```csharp
     builder.Configuration.AddJsonFile("appsettings.Secrets.json", optional: true, reloadOnChange: true);
     ```

---

## 2. الإجراءات التصحيحية الفورية المنفذة

### أ. تنظيف وتطهير `appsettings.json`
تم استبدال كافة البيانات السرية والحرجة داخل `src/Khadamat.WebAPI/appsettings.json` بقيم نائبة آمنة ومعيارية (Sanitized Placeholders):

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=YOUR_SERVER_HOST;Database=YOUR_DATABASE_NAME;User Id=YOUR_DB_USER;Password=YOUR_DB_PASSWORD;Encrypt=True;TrustServerCertificate=True;"
  },
  "JwtSettings": {
    "Secret": "REPLACE_WITH_A_SECURE_RANDOM_KEY_AT_LEAST_32_CHARACTERS",
    "Issuer": "KhadamatApp",
    "Audience": "KhadamatUsers",
    "ExpiryMinutes": 43200
  }
}
```

### ب. الحفاظ على استمرارية بيئة التطوير والتشغيل
- تم إبقاء القيم الحقيقية المشفرة داخل الملف المحلي المحمي `appsettings.Secrets.json` فقط، والذي لا يتم رفعه إلى Git مطلقاً.
- يستطيع أي مطور تشغيل الـ WebAPI محلياً بدون أي توقف، بينما الكود المرفوع على GitHub ومستودعات التخزين أصبح نظيفاً تماماً وخالياً من أي أسرار.

---

## 3. تحليل سجل Git (Git History Analysis)

> [!WARNING]
> بمجرد رفع مفتاح أو كلمة مرور إلى Git في أي Commit سابق، يُعتبر هذا السر **مكشوفاً ومخترقاً حكماً (Compromised by Definition)** حتى لو تم حذفه في الـ Commit التالي، لأن سجل الـ Commits يحتفظ بنسخ كاملة من الملفات السابقة.

تم رصد إدراج كلمات المرور وسلاسل الاتصال في الـ Commits التاريخية لمشروع خدماوي قبل تطبيق هذا الإجراء الأمني.

---

## 4. خطة تدوير المفاتيح الإلزامية (Secrets Rotation Action Plan)

قبل إطلاق التطبيق رسمياً في الإنتاج أو المتاجر التجارية، يجب اتباع خطوات التدوير التالية:

### الخطوة الأولى: تدوير كلمة مرور قاعدة البيانات (Database Credential Rotation)
1. الدخول إلى لوحة تحكم استضافة قاعدة البيانات (Site4Now Control Panel).
2. تغيير كلمة مرور المستخدم `db_abe0b0_khadamatdb_admin` فوراً إلى كلمة مرور عشوائية معقدة مكونة من 32 حرفاً ورمزاً على الأقل.
3. تحديث سلسلة الاتصال الجديدة في ملف `appsettings.Secrets.json` على الخادم الحقيقي والبيئة المحلية، أو حقنها عبر متغيرات البيئة السحابية `ConnectionStrings__DefaultConnection`.

### الخطوة الثانية: تدوير مفتاح توقيع JWT (JWT Signing Key Rotation)
1. توليد مفتاح تشفير عشوائي آمن (Cryptographically Secure Random Key 256-bit):
   - مثال: مفتاح بطول 64 حرفاً يحتوي على أحرف كبيرة وصغيرة وأرقام ورموز.
2. وضعه في `appsettings.Secrets.json` أو متغيرات البيئة `JwtSettings__Secret`.
3. *ملاحظة:* تدوير هذا المفتاح سيؤدي تلقائياً إلى تسجيل خروج كافة المستخدمين النشطين (وهذا أمر صحي وأمني مطلوب قبل الإطلاق الرسمي).

### الخطوة الثالثة: تدوير وتأمين مفاتيح OAuth (Google & Facebook)
1. في Google Cloud Console:
   - مراجعة قيود النطاقات المسموح لها (Authorized JavaScript origins & Redirect URIs).
   - قصر التوجيه فقط على النطاقات الرسمية المعتمدة: `https://khadamawy.eis-dev.com` و `https://www.khadamawy.com`.
2. في Meta for Developers:
   - مراجعة App Secret والتأكد من عدم استدعائه في كود الـ Client-Side.

### الخطوة الرابعة: بيانات خادم البريد (SMTP Credentials)
1. التأكد من استخدام "App Passwords" الخاصة بحساب Google وليس كلمة مرور الحساب الأصلية.
2. حصر كلمة مرور التطبيق داخل `appsettings.Secrets.json` أو متغيرات بيئة السيرفر.

### الخطوة الخامسة: تنظيف مستودع Git (Git Purge via BFG or git-filter-repo)
- بعد إتمام تدوير كافة المفاتيح وكلمات المرور في الأنظمة الحية، يمكن تشغيل أداة مثل `git-filter-repo` لمسح الملفات الحساسة من سجل Git القديم نهائياً، ثم عمل `git push --force` إذا تطلب الأمر لتطهير المستودع.

---

## 5. الخلاصة
تمت إزالة جميع الأسرار المكشوفة من الكود المصدري لملفات التكوين الموجهة للنسخ، وحصرها داخل الآلية الآمنة `appsettings.Secrets.json` المحمية بـ `.gitignore`، مع توفير خطة تدوير شاملة تضمن الأمان المؤسسي للمشروع.
