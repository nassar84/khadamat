/**
 * Google Play Tester Tracking & Auto-Presence Ping
 * Runs on every app open to record attendance, streak, and device identification
 */
(function () {
    function getOrCreateDeviceId() {
        let id = null;
        try {
            id = localStorage.getItem('khadamawy_device_id');
        } catch (e) { }

        if (!id) {
            const match = document.cookie.match(/(?:^|; )khadamawy_device_id=([^;]*)/);
            id = match ? match[1] : null;
        }

        if (!id) {
            id = 'dev_' + (window.crypto && crypto.randomUUID ? crypto.randomUUID() : (Date.now().toString(36) + Math.random().toString(36).substring(2, 10)));
            try { localStorage.setItem('khadamawy_device_id', id); } catch (e) { }
            document.cookie = 'khadamawy_device_id=' + id + '; path=/; max-age=315360000; SameSite=Lax';
        }
        return id;
    }

    function getDeviceInfo() {
        const ua = navigator.userAgent || '';
        let os = 'Android';
        let model = 'Android Device';

        if (/Android/i.test(ua)) {
            const osMatch = ua.match(/Android\s+([0-9.]+)/i);
            os = osMatch ? 'Android ' + osMatch[1] : 'Android';

            // Extract hardware model: e.g. "; SM-S918B Build/"
            const modelMatch = ua.match(/;\s*([^;)]+?)\s*Build\//i);
            if (modelMatch && modelMatch[1]) {
                model = modelMatch[1].trim();
            } else {
                const paren = ua.match(/\(([^)]+)\)/);
                if (paren) {
                    const parts = paren[1].split(';');
                    if (parts.length > 2) model = parts[parts.length - 1].trim();
                }
            }
        } else if (/iPhone|iPad/i.test(ua)) {
            os = 'iOS';
            model = /iPhone/i.test(ua) ? 'iPhone' : 'iPad';
        } else if (/Windows/i.test(ua)) {
            os = 'Windows';
            model = 'PC';
        }
        return { os, model };
    }

    async function pingTesterActivity() {
        try {
            const isNative = (navigator.userAgent && navigator.userAgent.indexOf('KhadamatNativeApp') !== -1) ||
                window.location.search.indexOf('nativeapp=1') !== -1 ||
                sessionStorage.getItem('nativeapp') === '1';

            const deviceId = getOrCreateDeviceId();
            const info = getDeviceInfo();

            let savedEmail = null;
            let savedName = null;
            let savedPhone = null;
            try {
                savedEmail = localStorage.getItem('khadamawy_tester_email') || null;
                savedName = localStorage.getItem('khadamawy_tester_name') || null;
                savedPhone = localStorage.getItem('khadamawy_tester_phone') || null;
            } catch (e) { }

            // Deduplicate pings within 15 minutes in the same session
            const lastPing = sessionStorage.getItem('khadamawy_last_ping_ts');
            const now = Date.now();
            if (lastPing && (now - parseInt(lastPing)) < 15 * 60 * 1000) {
                return;
            }
            sessionStorage.setItem('khadamawy_last_ping_ts', now.toString());

            const headers = { 'Content-Type': 'application/json' };
            try {
                const token = localStorage.getItem('authToken');
                if (token) headers['Authorization'] = 'Bearer ' + token.replace(/^"(.*)"$/, '$1');
            } catch (e) { }

            const payload = {
                deviceIdentifier: deviceId,
                testerEmail: savedEmail,
                testerName: savedName,
                deviceModel: info.model,
                operatingSystem: info.os,
                isNativeApp: isNative
            };

            fetch('/v1/testers/ping', {
                method: 'POST',
                headers: headers,
                body: JSON.stringify(payload),
                keepalive: true
            })
            .then(function (res) { return res.json(); })
            .then(function (resData) {
                if (resData && resData.data) {
                    if (resData.data.isTrackingEnabled === false) {
                        try { localStorage.setItem('khadamawy_tester_tracking_disabled', '1'); } catch (e) { }
                        const modal = document.getElementById('tester-identify-backdrop');
                        if (modal) modal.remove();
                        return;
                    } else {
                        try { localStorage.removeItem('khadamawy_tester_tracking_disabled'); } catch (e) { }
                    }
                }

                // If native app, tester email is unknown, and tracking is enabled: show friendly popup modal
                if (isNative && !savedEmail && localStorage.getItem('khadamawy_tester_tracking_disabled') !== '1') {
                    setTimeout(maybeShowTesterModal, 1500);
                }
            })
            .catch(function () { });
        } catch (err) { }
    }

    function maybeShowTesterModal() {
        try {
            // Check suppression flags
            if (localStorage.getItem('khadamawy_tester_tracking_disabled') === '1') return;
            if (sessionStorage.getItem('khadamawy_tester_prompt_dismissed_session') === '1') return;
            if (localStorage.getItem('khadamawy_tester_email')) return;
            if (document.getElementById('tester-identify-backdrop')) return;

            const backdrop = document.createElement('div');
            backdrop.id = 'tester-identify-backdrop';
            backdrop.dir = 'rtl';
            backdrop.style.cssText = `
                position: fixed;
                inset: 0;
                background: rgba(15, 23, 42, 0.65);
                backdrop-filter: blur(4px);
                -webkit-backdrop-filter: blur(4px);
                z-index: 999999;
                display: flex;
                align-items: center;
                justify-content: center;
                padding: 16px;
                animation: gpFadeIn 0.25s ease-out;
                font-family: 'Tajawal', system-ui, -apple-system, sans-serif;
            `;

            backdrop.innerHTML = `
                <style>
                    @keyframes gpFadeIn { from { opacity: 0; } to { opacity: 1; } }
                    @keyframes gpScaleUp { from { opacity: 0; transform: scale(0.92) translateY(12px); } to { opacity: 1; transform: scale(1) translateY(0); } }
                    .gp-tester-card {
                        background: #ffffff;
                        color: #0f172a;
                        width: 100%;
                        max-width: 440px;
                        border-radius: 24px;
                        padding: 24px 22px 20px;
                        box-shadow: 0 25px 50px -12px rgba(0, 0, 0, 0.35);
                        border: 1px solid #e2e8f0;
                        animation: gpScaleUp 0.3s cubic-bezier(0.16, 1, 0.3, 1);
                        box-sizing: border-box;
                    }
                    .gp-field {
                        border: 1.5px solid #cbd5e1;
                        border-radius: 14px;
                        padding: 4px 14px 8px 14px;
                        margin: 0 0 14px 0;
                        background: #ffffff;
                        transition: border-color 0.2s, box-shadow 0.2s;
                        box-sizing: border-box;
                    }
                    .gp-field:focus-within {
                        border-color: #2563eb;
                        box-shadow: 0 0 0 3px rgba(37, 99, 235, 0.12);
                    }
                    .gp-field.gp-error {
                        border-color: #ef4444;
                        box-shadow: 0 0 0 3px rgba(239, 68, 68, 0.12);
                    }
                    .gp-field legend {
                        font-size: 0.8rem;
                        font-weight: 700;
                        color: #475569;
                        padding: 0 8px;
                        margin-bottom: 0;
                        float: none;
                        width: auto;
                    }
                    .gp-input-row {
                        display: flex;
                        align-items: center;
                        gap: 10px;
                    }
                    .gp-input-row input {
                        flex: 1;
                        border: none;
                        outline: none;
                        background: transparent;
                        font-size: 0.95rem;
                        color: #0f172a;
                        font-family: inherit;
                        padding: 6px 0 2px;
                    }
                    .gp-input-row input::placeholder {
                        color: #94a3b8;
                    }
                    .gp-icon-box {
                        color: #64748b;
                        display: flex;
                        align-items: center;
                        justify-content: center;
                    }
                </style>

                <div class="gp-tester-card">
                    <!-- Header -->
                    <div style="display:flex; justify-content:space-between; align-items:center; margin-bottom:12px;">
                        <div style="display:flex; align-items:center; gap:8px;">
                            <h3 style="margin:0; font-size:1.15rem; font-weight:800; color:#0f172a;">
                                مختبرو Google Play (تحدي الـ 14 يوماً)
                            </h3>
                            <!-- Verified Check Shield Icon -->
                            <svg width="22" height="22" viewBox="0 0 24 24" fill="none" style="flex-shrink:0;">
                                <path d="M12 2L4 5V11.09C4 16.14 7.41 20.85 12 22C16.59 20.85 20 16.14 20 11.09V5L12 2Z" fill="#1d4ed8"/>
                                <path d="M9.5 12.5L8.09 11.09L7 12.18L9.5 14.68L17 7.18L15.91 6.09L9.5 12.5Z" fill="#ffffff"/>
                            </svg>
                        </div>
                        <button type="button" id="gp-close-btn" style="background:transparent; border:none; color:#64748b; font-size:1.25rem; cursor:pointer; padding:4px 8px; line-height:1; border-radius:8px;">✕</button>
                    </div>

                    <!-- Description -->
                    <p style="font-size:0.875rem; color:#334155; line-height:1.6; margin:0 0 18px 0;">
                        أهلاً بك في النسخة التجريبية! يُرجى إدخال بريدك المسجل في Google Play لاحتساب حضورك اليومي وضمان إتمام فترة الـ 14 يوماً بنجاح 🌿
                    </p>

                    <!-- Form Body -->
                    <div id="gp-form-body">
                        <!-- Mandatory Field: Email -->
                        <fieldset class="gp-field" id="gp-email-field">
                            <legend>بريد Google Play الخاص بك **</legend>
                            <div class="gp-input-row">
                                <input type="email" id="gp-email-input" placeholder="tester@gmail.com" dir="ltr" style="text-align:right;" />
                                <div class="gp-icon-box">
                                    <svg width="18" height="18" viewBox="0 0 24 24" fill="currentColor">
                                        <path d="M20 4H4C2.9 4 2 4.9 2 6V18C2 19.1 2.9 20 4 20H20C21.1 20 22 19.1 22 18V6C22 4.9 21.1 4 20 4ZM20 8L12 13L4 8V6L12 11L20 6V8Z"/>
                                    </svg>
                                </div>
                            </div>
                        </fieldset>

                        <!-- Optional Field: Full Name -->
                        <fieldset class="gp-field">
                            <legend>الاسم الكريم (اختياري)</legend>
                            <div class="gp-input-row">
                                <input type="text" id="gp-name-input" placeholder="اسمك الكريم" />
                                <div class="gp-icon-box">
                                    <svg width="18" height="18" viewBox="0 0 24 24" fill="currentColor">
                                        <path d="M12 12C14.21 12 16 10.21 16 8C16 5.79 14.21 4 12 4C9.79 4 8 5.79 8 8C8 10.21 9.79 12 12 12ZM12 14C9.33 14 4 15.34 4 18V20H20V18C20 15.34 14.67 14 12 14Z"/>
                                    </svg>
                                </div>
                            </div>
                        </fieldset>

                        <!-- Optional Field: WhatsApp -->
                        <fieldset class="gp-field">
                            <legend>رقم الواتساب للتذكير (اختياري)</legend>
                            <div class="gp-input-row">
                                <input type="tel" id="gp-phone-input" placeholder="01xxxxxxxxx" dir="ltr" style="text-align:right;" />
                                <div class="gp-icon-box">
                                    <svg width="18" height="18" viewBox="0 0 24 24" fill="currentColor">
                                        <path d="M6.62 10.79C8.06 13.62 10.38 15.93 13.21 17.38L15.41 15.18C15.69 14.9 16.08 14.82 16.43 14.93C17.55 15.3 18.75 15.5 20 15.5C20.55 15.5 21 15.95 21 16.5V20C21 20.55 20.55 21 20 21C10.61 21 3 13.39 3 4C3 3.45 3.45 3 4 3H7.5C8.05 3 8.5 3.45 8.5 4C8.5 5.25 8.7 6.45 9.07 7.57C9.18 7.92 9.1 8.31 8.82 8.59L6.62 10.79Z"/>
                                    </svg>
                                </div>
                            </div>
                        </fieldset>

                        <div id="gp-error-msg" style="display:none; color:#dc2626; font-size:0.8rem; font-weight:600; margin-bottom:12px;"></div>

                        <!-- Actions Row -->
                        <div style="display:flex; justify-content:space-between; align-items:center; margin-top:18px; gap:12px;">
                            <button type="button" id="gp-later-btn" style="background:transparent; border:none; color:#475569; font-weight:600; font-size:0.9rem; cursor:pointer; padding:8px 6px;">
                                تذكيري لاحقاً
                            </button>
                            <button type="button" id="gp-save-btn" style="background:#1d4ed8; color:#ffffff; border:none; border-radius:12px; padding:11px 22px; font-weight:700; font-size:0.92rem; cursor:pointer; display:flex; align-items:center; gap:8px; box-shadow:0 4px 12px rgba(29, 78, 216, 0.28); transition:transform 0.1s, background-color 0.2s;">
                                <span>حفظ وتأكيد الحضور</span>
                                <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round">
                                    <polyline points="20 6 9 17 4 12"></polyline>
                                </svg>
                            </button>
                        </div>
                    </div>
                </div>
            `;

            document.body.appendChild(backdrop);

            function dismissModal() {
                backdrop.remove();
                try { sessionStorage.setItem('khadamawy_tester_prompt_dismissed_session', '1'); } catch (e) { }
            }

            document.getElementById('gp-close-btn').onclick = dismissModal;
            document.getElementById('gp-later-btn').onclick = dismissModal;

            // Submit handler
            document.getElementById('gp-save-btn').onclick = async function () {
                const emailInput = document.getElementById('gp-email-input');
                const nameInput = document.getElementById('gp-name-input');
                const phoneInput = document.getElementById('gp-phone-input');
                const emailField = document.getElementById('gp-email-field');
                const errorMsg = document.getElementById('gp-error-msg');
                const saveBtn = document.getElementById('gp-save-btn');

                const email = (emailInput && emailInput.value ? emailInput.value : '').trim();
                const name = (nameInput && nameInput.value ? nameInput.value : '').trim();
                const phone = (phoneInput && phoneInput.value ? phoneInput.value : '').trim();

                // Validate email
                if (!email || email.indexOf('@') === -1 || email.indexOf('.') === -1) {
                    if (emailField) emailField.classList.add('gp-error');
                    if (errorMsg) {
                        errorMsg.textContent = 'يرجى إدخال بريد إلكتروني صحيح لتسجيل حضورك.';
                        errorMsg.style.display = 'block';
                    }
                    if (emailInput) emailInput.focus();
                    return;
                }

                if (emailField) emailField.classList.remove('gp-error');
                if (errorMsg) errorMsg.style.display = 'none';

                saveBtn.disabled = true;
                saveBtn.style.opacity = '0.7';
                saveBtn.innerHTML = 'جاري الحفظ...';

                try {
                    const deviceId = getOrCreateDeviceId();
                    const payload = {
                        deviceIdentifier: deviceId,
                        testerEmail: email,
                        testerName: name || null,
                        phoneNumber: phone || null
                    };

                    const resp = await fetch('/v1/testers/identify', {
                        method: 'POST',
                        headers: { 'Content-Type': 'application/json' },
                        body: JSON.stringify(payload)
                    });

                    const resJson = await resp.json();
                    if (resJson && resJson.success) {
                        try {
                            localStorage.setItem('khadamawy_tester_email', email);
                            if (name) localStorage.setItem('khadamawy_tester_name', name);
                            if (phone) localStorage.setItem('khadamawy_tester_phone', phone);
                        } catch (e) { }

                        const card = backdrop.querySelector('.gp-tester-card');
                        if (card) {
                            const displayName = name ? `يا ${name}` : '';
                            card.innerHTML = `
                                <div style="text-align:center; padding:18px 8px;">
                                    <div style="width:60px; height:60px; background:#dcfce7; border-radius:50%; display:flex; align-items:center; justify-content:center; margin:0 auto 14px auto;">
                                        <svg width="32" height="32" viewBox="0 0 24 24" fill="none" stroke="#16a34a" stroke-width="3" stroke-linecap="round" stroke-linejoin="round">
                                            <polyline points="20 6 9 17 4 12"></polyline>
                                        </svg>
                                    </div>
                                    <h3 style="color:#15803d; font-weight:800; font-size:1.2rem; margin:0 0 8px 0;">تم تأكيد حضورك بنجاح!</h3>
                                    <p style="color:#475569; font-size:0.9rem; line-height:1.6; margin:0;">
                                        شكراً جزيلاً لتعاونك ${displayName} 🌟<br/>تم احتساب اليوم في سلسلة الـ 14 يوماً الخاصة بمتجر Google Play.
                                    </p>
                                </div>
                            `;
                        }

                        setTimeout(function () {
                            backdrop.remove();
                        }, 2500);
                        return;
                    }
                } catch (e) { }

                saveBtn.disabled = false;
                saveBtn.style.opacity = '1';
                saveBtn.innerHTML = 'حفظ وتأكيد الحضور ✓';
                if (errorMsg) {
                    errorMsg.textContent = 'حدث خطأ في الاتصال، يرجى المحاولة مرة أخرى.';
                    errorMsg.style.display = 'block';
                }
            };
        } catch (e) { }
    }

    // Trigger ping on load
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', pingTesterActivity);
    } else {
        pingTesterActivity();
    }
})();
