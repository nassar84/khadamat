# تقرير 03: تدقيق نظام أندرويد ومتطلبات متجر Google Play

**تاريخ التدقيق:** 7 نوفمبر 2026  
**المراجع:** Senior Mobile Solutions Architect  
**المشروع المستهدف:** `src/Khadamat.MobileApp` (`com.nassar84.khadamawy`)

---

## 1. فحص متطلبات Google Play الرسمية (Google Play Compliance Checklist)

| المطلب الفني | متطلب Google Play الحالي | الحالة في مشروع Khadamawy | التقييم والقرار المعماري |
| :--- | :--- | :--- | :--- |
| **Target API Level** | **API 35** (Android 15) | في `csproj`: **36** (Preview)<br>في `Manifest`: **35** | **مرفوض حتماً في الإنتاج:** استهداف API 36 (معاينة) يؤدي لرفض Google Play الفوري لملف AAB. يجب توحيد الهدف على **API 35**. |
| **Minimum API Level** | API 21 أو أعلى | `minSdkVersion="24"` (Android 7.0) | **ممتاز:** يغطي أكثر من 98% من الأجهزة النشطة عالمياً مع دعم أحدث ميزات الأمان. |
| **صيغة حزمة النشر** | **Android App Bundle (.aab)** | في `csproj`: `<AndroidPackageFormat>apk</AndroidPackageFormat>` | **غير مقبول للمتجر:** يُشترط تغيير الصيغة إلى `aab` لإنشاء الحزمة النهائية للنشر، مع إبقاء خيار APK لاختبارات الـ QA الداخلية فقط. |
| **دعم معالجات 64-bit** | إلزامي (`arm64-v8a`) | تم تحديد: `android-arm64` فقط | **يحتاج توسيع:** حصر التطبيق على `android-arm64` يمنع تشغيله على بعض الأجهزة القديمة أو محاكيات الفحص. يجب دعم `android-arm;android-arm64;android-x64`. |
| **أداة الضغط والتحسين R8** | موصى بها بشدة لتقليص الحجم والـ Dex | تم تعطيلها: `<UseR8>false</UseR8>` | **سلوك غير سليم:** تم تعطيلها كحل مؤقت لتكرار فئة `ArrayMapKt`. يؤدي لزيادة هائلة في حجم التطبيق وإمكانية تجاوز حد 64K methods بدون MultiDex فعال. |
| **أمان حركة البيانات (Cleartext)** | تشفير HTTPS إلزامي | مفعل: `cleartextTrafficPermitted="true"` | **ثغرة أمنية عالية:** يُرفض أو يتم إرسال تنبيه أمني شديد في تقرير ما قبل الإطلاق (Pre-launch Report) من جوجل. |

---

## 2. مراجعة الصلاحيات (Android Permissions Audit)

تم فحص ملف `src/Khadamat.MobileApp/Platforms/Android/AndroidManifest.xml`:

```xml
<uses-permission android:name="android.permission.ACCESS_NETWORK_STATE" />
<uses-permission android:name="android.permission.INTERNET" />
<uses-permission android:name="android.permission.POST_NOTIFICATIONS" />
<uses-permission android:name="android.permission.CAMERA" />
<uses-permission android:name="android.permission.READ_EXTERNAL_STORAGE" android:maxSdkVersion="32" />
<uses-permission android:name="android.permission.READ_MEDIA_IMAGES" />
```

### تقييم الصلاحيات:
1. **الإنترنت وحالة الشبكة (`INTERNET`, `ACCESS_NETWORK_STATE`):** صلاحيات عادية غير خطرة مبررة لتشغيل التطبيق والاتصال بالخادم.
2. **الإشعارات (`POST_NOTIFICATIONS`):** مطبقة بشكل سليم لنظام Android 13+ (API 33+) وتتوافق مع متطلبات إشعارات Firebase.
3. **الكاميرا والصور (`CAMERA`, `READ_MEDIA_IMAGES`):** 
   - استخدام مبرر وشرعي تماماً لأن التطبيق يتيح لمقدمي الخدمات والعملاء التقاط صور الهوية، رخص المهن، وصور الخدمات والأعطال.
   - تم استخدام `android:maxSdkVersion="32"` على `READ_EXTERNAL_STORAGE` بشكل احترافي لمنع طلب الصلاحيات القديمة على أندرويد 13 فما فوق.
4. **عدم وجود صلاحيات خطرة غير مبررة:** لا يطلب التطبيق الموقع الجغرافي بالخلفية، جهات الاتصال، سجل المكالمات، الرسائل القصيرة SMS، أو التخزين الشامل `MANAGE_EXTERNAL_STORAGE`. هذا يضمن عبور مراجعة Google Play دون استمارات تبرير إضافية.

---

## 3. تدقيق إعدادات شبكة الاتصال (Network Security Config)

يحتوي المسار `Platforms/Android/Resources/xml/network_security_config.xml` على التكوين التالي:
```xml
<?xml version="1.0" encoding="utf-8"?>
<network-security-config>
    <base-config cleartextTrafficPermitted="true">
        <trust-anchors>
            <certificates src="system" />
            <certificates src="user" />
        </trust-anchors>
    </base-config>
</network-security-config>
```

### المخاطر:
1. السماح بالحركة غير المشفرة `cleartextTrafficPermitted="true"` يُعرّض بيانات المستخدمين والتوكنات لجلسات التنصت (Man-in-the-Middle).
2. الثقة في شهادات المستخدم `certificates src="user"` تمثل خطراً أمنياً يسمح بكسر تشفير SSL عبر شهادات غير موثوقة.
3. **الإجراء التصحيحي للإنتاج:**
   - جعل `cleartextTrafficPermitted="false"`.
   - قصر الثقة على شهادات النظام المعتمدة `src="system"`.
   - في حال الحاجة إلى اختبار خوادم محلية في مرحلة التطوير (Development)، يجب قصر السماح على نطاقات الاستثناء الخاصة بالـ Debug فقط عبر تكوين `<debug-overrides>`.

---

## 4. إعدادات التوقيع والنشر (Signing & Build Properties)

1. **معلومات الإصدار الحالية:**
   - `ApplicationTitle`: خدماوي
   - `ApplicationId`: `com.nassar84.khadamawy`
   - `ApplicationVersion`: `21`
   - `ApplicationDisplayVersion`: `2.9`
2. **تجهيز أوامر إنشاء حزمة Google Play الرسمية (.aab):**
   ```bash
   dotnet publish src/Khadamat.MobileApp/Khadamat.MobileApp.csproj `
     -f net9.0-android `
     -c Release `
     -p:AndroidPackageFormat=aab `
     -p:AndroidKeyStore=true `
     -p:AndroidSigningKeyStore="khadamawy.keystore" `
     -p:AndroidSigningKeyAlias="khadamawy" `
     -p:AndroidSigningKeyPass="******" `
     -p:AndroidSigningStorePass="******"
   ```
> [!IMPORTANT]
> يجب عدم تخزين كلمات مرور Keystore أو المفاتيح السرية داخل مستودع الأكواد (Git Repository). يُدار توقيع الحزمة إما محلياً في بيئة المطور أو عبر GitHub Actions Secrets / CI-CD Variables.
