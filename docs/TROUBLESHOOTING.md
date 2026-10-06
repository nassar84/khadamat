# 🔧 دليل الإصلاحات السريعة - Khadamat

> **مرجع سريع لحل المشاكل الشائعة**

---

## 🚨 المشكلة: الشريط الجانبي مختفي في صفحات Admin

### الأعراض:
- عند فتح `/admin/users` أو أي صفحة admin أخرى
- الشريط الجانبي الإداري لا يظهر
- يظهر الشريط الجانبي العادي (للعملاء) أو لا يظهر شيء

### الحل:
1. افتح ملف `Pages/Admin/_Imports.razor`
2. تأكد من أن السطر الأول هو:
   ```razor
   @layout Khadamat.BlazorUI.Layout.AdminLayout
   ```
3. **ليس** `@layout Khadamat.BlazorUI.Layout.MainLayout`

### الملفات المتأثرة:
- `src/Khadamat.BlazorUI/Pages/Admin/_Imports.razor`

---

## 🚨 المشكلة: دور المستخدم يظهر "عميل" للمدراء

### الأعراض:
- عند تسجيل الدخول كـ System Admin أو Super Admin
- يظهر في الشريط العلوي "عميل" بدلاً من "مدير نظام"

### الحل:
1. افتح `Layout/Header.razor`
2. ابحث عن `user-role-txt`
3. تأكد من الكود التالي:
   ```razor
   <span class="user-role-txt">
       @(context.User.IsInRole("SuperAdmin") ? "مدير خارق" : 
         context.User.IsInRole("SystemAdmin") ? "مدير نظام" : 
         State.IsProvider ? "مزود خدمة" : "عميل")
   </span>
   ```

### الملفات المتأثرة:
- `src/Khadamat.BlazorUI/Layout/Header.razor`

---

## 🚨 المشكلة: المدن لا تتحمل عند اختيار المحافظة

### الأعراض:
- عند اختيار محافظة في نموذج تعديل المستخدم
- قائمة المدن تبقى فارغة أو تظهر "جاري التحميل..." للأبد

### الحل:
1. افتح `Services/ApiClient.cs`
2. ابحث عن `GetCitiesAsync(int governorateId)`
3. تأكد من وجود `try-catch`:
   ```csharp
   public async Task<List<CityDto>> GetCitiesAsync(int governorateId)
   {
       try
       {
           var response = await _http.GetFromJsonAsync<ApiResponse<List<CityDto>>>($"api/v1/locations/governorates/{governorateId}/cities");
           return response?.Data ?? new List<CityDto>();
       }
       catch (Exception ex)
       {
           Console.WriteLine($"Error fetching cities: {ex.Message}");
           return new List<CityDto>();
       }
   }
   ```

### الملفات المتأثرة:
- `src/Khadamat.BlazorUI/Services/ApiClient.cs`
- `src/Khadamat.BlazorUI/Shared/Components/LocationSelector.razor`

---

## 🚨 المشكلة: شاشة فارغة عند النقر على تصنيف فارغ

### الأعراض:
- عند النقر على تصنيف رئيسي ليس به تصنيفات فرعية
- تظهر شاشة بيضاء فارغة بدون أي محتوى

### الحل:
1. افتح `Pages/Explore.razor`
2. تأكد من وجود Empty State handling:
   ```razor
   @if (categories != null)
   {
       @if (categories.Any())
       {
           @foreach (var cat in categories)
           {
               <!-- عرض البيانات -->
           }
       }
       else
       {
           <div class="col-12 text-center py-5">
               <i class="fa-solid fa-folder-open fs-1 text-muted mb-3 opacity-50"></i>
               <p class="text-muted fw-bold">لا توجد تصنيفات في هذا القسم حالياً</p>
           </div>
       }
   }
   else 
   { 
       <LoadingSpinner Message="جاري تحميل التصنيفات..." /> 
   }
   ```

### الملفات المتأثرة:
- `src/Khadamat.BlazorUI/Pages/Explore.razor`

---

## 🚨 المشكلة: التطبيق يتعطل عند فشل API

### الأعراض:
- التطبيق يتوقف تماماً عند فشل الاتصال بالـ API
- رسائل خطأ غير واضحة في Console

### الحل:
1. افتح `Services/ApiClient.cs`
2. تأكد من أن **جميع** الـ methods تحتوي على `try-catch`
3. مثال:
   ```csharp
   public async Task<List<T>> GetDataAsync()
   {
       try
       {
           var response = await _http.GetFromJsonAsync<ApiResponse<List<T>>>(url);
           return response?.Data ?? new List<T>();
       }
       catch (Exception ex)
       {
           Console.WriteLine($"Error: {ex.Message}");
           return new List<T>(); // قيمة افتراضية آمنة
       }
   }
   ```

### الملفات المتأثرة:
- `src/Khadamat.BlazorUI/Services/ApiClient.cs`

---

## 🛠️ أدوات التشخيص السريع

### فحص الـ Layout المستخدم:
```bash
# في PowerShell
Get-Content "src\Khadamat.BlazorUI\Pages\Admin\_Imports.razor" | Select-String "layout"
```

### فحص Error Handling في ApiClient:
```bash
# في PowerShell
Get-Content "src\Khadamat.BlazorUI\Services\ApiClient.cs" | Select-String "try" -Context 0,5
```

### فحص عرض الأدوار:
```bash
# في PowerShell
Get-Content "src\Khadamat.BlazorUI\Layout\Header.razor" | Select-String "user-role-txt" -Context 0,3
```

---

## 📝 خطوات التحقق بعد الإصلاح

### 1. Build التطبيق:
```bash
dotnet build src\Khadamat.BlazorUI\Khadamat.BlazorUI.csproj
```

### 2. تشغيل Backend:
```bash
dotnet run --project src\Khadamat.WebAPI\Khadamat.WebAPI.csproj
```

### 3. تشغيل Frontend:
```bash
dotnet run --project src\Khadamat.WasmHost\Khadamat.WasmHost.csproj
```

### 4. اختبار الإصلاح:
- افتح المتصفح على `http://localhost:5028`
- سجل دخول بالحساب المناسب
- اختبر الميزة المصلحة

---

## 🔍 كيفية منع تكرار المشكلة

1. **قبل كل Commit**:
   - راجع `DEVELOPER_CHECKLIST.md`
   - تأكد من اتباع القواعد في `ARCHITECTURE_DECISIONS.md`

2. **عند إضافة ميزة جديدة**:
   - استخدم الأمثلة الصحيحة من هذا الملف
   - اختبر مع بيانات فارغة و null

3. **عند مراجعة الكود**:
   - تحقق من وجود Error Handling
   - تحقق من استخدام Layout الصحيح
   - تحقق من عرض Empty States

---

## 📞 الحصول على المساعدة

إذا واجهت مشكلة غير موجودة هنا:

1. راجع `ARCHITECTURE_DECISIONS.md` للقواعد العامة
2. راجع `DEVELOPER_CHECKLIST.md` للتحقق من الأساسيات
3. ابحث في الكود عن أمثلة مشابهة تعمل بشكل صحيح
4. تحقق من Console للأخطاء المفصلة

---

**آخر تحديث**: 2026-01-29
