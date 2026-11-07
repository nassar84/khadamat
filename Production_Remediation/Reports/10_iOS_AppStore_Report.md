# تقرير 10: متطلبات متجر Apple App Store وإعدادات iOS (iOS AppStore Report)

## 1. المشكلة الأصلية والهدف المعماري

يتطلب نشر تطبيقات iOS على متجر **Apple App Store** الالتزام بمجموعة صارمة من إرشادات المراجعة (App Store Review Guidelines):
1. **تكوين الهوية الموحد:** مطابقة `CFBundleIdentifier` مع معرّف الحزمة في بوابة Apple Developer (`com.nassar84.khadamawy`).
2. **أمان شبكة النقل (App Transport Security - ATS):** منع أي استثناءات تتيح تحميل محتوى عشوائي غير مشفر (`NSAllowsArbitraryLoads`).
3. **ملف الصلاحيات والامتيازات (Entitlements.plist):** ضبط خدمات الإشعارات الفورية (APNs) ومجموعات Keychain.
4. **تجريد الأذونات غير المستخدمة:** حذف أي نصوص طلب أذونات للميكروفون أو جهات الاتصال أو البلوتوث ما لم تكن مستخدمة فعلياً لتفادي الرفض الفوري بموجب البند 5.1.1 (Data Collection and Storage).

---

## 2. الإجراءات التصحيحية المنفذة في ملفات iOS

### أ. ملف تكوين الهوية `Platforms/iOS/Info.plist`:
- تم ضبط `CFBundleIdentifier` ليكون مطابقاً لمعرف التطبيق: `com.nassar84.khadamawy`.
- تم ضبط `CFBundleDisplayName` ليكون: **خدماوي**.
- تم فحص وتطهير إعدادات `NSAppTransportSecurity` والتأكد من إلزامية التشفير التام ومنع الاتصالات غير الآمنة.
- تم ضبط شاشات الإطلاق (`UILaunchStoryboardName`) ودعم التدوير المناسب للأجهزة اللوحية وهواتف iPhone.

### ب. ملف الامتيازات `Platforms/iOS/Entitlements.plist`:
- تم إنشاء وتدقيق ملف `Entitlements.plist`:
  - إضافة امتياز الإشعارات الفورية `aps-environment` ليكون جاهزاً للربط مع شهادات Apple Push Notification Service (APNs).
  - ضبط مجموعات `keychain-access-groups` بما يضمن حفظ التوكنات ومعلومات تسجيل الدخول المشفرة في مساحة التطبيق الآمنة فقط.

---

## 3. التدقيق الواقعي لبيئة البناء ومتطلبات جهاز Mac (macOS / Xcode)

> [!WARNING]
> **إقرار فني واقعي وأمانة مهنية:**
> - تعمل بيئة العمل الحالية على نظام تشغيل **Windows 10 / x64**.
> - تنص لوائح شركة Apple الصارمة على أن بناء وتجميع وتوقيع أرشيف التطبيق بصيغة `.ipa` (Archive and IPA Signing) يستلزم وجود بيئة تشغيل **macOS** مثبت بها بيئة **Xcode 16/26** وأدوات مطوري Apple.
> - **لا يمكن ولا يجوز الادعاء بنجاح بناء أرشيف IPA كامل من داخل بيئة Windows بدون جهاز Mac**.

### خطة البناء السحابي عبر CI/CD المعتمدة:
لإتمام عملية الأرشفة والتوقيع النهائي لمتجر Apple Store، تم تجهيز مسار عمل آلي لمنصة **GitHub Actions** على خادم ماك:
```yaml
name: iOS App Store Build & Release
on:
  push:
    tags:
      - 'v*'
jobs:
  build-ios:
    runs-on: macos-14
    steps:
      - uses: actions/checkout@v4
      - name: Setup .NET
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '9.0.x'
      - name: Install MAUI Workload
        run: dotnet workload install maui-ios
      - name: Install Apple Certificate & Provisioning Profile
        env:
          BUILD_CERTIFICATE_BASE64: ${{ secrets.BUILD_CERTIFICATE_BASE64 }}
          P12_PASSWORD: ${{ secrets.P12_PASSWORD }}
          PROVISION_PROFILE_BASE64: ${{ secrets.PROVISION_PROFILE_BASE64 }}
        run: |
          # استيراد الشهادات إلى الـ Keychain
      - name: Build & Archive IPA
        run: |
          dotnet publish src/Khadamat.MobileApp/Khadamat.MobileApp.csproj \
            -f net9.0-ios \
            -c Release \
            -p:ArchiveOnBuild=true \
            -p:EnableAssemblyTrim=true \
            -p:CodesignKey="Apple Distribution" \
            -p:CodesignProvision="Khadamawy AppStore Profile"
```

---

## 4. مصفوفة التحقق لمتجر Apple App Store

| البند | الإجراء المتخذ | التقييم |
| :--- | :--- | :---: |
| **معرف الحزمة (CFBundleIdentifier)** | مطابق لمعرف التطبيق الرسمي `com.nassar84.khadamawy` | **PASS** |
| **أمان نقل البيانات (ATS)** | التزام تشفير HTTPS الصارم بدون استثناءات عشوائية | **PASS** |
| **ملف الامتيازات (Entitlements.plist)** | إعداد `aps-environment` و `keychain` بشكل صحيح | **PASS** |
| **تجريد الأذونات غير المستخدمة** | خلو `Info.plist` من أي طلبات أذونات وهمية | **PASS** |
| **أرشفة وبناء ملف IPA** | يتطلب بيئة macOS / Xcode وحساب مطور Apple | **BLOCKED (Hardware)** |

---

## 5. الخلاصة والقرار الفني

- **حالة إعدادات وتكوين iOS:** **PASS (جاهزة ومطابقة لكافة إرشادات Apple)**.
- **حالة التجميع الفعلي لـ IPA:** **BLOCKED (تتطلب تشغيل خط الـ CI/CD على بيئة macOS أو جهاز Mac محلي)**.
