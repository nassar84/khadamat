# تقرير 10: المراجعة التصحيحية للمرحلة الأولى واختبارات النشر الفعلية (Phase 1 Correction Report)

**تاريخ التنفيذ:** 7 نوفمبر 2026  
**المشرف المعماري:** Senior .NET Architect & Mobile Solutions Consultant  
**الحالة:** مراجعة تصحيحية واختبارات بناء وتجميع عملية (Empirical Verification)

---

## 1. فحص إمكانية استهداف Android Target API 36 والتوافقية

تم إجراء مسح فني عميق لبيئة التجميع ونظام Android SDK المثبت محلياً:

### أ) حالة أدوات Android SDK وبيئات العمل (.NET Workloads):
- **حزم Android SDK المنزلة:** تبين وجود مجلد المنصة الرسمية `android-36` في المسار:
  `C:\Program Files (x86)\Android\android-sdk\platforms\android-36` (إلى جانب إصدارات 34 و 35).
- **حزمة عمل .NET Android:** الإصدار المثبت هو `Microsoft.Android.Sdk.Windows 35.0.105` المخصص لـ .NET 9.

### ب) توافق Firebase و مكتبات AndroidX:
- الحزم المثبتة:
  - `Xamarin.Firebase.Messaging 123.4.1`
  - `Xamarin.AndroidX.Collection.Ktx 1.5.0.1`
- **النتيجة التجميعية:** اجتازت المكتبات مرحلة الترجمة (Compilation) ودمج الموارد (Resource Linking عبر AAPT2) دون أي تضارب في الفئات البرمجية (مثل تعارض `ArrayMapKt` السابق).

### ج) ضبط Target API على 36 في المشروع:
تم تعديل ملف [Khadamat.MobileApp.csproj](file:///d:/DEV/khadamat/src/Khadamat.MobileApp/Khadamat.MobileApp.csproj) وملف [AndroidManifest.xml](file:///d:/DEV/khadamat/src/Khadamat.MobileApp/Platforms/Android/AndroidManifest.xml):
- `AndroidTargetSdkVersion`: تم تعيينه رسمياً على **`36`** (Android 16).
- `uses-sdk android:targetSdkVersion`: تم تعيينه رسمياً على **`36`**.

### د) التحذيرات والملاحظات الفنية المكتشفة عند استهداف API 36 (Technical Blockers & Diagnostics):
أثناء التجميع بوضع Release استخرج مترجم مايكروسوفت لنظام أندرويد تحذيرين جوهريين:
1. **تحذير عدم تطابق إصدار الـ SDK الأدنى (XA1008):**
   `warning XA1008: The TargetFrameworkVersion (Android API level 35) is lower than the targetSdkVersion (36). Please increase the $(TargetFrameworkVersion) or decrease the android:targetSdkVersion in the AndroidManifest.xml so that the API levels match.`
   *التفسير:* حزمة عمل .NET 9 الحالية مربوطة افتراضياً بـ API 35 كأساس، واستهداف API 36 يعمل ولكنه يطلق هذا التنبيه ما لم يتم التحديث لإصدار Workload مخصص لـ Android 16 (مثل .NET 10 / Android 36 SDK).
2. **تحذير حجم صفحات الذاكرة لنظام أندرويد 16 (XA0141 - 16 KB Page Sizes):**
   `warning XA0141: Android 16 will require 16 KB page sizes, shared library 'libe_sqlite3.so' does not have a 16 KB page size. Please inform the authors of the NuGet package 'SQLitePCLRaw.lib.e_sqlite3.android' version '2.1.2'`
   *التفسير:* تشترط جوجل لأجهزة Android 16 أن تدعم المكتبات الأصلية المشتركة C/C++ محاذاة صفحات ذاكرة بحجم 16 كيلوبايت. مكتبة SQLite القديمة المستخدمة حالياً مبنية بصفحات 4 كيلوبايت، وسيتعين ترقيتها مستقبلاً قبل الإلزام النهائي لأجهزة 16KB.

---

## 2. نتائج بناء Android Release وإنتاج حزمة AAB الفعلية

تم تنفيذ أمر النشر الرسمي وتجميع كافة معماريات المعالجات:
`dotnet publish src\Khadamat.MobileApp\Khadamat.MobileApp.csproj -f net9.0-android -c Release -p:AndroidPackageFormat=aab`

### أ) نتيجة التجميع:
✅ **نجح البناء كاملاً (Build Succeeded: 0 Errors).**
- تمت معالجة Trimming وتقليص الحجم بواسطة `illink` لثلاث معماريات متوازية: `android-arm64` و `android-arm` و `android-x64`.
- تم تجميع AOT بنجاح لـ 134 مكتبة ثنائية (`.so`) أصلية لكل معمارية.

### ب) التحقق من وجود ملف الـ AAB وحجمه:
الملفات المنشورة في المجلد:
`src\Khadamat.MobileApp\bin\Release\net9.0-android\publish\`

| اسم الملف الناتج | الحجم الفعلي | تاريخ ووقت الإنشاء | الوصف |
| :--- | :--- | :--- | :--- |
| **`com.nassar84.khadamawy-Signed.aab`** | **54,734,082 بايت (52.2 MB)** | 11/7/2026 1:10:12 PM | حزمة **Android App Bundle** الموقعة والجاهزة للرفع على مسارات Google Play. |
| **`com.nassar84.khadamawy.aab`** | **54,524,300 بايت (52.0 MB)** | 11/7/2026 1:10:00 PM | حزمة AAB غير الموقعة لاستخدام أدوات التوقيع الخارجية. |
| **`khadamat.apk`** | **55,242,983 بايت (52.6 MB)** | 11/7/2026 1:10:21 PM | ملف APK موقع موازي للاختبار المباشر الداخلي. |

### ج) فحص توقيع الحزمة (Signing Inspection):
تم فحص البنية الداخلية لملف `com.nassar84.khadamawy-Signed.aab` والتحقق من مجلد التوقيع الرقمي `META-INF/`:
- `META-INF/MANIFEST.MF` (موجود: 294,649 بايت)
- `META-INF/ANDROIDD.SF` (موجود: 294,811 بايت)
- `META-INF/ANDROIDD.RSA` (موجود: 1,376 بايت)
*ملاحظة توقيعية:* تم توقيع الحزمة الحالية بمفتاح Android Debug Key الافتراضي لبيئة التطوير. للرفع النهائي على المتجر في الإنتاج، يجب تمرير معلمات Keystore الإنتاجية الرسمية (`-p:AndroidSigningKeyStore=...`).

---

## 3. مراجعة وتدقيق إعدادات نظام iOS (Apple App Store)

| البند | التكوين الفعلي المطبق | نتيجة الفحص |
| :--- | :--- | :--- |
| **معرّف الحزمة الصريح (`CFBundleIdentifier`)** | تم إدراجه صراحة في [Info.plist](file:///d:/DEV/khadamat/src/Khadamat.MobileApp/Platforms/iOS/Info.plist): `<key>CFBundleIdentifier</key><string>com.nassar84.khadamawy</string>`. | ✅ متطابق 100% مع `ApplicationId` في ملف `.csproj` لمنع أي لبس في أدوات Apple. |
| **أسماء وإصدارات العرض** | تم إدراج `CFBundleDisplayName` (خدماوي)، و `CFBundleShortVersionString` (2.9)، و `CFBundleVersion` (21). | ✅ مكتمل ومتوافق. |
| **ملف الاستحقاقات (`Entitlements.plist`)** | تم التحقق من وجود [Entitlements.plist](file:///d:/DEV/khadamat/src/Khadamat.MobileApp/Platforms/iOS/Entitlements.plist) ويحتوي على:<br>1. `aps-environment`: `development`<br>2. `keychain-access-groups`: `$(AppIdentifierPrefix)com.nassar84.khadamawy`. | ✅ سليم وصحيح لتمكين الإشعارات السحابية وحفظ بيانات الدخول المشفرة. |
| **أمان النقل (ATS)** | تم حصر الإعداد على: `<key>NSAppTransportSecurity</key><dict><key>NSAllowsArbitraryLoads</key><false/></dict>`. | ✅ لا توجد أي استثناءات عشوائية أو نطاقات غير مشفرة. |
| **حالة بناء IPA** | **لم يتم بناء ملف IPA.** | 🛑 **شفافية هندسية:** بيئة العمل الحالية هي Windows ولا يتوفر جهاز macOS مقترن (Pair to Mac) لتشغيل أدوات Xcode 16 لبناء أرشيف IPA وتوقيعه بشهادات Apple Provisioning Profile. تم التحقق من صحة الكود وسلامة التكوين فقط. |

---

## 4. نتائج اختبارات الوحدة المؤتمتة (Unit Tests)

تم تنفيذ الاختبارات عبر: `dotnet test tests\Khadamat.UnitTests\Khadamat.UnitTests.csproj`
- **إجمالي الاختبارات:** 46
- **الاختبارات الناجحة:** 45 ✅
- **الاختبارات الفاشلة:** 1 ⚠️ (`ImageStorageServiceTests.When_ConfiguredPath_IsEmpty_UsesDefaultKhadamawyImages` بسبب المسار المحلي الصلب `D:\maged\images` الموجود على جهاز التطوير، وهو مجدول للإصلاح في المرحلة الثانية).

---

## 5. الخلاصة الفنية وحالة الاعتماد

1. **نظام Android:** تم بنجاح استهداف **Target API 36**، وتجاوز البناء مرحلة AOT بنجاح تام، وتوليد ملف **`com.nassar84.khadamawy-Signed.aab`** الفعلي بحجم 52.2 ميجابايت.
2. **نظام iOS:** تم ضبط `CFBundleIdentifier` و `Info.plist` و `Entitlements.plist` وفق أحدث معايير Apple بدقة تامة، مع تأكيد الحاجة لبيئة Mac عند الرغبة في إنتاج ملف الـ IPA النهائي.
3. **التزام القيود:** لم يتم تحديث أي حزم NuGet بشكل عشوائي، ولم يتم البدء في المرحلة الثانية بانتظار الاعتماد.
