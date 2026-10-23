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

            // Only track when inside mobile app or if user is testing
            const deviceId = getOrCreateDeviceId();
            const info = getDeviceInfo();

            let savedEmail = null;
            let savedName = null;
            try {
                savedEmail = localStorage.getItem('khadamawy_tester_email') || null;
                savedName = localStorage.getItem('khadamawy_tester_name') || null;
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
            }).catch(function () { });

            // If native app and tester email is unknown, show optional friendly banner after 2 seconds
            if (isNative && !savedEmail) {
                setTimeout(maybeShowTesterBanner, 2000);
            }
        } catch (err) { }
    }

    function maybeShowTesterBanner() {
        try {
            if (localStorage.getItem('khadamawy_tester_prompt_dismissed')) return;
            if (localStorage.getItem('khadamawy_tester_email')) return;
            if (document.getElementById('tester-identify-banner')) return;

            const banner = document.createElement('div');
            banner.id = 'tester-identify-banner';
            banner.dir = 'rtl';
            banner.style.cssText = `
                position: fixed;
                bottom: 20px;
                left: 15px;
                right: 15px;
                max-width: 420px;
                margin: 0 auto;
                background: linear-gradient(135deg, #1e1b4b 0%, #312e81 100%);
                color: #ffffff;
                padding: 16px 18px;
                border-radius: 18px;
                box-shadow: 0 10px 30px rgba(0,0,0,0.35);
                z-index: 99999;
                font-family: 'Tajawal', sans-serif;
                border: 1px solid rgba(255,255,255,0.15);
                animation: slideUp 0.3s ease-out;
            `;

            banner.innerHTML = `
                <div style="display:flex; justify-content:space-between; align-items:center; margin-bottom:8px;">
                    <div style="font-weight:bold; font-size:0.95rem; display:flex; align-items:center; gap:6px;">
                        <span>🌟</span>
                        <span>مختبر Google Play؟</span>
                    </div>
                    <button type="button" id="tester-dismiss-btn" style="background:transparent; border:none; color:#a5b4fc; font-size:1.1rem; cursor:pointer; padding:0 4px;">✕</button>
                </div>
                <div style="font-size:0.8rem; color:#cbd5e1; margin-bottom:12px; line-height:1.4;">
                    فضلاً اكتب إيميل جوجل الخاص بك لتسجيل أيام حضورك في اختبار الـ 14 يوماً ✅
                </div>
                <div style="display:flex; gap:8px;">
                    <input type="email" id="tester-email-input" placeholder="example@gmail.com" style="flex:1; padding:8px 12px; border-radius:10px; border:1px solid #4338ca; background:#0f172a; color:#fff; font-size:0.85rem; outline:none;" />
                    <button type="button" id="tester-save-btn" style="background:#4f46e5; color:#fff; border:none; padding:8px 16px; border-radius:10px; font-weight:bold; font-size:0.85rem; cursor:pointer;">حفظ</button>
                </div>
            `;

            document.body.appendChild(banner);

            document.getElementById('tester-dismiss-btn').onclick = function () {
                banner.remove();
                try { localStorage.setItem('khadamawy_tester_prompt_dismissed', '1'); } catch (e) { }
            };

            document.getElementById('tester-save-btn').onclick = async function () {
                const input = document.getElementById('tester-email-input');
                const email = (input && input.value ? input.value : '').trim();
                if (!email || email.indexOf('@') === -1) {
                    alert('يرجى كتابة بريد إلكتروني صحيح.');
                    return;
                }

                try {
                    localStorage.setItem('khadamawy_tester_email', email);
                    const deviceId = getOrCreateDeviceId();
                    await fetch('/v1/testers/identify', {
                        method: 'POST',
                        headers: { 'Content-Type': 'application/json' },
                        body: JSON.stringify({ deviceIdentifier: deviceId, testerEmail: email })
                    });
                } catch (e) { }

                banner.innerHTML = `
                    <div style="text-align:center; padding:10px 0; color:#34d399; font-weight:bold; font-size:0.9rem;">
                        ✓ شكراً لك! تم تسجيل بريدك واحتساب مشاركتك بنجاح.
                    </div>
                `;
                setTimeout(function () { banner.remove(); }, 2500);
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
