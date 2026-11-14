using Khadamat.Domain.Common;

namespace Khadamat.Domain.Entities;

/// <summary>
/// إعدادات ونصوص فقرة تميز الخدمة في رسائل الترحيب والمتابعة التسويقية.
/// قابلة للتعديل بالكامل من لوحة الإدارة بدون الحاجة لتعديل الكود أو إعادة النشر.
/// </summary>
public class ServiceDistinctionConfig : BaseEntity
{
    /// <summary>
    /// تفعيل أو تعطيل فقرة التميز بشكل عام في رسائل النظام
    /// </summary>
    public bool IsEnabledGlobally { get; set; } = true;

    /// <summary>
    /// رأس فقرة التميز
    /// </summary>
    public string HeaderText { get; set; } = "🌟 *خبر مميز ليك!*";

    /// <summary>
    /// 1. نص اختيار إدارة خدماوي
    /// </summary>
    public string AdminSelectionText { get; set; } = "تم اختيار خدمتك وإضافتها ضمن الخدمات المميزة على تطبيق خدماوي";

    /// <summary>
    /// 2. نص ترشيح أعضاء مجتمع خدماوي
    /// </summary>
    public string CommunityNominationText { get; set; } = "تم ترشيح خدمتك من أعضاء مجتمع خدماوي لتكون ضمن الخدمات المميزة";

    /// <summary>
    /// 3. نص تقييمات إيجابية من المستخدمين
    /// </summary>
    public string PositiveReviewsText { get; set; } = "خدمتك من الخدمات التي حصلت على تقييمات إيجابية من مستخدمي خدماوي";

    /// <summary>
    /// 4. نص اختيار ترويجي خاص
    /// </summary>
    public string SpecialPromotionText { get; set; } = "تم اختيار خدمتك ضمن الاختيارات الترويجية المميزة على خدماوي";

    /// <summary>
    /// 5. نص الترحيب العام في حال عدم وجود سبب تميز محدد (بدون ادعاء اختيار أو ترشيح)
    /// </summary>
    public string GeneralWelcomeText { get; set; } = "سعداء بانضمامك لمنصة خدماوي لتوصيل خدماتك لأهل بلدك ومساعدتك في الوصول لعملاء جدد في منطقتك! 🚀";

    /// <summary>
    /// فقرة المتابعة والتشجيع الملحقة بفقرة التميز
    /// </summary>
    public string FollowUpText { get; set; } = "إحنا بنهتم نعرّف أهل بلدك بأصحاب الخدمات المميزة، ونساعد شغلك يوصل للناس اللي بتدور على الخدمة اللي بتقدمها.\n\nوجودك معانا خطوة جديدة علشان شغلك ينتشر ويكون أسهل للعملاء إنهم يلاقوك ويتواصلوا معاك! 🚀";

    public void UpdateConfig(
        bool isEnabledGlobally,
        string headerText,
        string adminSelectionText,
        string communityNominationText,
        string positiveReviewsText,
        string specialPromotionText,
        string generalWelcomeText,
        string followUpText)
    {
        IsEnabledGlobally = isEnabledGlobally;
        HeaderText = headerText;
        AdminSelectionText = adminSelectionText;
        CommunityNominationText = communityNominationText;
        PositiveReviewsText = positiveReviewsText;
        SpecialPromotionText = specialPromotionText;
        GeneralWelcomeText = generalWelcomeText;
        FollowUpText = followUpText;
        UpdatedAt = System.DateTime.UtcNow;
    }
}
