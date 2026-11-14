$connectionString = "Data Source=SQL5065.site4now.net;Initial Catalog=db_abe0b0_khadamatdb;User Id=db_abe0b0_khadamatdb_admin;Password=Secure#OS2025;Encrypt=True;TrustServerCertificate=True;"

$sql = @"
-- 1. Seed Marketing Campaign for Sers El Lyan
IF NOT EXISTS (SELECT 1 FROM MarketingCampaigns WHERE CityId = 212)
BEGIN
    INSERT INTO MarketingCampaigns (
        CampaignName, CityId, CityName, GovernorateId, GovernorateName,
        IsActive, IsDefault, Slogan, MessageTemplate,
        SubscriptionOfferMonths, SubscriptionOfferText, FacebookCommunityUrl, TargetStage
    ) VALUES (
        N'حملة إطلاق مجتمع خدماوي في سرس الليان', 212, N'سرس الليان', 10, N'المنوفية',
        1, 0, N'خدماوي.. خدمات بلدنا في مكان واحد ❤️',
        N'👋 {PersonalizedGreeting} ❤️
عندنا خبر حلو ليك! 🎉
تم تسجيل خدمتك ({ServiceName}) على تطبيق خدماوي – KHADAMAWY.

🚀 خدماوي بدأ رحلته في {CityName}، بهدف يجمع أصحاب الحرف والمهن والخدمات في مكان واحد، ويساعد أهل المدينة يوصلوا للخدمة اللي محتاجينها بسهولة ويتواصلوا مباشرة مع أصحابها.
يعني ببساطة: اللي بيدور على الخدمة اللي بتقدمها في {CityName} يقدر يلاقيك ويوصل لك من خلال خدماوي!

🎁 وعشان تكون من أوائل المشاركين في مجتمع خدماوي، عندك عرض مميز:
اشتراك مجاني لأول 3 شهور! ❤️

📸 وعايزين نساعدك تظهر شغلك بأحسن صورة، ابعتلنا على واتساب:
1️⃣ صورة شخصية ليك، أو لوجو نشاطك، أو صورة الكارت الشخصي.
2️⃣ صور من شغلك وأعمالك السابقة.
3️⃣ صور المحل أو الورشة أو المعدات، أو أي صور تحب تعرضها للعملاء.

📝 ولو فيه بيانات ناقصة في خدمتك، نقدر نساعدك تستكملها علشان تظهر بشكل مميز وتكون معلوماتك واضحة للعملاء.

🔎 شوف خدمتك على خدماوي:
{ServiceUrl}

📲 حمّل تطبيق خدماوي وكن واحد من رواد مجتمعنا:
{AppDownloadUrl}

👍 وانضم لمجتمعنا على فيسبوك:
{FacebookPageUrl}

خدماوي.. خدمات بلدنا في مكان واحد! ❤️
إيد في إيد نساعد بعض، ونكبر شغل بعض، ونخلي الوصول للخدمات أسهل لكل أهل {CityName}. 💪',
        3, N'اشتراك مجاني لأول 3 شهور! ❤️',
        N'https://www.facebook.com/profile.php?id=61595461124579',
        N'Launch'
    );
END

-- General Fallback Campaign
IF NOT EXISTS (SELECT 1 FROM MarketingCampaigns WHERE IsDefault = 1)
BEGIN
    INSERT INTO MarketingCampaigns (
        CampaignName, CityId, CityName, GovernorateId, GovernorateName,
        IsActive, IsDefault, Slogan, MessageTemplate,
        SubscriptionOfferMonths, SubscriptionOfferText, FacebookCommunityUrl, TargetStage
    ) VALUES (
        N'القالب العام لجميع المدن', NULL, N'', NULL, N'',
        1, 1, N'خدماوي.. خلي شغلك يوصل للي بيدور عليه!',
        N'👋 {PersonalizedGreeting} ❤️
عندنا خبر حلو ليك! 🎉
تم تسجيل خدمتك ({ServiceName}) على تطبيق خدماوي – KHADAMAWY.

🚀 خدماوي هو تطبيق بيجمع أصحاب الحرف والمهن والخدمات في مكان واحد، وبيساعد العملاء يوصلوا للخدمات اللي محتاجينها بسهولة ويتواصلوا مباشرة مع مقدميها!
يعني ببساطة: اللي بيدور على الخدمة اللي بتقدمها يقدر يلاقيك ويوصل لك من خلال خدماوي!

🎁 وعشان تكون من أوائل المشاركين في مجتمع خدماوي، عندك عرض مميز:
اشتراك مجاني لأول 3 شهور! ❤️

📸 وعايزين نساعدك تظهر شغلك بأحسن صورة، ابعتلنا على واتساب:
1️⃣ صورة شخصية ليك، أو لوجو نشاطك، أو صورة الكارت الشخصي.
2️⃣ صور من شغلك وأعمالك السابقة.
3️⃣ صور المحل أو الورشة أو المعدات، أو أي صور تحب تعرضها للعملاء.

📝 ونقدر نساعدك تستكمل أي بيانات ناقصة في خدمتك لتظهر بشكل احترافي للعملاء.

🔎 شوف خدمتك على خدماوي:
{ServiceUrl}

📲 حمّل تطبيق خدماوي:
{AppDownloadUrl}

👍 وتابع صفحتنا على فيسبوك:
{FacebookPageUrl}

خدماوي.. خلي شغلك يوصل للي بيدور عليه! ❤️',
        3, N'اشتراك مجاني لأول 3 شهور! ❤️',
        N'https://www.facebook.com/profile.php?id=61595461124579',
        N'General'
    );
END

-- 2. Seed Professional Title Rules
IF NOT EXISTS (SELECT 1 FROM ProfessionalTitleRules WHERE ProfessionKeyword = N'مدرس')
BEGIN
    INSERT INTO ProfessionalTitleRules (ProfessionKeyword, MaleTitle, FemaleTitle, DefaultTitle, Priority, Notes) VALUES
    (N'مدرس', N'مستر', N'مس', N'مستر / مس', 10, N'المدرس والمدرسة لقبهما الحصري مستر ومس'),
    (N'معلم', N'مستر', N'مس', N'مستر / مس', 10, N'المعلم والمعلمة لقبهما مستر ومس'),
    (N'طبيب', N'دكتور', N'دكتورة', N'دكتور', 9, N'الأطباء البشريون والأسنان والبيطريون'),
    (N'دكتور', N'دكتور', N'دكتورة', N'دكتور', 9, N'الأطباء'),
    (N'مهندس', N'باشمهندس', N'باشمهندسة', N'باشمهندس', 8, N'المهندسون بمختلف التخصصات'),
    (N'صيدلي', N'دكتور', N'دكتورة', N'دكتور', 7, N'الصيادلة'),
    (N'محام', N'أستاذ', N'أستاذة', N'أستاذ', 6, N'المحامون والمستشارون القانونيون'),
    (N'محامي', N'أستاذ', N'أستاذة', N'أستاذ', 6, N'المحامون'),
    (N'محاسب', N'أستاذ', N'أستاذة', N'أستاذ', 5, N'المحاسبون والمراجعون'),
    (N'فني', N'أستاذ', N'أستاذة', N'أستاذ', 4, N'الفنيون بمختلف المجالات'),
    (N'حرفي', N'أستاذ', N'أستاذة', N'أستاذ', 3, N'أصحاب الحرف اليدوية والصنايعية'),
    (N'صنايعي', N'أستاذ', N'أستاذة', N'أستاذ', 3, N'الصنايعية'),
    (N'تاجر', N'أستاذ', N'أستاذة', N'أستاذ', 2, N'أصحاب الأنشطة التجارية والمحلات');
END

-- 3. Seed Provider Engagement Reminders (10 Types)
IF NOT EXISTS (SELECT 1 FROM ProviderEngagementReminders WHERE ReminderType = N'WelcomeAfterRegistration')
BEGIN
    INSERT INTO ProviderEngagementReminders (
        ReminderType, Title, Description, TargetChannel,
        WhatsAppTemplate, PushTitleTemplate, PushBodyTemplate, DeepLinkRoute, DaysAfterRegistration, DisplayOrder
    ) VALUES
    (
        N'WelcomeAfterRegistration', N'رسالة ترحيب ومتابعة بعد تسجيل الخدمة', N'ترحب بصاحب الخدمة وتعرفه ببدء الخدمة وعرض 3 شهور مجاناً ورابط فيسبوك', N'All',
        N'👋 {PersonalizedGreeting} ❤️
عندنا خبر حلو ليك! 🎉
تم تسجيل خدمتك ({ServiceName}) على تطبيق خدماوي – KHADAMAWY.

🚀 خدماوي بدأ رحلته في {CityName}، بهدف يجمع أصحاب الحرف والمهن والخدمات في مكان واحد، ويساعد أهل المدينة يوصلوا للخدمة اللي محتاجينها بسهولة ويتواصلوا مباشرة مع أصحابها.

🎁 وعشان تكون من أوائل المشاركين في مجتمع خدماوي، عندك عرض مميز:
اشتراك مجاني لأول 3 شهور! ❤️

📸 ابعتلنا على واتساب صورة شخصية أو لوجو، وصور من شغلك عشان نظهر خدمتك بأفضل شكل!

🔎 شوف خدمتك: {ServiceUrl}
📲 حمل التطبيق: {AppDownloadUrl}
👍 وانضم لمجتمعنا على فيسبوك: {FacebookPageUrl}',
        N'أهلاً بك في مجتمع خدماوي! 🎉',
        N'تم تسجيل خدمتك ({ServiceName}) بنجاح. استمتع بعرض 3 شهور مجاناً وأكمل بياناتك لزيادة ظهورك.',
        N'/service/{ServiceId}', 0, 1
    ),
    (
        N'CompleteBasicData', N'استكمال البيانات الأساسية للخدمة', N'تذكير باستكمال بيانات الخدمة الناقصة لتسهيل وصول العملاء', N'All',
        N'👋 {PersonalizedGreeting} ❤️
نتمنى شغلك يكون في أفضل حال!
لاحظنا إن ملف خدمتك ({ServiceName}) على خدماوي لسه محتاج بعض البيانات الأساسية:
📌 {MissingFields}

استكمال البيانات بيخلي خدمتك تظهر في أول نتائج البحث ويزود ثقة العملاء فيك!
تقدر تبعتلنا البيانات هنا على واتساب ونحدثها لك فوراً، أو تعدلها مباشرة من التطبيق:
🔗 {ServiceUrl}',
        N'خطوة صغيرة وتكمل ملف خدمتك! 💪',
        N'أكمل بيانات خدمتك ({ServiceName}) على خدماوي علشان تظهر في أول نتائج البحث للعملاء القريبين منك.',
        N'/service/{ServiceId}', 3, 2
    ),
    (
        N'AddProfileOrLogo', N'إضافة صورة شخصية أو شعار النشاط', N'تذكير بإضافة صورة تعريفية أو لوجو لزيادة المصداقية', N'All',
        N'👋 {PersonalizedGreeting} ❤️
الصورة بتفرق كتير في قرار العميل! 🌟
خدمتك ({ServiceName}) لسه مافيهاش صورة شخصية ليك أو لوجو لنشاطك التجاري أو صورة الكارت الشخصي.
ابعتلنا صورتك أو كارتك هنا على واتساب وهنضبطها ونحطها لخدمتك فوراً! 📸',
        N'صورة خدمتك هي أول انطباع! 📸',
        N'أضف صورتك الشخصية أو شعار نشاطك في خدمة ({ServiceName}) علشان تبني ثقة أكبر مع العملاء.',
        N'/service/{ServiceId}', 4, 3
    ),
    (
        N'AddWorkImages', N'إضافة صور الأعمال السابقة', N'تذكير بإرسال صور الأعمال والمشاريع السابقة والمعرض', N'All',
        N'👋 {PersonalizedGreeting} ❤️
شغلك يستاهل الناس تشوفه وتنبهر بيه! 📸✨
العملاء دايماً بيفضلوا مقدم الخدمة اللي عارض صور حقيقية من شغله السابق، ورشته أو معداته.
ابعتلنا صور من أفضل أعمالك هنا على واتساب وهنضيفها في معرض أعمال خدمتك ({ServiceName})!',
        N'شغلك يستاهل الناس تشوفه! 📸',
        N'العملاء بيفضلوا يشوفوا صور شغلك السابق. أضف صور أعمالك في خدمة ({ServiceName}) الآن.',
        N'/service/{ServiceId}', 7, 4
    ),
    (
        N'CompleteDescription', N'استكمال وصف الخدمة', N'تذكير بإضافة وصف احترافي للخدمات المقدمة والمميزات', N'All',
        N'👋 {PersonalizedGreeting} ❤️
الوصف الواضح بيجاوب على أسئلة العملاء بسرعة! 📝
حابين نساعدك تكتب وصف مميز لخدمتك ({ServiceName}) يشرح كل تفاصيل شغلك ومميزاتك والأسعار أو العروض اللي بتقدمها.
ابعتلنا نبذة عن خدماتك وهنصيغها لك بأفضل أسلوب تسويقي!',
        N'أخبر العملاء بالمزيد عن خدماتك 📝',
        N'وصف الخدمة الواضح بيزود طلبات العملاء. اكتب تفاصيل مميزات خدمتك ({ServiceName}) الآن.',
        N'/service/{ServiceId}', 5, 5
    ),
    (
        N'CompleteLocation', N'استكمال الموقع ومناطق الخدمة', N'تذكير بتحديد المدينة والعنوان بدقة لاستهداف العملاء القريبين', N'All',
        N'👋 {PersonalizedGreeting} ❤️
العملاء بيبحثوا دايماً عن أقرب مقدم خدمة ليهم! 📍
تأكد من تحديد منطقتك وعنوانك بدقة لخدمة ({ServiceName}) في {CityName} علشان نوجه ليك كل الطلبات اللي في منطقتك.
لو تحب تعدل أو تضيف مناطق بتغطيها ابعتلنا هنا فوراً!',
        N'خلّي عملاء منطقتك يلاقوك بسهولة! 📍',
        N'حدد موقعك ومناطق عملك بدقة في ({ServiceName}) علشان نوصلك بالعملاء القريبين منك.',
        N'/service/{ServiceId}', 6, 6
    ),
    (
        N'UpdateContacts', N'مراجعة وتحديث بيانات التواصل', N'تذكير بالتأكد من أرقام الهواتف والواتساب', N'All',
        N'👋 {PersonalizedGreeting} ❤️
تأكد إن العملاء يقدروا يوصلوا لك في أي وقت! 📞
راجع أرقام التواصل والواتساب لخدمة ({ServiceName}) للتأكد من إنها شغالة تمام وجاهزة لاستقبال اتصالات ورسائل العملاء.',
        N'راجع أرقام التواصل الخاصة بك 📞',
        N'تأكد من صحة أرقام الهاتف والواتساب في ({ServiceName}) لتجنب ضياع أي اتصال من عميل جديد.',
        N'/service/{ServiceId}', 10, 7
    ),
    (
        N'UpdateProfile', N'تحديث الملف الشخصي لمقدم الخدمة', N'دعوة لاستكمال ملف مقدم الخدمة وتوثيقه', N'All',
        N'👋 {PersonalizedGreeting} ❤️
الملف الشخصي الموثق بيخليك الخيار الأول للعملاء! 🌟
ادخل على حسابك في تطبيق خدماوي واستكمل بيانات بروفايلك وسنوات خبرتك، ونقدر نساعدك في توثيق الحساب (حساب معتمد) لزيادة الموثوقية!',
        N'وثّق حسابك وكن في المقدمة 🌟',
        N'استكمل بيانات ملفك الشخصي في خدماوي واحصل على شارة التوثيق لزيادة ثقة العملاء.',
        N'/profile', 14, 8
    ),
    (
        N'EncourageEngagement', N'رسالة تشجيعية لتنشيط مقدم الخدمة', N'رسالة ودية لتشجيع صاحب الخدمة على التفاعل ومتابعة الطلبات', N'All',
        N'👋 {PersonalizedGreeting} ❤️
صباح الخير يا بطل! نتمنى لك يوم مليان شغل وبركة. ✨
حبينا نطمن عليك ونفكرك إن منصة خدماوي في {CityName} في ضهرك دايماً بتوصلك بعملاء جدد.
لو محتاج أي مساعدة في تعديل خدمتك ({ServiceName}) أو إضافة عروض جديدة، إحنا معاك خطوة بخطوة على واتساب!',
        N'صباح الخير والنشاط! ✨',
        N'تطبيق خدماوي معاك خطوة بخطوة لتكبير شغلك ووصولك لعملاء جدد في منطقتك.',
        N'/service/{ServiceId}', 21, 9
    ),
    (
        N'Custom', N'رسالة مخصصة يكتبها المسؤول', N'رسالة حرة يقوم المسؤول بصياغتها لأي متابعة خاصة', N'All',
        N'👋 {PersonalizedGreeting} ❤️
بخصوص خدمتك ({ServiceName}) على خدماوي:
{CustomText}

لأي استفسار تواصل معنا مباشرة!',
        N'تنبيه بخصوص خدمتك على خدماوي 📢',
        N'{CustomText}',
        N'/service/{ServiceId}', 0, 10
    );
END
"@

try {
    $conn = New-Object System.Data.SqlClient.SqlConnection($connectionString)
    $conn.Open()
    $cmd = New-Object System.Data.SqlClient.SqlCommand($sql, $conn)
    $cmd.ExecuteNonQuery() | Out-Null
    $conn.Close()
    Write-Host "Initial engagement campaigns, professional titles, and reminders seeded successfully!" -ForegroundColor Green
} catch {
    Write-Error $_.Exception.Message
}
