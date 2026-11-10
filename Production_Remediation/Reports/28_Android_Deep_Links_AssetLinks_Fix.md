# تقرير تصحيح ونشر روابط Android Deep Links – Digital Asset Links (تقرير 28)

**تاريخ الإنجاز:** 9 نوفمبر 2026  
**المشروع:** Khadamawy (خدماوي)  
**معرف الحزمة (Package ID):** `com.nassar84.khadamawy`  
**النطاق المستهدف (Domain):** `khadamawy.eis-dev.com`  
**الحالة النهائية:**  
- **الملف المصدري:** ✅ **Fixed**
- **النشر على السيرفر المباشر:** ✅ **Deployed**
- **التحقق من الرابط والشهادة عبر Google API:** ✅ **Verified ("linked": true)**
- **تحديث لوحة Google Play Console:** ⏳ **Pending Console UI Refresh (إجراء المطوّر)**

---

## 1. الأسباب الجذرية التي تم علاجها (Root Causes Resolved)

الملف القديم المنشور على السيرفر كان يحمل خطأين جوهريين:
1. اسم الحزمة كان `com.khadamawy.app` بدلاً من `com.nassar84.khadamawy`.
2. بصمة الشهادة كانت نصاً مؤقتاً (`PLACEHOLDER:REPLACE_WITH_ACTUAL_SHA256_FROM_KEYSTORE`).

---

## 2. البصمة الرسمية المعتمدة (Official App Signing SHA-256)

تم الحصول على البصمة الرسمية لشهادة **Google Play App Signing** مباشرة من لوحة التحكم (وليس شهادة الرفع):
```
2D:CD:90:8A:00:CC:43:A2:95:1C:4D:6E:F8:E3:0D:AC:C0:6D:7D:D8:37:C1:34:1C:72:E8:AD:30:B5:5A:EF:95
```

---

## 3. محتوى الملف المعتمد والمنشور (Deployed assetlinks.json)

```json
[
  {
    "relation": [
      "delegate_permission/common.handle_all_urls"
    ],
    "target": {
      "namespace": "android_app",
      "package_name": "com.nassar84.khadamawy",
      "sha256_cert_fingerprints": [
        "2D:CD:90:8A:00:CC:43:A2:95:1C:4D:6E:F8:E3:0D:AC:C0:6D:7D:D8:37:C1:34:1C:72:E8:AD:30:B5:5A:EF:95"
      ]
    }
  }
]
```

---

## 4. مسارات الملفات والنسخ الاحتياطي (Files & Backups)

- **المصدر المعتمد في المستودع (Authoritative Repo Source):**  
  `d:\DEV\khadamat\src\Khadamat.WebAPI\wwwroot\.well-known\assetlinks.json`
- **المصدر في مجلد النشر المباشر (Publish Directory Source):**  
  `D:\maged\Khadamat\wwwroot\.well-known\assetlinks.json`
- **النسخ الاحتياطية المحفوظة (Backups Preserved):**  
  - `src/Khadamat.WebAPI/wwwroot/.well-known/assetlinks.json.bak`
  - `D:/maged/Khadamat/wwwroot/.well-known/assetlinks.json.bak`

---

## 5. طريقة النشر (Deployment Method)

تمت المزامنة المباشرة لمجلد `.well-known` حصراً إلى خادم الإنتاج (SmarterASP / IIS) عبر أداة `msdeploy` الرسمية:
- **الأمر المنفذ:**
  ```powershell
  msdeploy.exe -verb:sync `
    -source:contentPath="D:\maged\Khadamat\wwwroot\.well-known" `
    -dest:contentPath="magedtest11-001-subsite8/wwwroot/.well-known",computerName="https://win8119.site4now.net:8172/msdeploy.axd?site=magedtest11-001-subsite8",...
  ```
- **نتيجة النشر:** تم تحديث ملف `assetlinks.json` وإضافة النسخة الاحتياطية على السيرفر مباشرة دون المساس بأي ملف كودي، ودون إيقاف السيرفر أو إعادة بناء التطبيق.

---

## 6. نتائج التحقق الحي من السيرفر (Production HTTP Verification)

تم فحص الرابط المباشر عبر `curl`:
`https://khadamawy.eis-dev.com/.well-known/assetlinks.json`

- **HTTP Status:** `200 OK`
- **Redirects:** لا يوجد (0 redirects)
- **Content-Type:** `application/json`
- **ETag:** `"1dd70383b8478d6"`
- **Package Name:** `com.nassar84.khadamawy` (مطابق 100%)
- **SHA-256 Fingerprint:** `2D:CD:90:8A:...:EF:95` (مطابق 100% لبصمة Google Play)
- **Relation:** `delegate_permission/common.handle_all_urls`

---

## 7. فحص Google Digital Asset Links API الرسمي (Google Verification)

تم استدعاء محرك فحص الروابط الرسمي لشركة Google عبر API:

### أ. التحقق من كشف البيانات (Statements Discovery):
**الرابط:** `https://digitalassetlinks.googleapis.com/v1/statements:list?source.web.site=https://khadamawy.eis-dev.com&relation=delegate_permission/common.handle_all_urls`  
**الاستجابة:**
```json
{
  "statements": [
    {
      "source": { "web": { "site": "https://khadamawy.eis-dev.com." } },
      "relation": "delegate_permission/common.handle_all_urls",
      "target": {
        "androidApp": {
          "packageName": "com.nassar84.khadamawy",
          "certificate": {
            "sha256Fingerprint": "2D:CD:90:8A:00:CC:43:A2:95:1C:4D:6E:F8:E3:0D:AC:C0:6D:7D:D8:37:C1:34:1C:72:E8:AD:30:B5:5A:EF:95"
          }
        }
      }
    }
  ]
}
```

### ب. التحقق من الربط الفعلي (AssetLinks Check):
**الرابط:**  
`https://digitalassetlinks.googleapis.com/v1/assetlinks:check?source.web.site=https://khadamawy.eis-dev.com&relation=delegate_permission/common.handle_all_urls&target.androidApp.packageName=com.nassar84.khadamawy&target.androidApp.certificate.sha256Fingerprint=2D:CD:90:8A:00:CC:43:A2:95:1C:4D:6E:F8:E3:0D:AC:C0:6D:7D:D8:37:C1:34:1C:72:E8:AD:30:B5:5A:EF:95`

**النتيجة الحاسمة:**
```json
{
  "linked": true,
  "maxAge": "3599.998109477s"
}
```
✅ **خوادم Google تؤكد رسمياً: الربط سليم ومتحقق منه 100% (`linked: true`).**

---

## 8. الخطوة المتبقية للمطوّر (Final Developer Action in Console)

يرجى الدخول إلى **Google Play Console**:
1. افتح صفحة تطبيق **Khadamawy**.
2. توجّه إلى: **Grow** (أو **Deep links**) ← **Deep links**.
3. في تبويب **Domains** (النطاقات):
   - ستجد النطاق `khadamawy.eis-dev.com`.
   - اضغط على زر التحديث / إعادة الفحص أو انتظر دورة التحديث التلقائي لـ Google Play.
   - ستتحول حالة النطاق إلى **Verified** (موثق) وتختفي رسالة الخطأ السابقة، وترتبط الروابط بتطبيقك تلقائياً دون الحاجة لرفع أي حزمة جديدة.
