# تقرير 05: تدقيق الأمان والخصوصية وسياسات حماية البيانات (Security & Privacy)

**تاريخ التدقيق:** 7 نوفمبر 2026  
**المراجع:** Senior Security & Cloud Solutions Architect  
**النطاق:** متطلبات Google Play Data Safety ومتطلبات Apple App Privacy

---

## 1. فحص حذف الحساب (Account Deletion Policy - Apple & Google)

تفرض كل من آبل (Apple Guideline 5.1.1(v)) وجوجل (Play Console Account Deletion Requirement) شرطاً صارماً: **أي تطبيق يتيح إنشاء حساب يجب أن يوفر مساراً واضحاً وسهلاً للمستخدم لحذف حسابه وكافة بياناته الشخصية من داخل التطبيق ومن خلال رابط ويب خارجي.**

### نتيجة التدقيق في مشروع خدماوي:
✅ **متوافق ومكتمل برمجياً (Fully Compliant):**
1. **الواجهة الخلفية (Backend API):**
   - يتوفر مسار صريح: `DELETE /v1/auth/delete-account` في ملف `AuthController.cs` (السطر 178).
   - يتحقق من هوية المستخدم الحالية عبر `User.FindFirst(ClaimTypes.NameIdentifier)` ويطلب تأكيد كلمة المرور لمنع الحذف العرضي أو الاحتيالي:
     ```csharp
     [HttpDelete("delete-account")]
     [Authorize]
     public async Task<IActionResult> DeleteAccount([FromBody] DeleteAccountRequest request)
     {
         var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
         var result = await _authService.DeleteAccountAsync(userId, request.Password);
         ...
     }
     ```
2. **واجهة المستخدم (UI Page):**
   - تتوفر صفحة مخصصة `DeleteAccount.razor` عبر المسار `@page "/delete-account"`.
   - توفر تحذيراً للمستخدم وتطلب إدخال كلمة المرور مع زر تأكيد الحذف النهائي.
   - تظهر روابط الوصول لصفحة الحذف داخل الملف الشخصي `Profile.razor` وشريط التذييل `Footer.razor`.
3. **متطلب رابط الويب الخارجي (Web URL):**
   - نظراً لأن واجهة Blazor متاحة عبر الويب أيضاً (`Khadamat.WasmHost`)، يمكن تقديم رابط الحذف الرسمي في استمارة المتجر:
     `https://khadamawy.com/delete-account`

---

## 2. إدارة البيانات الحساسة وأسرار التكوين (Secrets & Credentials)

### ⚠️ ملاحظة حرجة: بيانات الاتصال وقفل JWT في `appsettings.json`
عند فحص ملف `src/Khadamat.WebAPI/appsettings.json`، تبين وجود القيم التالية بنص صريح:
- **سلسلة الاتصال بقاعدة البيانات (Database Connection String):**
  `User Id=db_abe0b0_khadamatdb_admin;Password=Secure#OS2025;Server=...`
- **مفتاح تشفير توكنات JWT:**
  `"Secret": "Khadamat_Super_Secret_Key_For_Jwt_Authentication_2025_Very_Long_String_32bytes_Minimum"`

### التوصية والإجراء التصحيحي:
1. عدم رفع ملفات تحتوي على كلمات مرور إنتاجية إلى مستودع الأكواد العام (Git Repository).
2. في بيئة الإنتاج الفعلي (IIS / Azure / Linux VPS)، يجب تمرير بيانات الاتصال ومفتاح الـ JWT عبر **متغيرات البيئة (Environment Variables)** أو **Azure Key Vault** أو `appsettings.Production.json` المحمي بملف `.gitignore`.
3. تدوير كلمة مرور قاعدة البيانات (Password Rotation) وتغيير مفتاح JWT قبل الإطلاق الرسمي للعامة.

---

## 3. تدقيق تخزين الرموز والبيانات الحساسة في تطبيق الهاتف (Token Storage)

- يعتمد تطبيق الموبايل الهجين على تخزين رمز الدخول `JWT Access Token` عبر `LocalStorage` داخل سياق `BlazorWebView`.
- **التقييم:**
  - بالنسبة لتطبيقات الويب الهجينة، يعد الوصول معزولاً داخل ملفات تعريف الحاوية الأصلية لكل تطبيق (Sandbox Container Storage).
  - للوصول إلى أعلى معايير الأمان الموصى بها في متجر Apple (Keychain) وأندرويد (EncryptedSharedPreferences)، يُفضل ربط جلسة الـ Native Shell عبر واجهة `ISecureStorage` من مكتبة `.NET MAUI` لحفظ الـ Refresh Token والـ Access Token، وتمريرهما للويب عبر واجهة JavaScript Interop المشفرة.

---

## 4. إعدادات استمارة أمان البيانات (Data Safety Form & App Privacy)

عند تعبئة استمارة **Data Safety** في Google Play Console واستمارة **App Privacy** في Apple App Store Connect، يجب الإفصاح عن البيانات التالية استناداً إلى كود المنظومة:

| نوع البيانات المجمعة | الغرض من الجمع | هل تشارك مع أطراف ثالثة؟ | هل هي مشفرة أثناء النقل؟ |
| :--- | :--- | :--- | :--- |
| **الاسم، رقم الهاتف، البريد الإلكتروني** | إدارة الحساب، تسجيل الدخول، التواصل بين العميل ومقدم الخدمة | لا، تستخدم داخل المنظومة فقط | نعم (عبر HTTPS) |
| **الصور والملفات** | صور البطاقة الشخصية لمقدمي الخدمات، شهادات الخبرة، صور الخدمات المعروضة | لا، تخزن في الخادم الداخلي | نعم (عبر HTTPS) |
| **الموقع التقريبي / المدينة** | عرض الخدمات القريبة ومطابقة طلبات الصيانة | لا | نعم (عبر HTTPS) |
| **معرّف الجهاز ورمز الإشعار (FCM Device Token)** | إرسال الإشعارات وتنبيهات الطلبات الجديدة | نعم، يشارك مع Google Firebase | نعم (مشفر عبر بروتوكولات Google) |

---

## 5. مراجعة سياسة الخصوصية والشروط والأحكام

- **رابط سياسة الخصوصية:** موجود داخل شريط الروابط بالتذييل: `/privacy` ومربوط بصفحة تشرح بنود جمع البيانات واستخدامها.
- **التوصية للمتجر:** تجهيز صفحة ويب مستضافة ومتاحة بشكل دائم برابط مباشر:
  `https://khadamawy.com/privacy`
  حيث ترفض متاجر التطبيقات الروابط التي تتطلب تسجيل دخول أو التي تعيد خطأ 404.
