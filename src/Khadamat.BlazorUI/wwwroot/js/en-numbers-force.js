/**
 * en-numbers-force.js
 * Automatically forces English numeric keypad (0-9) on mobile devices (Android, iOS, MAUI)
 * for phone, WhatsApp, payment, wallet, and numeric inputs, and converts any Arabic-Indic
 * digits (٠-٩, ۰-۹) to standard English digits (0-9) in real-time.
 */
(function () {
    'use strict';

    var ARABIC_INDIC_MAP = {
        '٠': '0', '١': '1', '٢': '2', '٣': '3', '٤': '4',
        '٥': '5', '٦': '6', '٧': '7', '٨': '8', '٩': '9',
        '۰': '0', '۱': '1', '۲': '2', '۳': '3', '۴': '4',
        '۵': '5', '۶': '6', '۷': '7', '۸': '8', '۹': '9'
    };

    var ARABIC_DIGITS_REGEX = /[\u0660-\u0669\u06F0-\u06F9]/g;

    function toEnglishDigits(str) {
        if (!str) return str;
        return str.replace(ARABIC_DIGITS_REGEX, function (d) {
            return ARABIC_INDIC_MAP[d] || d;
        });
    }

    var MATCH_KEYWORDS = [
        'phone', 'tel', 'mobile', 'whatsapp', 'whats', 'instapay',
        'vodafone', 'wallet', 'cash', 'fawry', 'price', 'amount',
        'otp', 'pincode', 'postal', 'zip'
    ];

    var MATCH_ARABIC_KEYWORDS = [
        'هاتف', 'تليفون', 'موبايل', 'جوال', 'واتس', 'انستاباي',
        'فودافون', 'محفظة', 'كاش', 'سعر', 'مبلغ', 'رقم'
    ];

    function isNumericOrPhoneInput(el) {
        if (!el || el.tagName !== 'INPUT') return false;

        var type = (el.getAttribute('type') || 'text').toLowerCase();
        if (type === 'tel' || type === 'number') return true;

        if (el.hasAttribute('data-en-numbers') || el.classList.contains('en-numbers')) return true;

        var inputmode = (el.getAttribute('inputmode') || '').toLowerCase();
        if (inputmode === 'tel' || inputmode === 'numeric' || inputmode === 'decimal') return true;

        var id = (el.id || '').toLowerCase();
        var name = (el.name || '').toLowerCase();
        var placeholder = (el.placeholder || '').toLowerCase();
        var ariaLabel = (el.getAttribute('aria-label') || '').toLowerCase();
        var className = (el.className || '').toLowerCase();

        var combined = id + ' ' + name + ' ' + className;

        for (var i = 0; i < MATCH_KEYWORDS.length; i++) {
            if (combined.indexOf(MATCH_KEYWORDS[i]) !== -1) return true;
        }

        var combinedAr = placeholder + ' ' + ariaLabel;
        for (var j = 0; j < MATCH_ARABIC_KEYWORDS.length; j++) {
            if (combinedAr.indexOf(MATCH_ARABIC_KEYWORDS[j]) !== -1) return true;
        }

        return false;
    }

    function configureElement(el) {
        if (!isNumericOrPhoneInput(el)) return;

        // Force English keyboard layout on mobile devices:
        // Virtual keyboards use lang="en" and dir="ltr" to switch to standard Latin/English keypad
        if (el.getAttribute('lang') !== 'en') {
            el.setAttribute('lang', 'en');
        }
        if (el.getAttribute('dir') !== 'ltr') {
            el.setAttribute('dir', 'ltr');
        }

        var type = (el.getAttribute('type') || '').toLowerCase();
        var inputmode = (el.getAttribute('inputmode') || '').toLowerCase();

        if (!inputmode) {
            if (type === 'number') {
                el.setAttribute('inputmode', 'decimal');
            } else {
                el.setAttribute('inputmode', 'tel');
            }
        }
    }

    function processValueChange(input) {
        if (!input || !input.value) return;
        var val = input.value;
        if (ARABIC_DIGITS_REGEX.test(val)) {
            var start = input.selectionStart;
            var end = input.selectionEnd;
            var converted = toEnglishDigits(val);
            input.value = converted;

            // Restore cursor position
            if (start !== null && end !== null) {
                try {
                    input.setSelectionRange(start, end);
                } catch (e) { }
            }

            // Dispatch input and change events for Blazor two-way data binding
            input.dispatchEvent(new Event('input', { bubbles: true, cancelable: true }));
            input.dispatchEvent(new Event('change', { bubbles: true, cancelable: true }));
        }
    }

    // Global event delegation
    document.addEventListener('focusin', function (e) {
        if (isNumericOrPhoneInput(e.target)) {
            configureElement(e.target);
            processValueChange(e.target);
        }
    }, true);

    document.addEventListener('beforeinput', function (e) {
        if (!isNumericOrPhoneInput(e.target)) return;
        configureElement(e.target);

        if (e.data && ARABIC_DIGITS_REGEX.test(e.data)) {
            e.preventDefault();
            var converted = toEnglishDigits(e.data);
            if (document.execCommand) {
                document.execCommand('insertText', false, converted);
            } else {
                var target = e.target;
                var start = target.selectionStart || 0;
                var end = target.selectionEnd || 0;
                var prev = target.value || '';
                target.value = prev.substring(0, start) + converted + prev.substring(end);
                var nextPos = start + converted.length;
                try {
                    target.setSelectionRange(nextPos, nextPos);
                } catch (err) { }
                target.dispatchEvent(new Event('input', { bubbles: true, cancelable: true }));
                target.dispatchEvent(new Event('change', { bubbles: true, cancelable: true }));
            }
        }
    }, true);

    document.addEventListener('input', function (e) {
        if (isNumericOrPhoneInput(e.target)) {
            processValueChange(e.target);
        }
    }, true);

    document.addEventListener('paste', function (e) {
        if (!isNumericOrPhoneInput(e.target)) return;
        var clipboardData = e.clipboardData || window.clipboardData;
        if (!clipboardData) return;
        var pastedText = clipboardData.getData('text');
        if (pastedText && ARABIC_DIGITS_REGEX.test(pastedText)) {
            e.preventDefault();
            var converted = toEnglishDigits(pastedText);
            if (document.execCommand) {
                document.execCommand('insertText', false, converted);
            } else {
                var target = e.target;
                var start = target.selectionStart || 0;
                var end = target.selectionEnd || 0;
                var prev = target.value || '';
                target.value = prev.substring(0, start) + converted + prev.substring(end);
                var nextPos = start + converted.length;
                try {
                    target.setSelectionRange(nextPos, nextPos);
                } catch (err) { }
                target.dispatchEvent(new Event('input', { bubbles: true, cancelable: true }));
                target.dispatchEvent(new Event('change', { bubbles: true, cancelable: true }));
            }
        }
    }, true);

    function scanAndConfigureAll() {
        var inputs = document.querySelectorAll('input');
        for (var i = 0; i < inputs.length; i++) {
            if (isNumericOrPhoneInput(inputs[i])) {
                configureElement(inputs[i]);
                processValueChange(inputs[i]);
            }
        }
    }

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', scanAndConfigureAll);
    } else {
        scanAndConfigureAll();
    }

    var observer = new MutationObserver(function (mutations) {
        for (var i = 0; i < mutations.length; i++) {
            var m = mutations[i];
            if (m.type === 'childList' && m.addedNodes.length > 0) {
                for (var j = 0; j < m.addedNodes.length; j++) {
                    var node = m.addedNodes[j];
                    if (node.nodeType === 1) {
                        if (node.tagName === 'INPUT' && isNumericOrPhoneInput(node)) {
                            configureElement(node);
                        } else if (node.querySelectorAll) {
                            var subInputs = node.querySelectorAll('input');
                            for (var k = 0; k < subInputs.length; k++) {
                                if (isNumericOrPhoneInput(subInputs[k])) {
                                    configureElement(subInputs[k]);
                                }
                            }
                        }
                    }
                }
            }
        }
    });

    observer.observe(document.body || document.documentElement, {
        childList: true,
        subtree: true
    });
})();
