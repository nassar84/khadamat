# تقرير 27: تصميم وتنفيذ وتوثيق نظام تدوير مفاتيح JWT عديم التوقف (Phase 2.2)

## 1. ملخص تنفيذي (Executive Summary)

تم بنجاح تصميم وبرمجة ودمج نظام متقدم لإدارة وتدوير مفاتيح توقيع التوكنات (**Zero-Downtime Multi-Key JWT Rotation**) داخل مشروع `Khadamat.Infrastructure`.

يعالج هذا النظام أحد أخطر أوجه القصور الشائعة في معمارية المصادقة؛ حيث كان الاعتماد سابقاً على مفتاح توقيع أحادي وثابت (`JwtSettings:Secret`)، مما يعني أنه في حال الرغبة في تدوير المفتاح أمنياً، يتم إلغاء وتخريب كافة جلسات المستخدمين فوراً وتسجيل خروجهم المفاجئ (Mass Disconnection).

النظام الجديد يتيح:
1. دعم المفاتيح المتعددة مع التمييز بمعرّف المفتاح في ترويسة التوكن (`kid` - Key ID).
2. فترات سماح مرنة للمفاتيح السابقة (`Grace Period`) تسمح بقبول التوكنات الصالحة القديمة حتى انتهاء صلاحيتها الزمنية.
3. الرفض الصارم والفوري للمفاتيح المتقاعدة أو المسحوبة أمنياً (`Retired Keys`).
4. التوافق العكسي التام (100% Backward Compatibility) مع التوكنات التاريخية التي لا تحتوي على معرّف `kid`.
5. تغطية اختبارية شاملة عبر 7 اختبارات وحدة جديدة، ليرتفع إجمالي اختبارات الحل الناجحة إلى 53 اختباراً بنسبة نجاح 100%.

| بند المعمارية | التوصيف البرمجي | نتيجة التحقق |
| :--- | :--- | :--- |
| **توليد التوكنات الجديدة** | استخدام المفتاح النشط الأحدث مع تضمين ترويسة `kid` | **PASS** |
| **التحقق من التوكنات المتزامنة** | استخدام `IssuerSigningKeyResolver` لدعم المفاتيح المتعددة | **PASS** |
| **فترة السماح (Grace Period)** | قبول التوكنات الصادرة بالمفتاح السابق الصالح | **PASS** |
| **رفض المفاتيح المتقاعدة** | رفض فوري لتوكنات المفتاح `retired` ولو كانت غير منتهية زمنياً | **PASS** |
| **التوافق العكسي للتوكنات القديمة** | فحص المفتاح النشط عند غياب `kid` في الترويسة | **PASS** |
| **نتائج اختبارات الوحدة** | 53 من أصل 53 اختباراً ناجحاً (0 أخطاء) | **PASS** |

---

## 2. الهيكل البرمجي والملفات المنفذة

تم إدراج التعديلات المعمارية في المسارات التالية:
1. `src/Khadamat.Infrastructure/Identity/JwtKeyManager.cs` (تعريف `IJwtKeyManager` و `JwtKeyManager`).
2. `src/Khadamat.Infrastructure/DependencyInjection.cs` (تسجيل الخدمة وضبط `AddJwtBearer`).
3. `src/Khadamat.Infrastructure/Identity/AuthService.cs` (توليد التوكنات والتحقق عند التجديد).
4. `tests/Khadamat.UnitTests/JwtKeyRotationTests.cs` (ملف الاختبارات المعيارية الشاملة).

---

## 3. تفاصيل بنية `JwtKeyManager.cs`

يقدم مدير المفاتيح واجهة نقية ومستقلة:

```csharp
public interface IJwtKeyManager
{
    SecurityKey GetSigningKey();
    string GetActiveKeyId();
    IEnumerable<SecurityKey> GetValidationKeys();
    SecurityKey? ResolveKey(string? kid);
    bool IsKeyRetired(string? kid);
}
```

### الخصائص الهندسية للتنفيذ:
- **`GetSigningKey()` و `GetActiveKeyId()`:** يقوم بقراءة المفتاح النشط الحالي (`Status == "active"` أو أول مفتاح صالح) لتوقيع أي توكن جديد، مع إضافة خاصية `KeyId` تلقائياً إلى ترويسة الـ JWT (`header.kid`).
- **`GetValidationKeys()`:** يجمع كافة مفاتيح التحقق المسموح بها (المفتاح النشط + كافة المفاتيح التي لا تزال في فترة السماح `grace_period`).
- **`ResolveKey(string? kid)`:** دالة ديناميكية تُمرر إلى `TokenValidationParameters.IssuerSigningKeyResolver`؛ في حال وجود `kid` محدد تبحث عنه فوراً، وفي حال غيابه (التوكنات التاريخية القديمة) ترجع المفتاح النشط أو المفاتيح المقبولة تباعاً، مما يمنع تعطل أي عميل قديم.
- **`IsKeyRetired(string? kid)`:** تمنع أي مفتاح وُضع في حالة `retired` أو تم اختراقه من المصادقة نهائياً.

---

## 4. نموذج الإعدادات الهجين في `appsettings.json`

يدعم النظام النمطين القديم والجديد دون الحاجة لتغيير فوري في ملفات الإعدادات:

### النمط المتقدم لتدوير المفاتيح (Multi-Key Schema):
```json
{
  "JwtSettings": {
    "Issuer": "Khadamat.WebAPI",
    "Audience": "Khadamat.Clients",
    "ExpiryMinutes": 120,
    "RefreshTokenExpiryDays": 7,
    "Keys": [
      {
        "Id": "key-2026-v2",
        "Secret": "A_Secure_Key_That_Has_At_Least_32_Characters_V2",
        "Status": "active",
        "ExpiresAt": "2027-01-01T00:00:00Z"
      },
      {
        "Id": "key-2026-v1",
        "Secret": "A_Secure_Key_That_Has_At_Least_32_Characters_V1",
        "Status": "grace_period",
        "ExpiresAt": "2026-12-01T00:00:00Z"
      },
      {
        "Id": "key-2025-v0",
        "Secret": "Compromised_Or_Old_Key_That_Should_Be_Rejected",
        "Status": "retired",
        "ExpiresAt": "2025-11-01T00:00:00Z"
      }
    ]
  }
}
```

### التوافق مع النمط الأحادي القديم (Fallback):
في حال كان ملف الإعدادات يحتوي فقط على:
```json
"JwtSettings": {
  "Secret": "LegacySingleSecretKey..."
}
```
يتعرف النظام عليه تلقائياً وينشئ منه مفتاحاً افتراضياً بمعرّف `default` وحالة `active`، دون أن يتوقف التطبيق أو يلقي أي استثناء برمجي.

---

## 5. دمج مصادقة الـ Middleware في `DependencyInjection.cs`

تم تحديث تسجيل خدمة `JwtBearer` لتعمل بالتزامن مع `IJwtKeyManager`:

```csharp
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        // استدعاء مدير المفاتيح المسجل
        var serviceProvider = builder.Services.BuildServiceProvider();
        var keyManager = serviceProvider.GetRequiredService<IJwtKeyManager>();

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings["Issuer"],
            ValidAudience = jwtSettings["Audience"],
            IssuerSigningKeyResolver = (token, securityToken, kid, validationParameters) =>
            {
                var resolved = keyManager.ResolveKey(kid);
                return resolved != null ? new[] { resolved } : Enumerable.Empty<SecurityKey>();
            }
        };
    });
```

---

## 6. نتائج اختبارات الوحدة المعمارية (7 Tests)

تم إنشاء ملف الاختبارات `tests/Khadamat.UnitTests/JwtKeyRotationTests.cs` واختبار كافة السيناريوهات الواقعية:

1. **`JwtKeyManager_GeneratesToken_WithActiveKeyId`:** التحقق من وجود `kid` الصحيح في ترويسة التوكن الجديد.
2. **`JwtKeyManager_AcceptsToken_FromGracePeriodKey`:** التحقق من نجاح فك وتوثيق توكن صادر بمفتاح في فترة السماح.
3. **`JwtKeyManager_RejectsToken_FromRetiredKey`:** التحقق من فشل ورفض توكن صادر بمفتاح متقاعد.
4. **`JwtKeyManager_BackwardCompatibility_AcceptsLegacyTokenWithoutKid`:** التحقق من قبول توكن قديم صادر بدون `kid`.
5. **`JwtKeyManager_FallbackToSingleSecret_WhenNoKeysListConfigured`:** التحقق من العمل التلقائي على المفتاح الأحادي القديم.
6. **`JwtKeyManager_GetValidationKeys_ReturnsActiveAndGracePeriodOnly`:** التحقق من استبعاد المفاتيح المتقاعدة من قائمة مفاتيح التحقق.
7. **`JwtKeyManager_Throws_WhenNoValidKeysConfigured`:** التحقق من إلقاء استثناء عند غياب أي مفتاح صالح في الإعدادات.

### مخرجات تشغيل الاختبارات الفعلية:
```text
Passed!  - Failed: 0, Passed: 53, Skipped: 0, Total: 53, Duration: 2.1 s
```
*(نجاح 100% لجميع اختبارات الحل دون أي إخفاق).*

---

## 7. الخلاصة والقرار الفني (Pass/Fail)

- **حالة بند تدوير مفاتيح JWT:** **PASS**
- **مستوى الأمان المعماري:** Enterprise Grade (Zero-Downtime Key Rotation).
- **الجاهزية للإنتاج:** جاهز تماماً للاستخدام عند تدوير المفاتيح في أي وقت دون التأثير على المستخدمين النشطين.
