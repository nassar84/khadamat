@echo off
echo ========================================
echo   اختبار Posts و Comments - Khadamat
echo ========================================
echo.

echo [1/4] التحقق من تشغيل Backend...
curl -s http://localhost:5144/api/v1/categories/main >nul 2>&1
if %errorlevel% equ 0 (
    echo ✅ Backend يعمل على http://localhost:5144
) else (
    echo ❌ Backend لا يعمل! يرجى تشغيله أولاً
    echo    الأمر: dotnet run --project src\Khadamat.WebAPI
    pause
    exit /b 1
)

echo.
echo [2/4] التحقق من تشغيل Frontend...
curl -s http://localhost:5028 >nul 2>&1
if %errorlevel% equ 0 (
    echo ✅ Frontend يعمل على http://localhost:5028
) else (
    echo ❌ Frontend لا يعمل! يرجى تشغيله أولاً
    echo    الأمر: dotnet run --project src\Khadamat.WasmHost --urls http://localhost:5028
    pause
    exit /b 1
)

echo.
echo [3/4] فتح صفحة تسجيل الدخول...
start http://localhost:5028/login

echo.
echo [4/4] تعليمات الاختبار:
echo.
echo 📋 خطوات الاختبار:
echo ==================
echo.
echo 1. سجل دخول بـ:
echo    Email: user1@khadamat.com
echo    Password: User@123
echo.
echo 2. اذهب إلى: خدماتي
echo.
echo 3. اختر أي خدمة
echo.
echo 4. اضغط تبويب "المنشورات"
echo.
echo 5. أضف منشور:
echo    "🎉 عرض خاص: خصم 20%%!"
echo.
echo 6. اضغط "تعليق" أسفل المنشور
echo.
echo 7. أضف تعليق:
echo    "عرض ممتاز! 👍"
echo.
echo ✅ النتيجة المتوقعة:
echo    - المنشور يظهر فوراً
echo    - التعليق يظهر فوراً
echo    - العداد يتحدث من (0) إلى (1)
echo.
echo ========================================
echo   للمزيد: راجع MANUAL_TEST_GUIDE.md
echo ========================================
echo.
pause
