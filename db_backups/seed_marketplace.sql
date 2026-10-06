USE KhadamatDb;
GO
DECLARE @marketplaceId INT;

IF NOT EXISTS(SELECT 1 FROM MainCategories WHERE Name = N'متجر السلع')
BEGIN
    INSERT INTO MainCategories (Name, Icon, Color, [Order], CreatedAt, IsDeleted) 
    VALUES (N'متجر السلع', N'🏪', N'marketplace', 99, GETUTCDATE(), 0);
    SET @marketplaceId = SCOPE_IDENTITY();
END
ELSE
BEGIN
    SELECT @marketplaceId = Id FROM MainCategories WHERE Name = N'متجر السلع';
END

IF NOT EXISTS(SELECT 1 FROM Categories WHERE Name = N'سيارات ومركبات' AND MainCategoryId = @marketplaceId)
BEGIN
    INSERT INTO Categories (Name, MainCategoryId, CreatedAt, IsDeleted) VALUES (N'سيارات ومركبات', @marketplaceId, GETUTCDATE(), 0);
    INSERT INTO Categories (Name, MainCategoryId, CreatedAt, IsDeleted) VALUES (N'إلكترونيات وأجهزة', @marketplaceId, GETUTCDATE(), 0);
    INSERT INTO Categories (Name, MainCategoryId, CreatedAt, IsDeleted) VALUES (N'عقارات', @marketplaceId, GETUTCDATE(), 0);
    INSERT INTO Categories (Name, MainCategoryId, CreatedAt, IsDeleted) VALUES (N'ملابس و اكسسوارات', @marketplaceId, GETUTCDATE(), 0);
    INSERT INTO Categories (Name, MainCategoryId, CreatedAt, IsDeleted) VALUES (N'أثاث وديكور', @marketplaceId, GETUTCDATE(), 0);
    INSERT INTO Categories (Name, MainCategoryId, CreatedAt, IsDeleted) VALUES (N'مستلزمات أطفال', @marketplaceId, GETUTCDATE(), 0);
    INSERT INTO Categories (Name, MainCategoryId, CreatedAt, IsDeleted) VALUES (N'حيوانات أليفة', @marketplaceId, GETUTCDATE(), 0);
    INSERT INTO Categories (Name, MainCategoryId, CreatedAt, IsDeleted) VALUES (N'أدوات رياضية', @marketplaceId, GETUTCDATE(), 0);
    INSERT INTO Categories (Name, MainCategoryId, CreatedAt, IsDeleted) VALUES (N'أخرى', @marketplaceId, GETUTCDATE(), 0);
END

-- SubCategories Insert logic
-- 1. سيارات ومركبات
DECLARE @categoryId INT;
SELECT @categoryId = Id FROM Categories WHERE Name = N'سيارات ومركبات' AND MainCategoryId = @marketplaceId;
IF @categoryId IS NOT NULL AND NOT EXISTS(SELECT 1 FROM SubCategories WHERE CategoryId = @categoryId)
BEGIN
    INSERT INTO SubCategories (Name, CategoryId, CreatedAt, IsDeleted) VALUES (N'سيارات للبيع', @categoryId, GETUTCDATE(), 0);
    INSERT INTO SubCategories (Name, CategoryId, CreatedAt, IsDeleted) VALUES (N'قطع غيار وإكسسوارات', @categoryId, GETUTCDATE(), 0);
    INSERT INTO SubCategories (Name, CategoryId, CreatedAt, IsDeleted) VALUES (N'دراجات نارية', @categoryId, GETUTCDATE(), 0);
    INSERT INTO SubCategories (Name, CategoryId, CreatedAt, IsDeleted) VALUES (N'قوارب', @categoryId, GETUTCDATE(), 0);
    INSERT INTO SubCategories (Name, CategoryId, CreatedAt, IsDeleted) VALUES (N'أخرى', @categoryId, GETUTCDATE(), 0);
END

-- 2. إلكترونيات وأجهزة
SELECT @categoryId = Id FROM Categories WHERE Name = N'إلكترونيات وأجهزة' AND MainCategoryId = @marketplaceId;
IF @categoryId IS NOT NULL AND NOT EXISTS(SELECT 1 FROM SubCategories WHERE CategoryId = @categoryId)
BEGIN
    INSERT INTO SubCategories (Name, CategoryId, CreatedAt, IsDeleted) VALUES (N'هواتف محمولة', @categoryId, GETUTCDATE(), 0);
    INSERT INTO SubCategories (Name, CategoryId, CreatedAt, IsDeleted) VALUES (N'أجهزة كمبيوتر ولابتوب', @categoryId, GETUTCDATE(), 0);
    INSERT INTO SubCategories (Name, CategoryId, CreatedAt, IsDeleted) VALUES (N'أجهزة منزلية', @categoryId, GETUTCDATE(), 0);
    INSERT INTO SubCategories (Name, CategoryId, CreatedAt, IsDeleted) VALUES (N'ألعاب فيديو ووحدات تحكم', @categoryId, GETUTCDATE(), 0);
    INSERT INTO SubCategories (Name, CategoryId, CreatedAt, IsDeleted) VALUES (N'كاميرات وتصوير', @categoryId, GETUTCDATE(), 0);
    INSERT INTO SubCategories (Name, CategoryId, CreatedAt, IsDeleted) VALUES (N'شاشات وتلفزيونات', @categoryId, GETUTCDATE(), 0);
    INSERT INTO SubCategories (Name, CategoryId, CreatedAt, IsDeleted) VALUES (N'أخرى', @categoryId, GETUTCDATE(), 0);
END

-- 3. عقارات
SELECT @categoryId = Id FROM Categories WHERE Name = N'عقارات' AND MainCategoryId = @marketplaceId;
IF @categoryId IS NOT NULL AND NOT EXISTS(SELECT 1 FROM SubCategories WHERE CategoryId = @categoryId)
BEGIN
    INSERT INTO SubCategories (Name, CategoryId, CreatedAt, IsDeleted) VALUES (N'شقق وإستوديوهات', @categoryId, GETUTCDATE(), 0);
    INSERT INTO SubCategories (Name, CategoryId, CreatedAt, IsDeleted) VALUES (N'فلل ومنازل', @categoryId, GETUTCDATE(), 0);
    INSERT INTO SubCategories (Name, CategoryId, CreatedAt, IsDeleted) VALUES (N'أراضي', @categoryId, GETUTCDATE(), 0);
    INSERT INTO SubCategories (Name, CategoryId, CreatedAt, IsDeleted) VALUES (N'محلات ومكاتب تجارية', @categoryId, GETUTCDATE(), 0);
    INSERT INTO SubCategories (Name, CategoryId, CreatedAt, IsDeleted) VALUES (N'أخرى', @categoryId, GETUTCDATE(), 0);
END

-- 4. ملابس و اكسسوارات
SELECT @categoryId = Id FROM Categories WHERE Name = N'ملابس و اكسسوارات' AND MainCategoryId = @marketplaceId;
IF @categoryId IS NOT NULL AND NOT EXISTS(SELECT 1 FROM SubCategories WHERE CategoryId = @categoryId)
BEGIN
    INSERT INTO SubCategories (Name, CategoryId, CreatedAt, IsDeleted) VALUES (N'ملابس رجالية', @categoryId, GETUTCDATE(), 0);
    INSERT INTO SubCategories (Name, CategoryId, CreatedAt, IsDeleted) VALUES (N'ملابس نسائية', @categoryId, GETUTCDATE(), 0);
    INSERT INTO SubCategories (Name, CategoryId, CreatedAt, IsDeleted) VALUES (N'أحذية', @categoryId, GETUTCDATE(), 0);
    INSERT INTO SubCategories (Name, CategoryId, CreatedAt, IsDeleted) VALUES (N'ساعات ومجوهرات', @categoryId, GETUTCDATE(), 0);
    INSERT INTO SubCategories (Name, CategoryId, CreatedAt, IsDeleted) VALUES (N'حقائب ومحافظ', @categoryId, GETUTCDATE(), 0);
    INSERT INTO SubCategories (Name, CategoryId, CreatedAt, IsDeleted) VALUES (N'أخرى', @categoryId, GETUTCDATE(), 0);
END

-- 5. أثاث وديكور
SELECT @categoryId = Id FROM Categories WHERE Name = N'أثاث وديكور' AND MainCategoryId = @marketplaceId;
IF @categoryId IS NOT NULL AND NOT EXISTS(SELECT 1 FROM SubCategories WHERE CategoryId = @categoryId)
BEGIN
    INSERT INTO SubCategories (Name, CategoryId, CreatedAt, IsDeleted) VALUES (N'أثاث منزلي', @categoryId, GETUTCDATE(), 0);
    INSERT INTO SubCategories (Name, CategoryId, CreatedAt, IsDeleted) VALUES (N'أثاث مكتبي', @categoryId, GETUTCDATE(), 0);
    INSERT INTO SubCategories (Name, CategoryId, CreatedAt, IsDeleted) VALUES (N'سجاد وستائر', @categoryId, GETUTCDATE(), 0);
    INSERT INTO SubCategories (Name, CategoryId, CreatedAt, IsDeleted) VALUES (N'إضاءة', @categoryId, GETUTCDATE(), 0);
    INSERT INTO SubCategories (Name, CategoryId, CreatedAt, IsDeleted) VALUES (N'ديكورات وتحف', @categoryId, GETUTCDATE(), 0);
    INSERT INTO SubCategories (Name, CategoryId, CreatedAt, IsDeleted) VALUES (N'أخرى', @categoryId, GETUTCDATE(), 0);
END

-- 6. مستلزمات أطفال
SELECT @categoryId = Id FROM Categories WHERE Name = N'مستلزمات أطفال' AND MainCategoryId = @marketplaceId;
IF @categoryId IS NOT NULL AND NOT EXISTS(SELECT 1 FROM SubCategories WHERE CategoryId = @categoryId)
BEGIN
    INSERT INTO SubCategories (Name, CategoryId, CreatedAt, IsDeleted) VALUES (N'ملابس أطفال', @categoryId, GETUTCDATE(), 0);
    INSERT INTO SubCategories (Name, CategoryId, CreatedAt, IsDeleted) VALUES (N'ألعاب أطفال', @categoryId, GETUTCDATE(), 0);
    INSERT INTO SubCategories (Name, CategoryId, CreatedAt, IsDeleted) VALUES (N'عربات ومقاعد سيارات', @categoryId, GETUTCDATE(), 0);
    INSERT INTO SubCategories (Name, CategoryId, CreatedAt, IsDeleted) VALUES (N'أثاث غرف أطفال', @categoryId, GETUTCDATE(), 0);
    INSERT INTO SubCategories (Name, CategoryId, CreatedAt, IsDeleted) VALUES (N'أخرى', @categoryId, GETUTCDATE(), 0);
END

-- 7. حيوانات أليفة
SELECT @categoryId = Id FROM Categories WHERE Name = N'حيوانات أليفة' AND MainCategoryId = @marketplaceId;
IF @categoryId IS NOT NULL AND NOT EXISTS(SELECT 1 FROM SubCategories WHERE CategoryId = @categoryId)
BEGIN
    INSERT INTO SubCategories (Name, CategoryId, CreatedAt, IsDeleted) VALUES (N'طيور', @categoryId, GETUTCDATE(), 0);
    INSERT INTO SubCategories (Name, CategoryId, CreatedAt, IsDeleted) VALUES (N'قطط', @categoryId, GETUTCDATE(), 0);
    INSERT INTO SubCategories (Name, CategoryId, CreatedAt, IsDeleted) VALUES (N'كلاب', @categoryId, GETUTCDATE(), 0);
    INSERT INTO SubCategories (Name, CategoryId, CreatedAt, IsDeleted) VALUES (N'أسماك', @categoryId, GETUTCDATE(), 0);
    INSERT INTO SubCategories (Name, CategoryId, CreatedAt, IsDeleted) VALUES (N'مستلزمات حيوانات', @categoryId, GETUTCDATE(), 0);
    INSERT INTO SubCategories (Name, CategoryId, CreatedAt, IsDeleted) VALUES (N'أخرى', @categoryId, GETUTCDATE(), 0);
END

-- 8. أدوات رياضية
SELECT @categoryId = Id FROM Categories WHERE Name = N'أدوات رياضية' AND MainCategoryId = @marketplaceId;
IF @categoryId IS NOT NULL AND NOT EXISTS(SELECT 1 FROM SubCategories WHERE CategoryId = @categoryId)
BEGIN
    INSERT INTO SubCategories (Name, CategoryId, CreatedAt, IsDeleted) VALUES (N'أجهزة لياقة بدنية', @categoryId, GETUTCDATE(), 0);
    INSERT INTO SubCategories (Name, CategoryId, CreatedAt, IsDeleted) VALUES (N'دراجات هوائية', @categoryId, GETUTCDATE(), 0);
    INSERT INTO SubCategories (Name, CategoryId, CreatedAt, IsDeleted) VALUES (N'ملابس رياضية', @categoryId, GETUTCDATE(), 0);
    INSERT INTO SubCategories (Name, CategoryId, CreatedAt, IsDeleted) VALUES (N'أخرى', @categoryId, GETUTCDATE(), 0);
END

-- 9. أخرى
SELECT @categoryId = Id FROM Categories WHERE Name = N'أخرى' AND MainCategoryId = @marketplaceId;
IF @categoryId IS NOT NULL AND NOT EXISTS(SELECT 1 FROM SubCategories WHERE CategoryId = @categoryId)
BEGIN
    INSERT INTO SubCategories (Name, CategoryId, CreatedAt, IsDeleted) VALUES (N'متنوع', @categoryId, GETUTCDATE(), 0);
END

GO
