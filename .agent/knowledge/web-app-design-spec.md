---
description: Khadamat Web Application Design Specifications & Fixes
---

# Khadamat Web Application Design Specification

This document serves as a permanent reference for the intended design, layout, and functionality of the Khadamat Web Application, specifically created to prevent recurring UI issues such as missing category images or the advertisement placeholder appearing incorrectly.

## 1. Main Categories Bar (Marquee Slider)
**Recurring Issue:** The categories bar sometimes disappears, or the icons revert to old/default ones.
**Correct Implementation:**
- **Component File:** `src/Khadamat.BlazorUI/Shared/CategoryMarquee.razor`
- **Location of Assets:** `src/Khadamat.WasmHost/wwwroot/images/maincategories/` and `src/Khadamat.BlazorUI/wwwroot/images/maincategories/`
- **Design:** The bar is a horizontal scrolling marquee that loops infinitely.
- **Images Mapping:** Images are mapped via the `MainCategory.ImageUrl` in the database. When seeding the database (e.g., in `KhadamatDbContextSeed.cs`), the properties must exactly match the files:
  - صحة: `cat_1.png`
  - تعليم: `cat_2.png`
  - متاجر: `cat_3.png`
  - مأكولات ومشروبات: `cat_4.png`
  - مكاتب: `cat_5.png`
  - خدمات أخرى: `cat_6.png`
  - متجر السلع: `cat_7.png`
  - حرفيون: `cat_8.png`
  - تسوق اون لاين: `cat_9.png`
  - مواصلات: `cat_10.png`
  - صيانة سيارات: `cat_11.png`
  - خدمات حكومية: `cat_12.png` (or mapped back to `cat_1.png` if missing)
- **Fallback Icon:** If an image is missing (HTTP 404), there should be a fallback mechanism displaying an avatar or a default icon, but the primary goal is to ensure the `.png` files are properly copied to the `wwwroot` directories of both the UI and WasmHost projects.

## 2. Categories, Subcategories, and Services Images
**Recurring Issue:** Images for Categories, Subcategories, and Services fail to load on the Explore page (`Explore.razor`) and default to placeholders.
**Correct Implementation:**
- **Categories & Subcategories Folders:** The image folders `categories` and `subcategories` must be present inside `src/Khadamat.WasmHost/wwwroot/images/`. A common mistake is leaving them only inside `src/Khadamat.BlazorUI/wwwroot/images/`, so you must copy them over using `Copy-Item`. 
- **Explore.razor Pathing:** Categories (`Level == "main"`) must correctly point to `images/categories/@cat.ImageUrl` (NOT `images/maincategories`). Subcategories correctly point to `images/subcategories/@sub.ImageUrl`.
- **Services:** Service images are loaded dynamically via `DefaultImages.GetServiceImage` which uses Intelligent Keyword Matching (analyzing names like "سباكة", "كهرباء", "نظافة" etc. to pick a relevant Unsplash default picture) if the actual DB image is missing or cannot load.

## 3. Advertisement Section
**Recurring Issue:** The text `لا توجد إعلانات نشطة` (No active ads) appears despite having ads.
**Correct Implementation:**
- **Component File:** `src/Khadamat.BlazorUI/Components/Ads/AdsSlider.razor`
- **Logic:** The component calls `Api.GetSliderAdsAsync()` which fetches ads from the `AdsController`.
- **Database Query:** The API (`GET api/v1/Ads/placements/Slider`) relies on strict conditions:
  - `IsDeleted == false`
  - `Approved == true`
  - `Placement == "Slider"`
  - `StartDate <= DateTime.UtcNow`
  - `EndDate >= DateTime.UtcNow`
- **Fixing Missing Ads:** Always verify the `EndDate` in the `Ads` SQL table. If the database was seeded in the past, the ads will naturally expire and hide themselves, falling back to the dashed placeholder. Make sure `EndDate` is extended.

## General UI Style & Preferences
To avoid redesigning to a "basic MVP" state:
- **Colors:** Vibrant and matching the core branding (using Tailwind or custom HSL/Glassmorphism variables if configured).
- **Styles:** Retain soft shadows, border radii (`rounded-4`), and smooth transitions (`transition: all 0.3s ease`).

*Note for AI Assistants: Do NOT change the layout structure of `CategoryMarquee.razor` or `AdsSlider.razor` to a basic default state. Always reference this document before making structural UI changes.*
