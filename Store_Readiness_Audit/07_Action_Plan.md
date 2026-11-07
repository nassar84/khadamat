# تقرير 07: خطة العمل وخريطة الطريق لتجهيز المنظومة للنشر (Action Plan & Remediation Matrix)

**تاريخ التدقيق:** 7 نوفمبر 2026  
**المراجع:** Senior .NET Architect & Mobile Lead  
**الحالة:** خطة استرشادية بانتظار اعتماد المطور / العميل قبل بدء التنفيذ البرمجي.

---

## 1. جدول الإجراءات التصحيحية المصنفة حسب الأولوية

| # | المشكلة المكتشفة | مستوى الأولوية | الملفات المتأثرة | التكوين الحالي | الإجراء التصحيحي المستهدف | تقييم مخاطر التغيير |
| :-: | :--- | :---: | :--- | :--- | :--- | :--- |
| **1** | **انتهاك ATS وسماح المرور غير المشفر في iOS** | 🔴 **حرجة جداً (Critical)** | `Platforms/iOS/Info.plist` | `<NSAllowsArbitraryLoads>true</NSAllowsArbitraryLoads>` | حذف الوسم كلياً وفرض اتصالات HTTPS فقط. | آمن 100% بشرط عمل واجهات الخادم بنطاق HTTPS رسمي بشهادة SSL سارية. |
| **2** | **طلب صلاحيات غير مستخدمة (الميكروفون وجهات الاتصال) في iOS** | 🔴 **حرجة جداً (Critical)** | `Platforms/iOS/Info.plist` | وجود `NSMicrophoneUsageDescription` و `NSContactsUsageDescription` | حذفهما كلياً من الملف لعدم وجود أي استخدام لهما في كود التطبيق. | آمن تماماً، ويحمي من الرفض الفوري أثناء المراجعة اليدوية لـ Apple. |
| **3** | **استهداف إصدار معاينة Android API 36 وتناقض الـ Manifest** | 🔴 **حرجة جداً (Critical)** | `Khadamat.MobileApp.csproj`<br>`Platforms/Android/AndroidManifest.xml` | في csproj: `36` (Preview)<br>في Manifest: `35` | توحيد الهدف في الملفين على **API 35** (Android 15 الرسمي المعتمد لـ Google Play). | آمن؛ متوافق تماماً مع حزم .NET 9 Android SDK. |
| **4** | **إخراج الحزمة كـ APK بدلاً من AAB** | 🔴 **حرجة (Critical)** | `Khadamat.MobileApp.csproj` | `<AndroidPackageFormat>apk</AndroidPackageFormat>` | تغيير الإعداد للإنتاج إلى `aab` (Android App Bundle). | إلزامي لقبول التطبيق على Google Play Console. |
| **5** | **سماح أندرويد بالمرور غير المشفر والثقة بشهادات المستخدم** | 🟠 **عالية (High)** | `Platforms/Android/Resources/xml/network_security_config.xml` | `cleartextTrafficPermitted="true"` وثقة شهادات `user` | جعلها `false` وحصر الثقة على شهادات `system` المعتمدة. | آمن عند الاتصال بالدومين المشفر. |
| **6** | **تثبيت Serilog 10.0.0 المستقبلي على WebAPI (net8.0)** | 🟠 **عالية (High)** | `src/Khadamat.WebAPI/Khadamat.WebAPI.csproj` | `Serilog.AspNetCore 10.0.0` | خفض الحزمة إلى `Serilog.AspNetCore 8.0.3` المتوافقة مع .NET 8 LTS. | مستقر، يمنع سحب حزم .NET 10 غير المستقرة. |
| **7** | **ثغرات مكتبة الصور SixLabors.ImageSharp** | 🟠 **عالية (High)** | `src/Khadamat.WebAPI/Khadamat.WebAPI.csproj` | `SixLabors.ImageSharp 2.1.9` | ترقية الحزمة إلى الإصدار الأحدث المستقر والمحمِي `3.1.5+`. | منخفض؛ يتطلب فحص استدعاءات `Image.Load` لتوافق API v3. |
| **8** | **تعارض فئة `ArrayMapKt` وتعطيل الـ R8** | 🟠 **عالية (High)** | `src/Khadamat.MobileApp/Khadamat.MobileApp.csproj` | وجود تكرار وتعطيل R8: `<UseR8>false</UseR8>` | إزالة التبعية الزائدة `Xamarin.AndroidX.Collection.Ktx` وإعادة تفعيل R8 لضغط الكود وحجم التطبيق. | ممتاز للأداء والحجم؛ يحتاج بناء تجريبي للتأكد من اكتمال الرسوم. |
| **9** | **غياب ملف الاستحقاقات Entitlements في iOS** | 🟡 **متوسطة (Medium)** | `Platforms/iOS/Entitlements.plist` | غير موجود | إنشاء ملف `Entitlements.plist` وتضمين بيئة الإشعارات ومجموعات Keychain. | آمن وإلزامي عند تفعيل الإشعارات في آبل. |
| **10** | **سلسلة الاتصال وقفل الـ JWT مكشوفان في WebAPI** | 🟡 **متوسطة (Medium)** | `src/Khadamat.WebAPI/appsettings.json` | نصوص صريحة لكلمة مرور قاعدة الإنتاج | نقل البيانات الحساسة لمتغيرات البيئة (Environment Variables) في خادم الاستضافة. | أساسي للأمان وحماية بيانات المستخدمين. |
| **11** | **وجود المشروع المهجور Khadamat.Mobile** | 🟢 **منخفضة (Low)** | `Khadamat.sln` | قالب افتراضي فارغ متروك | استبعاد أو أرشفة المشروع لتنظيف الحل وتفادي الالتباس. | لا يؤثر على أي كود وظيفي. |

---

## 2. مراحل التنفيذ الموصى بها (Execution Phases)

### المرحلة الأولى: التجهيز السريع للمتاجر وتفادي الرفض الفوري (Quick Store Compliance)
1. تعديل ملف `Platforms/iOS/Info.plist` (إزالة الميكروفون وجهات الاتصال، وإزالة `NSAllowsArbitraryLoads`).
2. إضافة ملف `Platforms/iOS/Entitlements.plist` وضبط استهداف iOS الأدنى على 15.0.
3. تعديل `Khadamat.MobileApp.csproj` لضبط Android Target API 35 وصيغة AAB وإزالة السماح بالمرور غير المشفر من `network_security_config.xml`.

### المرحلة الثانية: استقرار الحزم والأمان (Packages & Vulnerability Remediation)
1. معالجة حزمة Serilog في WebAPI وضبطها على إصدارات .NET 8.
2. ترقية `SixLabors.ImageSharp` وترقية `System.Text.Json` لسد الثغرات الأمنية المكتشفة.
3. فحص معالجة `ArrayMapKt` وإعادة تفعيل `UseR8=true` لتقليص الحجم الإجمالي للتطبيق بنسبة تتجاوز 40%.

### المرحلة الثالثة: إصدارات الإنتاج والتوقيع (Release & Store Artifacts)
1. إصدار ملف `com.nassar84.khadamawy.aab` الموقّع لاختباره على مسار الفحص الداخلي (Internal Testing Track) في Google Play Console.
2. تجميع تطبيق iOS على جهاز macOS ورفعه إلى TestFlight للمراجعة والتجربة الميدانية.
