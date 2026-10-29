using Khadamat.Domain.Entities;
using Khadamat.Domain.Enums;
using Khadamat.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Khadamat.Infrastructure.Persistence;

public static class KhadamatDbContextSeed
{
    public static async Task SeedAsync(KhadamatDbContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
    {
        try 
        {
            // Ensure AppSettings payment and contact columns exist in database
            try
            {
                await context.Database.ExecuteSqlRawAsync(@"
                    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('AppSettings') AND name = 'ContactWhatsApp')
                        ALTER TABLE AppSettings ADD ContactWhatsApp nvarchar(max) NULL;
                    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('AppSettings') AND name = 'InstaPayNumber')
                        ALTER TABLE AppSettings ADD InstaPayNumber nvarchar(max) NULL;
                    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('AppSettings') AND name = 'VodafoneCashNumber')
                        ALTER TABLE AppSettings ADD VodafoneCashNumber nvarchar(max) NULL;
                    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('AppSettings') AND name = 'OtherWalletNumber')
                        ALTER TABLE AppSettings ADD OtherWalletNumber nvarchar(max) NULL;
                    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('AppSettings') AND name = 'PaymentInstructions')
                        ALTER TABLE AppSettings ADD PaymentInstructions nvarchar(max) NULL;
                    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('AppSettings') AND name = 'EnableTesterTracking')
                        ALTER TABLE AppSettings ADD EnableTesterTracking BIT NOT NULL CONSTRAINT DF_AppSettings_EnableTesterTracking DEFAULT 1;
                    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('AppSettings') AND name = 'ShowContactNumbers')
                        ALTER TABLE AppSettings ADD ShowContactNumbers BIT NOT NULL CONSTRAINT DF_AppSettings_ShowContactNumbers DEFAULT 0;
                    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('AppSettings') AND name = 'RequireLoginToViewContactNumbers')
                        ALTER TABLE AppSettings ADD RequireLoginToViewContactNumbers BIT NOT NULL CONSTRAINT DF_AppSettings_RequireLoginToViewContactNumbers DEFAULT 1;

                    IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'ServiceImages')
                    BEGIN
                        CREATE TABLE ServiceImages (
                            Id INT IDENTITY(1,1) PRIMARY KEY,
                            ServiceId INT NOT NULL,
                            ImageUrl NVARCHAR(MAX) NOT NULL,
                            DisplayOrder INT NOT NULL DEFAULT 0,
                            IsMain BIT NOT NULL DEFAULT 0,
                            CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
                            CreatedBy NVARCHAR(MAX) NULL,
                            UpdatedAt DATETIME2 NULL,
                            UpdatedBy NVARCHAR(MAX) NULL,
                            IsDeleted BIT NOT NULL DEFAULT 0,
                            DeletedAt DATETIME2 NULL,
                            DeletedBy NVARCHAR(MAX) NULL,
                            CONSTRAINT FK_ServiceImages_Services_ServiceId FOREIGN KEY (ServiceId) REFERENCES Services(Id) ON DELETE CASCADE
                        );
                        CREATE INDEX IX_ServiceImages_ServiceId ON ServiceImages(ServiceId);
                    END
                    ELSE
                    BEGIN
                        IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('ServiceImages') AND name = 'CreatedBy')
                            ALTER TABLE ServiceImages ADD CreatedBy NVARCHAR(MAX) NULL;
                        IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('ServiceImages') AND name = 'UpdatedBy')
                            ALTER TABLE ServiceImages ADD UpdatedBy NVARCHAR(MAX) NULL;
                        IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('ServiceImages') AND name = 'DeletedAt')
                            ALTER TABLE ServiceImages ADD DeletedAt DATETIME2 NULL;
                        IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('ServiceImages') AND name = 'DeletedBy')
                            ALTER TABLE ServiceImages ADD DeletedBy NVARCHAR(MAX) NULL;
                    END

                    IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'ReminderCampaigns')
                    BEGIN
                        CREATE TABLE ReminderCampaigns (
                            Id INT IDENTITY(1,1) PRIMARY KEY,
                            Title NVARCHAR(250) NOT NULL,
                            InternalDescription NVARCHAR(MAX) NULL,
                            Message NVARCHAR(MAX) NOT NULL,
                            Language NVARCHAR(20) NOT NULL DEFAULT 'ar',
                            Status INT NOT NULL DEFAULT 1,
                            StartDate DATETIME2 NOT NULL,
                            EndDate DATETIME2 NULL,
                            RecurrenceType INT NOT NULL DEFAULT 1,
                            ScheduledTime TIME NOT NULL DEFAULT '20:00:00',
                            IntervalValue INT NOT NULL DEFAULT 1,
                            DaysOfWeek NVARCHAR(100) NULL,
                            DurationDays INT NULL,
                            MaxOccurrences INT NULL,
                            TimeZoneId NVARCHAR(100) NOT NULL DEFAULT 'Local',
                            Priority INT NOT NULL DEFAULT 1,
                            TargetAudience INT NOT NULL DEFAULT 0,
                            TargetRole NVARCHAR(100) NULL,
                            DisplayType INT NOT NULL DEFAULT 0,
                            ActionRoute NVARCHAR(300) NOT NULL DEFAULT '/',
                            IsActive BIT NOT NULL DEFAULT 1,
                            ExecutionCount INT NOT NULL DEFAULT 0,
                            LastTriggeredAt DATETIME2 NULL,
                            CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
                            CreatedBy NVARCHAR(100) NULL,
                            UpdatedAt DATETIME2 NULL,
                            UpdatedBy NVARCHAR(100) NULL,
                            IsDeleted BIT NOT NULL DEFAULT 0,
                            DeletedAt DATETIME2 NULL,
                            DeletedBy NVARCHAR(100) NULL
                        );
                    END

                    IF OBJECT_ID('ReminderCampaigns', 'U') IS NOT NULL
                    BEGIN
                        IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('ReminderCampaigns') AND name = 'BankCategory')
                            ALTER TABLE ReminderCampaigns ADD BankCategory NVARCHAR(100) NULL;
                        IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('ReminderCampaigns') AND name = 'LastSequentialIndex')
                            ALTER TABLE ReminderCampaigns ADD LastSequentialIndex INT NOT NULL DEFAULT 0;
                        IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('ReminderCampaigns') AND name = 'CampaignType')
                            ALTER TABLE ReminderCampaigns ADD CampaignType NVARCHAR(50) NOT NULL DEFAULT 'Regular';
                        IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('ReminderCampaigns') AND name = 'OverridesRegularReminders')
                            ALTER TABLE ReminderCampaigns ADD OverridesRegularReminders BIT NOT NULL DEFAULT 0;
                        IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('ReminderCampaigns') AND name = 'OccasionName')
                            ALTER TABLE ReminderCampaigns ADD OccasionName NVARCHAR(150) NULL;
                    END

                    IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'ReminderBankItems')
                    BEGIN
                        CREATE TABLE [dbo].[ReminderBankItems] (
                            [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
                            [Title] NVARCHAR(250) NOT NULL,
                            [Message] NVARCHAR(MAX) NOT NULL,
                            [Category] NVARCHAR(100) NOT NULL DEFAULT 'General',
                            [OrderIndex] INT NOT NULL DEFAULT 1,
                            [SuggestedTime] TIME NULL,
                            [ImageUrl] NVARCHAR(MAX) NULL,
                            [ActionRoute] NVARCHAR(300) NOT NULL DEFAULT '/',
                            [IsActive] BIT NOT NULL DEFAULT 1,
                            [UsageCount] INT NOT NULL DEFAULT 0,
                            [LastUsedAt] DATETIME2 NULL,
                            [CreatedAt] DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
                            [CreatedBy] NVARCHAR(100) NULL,
                            [UpdatedAt] DATETIME2 NULL,
                            [UpdatedBy] NVARCHAR(100) NULL,
                            [IsDeleted] BIT NOT NULL DEFAULT 0,
                            [DeletedAt] DATETIME2 NULL,
                            [DeletedBy] NVARCHAR(100) NULL
                        );
                    END

                    IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'TesterDevices')
                    BEGIN
                        CREATE TABLE [dbo].[TesterDevices] (
                            [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
                            [DeviceIdentifier] NVARCHAR(450) NOT NULL,
                            [UserId] NVARCHAR(MAX) NULL,
                            [TesterEmail] NVARCHAR(450) NULL,
                            [TesterName] NVARCHAR(MAX) NULL,
                            [PhoneNumber] NVARCHAR(MAX) NULL,
                            [DeviceModel] NVARCHAR(MAX) NULL,
                            [OperatingSystem] NVARCHAR(MAX) NULL,
                            [IpAddress] NVARCHAR(MAX) NULL,
                            [FirstSeenAt] DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
                            [LastSeenAt] DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
                            [LastStreakDate] DATETIME2 NULL,
                            [ConsecutiveDaysStreak] INT NOT NULL DEFAULT 1,
                            [TotalActiveDays] INT NOT NULL DEFAULT 1,
                            [TargetDays] INT NOT NULL DEFAULT 14,
                            [HasMissedStreak] BIT NOT NULL DEFAULT 0,
                            [Notes] NVARCHAR(MAX) NULL,
                            [CreatedAt] DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
                            [CreatedBy] NVARCHAR(MAX) NULL,
                            [UpdatedAt] DATETIME2 NULL,
                            [UpdatedBy] NVARCHAR(MAX) NULL,
                            [IsDeleted] BIT NOT NULL DEFAULT 0,
                            [DeletedAt] DATETIME2 NULL,
                            [DeletedBy] NVARCHAR(MAX) NULL
                        );
                        CREATE UNIQUE INDEX [IX_TesterDevices_DeviceIdentifier] ON [dbo].[TesterDevices]([DeviceIdentifier]);
                        CREATE INDEX [IX_TesterDevices_TesterEmail] ON [dbo].[TesterDevices]([TesterEmail]);
                    END

                    IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'TesterActivityLogs')
                    BEGIN
                        CREATE TABLE [dbo].[TesterActivityLogs] (
                            [Id] BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
                            [TesterDeviceId] INT NOT NULL,
                            [DeviceIdentifier] NVARCHAR(450) NOT NULL,
                            [UserId] NVARCHAR(MAX) NULL,
                            [TesterEmail] NVARCHAR(MAX) NULL,
                            [OpenedAt] DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
                            [VisitDate] DATETIME2 NOT NULL,
                            [IpAddress] NVARCHAR(MAX) NULL,
                            [DeviceModel] NVARCHAR(MAX) NULL,
                            [OperatingSystem] NVARCHAR(MAX) NULL,
                            [IsNativeApp] BIT NOT NULL DEFAULT 0,
                            [CreatedAt] DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
                            [CreatedBy] NVARCHAR(MAX) NULL,
                            [UpdatedAt] DATETIME2 NULL,
                            [UpdatedBy] NVARCHAR(MAX) NULL,
                            [IsDeleted] BIT NOT NULL DEFAULT 0,
                            [DeletedAt] DATETIME2 NULL,
                            [DeletedBy] NVARCHAR(MAX) NULL,
                            CONSTRAINT [FK_TesterActivityLogs_TesterDevices_TesterDeviceId] FOREIGN KEY ([TesterDeviceId]) REFERENCES [dbo].[TesterDevices] ([Id]) ON DELETE CASCADE
                        );
                        CREATE INDEX [IX_TesterActivityLogs_DeviceIdentifier] ON [dbo].[TesterActivityLogs]([DeviceIdentifier]);
                        CREATE INDEX [IX_TesterActivityLogs_VisitDate] ON [dbo].[TesterActivityLogs]([VisitDate]);
                        CREATE INDEX [IX_TesterActivityLogs_TesterDeviceId] ON [dbo].[TesterActivityLogs]([TesterDeviceId]);
                    END
                ");

                // Seed / renew the default Google Play tester campaign
                var existingTesterCampaign = await context.ReminderCampaigns
                    .FirstOrDefaultAsync(r => !r.IsDeleted &&
                        r.Title == "تذكير المختبرين بالاستخدام اليومي");

                if (existingTesterCampaign == null)
                {
                    // First time seed
                    var defaultTesterReminder = new ReminderCampaign
                    {
                        Title = "تذكير المختبرين بالاستخدام اليومي",
                        InternalDescription = "دعم الاختبار المغلق لتطبيق خدماوى على Google Play",
                        Message = "مرحبًا بك في اختبار خدماوى! افتح التطبيق اليوم، وجرّب إحدى وظائفه، وساعدنا في اكتشاف أي مشكلة أو اقتراح تحسين قبل الإطلاق الرسمي. شكرًا لمشاركتك.",
                        Language = "ar",
                        Status = ReminderStatus.Active,
                        StartDate = DateTime.UtcNow,
                        EndDate = null, // No hard end date - admin controls via Status
                        RecurrenceType = ReminderRecurrenceType.Daily,
                        ScheduledTime = new TimeSpan(20, 0, 0),
                        IntervalValue = 1,
                        DurationDays = null, // Indefinite
                        TimeZoneId = "Local",
                        Priority = ReminderPriority.Normal,
                        TargetAudience = ReminderTargetAudience.Testers,
                        DisplayType = ReminderDisplayType.NotificationBar,
                        ActionRoute = "/",
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow,
                        CreatedBy = "System"
                    };
                    await context.ReminderCampaigns.AddAsync(defaultTesterReminder);
                    await context.SaveChangesAsync();
                }
                else if (existingTesterCampaign.Status == ReminderStatus.Active &&
                         existingTesterCampaign.EndDate.HasValue &&
                         existingTesterCampaign.EndDate.Value < DateTime.UtcNow)
                {
                    // Campaign is still marked Active but EndDate has passed — auto-renew it
                    existingTesterCampaign.StartDate = DateTime.UtcNow;
                    existingTesterCampaign.EndDate = null; // Remove expiry to keep it running
                    existingTesterCampaign.DurationDays = null;
                    existingTesterCampaign.UpdatedAt = DateTime.UtcNow;
                    existingTesterCampaign.UpdatedBy = "System";
                    await context.SaveChangesAsync();
                }

                // Seed Default Reminder Bank Items if empty
                if (!await context.ReminderBankItems.AnyAsync())
                {
                    var defaultBank = new List<ReminderBankItem>
                    {
                        new ReminderBankItem
                        {
                            Title = "☀️ صباح الفل يا بطل!",
                            Message = "افتح عينك على الدنيا بابتسامة 😄 ولو عندك حاجة محتاجة تتظبط، خدماوي يساعدك تلاقي الخدمة المناسبة.",
                            Category = "صباحية",
                            OrderIndex = 1,
                            SuggestedTime = new TimeSpan(8, 0, 0),
                            ActionRoute = "/services",
                            IsActive = true
                        },
                        new ReminderBankItem
                        {
                            Title = "☕ القهوة بتقولك صباح الخير!",
                            Message = "خد رشفة قهوة، وافتح خدماوي… يمكن تلاقي الحل قبل ما القهوة تبرد! 😂",
                            Category = "صباحية",
                            OrderIndex = 2,
                            SuggestedTime = new TimeSpan(8, 30, 0),
                            ActionRoute = "/",
                            IsActive = true
                        },
                        new ReminderBankItem
                        {
                            Title = "🚀 يلا نطير بالإنجازات!",
                            Message = "سيب الكسل ياخد أجازة النهارده 😎 وابحث على خدماوي عن حد يساعدك تخلّص اللي وراك.",
                            Category = "صباحية",
                            OrderIndex = 3,
                            SuggestedTime = new TimeSpan(9, 0, 0),
                            ActionRoute = "/search",
                            IsActive = true
                        },
                        new ReminderBankItem
                        {
                            Title = "🔧 الحنفية عاملة فيها دي جي؟",
                            Message = "لو البيت قرر يعمل حفلة أصوات غريبة، متقلقش! دور على فني مناسب في خدماوي. 😅",
                            Category = "صباحية",
                            OrderIndex = 4,
                            SuggestedTime = new TimeSpan(9, 15, 0),
                            ActionRoute = "/services",
                            IsActive = true
                        },
                        new ReminderBankItem
                        {
                            Title = "🌞 الشمس طلعت.. قوم يا أسطورة!",
                            Message = "نهار جديد وفرصة جديدة! خد لفة على خدماوي وشوف إيه اللي ممكن يسهّل يومك.",
                            Category = "صباحية",
                            OrderIndex = 5,
                            SuggestedTime = new TimeSpan(9, 30, 0),
                            ActionRoute = "/",
                            IsActive = true
                        },
                        new ReminderBankItem
                        {
                            Title = "🦸‍♂️ البطل مش لازم يعمل كل حاجة!",
                            Message = "حتى الأبطال بيستعينوا بالمحترفين 😄 اكتشف الحرفيين ومقدمي الخدمات على خدماوي.",
                            Category = "صباحية",
                            OrderIndex = 6,
                            SuggestedTime = new TimeSpan(9, 45, 0),
                            ActionRoute = "/services",
                            IsActive = true
                        },
                        new ReminderBankItem
                        {
                            Title = "🛍️ صباحك فرص يا كينج!",
                            Message = "قبل ما تشتري، بص على إعلانات خدماوي… يمكن تلاقي اختيار مناسب يخلّي يومك أحلى.",
                            Category = "صباحية",
                            OrderIndex = 7,
                            SuggestedTime = new TimeSpan(10, 0, 0),
                            ActionRoute = "/ads",
                            IsActive = true
                        },
                        new ReminderBankItem
                        {
                            Title = "😂 المهمة المستحيلة؟ سهلة!",
                            Message = "في حاجة مأجلها من الأسبوع اللي فات؟ ابدأ بالبحث عن حد متخصص يساعدك تخلصها.",
                            Category = "صباحية",
                            OrderIndex = 8,
                            SuggestedTime = new TimeSpan(10, 15, 0),
                            ActionRoute = "/search",
                            IsActive = true
                        },
                        new ReminderBankItem
                        {
                            Title = "🎯 صباحك فل وإنجازك عال!",
                            Message = "حط هدف صغير لنهارده وابدأ بيه. خدماوي يسهّل عليك اكتشاف الخدمات اللي تحتاجها.",
                            Category = "صباحية",
                            OrderIndex = 9,
                            SuggestedTime = new TimeSpan(10, 30, 0),
                            ActionRoute = "/services",
                            IsActive = true
                        },
                        new ReminderBankItem
                        {
                            Title = "🐴 الحمار الشاطر بدأ يومه!",
                            Message = "ما تسيبش المهام تركب فوق دماغك 😂 افتح خدماوي ودوّر على اللي يساعدك تنجزها.",
                            Category = "صباحية",
                            OrderIndex = 10,
                            SuggestedTime = new TimeSpan(10, 45, 0),
                            ActionRoute = "/",
                            IsActive = true
                        },
                        new ReminderBankItem
                        {
                            Title = "🥪 الساندوتش خلص.. والمهام لأ!",
                            Message = "استراحتك تستاهل ابتسامة 😄 وبعدها خد دقيقة وشوف على خدماوي مين يقدر يساعدك في اللي وراك.",
                            Category = "وسط اليوم",
                            OrderIndex = 1,
                            SuggestedTime = new TimeSpan(12, 0, 0),
                            ActionRoute = "/services",
                            IsActive = true
                        },
                        new ReminderBankItem
                        {
                            Title = "😎 الظهر جه.. ولسه الحماس!",
                            Message = "اشحن طاقتك بحاجة بتحبها، وبعدها ابحث على خدماوي عن الخدمة اللي هتوفر عليك وقت ومجهود.",
                            Category = "وسط اليوم",
                            OrderIndex = 2,
                            SuggestedTime = new TimeSpan(12, 30, 0),
                            ActionRoute = "/search",
                            IsActive = true
                        },
                        new ReminderBankItem
                        {
                            Title = "🔍 المحقق كرومبو في الخدمة!",
                            Message = "عندك طلب محيّرك؟ شغّل حسّك البوليسي وابحث على خدماوي عن الخدمة أو المنتج اللي محتاجه. 🕵️",
                            Category = "وسط اليوم",
                            OrderIndex = 3,
                            SuggestedTime = new TimeSpan(13, 0, 0),
                            ActionRoute = "/search",
                            IsActive = true
                        },
                        new ReminderBankItem
                        {
                            Title = "⚡ الأسلاك عاملة حفلة؟",
                            Message = "لو الأجهزة بتطلع أصوات كأنها فرقة موسيقية 😂 دور على فني مناسب من دليل خدمات خدماوي.",
                            Category = "وسط اليوم",
                            OrderIndex = 4,
                            SuggestedTime = new TimeSpan(13, 30, 0),
                            ActionRoute = "/services",
                            IsActive = true
                        },
                        new ReminderBankItem
                        {
                            Title = "💸 المحفظة بتقول: بالراحة!",
                            Message = "قبل ما تدفع، خد جولة في الإعلانات وقارن الخيارات المتاحة. المحفظة هتشكرك! 😄",
                            Category = "وسط اليوم",
                            OrderIndex = 5,
                            SuggestedTime = new TimeSpan(14, 0, 0),
                            ActionRoute = "/ads",
                            IsActive = true
                        },
                        new ReminderBankItem
                        {
                            Title = "🛒 جولة تسوّق من غير زحمة!",
                            Message = "اتفرج على المنتجات المتاحة في سوق خدماوي وإنت مرتاح؛ الشوبينج مش لازم يبقى ماراثون. 😂",
                            Category = "وسط اليوم",
                            OrderIndex = 6,
                            SuggestedTime = new TimeSpan(14, 30, 0),
                            ActionRoute = "/marketplace",
                            IsActive = true
                        },
                        new ReminderBankItem
                        {
                            Title = "🥱 ممنوع الاستسلام للنعاس!",
                            Message = "خد نفس وافرد ضهرك يا بطل! ولو فيه مهمة معطلاك، ابحث على خدماوي عن حد يساعدك.",
                            Category = "وسط اليوم",
                            OrderIndex = 7,
                            SuggestedTime = new TimeSpan(15, 0, 0),
                            ActionRoute = "/services",
                            IsActive = true
                        },
                        new ReminderBankItem
                        {
                            Title = "💡 لمبة الأفكار نورت!",
                            Message = "افتكرت حاجة محتاجها؟ قبل ما تطير من دماغك تاني، دوّر عليها في خدماوي. 😄",
                            Category = "وسط اليوم",
                            OrderIndex = 8,
                            SuggestedTime = new TimeSpan(15, 30, 0),
                            ActionRoute = "/search",
                            IsActive = true
                        },
                        new ReminderBankItem
                        {
                            Title = "🎁 الفضول مش عيب يا نجم!",
                            Message = "خد لفة سريعة على عروض وإعلانات خدماوي؛ يمكن تكتشف حاجة مفيدة ما كانتش في بالك.",
                            Category = "وسط اليوم",
                            OrderIndex = 9,
                            SuggestedTime = new TimeSpan(16, 0, 0),
                            ActionRoute = "/ads",
                            IsActive = true
                        },
                        new ReminderBankItem
                        {
                            Title = "😂 فاصل ضحك وإنجاز!",
                            Message = "افصل دقيقة عن الزحمة، وارجع بطاقة جديدة. خدماوي هنا لو محتاج تلاقي خدمة أو منتج.",
                            Category = "وسط اليوم",
                            OrderIndex = 10,
                            SuggestedTime = new TimeSpan(16, 30, 0),
                            ActionRoute = "/",
                            IsActive = true
                        },
                        new ReminderBankItem
                        {
                            Title = "🌙 مساء العسل يا جميل!",
                            Message = "اقفل وضع الجري وافتح وضع الروقان 😌 ولو فيه حاجة محتاجة تتعمل، اكتشف الخدمات على خدماوي.",
                            Category = "مسائية",
                            OrderIndex = 1,
                            SuggestedTime = new TimeSpan(18, 0, 0),
                            ActionRoute = "/services",
                            IsActive = true
                        },
                        new ReminderBankItem
                        {
                            Title = "🛋️ الكنبة نادت عليك؟",
                            Message = "استريح براحتك، ومن مكانك تقدر تتصفح إعلانات خدماوي وتشوف الخيارات اللي تهمك. 😎",
                            Category = "مسائية",
                            OrderIndex = 2,
                            SuggestedTime = new TimeSpan(18, 30, 0),
                            ActionRoute = "/ads",
                            IsActive = true
                        },
                        new ReminderBankItem
                        {
                            Title = "📝 لسه في مهام بتبصلك؟",
                            Message = "متخليهاش تكسب الجولة 😂 ابدأ بخطوة بسيطة وابحث على خدماوي عن الخدمة المناسبة.",
                            Category = "مسائية",
                            OrderIndex = 3,
                            SuggestedTime = new TimeSpan(19, 0, 0),
                            ActionRoute = "/search",
                            IsActive = true
                        },
                        new ReminderBankItem
                        {
                            Title = "🕵️ مهمة سرية قبل العشا!",
                            Message = "المطلوب: خدمة أو منتج. المحقق: إنت. ومكان البحث: خدماوي! يلا يا أسطورة. 😄",
                            Category = "مسائية",
                            OrderIndex = 4,
                            SuggestedTime = new TimeSpan(19, 30, 0),
                            ActionRoute = "/search",
                            IsActive = true
                        },
                        new ReminderBankItem
                        {
                            Title = "🛍️ الشوبينج بيقولك تعالى!",
                            Message = "خد جولة في سوق المنتجات على خدماوي وإنت مرتاح؛ لا زحمة ولا جري ورا المواصلات. 😄",
                            Category = "مسائية",
                            OrderIndex = 5,
                            SuggestedTime = new TimeSpan(20, 0, 0),
                            ActionRoute = "/marketplace",
                            IsActive = true
                        },
                        new ReminderBankItem
                        {
                            Title = "✨ بكرة مش محتاج مفاجآت!",
                            Message = "جهّز احتياجاتك من الليلة، وابحث عن الخدمات اللي ممكن تخلّي بكرة أهدى وأسهل.",
                            Category = "مسائية",
                            OrderIndex = 6,
                            SuggestedTime = new TimeSpan(20, 30, 0),
                            ActionRoute = "/services",
                            IsActive = true
                        },
                        new ReminderBankItem
                        {
                            Title = "😂 آخر لفة يا معلم!",
                            Message = "قبل ما تقول تصبحوا على خير، خد جولة خفيفة في خدماوي… يمكن تلاقي اللي كنت بتدور عليه.",
                            Category = "مسائية",
                            OrderIndex = 7,
                            SuggestedTime = new TimeSpan(21, 0, 0),
                            ActionRoute = "/",
                            IsActive = true
                        },
                        new ReminderBankItem
                        {
                            Title = "💡 أهي نورت في دماغك!",
                            Message = "افتكرت حاجة ناقصاك؟ اكتبها في البحث قبل ما تنساها تاني… الذاكرة مش دفتر ملاحظات! 😂",
                            Category = "مسائية",
                            OrderIndex = 8,
                            SuggestedTime = new TimeSpan(21, 15, 0),
                            ActionRoute = "/search",
                            IsActive = true
                        },
                        new ReminderBankItem
                        {
                            Title = "🔧 الصنايعي مش بيظهر بالسحر!",
                            Message = "لو محتاج فني أو حرفي، ابدأ البحث على خدماوي وخلي أول خطوة سهلة وواضحة.",
                            Category = "مسائية",
                            OrderIndex = 9,
                            SuggestedTime = new TimeSpan(21, 30, 0),
                            ActionRoute = "/services",
                            IsActive = true
                        },
                        new ReminderBankItem
                        {
                            Title = "🌛 تصبح على خير يا نجم!",
                            Message = "نام على ابتسامة وخلي بكرة مليان أمل 😄 وخدماوي موجود لما تحتاج تكتشف خدمة أو منتج جديد.",
                            Category = "مسائية",
                            OrderIndex = 10,
                            SuggestedTime = new TimeSpan(21, 45, 0),
                            ActionRoute = "/",
                            IsActive = true
                        },
                        // يوم الجمعة
                        new ReminderBankItem
                        {
                            Title = "جمعة مباركة وطيبة 🕊️",
                            Message = "تقبل الله طاعاتكم وبارك في أوقاتكم. نتمنى لكم يوماً سعيداً ومباركاً برفقة الأهل والأحباب.",
                            Category = "يوم الجمعة",
                            OrderIndex = 1,
                            SuggestedTime = new TimeSpan(10, 0, 0),
                            ActionRoute = "/",
                            IsActive = true
                        },
                        new ReminderBankItem
                        {
                            Title = "نور ما بين الجمعتين ✨",
                            Message = "لا تنس قراءة سورة الكهف والإكثار من الصلاة على النبي ﷺ. جمعة مباركة وطيبة عليكم جميعاً.",
                            Category = "يوم الجمعة",
                            OrderIndex = 2,
                            SuggestedTime = new TimeSpan(10, 30, 0),
                            ActionRoute = "/",
                            IsActive = true
                        },
                        new ReminderBankItem
                        {
                            Title = "ساعة استجابة 🤲",
                            Message = "في يوم الجمعة ساعة لا يوافقها عبد مسلم يسأل الله خيراً إلا أعطاه إياه. اذكروا من تحبون بدعوة صادقة.",
                            Category = "يوم الجمعة",
                            OrderIndex = 3,
                            SuggestedTime = new TimeSpan(16, 30, 0),
                            ActionRoute = "/",
                            IsActive = true
                        },
                        // مناسبات وأعياد
                        new ReminderBankItem
                        {
                            Title = "شم نسيم مبهج وسعيد! 🌸🥚",
                            Message = "كل سنة وأنتم طيبين! يومكم مليان بهجة وفرحة ولمّة عيلة وأوقات حلوة في أجواء الربيع.",
                            Category = "مناسبات وأعياد",
                            OrderIndex = 1,
                            SuggestedTime = new TimeSpan(9, 30, 0),
                            ActionRoute = "/",
                            IsActive = true
                        },
                        new ReminderBankItem
                        {
                            Title = "فسحة وراحة في شم النسيم 🌿☀️",
                            Message = "استمتع بيوم شم النسيم مع الحبايب والفسحة الحلوة، وخدماوي معاك دائماً في كل وقت.",
                            Category = "مناسبات وأعياد",
                            OrderIndex = 2,
                            SuggestedTime = new TimeSpan(12, 0, 0),
                            ActionRoute = "/",
                            IsActive = true
                        },
                        new ReminderBankItem
                        {
                            Title = "عيد فطر سعيد ومبارك! 🎉🍬",
                            Message = "كل عام وأنتم بألف خير وسعادة! تقبل الله طاعاتكم وأعاده عليكم بالخير واليمن والبركات.",
                            Category = "مناسبات وأعياد",
                            OrderIndex = 3,
                            SuggestedTime = new TimeSpan(8, 0, 0),
                            ActionRoute = "/",
                            IsActive = true
                        },
                        new ReminderBankItem
                        {
                            Title = "عيد أضحى مبارك 🐑🎈",
                            Message = "أضحى مبارك وكل عام وأنتم بخير! أيام فرحة وبركة وطاعات ولمة عيلة لا تُعوض.",
                            Category = "مناسبات وأعياد",
                            OrderIndex = 4,
                            SuggestedTime = new TimeSpan(8, 30, 0),
                            ActionRoute = "/",
                            IsActive = true
                        },
                        // شهر رمضان
                        new ReminderBankItem
                        {
                            Title = "رمضان كريم ومبارك! 🌙✨",
                            Message = "أهلاً بشهر الخير والبركات! نسأل الله أن يتقبل صيامكم وقيامكم وصالح أعمالكم.",
                            Category = "شهر رمضان",
                            OrderIndex = 1,
                            SuggestedTime = new TimeSpan(15, 0, 0),
                            ActionRoute = "/",
                            IsActive = true
                        },
                        new ReminderBankItem
                        {
                            Title = "صوماً مقبولاً وإفطاراً شهياً 🥣🌴",
                            Message = "تقبل الله طاعتكم وبلل عروقكم، إفطار هنيء وصحة وعافية لجميع أفراد الأسرة.",
                            Category = "شهر رمضان",
                            OrderIndex = 2,
                            SuggestedTime = new TimeSpan(18, 15, 0),
                            ActionRoute = "/",
                            IsActive = true
                        },
                        new ReminderBankItem
                        {
                            Title = "بركة السحور 🥛🫓",
                            Message = "تسحروا فإن في السحور بركة. تقبل الله قيامكم وطاعاتكم وأعانكم على صيام الغد.",
                            Category = "شهر رمضان",
                            OrderIndex = 3,
                            SuggestedTime = new TimeSpan(3, 0, 0),
                            ActionRoute = "/",
                            IsActive = true
                        },
                        // العشر من ذي الحجة
                        new ReminderBankItem
                        {
                            Title = "أيام العشر الفضيلة 🕋🌿",
                            Message = "ما من أيام العمل الصالح فيها أحب إلى الله من هذه الأيام! اغتنموا أوقاتكم بالذكر والتكبير والطاعات.",
                            Category = "العشر من ذي الحجة",
                            OrderIndex = 1,
                            SuggestedTime = new TimeSpan(9, 0, 0),
                            ActionRoute = "/",
                            IsActive = true
                        },
                        new ReminderBankItem
                        {
                            Title = "فضل صيام يوم عرفة 🤲🕊️",
                            Message = "خير الدعاء دعاء يوم عرفة. نسأل الله أن يتقبل دعواتكم ويغفر ذنوبكم ويبلغكم العيد مسرورين.",
                            Category = "العشر من ذي الحجة",
                            OrderIndex = 2,
                            SuggestedTime = new TimeSpan(14, 0, 0),
                            ActionRoute = "/",
                            IsActive = true
                        }
                    };

                    await context.ReminderBankItems.AddRangeAsync(defaultBank);
                    await context.SaveChangesAsync();
                }

            }
            catch { }

            await SeedRolesAsync(roleManager);
            await SeedUsersAsync(userManager);

            if (!await context.MainCategories.AnyAsync())
            {
                var mainCategories = new List<MainCategory>
                {
                    new MainCategory { Name = "صحة", Icon = "🏥", Color = "medical", DisplayOrder = 1, ImageUrl = "images/maincategories/cat_1.webp" },
                    new MainCategory { Name = "تعليم", Icon = "🎓", Color = "education", DisplayOrder = 2, ImageUrl = "images/maincategories/cat_2.webp" },
                    new MainCategory { Name = "متاجر", Icon = "🏪", Color = "stores", DisplayOrder = 3, ImageUrl = "images/maincategories/cat_3.webp" },
                    new MainCategory { Name = "ماكولات ومشروبات", Icon = "🍲", Color = "food", DisplayOrder = 4, ImageUrl = "images/maincategories/cat_4.webp" },
                    new MainCategory { Name = "مكاتب", Icon = "🏢", Color = "offices", DisplayOrder = 5, ImageUrl = "images/maincategories/cat_5.webp" },
                    new MainCategory { Name = "حرفيون", Icon = "🛠️", Color = "crafts", DisplayOrder = 6, ImageUrl = "images/maincategories/cat_8.webp" },
                    new MainCategory { Name = "تسوق اون لين", Icon = "🛒", Color = "online", DisplayOrder = 7, ImageUrl = "images/maincategories/cat_9.webp" },
                    new MainCategory { Name = "مواصلات", Icon = "🚗", Color = "transport", DisplayOrder = 8, ImageUrl = "images/maincategories/cat_10.webp" },
                    new MainCategory { Name = "صيانة سيارات", Icon = "🔧", Color = "auto", DisplayOrder = 9, ImageUrl = "images/maincategories/cat_11.webp" },
                    new MainCategory { Name = "خدمات حكومية", Icon = "🏛️", Color = "gov", DisplayOrder = 10, ImageUrl = "images/maincategories/cat_12.webp" },
                    new MainCategory { Name = "متجر السلع", Icon = "🛍️", Color = "marketplace", DisplayOrder = 11, ImageUrl = "images/maincategories/cat_7.webp" },
                    new MainCategory { Name = "خدمات اخرى", Icon = "✨", Color = "other", DisplayOrder = 12, ImageUrl = "images/maincategories/cat_6.webp" }
                };
                await context.MainCategories.AddRangeAsync(mainCategories);
                await context.SaveChangesAsync();
            }
            // NOTE: If MainCategories table already has data, do NOT modify it.
            // ImageUrl and all other fields are managed exclusively through the Admin UI.
            // Agreed policy: Seed data only runs once when the table is empty.

            if (!await context.Categories.AnyAsync())
            {
                await SeedCategoriesAndSubCategoriesAsync(context);
            }

            if (!await context.MarketplaceCategories.AnyAsync())
            {
                await SeedMarketplaceCategoriesAsync(context);
            }

            await SeedLocationsAsync(context);
            await SeedRandomUsersAsync(userManager, context);
            await SeedServicesAsync(context);
            await SeedRatingsAsync(context);
            await SeedMessagesAsync(context);

            if (!await context.Ads.AnyAsync())
            {
                await SeedAdsAsync(context);
            }

            if (!await context.SubscriptionPlans.AnyAsync())
            {
                await SeedSubscriptionPlansAsync(context);
            }

            await EnsureProviderSubscriptionsAsync(context);

            if (!await context.MarketplaceItems.AnyAsync())
            {
                await SeedMarketplaceItemsAsync(context);
            }

            // ── Advertisement System Data ───────────────────────────────
            if (!await context.AdPackages.AnyAsync())
                await SeedAdPackagesAsync(context);

            if (!await context.PointRewardRules.AnyAsync())
                await SeedPointRewardRulesAsync(context);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"SEED ERROR: {ex.Message}");
        }
    }

    private static async Task SeedMarketplaceItemsAsync(KhadamatDbContext context)
    {
        var users = await context.Users.Take(5).ToListAsync();
        var subCats = await context.MarketplaceSubCategories.Take(10).ToListAsync();
        var cities = await context.Cities.Take(2).ToListAsync();

        if (!users.Any() || !subCats.Any()) return;

        var random = new Random();
        for (int i = 1; i <= 10; i++)
        {
            var user = users[random.Next(users.Count)];
            var subCat = subCats[random.Next(subCats.Count)];
            var city = cities.Any() ? cities[random.Next(cities.Count)] : null;

            var item = new MarketplaceItem(
                $"سلعة تجريبية {i}",
                $"وصف السلعة التجريبية رقم {i}. هذه السلعة مخصصة للاختبار فقط.",
                random.Next(100, 5000),
                user.Id,
                subCat.CategoryId,
                "01000000000",
                subCat.Id,
                city?.Id,
                i % 2 == 0 ? "New" : "Used"
            );

            if (i <= 4) item.Approve(); // Approve some items
            if (i == 1 || i == 5) item.SetFeatured(7);
            if (i == 2 || i == 6) item.SetPromoted(7);

            context.MarketplaceItems.Add(item);
        }
        await context.SaveChangesAsync();
    }

    private static async Task SeedSubscriptionPlansAsync(KhadamatDbContext context)
    {
        var plans = new List<SubscriptionPlan>
        {
            new SubscriptionPlan("الخطة المجانية", 0, 90, 5, false),
            new SubscriptionPlan("الباقة الأساسية", 150, 30, 10, false),
            new SubscriptionPlan("الباقة المميزة (Premium)", 400, 90, 50, true),
            new SubscriptionPlan("الباقة السنوية للمحترفين", 1500, 365, 100, true)
        };
        await context.SubscriptionPlans.AddRangeAsync(plans);
        await context.SaveChangesAsync();
    }

    public static async Task EnsureProviderSubscriptionsAsync(KhadamatDbContext context)
    {
        try
        {
            // Ensure Free Plan exists (90 days)
            var freePlan = await context.SubscriptionPlans.FirstOrDefaultAsync(p => p.Price == 0);
            if (freePlan == null)
            {
                freePlan = new SubscriptionPlan("الخطة المجانية", 0, 90, 5, false);
                context.SubscriptionPlans.Add(freePlan);
                await context.SaveChangesAsync();
            }

            // Get all providers
            var providers = await context.ProviderProfiles
                .Include(p => p.Subscription)
                .ToListAsync();

            bool changed = false;
            foreach (var provider in providers)
            {
                // If provider has no subscription or no active subscription
                if (provider.SubscriptionId == null || provider.Subscription == null || !provider.Subscription.IsActive)
                {
                    var newSub = new ProviderSubscription(provider.Id, freePlan.Id, freePlan.DurationInDays > 0 ? freePlan.DurationInDays : 90);
                    context.ProviderSubscriptions.Add(newSub);
                    await context.SaveChangesAsync();

                    provider.SubscriptionId = newSub.Id;
                    changed = true;
                }
            }

            if (changed)
            {
                await context.SaveChangesAsync();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[EnsureProviderSubscriptionsAsync] Error: {ex.Message}");
        }
    }


    private static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
    {
        string[] roles = { "SuperAdmin", "SystemAdmin", "User" };
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));
        }
    }

    private static async Task SeedUsersAsync(UserManager<ApplicationUser> userManager)
    {
        // System Admin
        if (await userManager.FindByEmailAsync("admin@khadamat.com") == null)
        {
            var admin = new ApplicationUser { UserName = "Admin", Email = "admin@khadamat.com", FullName = "System Admin", Role = UserRole.SystemAdmin, EmailConfirmed = true };
            await userManager.CreateAsync(admin, "Admin@123");
            await userManager.AddToRoleAsync(admin, "SystemAdmin");
        }

        // Super Admin
        if (await userManager.FindByNameAsync("SuperAdmin") == null)
        {
            var superAdmin = new ApplicationUser { UserName = "SuperAdmin", Email = "superadmin@khadamat.com", FullName = "Super Admin User", Role = UserRole.SuperAdmin, EmailConfirmed = true };
            await userManager.CreateAsync(superAdmin, "Admin@123");
            await userManager.AddToRoleAsync(superAdmin, "SuperAdmin");
        }

        // Regular User
        if (await userManager.FindByNameAsync("RegularUser") == null)
        {
            var regularUser = new ApplicationUser { UserName = "RegularUser", Email = "regular@khadamat.com", FullName = "Regular User", Role = UserRole.User, EmailConfirmed = true };
            await userManager.CreateAsync(regularUser, "Admin@123");
            await userManager.AddToRoleAsync(regularUser, "User");
        }
    }

    private static async Task SeedRandomUsersAsync(UserManager<ApplicationUser> userManager, KhadamatDbContext context)
    {
        if (await userManager.Users.CountAsync() > 10) return;

        var cities = await context.Cities.ToListAsync();
        string[] names = { "أحمد", "محمد", "سارة", "ليلى", "هاني", "لينا", "كريم", "ليان", "ياسر" };
        var random = new Random();

        for (int i = 1; i <= 10; i++)
        {
            var name = names[random.Next(names.Length)];
            var email = $"user{i}@khadamat.com";
            if (await userManager.FindByEmailAsync(email) == null)
            {
                var city = cities.Any() ? cities[random.Next(cities.Count)] : null;
                var user = new ApplicationUser
                {
                    UserName = $"user{i}",
                    Email = email,
                    FullName = $"{name} {i}",
                    Role = UserRole.User,
                    IsActive = true,
                    EmailConfirmed = true,
                    CityId = city?.Id,
                    IsProvider = i <= 5
                };
                await userManager.CreateAsync(user, "User@123");
                await userManager.AddToRoleAsync(user, "User");

                if (user.IsProvider)
                {
                    var profile = new ProviderProfile
                    {
                        UserId = user.Id,
                        BusinessName = $"مؤسسة {name} للخدمات",
                        Bio = "نعمل بجودة عالية وإتقان لخدمتكم دائماً.",
                        ContactNumber = "010" + random.Next(10000000, 99999999),
                        Verified = true,
                        CityId = city?.Id
                    };
                    await context.ProviderProfiles.AddAsync(profile);
                    user.IsVerified = true;
                    await userManager.UpdateAsync(user);
                }
            }
        }
        await context.SaveChangesAsync();
    }

    private static async Task SeedServicesAsync(KhadamatDbContext context)
    {
        if (await context.Services.CountAsync() > 20) return;

        var providers = await context.ProviderProfiles.ToListAsync();
        var subCats = await context.SubCategories.ToListAsync();
        var cities = await context.Cities.ToListAsync();
        if (!providers.Any() || !subCats.Any()) return;

        var random = new Random();
        for (int i = 1; i <= 30; i++)
        {
            var provider = providers[random.Next(providers.Count)];
            var subCat = subCats[random.Next(subCats.Count)];
            var city = cities.Any() ? cities[random.Next(cities.Count)] : null;

            var service = new Service(
                subCategoryId: subCat.Id,
                categoryId: null,
                cityId: city?.Id,
                name: $"خدمة {subCat.Name} مميزة {i}",
                description: $"نقدم أفضل خدمات {subCat.Name} في بلدية {city?.City_Name_AR} بأسعار منافسة.",
                address: city?.City_Name_AR ?? "موقع العمل",
                providerProfileId: provider.Id,
                userCreated: provider.UserId
            );
            service.UpdateDetails(service.Name, service.Description, service.Address, price: random.Next(200, 3000));
            service.SetImage($"https://picsum.photos/seed/s{i}/800/600");
            service.Approve();
            context.Services.Add(service);
        }
        await context.SaveChangesAsync();
    }

    private static async Task SeedRatingsAsync(KhadamatDbContext context)
    {
        if (await context.Ratings.AnyAsync()) return;

        var services = await context.Services.ToListAsync();
        var users = await context.Users.Where(u => !u.IsProvider).ToListAsync();
        if (!services.Any() || !users.Any()) return;

        var random = new Random();
        foreach (var s in services)
        {
            var count = random.Next(1, 3);
            for (int i = 0; i < count; i++)
            {
                var user = users[random.Next(users.Count)];
                context.Ratings.Add(new Rating(s.Id, user.Id, random.Next(4, 6), "خدمة رائعة جداً، شكراً لكم!"));
            }
        }
        await context.SaveChangesAsync();
    }

    private static async Task SeedMessagesAsync(KhadamatDbContext context)
    {
        if (await context.Messages.AnyAsync()) return;

        var users = await context.Users.Take(5).ToListAsync();
        if (users.Count < 2) return;

        var random = new Random();
        for (int i = 0; i < 15; i++)
        {
            var sender = users[random.Next(users.Count)];
            var receiver = users[random.Next(users.Count)];
            if (sender.Id == receiver.Id) continue;

            context.Messages.Add(new Message(sender.Id, receiver.Id, "مرحباً، أود الاستفسار عن تفاصيل الخدمة المتاحة لديكم."));
        }
        await context.SaveChangesAsync();
    }

    private static async Task SeedMarketplaceCategoriesAsync(KhadamatDbContext context)
    {
        var marketplaceData = new List<(string Name, string Icon, List<string> Subs)>
        {
            ( "الأثاث", "🛋️", new List<string> { "غرف نوم", "غرف معيشة", "سفرة وطاولات", "كراسي ومكاتب", "أثاث مكتبي", "أثاث أطفال", "أثاث خارجي", "أثاث مستعمل", "أثاث جديد" } ),
            ( "الأجهزة الإلكترونية", "📱", new List<string> { "موبايلات", "تابلت", "لابتوب", "كمبيوتر مكتبي", "شاشات", "طابعات", "كاميرات", "سماعات", "أجهزة ألعاب" } ),
            ( "الأجهزة المنزلية", "🏠", new List<string> { "ثلاجات", "غسالات", "بوتاجازات", "ميكروويف", "تكييفات", "مراوح", "سخانات", "أجهزة مطبخ صغيرة" } ),
            ( "السيارات والمركبات", "🚗", new List<string> { "سيارات", "موتوسيكلات", "دراجات", "قطع غيار", "إكسسوارات سيارات" } ),
            ( "الحيوانات الأليفة", "🐾", new List<string> { "كلاب", "قطط", "طيور", "أسماك", "أدوات الحيوانات", "طعام الحيوانات" } ),
            ( "الملابس والأزياء", "👕", new List<string> { "ملابس رجالي", "ملابس نسائي", "ملابس أطفال", "أحذية", "شنط", "إكسسوارات", "ساعات" } ),
            ( "ألعاب وأطفال", "🧸", new List<string> { "ألعاب أطفال", "عربيات أطفال", "سرير أطفال", "ملابس أطفال", "أدوات تعليمية" } ),
            ( "أدوات رياضية", "⚽", new List<string> { "أجهزة رياضية منزلية", "أثقال", "أدوات جيم", "دراجات رياضية", "ملابس رياضية" } ),
            ( "كتب وأدوات تعليمية", "📚", new List<string> { "كتب مدرسية", "كتب جامعية", "روايات", "أدوات مكتبية" } ),
            ( "أدوات منزلية", "🍳", new List<string> { "أدوات مطبخ", "أدوات ديكور", "سجاد", "ستائر", "إضاءة" } ),
            ( "أدوات ومعدات", "🛠️", new List<string> { "أدوات كهربائية", "أدوات يدوية", "معدات صناعية" } ),
            ( "أشياء متنوعة", "📦", new List<string> { "أخرى" } )
        };

        int displayOrder = 1;
        foreach (var item in marketplaceData)
        {
            var category = new MarketplaceCategory { Name = item.Name, Icon = item.Icon, DisplayOrder = displayOrder++ };
            await context.MarketplaceCategories.AddAsync(category);
            await context.SaveChangesAsync();

            int subOrder = 1;
            foreach (var subName in item.Subs)
            {
                await context.MarketplaceSubCategories.AddAsync(new MarketplaceSubCategory { Name = subName, CategoryId = category.Id, DisplayOrder = subOrder++ });
            }
            await context.SaveChangesAsync();
        }
    }

    private static async Task SeedCategoriesAndSubCategoriesAsync(KhadamatDbContext context)
    {
        var mainCats = await context.MainCategories.ToDictionaryAsync(m => m.Name);

        // 1. صحة (Health)
        if (mainCats.TryGetValue("صحة", out var health))
        {
            var healthData = new Dictionary<string, List<string>>
            {
                { "عيادات", new List<string> { "اسنان", "اطفال", "عيون", "جلدية", "باطنة", "عظام", "مخ واعصاب", "انف واذن وحنجرة" } },
                { "صيدليات", new List<string> { "صيدلية 24 ساعة", "صيدلية منزلية", "مستلزمات طبية" } },
                { "مستشفيات", new List<string> { "مستشفى عام", "مركز جراحة", "طوارئ" } },
                { "معامل تحاليل", new List<string> { "تحاليل دم", "تحاليل شاملة", "مسحات" } },
                { "مراكز اشعة", new List<string> { "اشعة مقطعية", "سونار", "رنين مغناطيسي" } }
            };
            await SeedSectionAsync(context, health.Id, healthData);
        }

        // 2. تعليم (Education)
        if (mainCats.TryGetValue("تعليم", out var education))
        {
            var eduData = new Dictionary<string, List<string>>
            {
                { "دروس خصوصية", new List<string> { "ابتدائي", "اعدادي", "ثانوي", "لغات", "مواد علمية", "مواد ادبية" } },
                { "كورسات", new List<string> { "برمجة", "لغة انجليزية", "لغة المانية", "فوتوشوب", "جرافيك ديزاين", "تسويق الكتروني" } },
                { "حضانات", new List<string> { "حضانة لغات", "تنمية مهارات", "تخاطب" } },
                { "مراكز تدريب", new List<string> { "تنمية بشرية", "ادارة اعمال", "محاسبة" } }
            };
            await SeedSectionAsync(context, education.Id, eduData);
        }

        // 3. حرفيون (Craftsmen)
        if (mainCats.TryGetValue("حرفيون", out var crafts))
        {
            var craftsData = new Dictionary<string, List<string>>
            {
                { "سباكة", new List<string> { "تأسيس سباكة", "صيانة اعطال", "تركيب ادوات صحية", "تسليك بلاعات" } },
                { "كهرباء", new List<string> { "تأسيس كهرباء", "تركيب نجفات", "صيانة اجهزة", "لوحات توزيع" } },
                { "نجارة", new List<string> { "تصنيع اثاث", "تصليح ابواب", "تجديد مطابخ", "فك وتركيب" } },
                { "نقاشة ودهانات", new List<string> { "دهانات داخلية", "ديكورات جبس", "ورق حائط" } },
                { "صيانة اجهزة منزلية", new List<string> { "ثلاجات", "غسالات", "تكييفات", "بوتاجازات" } }
            };
            await SeedSectionAsync(context, crafts.Id, craftsData);
        }

        // 4. مواصلات (Transportation)
        if (mainCats.TryGetValue("مواصلات", out var transport))
        {
            var transportData = new Dictionary<string, List<string>>
            {
                { "توصيل طلبات", new List<string> { "دليفري مطاعم", "شحن محافظات", "توصيل هدايا" } },
                { "نقل اثاث", new List<string> { "ونش رفع", "فك وتركيب اثاث", "سيارات نقل" } },
                { "تاكسي ورحلات", new List<string> { "مشاوير خاصة", "توصيل مطار", "رحلات سياحية" } }
            };
            await SeedSectionAsync(context, transport.Id, transportData);
        }

        await context.SaveChangesAsync();
    }

    private static async Task SeedSectionAsync(KhadamatDbContext context, int mainId, Dictionary<string, List<string>> data)
    {
        foreach (var categoryPair in data)
        {
            var catName = categoryPair.Key;
            var subNames = categoryPair.Value;

            var category = await context.Categories.FirstOrDefaultAsync(c => c.Name == catName && c.MainCategoryId == mainId);
            if (category == null)
            {
                category = new Category { Name = catName, MainCategoryId = mainId };
                context.Categories.Add(category);
                await context.SaveChangesAsync();
            }

            foreach (var subName in subNames)
            {
                if (!await context.SubCategories.AnyAsync(s => s.Name == subName && s.CategoryId == category.Id))
                {
                    context.SubCategories.Add(new SubCategory { Name = subName, CategoryId = category.Id });
                }
            }
        }
    }

    private static async Task SeedLocationsAsync(KhadamatDbContext context)
    {
        if (await context.Governorates.AnyAsync()) return;

        var locationData = new List<(string NameAr, string NameEn, List<(string NameAr, string NameEn)> Cities)>
        {
            ("القاهرة", "Cairo", new List<(string, string)> { ("مدينة نصر", "Nasr City"), ("المعادي", "Maadi"), ("القاهرة الجديدة", "New Cairo"), ("الشروق", "Sherouk"), ("مدينتي", "Madinaty"), ("وسط البلد", "Downtown"), ("مصر الجديدة", "Heliopolis"), ("حلوان", "Helwan") }),
            ("الجيزة", "Giza", new List<(string, string)> { ("السادس من أكتوبر", "6th of October"), ("الشيخ زايد", "Sheikh Zayed"), ("الهرم", "Haram"), ("فيصل", "Faisal"), ("الدقي", "Dokki"), ("المهندسين", "Mohandessin"), ("العجوزة", "Agouza") }),
            ("الإسكندرية", "Alexandria", new List<(string, string)> { ("سموحة", "Smouha"), ("سيدي جابر", "Sidi Gaber"), ("المنتزة", "Montaza"), ("العجمي", "Agami"), ("برج العرب", "Borg El Arab") }),
            ("الدقهلية", "Dakahlia", new List<(string, string)> { ("المنصورة", "Mansoura"), ("ميت غمر", "Mit Ghamr"), ("السنبلاوين", "Senbellawein") }),
            ("البحر الأحمر", "Red Sea", new List<(string, string)> { ("الغردقة", "Hurghada"), ("سفاجا", "Safaga"), ("مرسى علم", "Marsa Alam"), ("الجونة", "Gouna") }),
            ("البحيرة", "Beheira", new List<(string, string)> { ("دمنهور", "Damanhur"), ("كفر الدوار", "Kafr El Dawwar") }),
            ("الفيوم", "Fayoum", new List<(string, string)> { ("الفيوم", "Fayoum City"), ("طامية", "Tamia") }),
            ("الغربية", "Gharbia", new List<(string, string)> { ("طنطا", "Tanta"), ("المحلة الكبرى", "El Mahalla El Kubra") }),
            ("الإسماعيلية", "Ismailia", new List<(string, string)> { ("الإسماعيلية", "Ismailia City"), ("فايد", "Fayed") }),
            ("المنوفية", "Monufia", new List<(string, string)> { ("شبين الكوم", "Shibin El Kom"), ("قويسنا", "Quwaysina"), ("السادات", "Sadat City") }),
            ("المنيا", "Minya", new List<(string, string)> { ("المنيا", "Minya City"), ("ملوي", "Mallawi") }),
            ("القليوبية", "Qalyubia", new List<(string, string)> { ("بنها", "Banha"), ("شبرا الخيمة", "Shubra El Kheima"), ("العبور", "Obour City") }),
            ("الوادي الجديد", "New Valley", new List<(string, string)> { ("الخارجة", "Kharga"), ("الداخلة", "Dakhla") }),
            ("السويس", "Suez", new List<(string, string)> { ("السويس", "Suez City") }),
            ("اسوان", "Aswan", new List<(string, string)> { ("اسوان", "Aswan City"), ("كوم امبو", "Kom Ombo") }),
            ("اسيوط", "Assiut", new List<(string, string)> { ("اسيوط", "Assiut City"), ("ديروط", "Dayrut") }),
            ("بني سويف", "Beni Suef", new List<(string, string)> { ("بني سويف", "Beni Suef City") }),
            ("بورسعيد", "Port Said", new List<(string, string)> { ("بورسعيد", "Port Said City"), ("بورفؤاد", "Port Fuad") }),
            ("دمياط", "Damietta", new List<(string, string)> { ("دمياط", "Damietta City"), ("رأس البر", "Ras El Bar") }),
            ("الشرقية", "Sharkia", new List<(string, string)> { ("الزقازيق", "Zagazig"), ("العاشر من رمضان", "10th of Ramadan") }),
            ("جنوب سيناء", "South Sinai", new List<(string, string)> { ("شرم الشيخ", "Sharm El Sheikh"), ("دهب", "Dahab"), ("طور سيناء", "Tur Sinai") }),
            ("كفر الشيخ", "Kafr El Sheikh", new List<(string, string)> { ("كفر الشيخ", "Kafr El Sheikh City"), ("دسوق", "Desouk") }),
            ("مطروح", "Matrouh", new List<(string, string)> { ("مرسى مطروح", "Marsa Matrouh"), ("الساحل الشمالي", "North Coast"), ("سيوة", "Siwa") }),
            ("قنا", "Qena", new List<(string, string)> { ("قنا", "Qena City"), ("نجع حمادي", "Nag Hammadi") }),
            ("شمال سيناء", "North Sinai", new List<(string, string)> { ("العريش", "Arish") }),
            ("سوهاج", "Sohag", new List<(string, string)> { ("سوهاج", "Sohag City"), ("طما", "Tama") }),
            ("الاقصر", "Luxor", new List<(string, string)> { ("الاقصر", "Luxor City") })
        };

        int govOrder = 1;
        foreach (var govData in locationData)
        {
            var gov = new Governorate 
            { 
                Governorate_Name_AR = govData.NameAr, 
                Governorate_Name_EN = govData.NameEn, 
                DisplayOrder = govOrder++, 
                Approved = true 
            };
            context.Governorates.Add(gov);
            await context.SaveChangesAsync();

            int cityOrder = 1;
            foreach (var cityData in govData.Cities)
            {
                context.Cities.Add(new City 
                { 
                    GovernorateId = gov.Id, 
                    City_Name_AR = cityData.NameAr, 
                    City_Name_EN = cityData.NameEn, 
                    Approved = true, 
                    DisplayOrder = cityOrder++ 
                });
            }
            await context.SaveChangesAsync();
        }
    }

    private static async Task SeedAdsAsync(KhadamatDbContext context)
    {
        var now = DateTime.UtcNow;
        var end = now.AddYears(4);

        // 1. Slider Ads — تستخدم ألوان متدرجة (hero-gradient) لأن ملفات الصور الافتراضية غير موجودة
        var ad1 = new Ad("مرحباً بكم في خدماوي", "اكتشف واطلب أفضل الخدمات في مكان واحد بسهولة وأمان.", now, end, "Image");
        ad1.UpdateDetails(ad1.Title, ad1.Description, ad1.StartDate, ad1.EndDate, placement: "Slider");
        ad1.SetMainImage("hero-gradient-1");
        ad1.Approve();
        ad1.SetDisplayOrder(1);
        context.Ads.Add(ad1);

        var ad2 = new Ad("انضم كمزود خدمة معنا", "سجل مهاراتك ووصّل خدماتك لآلاف العملاء في منطقتك مجاناً.", now, end, "Image");
        ad2.UpdateDetails(ad2.Title, ad2.Description, ad2.StartDate, ad2.EndDate, placement: "Slider");
        ad2.SetMainImage("hero-gradient-2");
        ad2.Approve();
        ad2.SetDisplayOrder(2);
        context.Ads.Add(ad2);

        // 2. Bottom Ads
        var adBottom = new Ad("تخفيضات الصيف الكبرى", "خصومات تصل إلى 40% على خدمات الصيانة المنزلية والتركيبات.", now, end, "Image");
        adBottom.UpdateDetails(adBottom.Title, adBottom.Description, adBottom.StartDate, adBottom.EndDate, placement: "Bottom");
        adBottom.SetMainImage("hero-gradient-3");
        adBottom.Approve();
        adBottom.SetDisplayOrder(1);
        context.Ads.Add(adBottom);

        // 3. Sidebar Ads
        var adSidebar = new Ad("مستشار قانوني في خدمتك", "احصل على استشارات قانونية سريعة وموثوقة من أفضل المحامين.", now, end, "Image");
        adSidebar.UpdateDetails(adSidebar.Title, adSidebar.Description, adSidebar.StartDate, adSidebar.EndDate, placement: "Sidebar");
        adSidebar.SetMainImage("hero-gradient-4");
        adSidebar.Approve();
        adSidebar.SetDisplayOrder(1);
        context.Ads.Add(adSidebar);

        await context.SaveChangesAsync();
    }

    // ────────────────────────────────────────────────────────────────────────
    // Phase 9: Seed Ad Packages (Basic / Silver / Gold / Platinum)
    // ────────────────────────────────────────────────────────────────────────
    private static async Task SeedAdPackagesAsync(KhadamatDbContext context)
    {
        var packages = new List<AdPackage>
        {
            new AdPackage(
                name: "الباقة الأساسية",
                price: 99,
                durationDays: 30,
                tier: AdPackageTier.Basic,
                maxAds: 1,
                isFeatured: false,
                isSponsored: false,
                isBanner: false,
                priorityBoost: 0,
                description: "إعلان واحد لمدة 30 يوماً"),

            new AdPackage(
                name: "الباقة الفضية",
                price: 199,
                durationDays: 30,
                tier: AdPackageTier.Silver,
                maxAds: 3,
                isFeatured: false,
                isSponsored: true,
                isBanner: false,
                priorityBoost: 2,
                description: "3 إعلانات ممولة في نتائج البحث"),

            new AdPackage(
                name: "الباقة الذهبية",
                price: 399,
                durationDays: 30,
                tier: AdPackageTier.Gold,
                maxAds: 5,
                isFeatured: true,
                isSponsored: true,
                isBanner: true,
                priorityBoost: 5,
                description: "مزودون مميزون + بانر رئيسي + إعلانات ممولة"),

            new AdPackage(
                name: "الباقة البلاتينية",
                price: 699,
                durationDays: 30,
                tier: AdPackageTier.Platinum,
                maxAds: 10,
                isFeatured: true,
                isSponsored: true,
                isBanner: true,
                priorityBoost: 10,
                description: "أفضل ظهور على المنصة مع كل المميزات")
        };

        await context.AdPackages.AddRangeAsync(packages);
        await context.SaveChangesAsync();
    }

    // ────────────────────────────────────────────────────────────────────────
    // Phase 10: Seed Point Reward Rules
    // ────────────────────────────────────────────────────────────────────────
    private static async Task SeedPointRewardRulesAsync(KhadamatDbContext context)
    {
        var rules = new List<PointRewardRule>
        {
            new PointRewardRule(PointActionType.Referral,       50, "50 نقطة لكل صديق يسجل بالرابط"),
            new PointRewardRule(PointActionType.Review,         10, "10 نقاط عند استلام تقييم من عميل"),
            new PointRewardRule(PointActionType.OrderCompleted, 20, "20 نقطة عند إتمام طلب خدمة")
        };

        await context.PointRewardRules.AddRangeAsync(rules);
        await context.SaveChangesAsync();
    }
}
