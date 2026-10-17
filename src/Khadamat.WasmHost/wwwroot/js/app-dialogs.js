/**
 * Khadamawy Universal App Dialogs & Alerts System
 * Replaces native browser alert() and confirm() with modern, elegant, branded glassmorphic modals.
 * Eliminates ugly browser popups and raw website URLs.
 */
(function () {
    // Preserve originals just in case
    window._nativeAlert = window.alert;
    window._nativeConfirm = window.confirm;

    // Inject styles once
    const styleId = 'khadamawy-dialog-styles';
    if (!document.getElementById(styleId)) {
        const style = document.createElement('style');
        styleId && (style.id = styleId);
        style.textContent = `
            /* Khadamawy Modal Backdrop */
            .khad-modal-backdrop {
                position: fixed;
                inset: 0;
                z-index: 9999999;
                background: rgba(15, 23, 42, 0.65);
                backdrop-filter: blur(10px);
                -webkit-backdrop-filter: blur(10px);
                display: flex;
                align-items: center;
                justify-content: center;
                padding: 16px;
                opacity: 0;
                transition: opacity 0.25s cubic-bezier(0.16, 1, 0.3, 1);
                direction: rtl;
                font-family: 'Tajawal', 'Outfit', system-ui, -apple-system, sans-serif;
            }
            .khad-modal-backdrop.khad-show {
                opacity: 1;
            }

            /* Khadamawy Dialog Box */
            .khad-dialog-box {
                position: relative;
                width: 100%;
                max-width: 440px;
                background: #ffffff;
                border-radius: 26px;
                box-shadow: 0 25px 60px -15px rgba(0, 0, 0, 0.35), 0 0 0 1px rgba(255, 255, 255, 0.8) inset;
                padding: 24px 24px 22px;
                text-align: center;
                transform: scale(0.9) translateY(15px);
                transition: transform 0.28s cubic-bezier(0.175, 0.885, 0.32, 1.275);
                overflow: hidden;
            }
            .khad-modal-backdrop.khad-show .khad-dialog-box {
                transform: scale(1) translateY(0);
            }

            /* Colored top decorative bar */
            .khad-top-bar {
                position: absolute;
                top: 0;
                left: 0;
                right: 0;
                height: 5px;
            }
            .khad-top-bar.success { background: linear-gradient(90deg, #10b981, #059669); }
            .khad-top-bar.danger  { background: linear-gradient(90deg, #ef4444, #dc2626); }
            .khad-top-bar.warning { background: linear-gradient(90deg, #f59e0b, #d97706); }
            .khad-top-bar.info    { background: linear-gradient(90deg, #0ea5e9, #0284c7); }

            /* Header: Brand & Close */
            .khad-header {
                display: flex;
                align-items: center;
                justify-content: space-between;
                margin-bottom: 16px;
            }
            .khad-brand {
                display: flex;
                align-items: center;
                gap: 8px;
            }
            .khad-brand img {
                width: 24px;
                height: 24px;
                border-radius: 50%;
                object-fit: cover;
            }
            .khad-brand span {
                font-weight: 800;
                font-size: 13.5px;
                color: #2563eb;
                letter-spacing: -0.2px;
            }
            .khad-close-btn {
                background: #f1f5f9;
                border: none;
                width: 28px;
                height: 28px;
                border-radius: 50%;
                color: #64748b;
                display: flex;
                align-items: center;
                justify-content: center;
                cursor: pointer;
                font-size: 15px;
                transition: all 0.15s ease;
            }
            .khad-close-btn:hover {
                background: #e2e8f0;
                color: #0f172a;
            }

            /* Hero Icon */
            .khad-icon-hero {
                width: 72px;
                height: 72px;
                margin: 0 auto 16px;
                border-radius: 50%;
                display: flex;
                align-items: center;
                justify-content: center;
                font-size: 32px;
                color: #ffffff;
                position: relative;
                animation: khadPulse 2s infinite;
            }
            .khad-icon-hero.success {
                background: linear-gradient(135deg, #10b981, #059669);
                box-shadow: 0 10px 25px rgba(16, 185, 129, 0.35);
            }
            .khad-icon-hero.danger {
                background: linear-gradient(135deg, #ef4444, #dc2626);
                box-shadow: 0 10px 25px rgba(239, 68, 68, 0.35);
            }
            .khad-icon-hero.warning {
                background: linear-gradient(135deg, #f59e0b, #d97706);
                box-shadow: 0 10px 25px rgba(245, 158, 11, 0.35);
            }
            .khad-icon-hero.info {
                background: linear-gradient(135deg, #0ea5e9, #0284c7);
                box-shadow: 0 10px 25px rgba(14, 165, 233, 0.35);
            }

            @keyframes khadPulse {
                0% { transform: scale(1); }
                50% { transform: scale(1.04); }
                100% { transform: scale(1); }
            }

            /* Title & Message */
            .khad-title {
                font-size: 19px;
                font-weight: 800;
                color: #1e293b;
                margin-bottom: 8px;
            }
            .khad-message {
                font-size: 14.5px;
                line-height: 1.65;
                color: #475569;
                margin-bottom: 22px;
                word-break: break-word;
                white-space: pre-line;
            }

            /* Buttons */
            .khad-actions {
                display: flex;
                gap: 10px;
                justify-content: center;
            }
            .khad-btn {
                flex: 1;
                padding: 12px 20px;
                border-radius: 50px;
                font-size: 14.5px;
                font-weight: 700;
                border: none;
                cursor: pointer;
                transition: transform 0.15s ease, box-shadow 0.15s ease, filter 0.15s ease;
                display: inline-flex;
                align-items: center;
                justify-content: center;
                gap: 6px;
                text-decoration: none;
            }
            .khad-btn:active {
                transform: scale(0.97);
            }
            .khad-btn-primary {
                color: #ffffff;
                box-shadow: 0 6px 16px rgba(37, 99, 235, 0.35);
            }
            .khad-btn-primary.success {
                background: linear-gradient(135deg, #10b981, #059669);
                box-shadow: 0 6px 16px rgba(16, 185, 129, 0.35);
            }
            .khad-btn-primary.danger {
                background: linear-gradient(135deg, #ef4444, #dc2626);
                box-shadow: 0 6px 16px rgba(239, 68, 68, 0.35);
            }
            .khad-btn-primary.warning {
                background: linear-gradient(135deg, #f59e0b, #d97706);
                box-shadow: 0 6px 16px rgba(245, 158, 11, 0.35);
            }
            .khad-btn-primary.info {
                background: linear-gradient(135deg, #1d4ed8, #2563eb);
                box-shadow: 0 6px 16px rgba(37, 99, 235, 0.35);
            }
            .khad-btn-primary:hover {
                filter: brightness(1.06);
            }
            .khad-btn-secondary {
                background: #f1f5f9;
                color: #475569;
                border: 1px solid #e2e8f0;
            }
            .khad-btn-secondary:hover {
                background: #e2e8f0;
                color: #0f172a;
            }

            /* Floating Toast */
            .khad-toast-container {
                position: fixed;
                top: 24px;
                left: 50%;
                transform: translateX(-50%);
                z-index: 99999999;
                display: flex;
                flex-direction: column;
                gap: 8px;
                pointer-events: none;
                width: 90%;
                max-width: 380px;
                direction: rtl;
                font-family: 'Tajawal', 'Outfit', sans-serif;
            }
            .khad-toast {
                pointer-events: auto;
                background: rgba(255, 255, 255, 0.95);
                backdrop-filter: blur(12px);
                border-radius: 18px;
                box-shadow: 0 15px 35px rgba(0, 0, 0, 0.15), 0 0 0 1px rgba(0, 0, 0, 0.05);
                padding: 12px 18px;
                display: flex;
                align-items: center;
                gap: 12px;
                font-size: 14px;
                font-weight: 600;
                color: #1e293b;
                transform: translateY(-20px) scale(0.95);
                opacity: 0;
                transition: all 0.25s cubic-bezier(0.16, 1, 0.3, 1);
            }
            .khad-toast.khad-show {
                transform: translateY(0) scale(1);
                opacity: 1;
            }
            .khad-toast-icon {
                width: 32px;
                height: 32px;
                border-radius: 50%;
                display: flex;
                align-items: center;
                justify-content: center;
                color: #fff;
                font-size: 15px;
                flex-shrink: 0;
            }
            .khad-toast-icon.success { background: #10b981; }
            .khad-toast-icon.danger  { background: #ef4444; }
            .khad-toast-icon.warning { background: #f59e0b; }
            .khad-toast-icon.info    { background: #2a7f8f; }
        `;
        document.head.appendChild(style);
    }

    // Helper to detect type from text
    function detectType(message) {
        if (!message) return 'info';
        const str = String(message).toLowerCase();
        const successKeywords = ['تم ', 'بنجاح', 'مبروك', 'تهانينا', 'حفظت', 'تمت', 'شكراً', 'أضيف', 'سجلت', 'success'];
        const dangerKeywords = ['خطأ', 'فشل', 'غير مصرح', 'عفواً', 'لا يمكنك', 'محظور', 'حذف نهائياً', 'error', 'failed'];
        const warningKeywords = ['تحذير', 'تنبيه', 'يرجى', 'تأكد', 'يجب', 'هل أنت متأكد', 'هل تريد', 'warning'];

        for (const k of dangerKeywords) {
            if (str.includes(k)) return 'danger';
        }
        for (const k of successKeywords) {
            if (str.includes(k)) return 'success';
        }
        for (const k of warningKeywords) {
            if (str.includes(k)) return 'warning';
        }
        return 'info';
    }

    // Helper to get default title
    function getDefaultTitle(type, isConfirm) {
        if (isConfirm) return 'تأكيد الإجراء';
        switch (type) {
            case 'success': return 'تمت العملية بنجاح! ✨';
            case 'danger':  return 'تنبيه بالخطأ';
            case 'warning': return 'تنبيه هام';
            default:        return 'إشعار من خدماوي';
        }
    }

    // Helper to get icon HTML
    function getHeroIcon(type) {
        switch (type) {
            case 'success':
                return `<svg width="34" height="34" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="3" stroke-linecap="round" stroke-linejoin="round"><polyline points="20 6 9 17 4 12"></polyline></svg>`;
            case 'danger':
                return `<svg width="34" height="34" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round"><line x1="18" y1="6" x2="6" y2="18"></line><line x1="6" y1="6" x2="18" y2="18"></line></svg>`;
            case 'warning':
                return `<svg width="34" height="34" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round"><path d="M10.29 3.86L1.82 18a2 2 0 0 0 1.71 3h16.94a2 2 0 0 0 1.71-3L13.71 3.86a2 2 0 0 0-3.42 0z"></path><line x1="12" y1="9" x2="12" y2="13"></line><line x1="12" y1="17" x2="12.01" y2="17"></line></svg>`;
            default:
                return `<svg width="34" height="34" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round"><circle cx="12" cy="12" r="10"></circle><line x1="12" y1="16" x2="12" y2="12"></line><line x1="12" y1="8" x2="12.01" y2="8"></line></svg>`;
        }
    }

    // Play subtle sound if sounds module available
    function playSoundForType(type) {
        try {
            if (window.KhadamatSounds && typeof window.KhadamatSounds.playStoredSound === 'function') {
                if (type === 'success') window.KhadamatSounds.playStoredSound('notification');
                else if (type === 'danger') window.KhadamatSounds.playSynthBeep(320, 0.2);
            }
        } catch (e) { }
    }

    /**
     * Show App Modal (Universal Dialog)
     */
    window.showAppModal = function (options) {
        return new Promise((resolve) => {
            const rawMessage = typeof options === 'string' ? options : (options.message || '');
            const isConfirm = options.isConfirm || false;
            const type = options.type || detectType(rawMessage);
            const title = options.title || getDefaultTitle(type, isConfirm);
            const confirmText = options.confirmText || (isConfirm ? 'تأكيد' : 'حسناً');
            const cancelText = options.cancelText || 'إلغاء';

            // Clean message: if it mentions ugly browser details, strip them
            const message = String(rawMessage)
                .replace(/^https?:\/\/[^\s]+\s+يقول:?\s*/i, '')
                .replace(/^https?:\/\/[^\s]+\s+says:?\s*/i, '');

            playSoundForType(type);

            // Create Backdrop
            const backdrop = document.createElement('div');
            backdrop.className = 'khad-modal-backdrop';

            backdrop.innerHTML = `
                <div class="khad-dialog-box" role="dialog" aria-modal="true">
                    <div class="khad-top-bar ${type}"></div>
                    
                    <div class="khad-header">
                        <div class="khad-brand">
                            <img src="/logo.png" alt="خدماوي" onerror="this.style.display='none'" />
                            <span>خدماوي | Khadamawy</span>
                        </div>
                        <button type="button" class="khad-close-btn" aria-label="إغلاق">&times;</button>
                    </div>

                    <div class="khad-icon-hero ${type}">
                        ${getHeroIcon(type)}
                    </div>

                    <h3 class="khad-title">${title}</h3>
                    <p class="khad-message">${message}</p>

                    <div class="khad-actions">
                        ${isConfirm ? `<button type="button" class="khad-btn khad-btn-secondary khad-cancel-btn">${cancelText}</button>` : ''}
                        <button type="button" class="khad-btn khad-btn-primary ${type} khad-confirm-btn">${confirmText}</button>
                    </div>
                </div>
            `;

            document.body.appendChild(backdrop);

            // Trigger animation
            requestAnimationFrame(() => {
                backdrop.classList.add('khad-show');
            });

            function cleanup(result) {
                backdrop.classList.remove('khad-show');
                setTimeout(() => {
                    if (backdrop.parentNode) {
                        backdrop.parentNode.removeChild(backdrop);
                    }
                    resolve(result);
                }, 220);
                document.removeEventListener('keydown', onKeyDown);
            }

            const confirmBtn = backdrop.querySelector('.khad-confirm-btn');
            const cancelBtn = backdrop.querySelector('.khad-cancel-btn');
            const closeBtn = backdrop.querySelector('.khad-close-btn');

            if (confirmBtn) {
                confirmBtn.addEventListener('click', () => cleanup(true));
                confirmBtn.focus();
            }
            if (cancelBtn) {
                cancelBtn.addEventListener('click', () => cleanup(false));
            }
            if (closeBtn) {
                closeBtn.addEventListener('click', () => cleanup(false));
            }

            // Close on backdrop click
            backdrop.addEventListener('click', (e) => {
                if (e.target === backdrop) {
                    cleanup(false);
                }
            });

            // Keyboard handler (Enter / ESC)
            function onKeyDown(e) {
                if (e.key === 'Escape') {
                    cleanup(false);
                } else if (e.key === 'Enter') {
                    cleanup(true);
                }
            }
            document.addEventListener('keydown', onKeyDown);
        });
    };

    /**
     * Show App Floating Toast
     */
    window.showAppToast = function (message, type, duration) {
        type = type || detectType(message);
        duration = duration || 3500;

        let container = document.getElementById('khad-toast-container');
        if (!container) {
            container = document.createElement('div');
            container.id = 'khad-toast-container';
            container.className = 'khad-toast-container';
            document.body.appendChild(container);
        }

        const toast = document.createElement('div');
        toast.className = 'khad-toast';
        toast.innerHTML = `
            <div class="khad-toast-icon ${type}">
                ${type === 'success' ? '✓' : type === 'danger' ? '✕' : '!'}
            </div>
            <div style="flex:1;">${message}</div>
        `;

        container.appendChild(toast);
        requestAnimationFrame(() => toast.classList.add('khad-show'));

        setTimeout(() => {
            toast.classList.remove('khad-show');
            setTimeout(() => {
                if (toast.parentNode) toast.parentNode.removeChild(toast);
            }, 250);
        }, duration);
    };

    // Override Global Alert & Confirm
    window.alert = function (message) {
        return window.showAppModal({
            message: message,
            isConfirm: false
        });
    };

    window.confirm = function (message) {
        return window.showAppModal({
            message: message,
            isConfirm: true
        });
    };

    console.log('[Khadamawy] Universal custom dialogs & alerts initialized.');
})();
