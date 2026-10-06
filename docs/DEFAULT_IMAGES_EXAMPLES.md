# 🎯 أمثلة عملية - نظام الصور الافتراضية الذكي

## أمثلة واقعية لكيفية عمل النظام

### 🔧 **مثال 1: خدمة سباكة**

```csharp
// جميع هذه الأسماء ستحصل على نفس الصورة (سباكة)
DefaultImages.GetServiceImage("سباك منزلي", null)
DefaultImages.GetServiceImage("صيانة صحية", null)
DefaultImages.GetServiceImage("تركيب مياه", null)
DefaultImages.GetServiceImage("إصلاح حنفيات", null)
DefaultImages.GetServiceImage("Plumbing Services", null)
```

**النتيجة**: صورة احترافية لأدوات سباكة 🔧

---

### ⚡ **مثال 2: خدمة كهرباء**

```csharp
// جميع هذه الأسماء ستحصل على صورة كهرباء
DefaultImages.GetServiceImage("كهربائي محترف", null)
DefaultImages.GetServiceImage("صيانة إضاءة", null)
DefaultImages.GetServiceImage("تركيب محولات كهربائية", null)
DefaultImages.GetServiceImage("Electrical Repairs", null)
```

**النتيجة**: صورة أدوات كهربائية ⚡

---

### 🎨 **مثال 3: خدمة دهانات**

```csharp
// أسماء مختلفة، نفس الصورة
DefaultImages.GetServiceImage("دهان وديكور", null)
DefaultImages.GetServiceImage("طلاء جدران", null)
DefaultImages.GetServiceImage("أعمال جبس بورد", null)
DefaultImages.GetServiceImage("ديكورات حديثة", null)
```

**النتيجة**: صورة دهانات وألوان 🎨

---

### 💻 **مثال 4: خدمات تقنية**

```csharp
// خدمات تقنية متنوعة
DefaultImages.GetServiceImage("صيانة كمبيوتر", null)
DefaultImages.GetServiceImage("برمجة مواقع", null)
DefaultImages.GetServiceImage("تركيب شبكات", null)
DefaultImages.GetServiceImage("IT Support", null)
```

**النتيجة**: صورة تكنولوجيا 💻

---

### 🚗 **مثال 5: خدمات سيارات**

```csharp
// خدمات سيارات
DefaultImages.GetServiceImage("ميكانيكي سيارات", null)
DefaultImages.GetServiceImage("صيانة مركبات", null)
DefaultImages.GetServiceImage("Car Mechanic", null)
```

**النتيجة**: صورة سيارات وأدوات ميكانيكا 🚗

---

### 📚 **مثال 6: خدمات تعليمية**

```csharp
// خدمات تعليم
DefaultImages.GetServiceImage("دروس خصوصية", null)
DefaultImages.GetServiceImage("معلم رياضيات", null)
DefaultImages.GetServiceImage("تدريس لغات", null)
DefaultImages.GetServiceImage("كورسات تدريبية", null)
```

**النتيجة**: صورة تعليمية 📚

---

## 👤 أمثلة صور المستخدمين

### مستخدم ذكر

```csharp
var avatar = DefaultImages.GetUserAvatar("محمد أحمد", "Male", null);
// النتيجة: صورة زرقاء نيلية مع الحروف "MA"
```

![Male Avatar](https://ui-avatars.com/api/?name=MA&background=4F46E5&color=fff&size=200&bold=true&format=svg)

---

### مستخدمة أنثى

```csharp
var avatar = DefaultImages.GetUserAvatar("فاطمة علي", "Female", null);
// النتيجة: صورة وردية مع الحروف "FA"
```

![Female Avatar](https://ui-avatars.com/api/?name=FA&background=EC4899&color=fff&size=200&bold=true&format=svg)

---

### مستخدم بدون تحديد جنس

```csharp
var avatar = DefaultImages.GetUserAvatar("أحمد محمود", null, null);
// النتيجة: صورة بلون عشوائي (بناءً على hash الاسم) مع "AM"
```

---

## 📁 أمثلة صور التصنيفات

### تصنيف خدمات منزلية

```csharp
DefaultImages.GetCategoryImage("خدمات منزلية", null)
DefaultImages.GetCategoryImage("Home Services", null)
DefaultImages.GetCategoryImage("صيانة بيوت", null)
```

**النتيجة**: صورة منزل جميل 🏠

---

### تصنيف بناء وتشييد

```csharp
DefaultImages.GetCategoryImage("بناء وتشييد", null)
DefaultImages.GetCategoryImage("مقاولات عامة", null)
DefaultImages.GetCategoryImage("Construction", null)
```

**النتيجة**: صورة موقع بناء 🏗️

---

### تصنيف أعمال ومكاتب

```csharp
DefaultImages.GetCategoryImage("خدمات أعمال", null)
DefaultImages.GetCategoryImage("Business Services", null)
DefaultImages.GetCategoryImage("خدمات مكتبية", null)
```

**النتيجة**: صورة مكتب احترافي 💼

---

## 🎨 أمثلة توليد صور بألوان مخصصة

```csharp
// صورة بلون أخضر
var greenAvatar = DefaultImages.GenerateColoredAvatar("علي محمود", "10B981");

// صورة بلون برتقالي
var orangeAvatar = DefaultImages.GenerateColoredAvatar("سارة أحمد", "F97316");

// صورة بلون بنفسجي
var purpleAvatar = DefaultImages.GenerateColoredAvatar("خالد عبدالله", "8B5CF6");
```

---

## 🔄 سيناريوهات الاستخدام

### سيناريو 1: مستخدم جديد بدون صورة

```csharp
var user = new UserDto 
{ 
    FullName = "محمد علي",
    Gender = "Male",
    ProfileImageUrl = null  // لا توجد صورة
};

var avatar = DefaultImages.GetUserAvatar(user.FullName, user.Gender, user.ProfileImageUrl);
// النتيجة: صورة زرقاء مع "ME"
```

---

### سيناريو 2: خدمة جديدة بدون صورة

```csharp
var service = new ServiceDto
{
    Title = "سباك محترف - صيانة وتركيب",
    CategoryName = "سباكة",
    ImageUrl = null  // لا توجد صورة
};

var serviceImage = DefaultImages.GetServiceImage(service.Title, service.ImageUrl);
// النتيجة: صورة أدوات سباكة (لأن العنوان يحتوي على "سباك")
```

---

### سيناريو 3: مستخدم لديه صورة موجودة

```csharp
var user = new UserDto 
{ 
    FullName = "أحمد محمد",
    Gender = "Male",
    ProfileImageUrl = "https://example.com/my-photo.jpg"  // صورة موجودة
};

var avatar = DefaultImages.GetUserAvatar(user.FullName, user.Gender, user.ProfileImageUrl);
// النتيجة: "https://example.com/my-photo.jpg" (لن يتم استبدالها)
```

---

## 🧪 اختبار الذكاء

### اختبار 1: كلمات مركبة

```csharp
// سيتعرف على "سباك" في الجملة
DefaultImages.GetServiceImage("أفضل سباك في القاهرة", null)
// → صورة سباكة ✅

// سيتعرف على "كهرب" في الجملة
DefaultImages.GetServiceImage("كهربائي منازل وشركات", null)
// → صورة كهرباء ✅
```

---

### اختبار 2: لغات مختلفة

```csharp
// عربي
DefaultImages.GetServiceImage("نجار خشب", null)
// → صورة نجارة ✅

// إنجليزي
DefaultImages.GetServiceImage("Carpenter Services", null)
// → صورة نجارة ✅

// مختلط
DefaultImages.GetServiceImage("Plumber سباك", null)
// → صورة سباكة ✅
```

---

### اختبار 3: كلمات مترادفة

```csharp
// "صحي" = "سباكة"
DefaultImages.GetServiceImage("صيانة صحية", null)
// → صورة سباكة ✅

// "إضاءة" = "كهرباء"
DefaultImages.GetServiceImage("تركيب إضاءة", null)
// → صورة كهرباء ✅

// "موبيليا" = "نجارة"
DefaultImages.GetServiceImage("صناعة موبيليا", null)
// → صورة نجارة ✅
```

---

## 📊 إحصائيات التغطية

النظام الحالي يغطي:
- ✅ **14 نوع خدمة** مختلف
- ✅ **8 أنواع تصنيفات** رئيسية
- ✅ **60+ كلمة مفتاحية** عربية وإنجليزية
- ✅ **3 أنواع جنس** للمستخدمين (ذكر، أنثى، غير محدد)
- ✅ **8 ألوان** مختلفة للـ avatars

---

**آخر تحديث**: 26 يناير 2026
