# تقرير 04: تدقيق نظام iOS ومتطلبات متجر Apple App Store

**تاريخ التدقيق:** 7 نوفمبر 2026  
**المراجع:** Senior iOS & .NET MAUI Architect  
**المشروع المستهدف:** `src/Khadamat.MobileApp` (`com.nassar84.khadamawy`)

---

## 1. فحص متطلبات Apple الرسمية وتحديثات 2025/2026

| المعيار الفني | متطلب Apple App Store | الحالة في مشروع Khadamawy | التقييم ومستوى الخطر |
| :--- | :--- | :--- | :--- |
| **App Transport Security (ATS)** | تشفير HTTPS إلزامي ومنع التنزيل العشوائي غير المشفر | `<key>NSAllowsArbitraryLoads</key><true/>` | 🛑 **سبب حتمي للرفض (Instant Rejection):** مخالف للمعيار 5.1.1 ما لم تكن هناك ضرورة قصوى كمتصفح ويب عام. |
| **صلاحيات غير مستخدمة (Unused Permissions)** | منع طلب أي صلاحيات حساسة لا يستخدمها الكود الفعلي | تم طلب **الميكروفون** و**جهات الاتصال** في `Info.plist` | 🛑 **سبب حتمي للرفض (Rejection):** الكود لا يحتوي على أي تسجيل صوتي أو وصول لدفتر العناوين. |
| **الحد الأدنى لنظام التشغيل (Min iOS)** | .NET MAUI 8/9 يدعم **iOS 15.0+** | تم تحديد: `<SupportedOSPlatformVersion>12.0</SupportedOSPlatformVersion>` | ⚠️ **خطأ في التكوين:** إصدار iOS 12 لم يعد مدعوماً في أطر MAUI الحديثة ويسبب تحذيرات بناء. |
| **ملف الاستحقاقات (Entitlements.plist)** | إلزامي للإشعارات وتخزين Keychain المشفر | **غير موجود** في مسار `Platforms/iOS/` | ⚠️ **ناقص:** ضروري لتمكين خدمة APNs الخاصة بـ Push Notifications. |
| **بيئة البناء والاعتماد (Xcode/SDK)** | يتطلب حالياً البناء باستخدام **Xcode 16 / iOS 18 SDK** | التجميع المحلي يعمل على Windows | ℹ️ **ملاحظة تشغيلية:** يتطلب رفع التطبيق لـ App Store Connect جهاز Mac أو خط إنتاج سحابي (Mac Runner). |

---

## 2. التدقيق التفصيلي لملف `Platforms/iOS/Info.plist`

### أ) ثغرة أمان الاتصال App Transport Security (ATS)
الملف يحتوي على:
```xml
<key>NSAppTransportSecurity</key>
<dict>
    <key>NSAllowsArbitraryLoads</key>
    <true/>
</dict>
```
- **التحليل:** هذا السطر يخبر نظام iOS بالسماح بتحميل أي صفحات أو واجهات عبر HTTP غير المشفر (Cleartext).
- **موقف مراجعي Apple:** ترفض Apple التطبيق بشكل فوري مع رسالة استفسار تطلب تبريراً دقيقاً لتعطيل ATS، وبما أن منصة خدماوي تتصل بواجهات الـ API الخاصة بها، فإن التبرير سيكون غير مقبول ومرفوض.
- **التصحيح الإلزامي:** حذف `NSAllowsArbitraryLoads` وتأمين كافة الروابط لتكون HTTPS فقط.

### ب) أذونات الخصوصية (Privacy Usage Descriptions)
يحتوي الملف على الأذونات التالية:
1. `NSCameraUsageDescription` (كاميرا): مبررة تماماً لرفع صور الأعمال والهوية.
2. `NSPhotoLibraryUsageDescription` (مكتبة الصور): مبررة لاختيار الصور من ألبوم المستخدم.
3. `NSPhotoLibraryAddUsageDescription` (حفظ الصور): مبررة لحفظ الفواتير والشهادات.
4. `NSLocationWhenInUseUsageDescription` (الموقع الجغرافي): مبررة لعرض الخدمات الأقرب للمستخدم.
5. 🛑 `NSMicrophoneUsageDescription`:
   - النص المكتوب: *"يحتاج التطبيق للوصول إلى الميكروفون لتسجيل الصوت"*.
   - **الواقع الفعلي للكود:** لا يوجد أي كود يقوم بتسجيل الصوت أو استخدام الـ Audio Recorder.
   - **الخطر:** ترسل Apple تنبيهاً أثناء المراجعة الآلية أو اليدوية بالرفض لطلب صلاحيات جهاز غير مستخدمة (Guideline 5.1.1 - Data Collection and Storage).
6. 🛑 `NSContactsUsageDescription`:
   - النص المكتوب: *"يحتاج التطبيق للوصول إلى جهات الاتصال لمشاركتها"*.
   - **الواقع الفعلي للكود:** التطبيق لا يطلب إذن دفتر العناوين ولا يقرأ أي جهة اتصال في المنظومة.
   - **الخطر:** رفض مؤكد وفوري من Apple لانتهاك الخصوصية دون وجود وظيفة تطابق ذلك.

---

## 3. ملف الاستحقاقات المفقود (Missing `Entitlements.plist`)

يحتاج مشروع iOS الفعلي عند رفعه لمتجر App Store إلى ملف `Platforms/iOS/Entitlements.plist` لتعريف:
```xml
<?xml version="1.0" encoding="UTF-8"?>
<!DOCTYPE plist PUBLIC "-//Apple//DTD PLIST 1.0//EN" "http://www.apple.com/DTDs/PropertyList-1.0.dtd">
<plist version="1.0">
<dict>
    <!-- تمكين الإشعارات السحابية APNs -->
    <key>aps-environment</key>
    <string>production</string>
    <!-- تمكين Keychain لتخزين التوكنات الآمنة -->
    <key>keychain-access-groups</key>
    <array>
        <string>$(AppIdentifierPrefix)com.nassar84.khadamawy</string>
    </array>
</dict>
</plist>
```
ويتم ربطه في ملف `.csproj`:
```xml
<CodesignEntitlements Condition="$([MSBuild]::GetTargetPlatformIdentifier('$(TargetFramework)')) == 'ios'">Platforms\iOS\Entitlements.plist</CodesignEntitlements>
```

---

## 4. إعدادات إصدار التطبيق والهوية (Bundle Versioning)

- **ApplicationId:** `com.nassar84.khadamawy`
- **CFBundleShortVersionString (Version Name):** `2.9`
- **CFBundleVersion (Build Number):** `21`
- **تنبيه هام لنظام iOS:** في `Info.plist` الحالي، تم تعيين `CFBundleURLName` كـ `com.khadamat.app`، ويجب توحيدها لتطابق الـ ApplicationId الرسمي `com.nassar84.khadamawy` لتفادي أي تعارض في معالجة روابط الـ Deep Linking.

---

## 5. متطلبات البناء والنشر عبر macOS

نظراً لأن بيئة التطوير الحالية مبنية على نظام تشغيل Windows:
1. يمكن فحص وتجميع كود C# و Razor لـ iOS بنجاح على Windows.
2. يتطلب إنشاء ملف الـ Archive الموقّع بصيغة `.ipa` ورفعه إلى **App Store Connect / TestFlight** وجود بيئة Mac (إما عن طريق ربط Visual Studio بجهاز Mac عبر الشبكة `Pair to Mac`، أو استخدام سير عمل مؤتمت مثل GitHub Actions على رنر `macos-14` مزود بـ Xcode 16).
