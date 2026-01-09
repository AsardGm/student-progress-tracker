// Student Progress Tracker - JavaScript utilities

// Device ID management
function getOrCreateDeviceId() {
    let deviceId = localStorage.getItem('spt_device_id');
    if (!deviceId) {
        deviceId = 'dev_' + Date.now() + '_' + Math.random().toString(36).substr(2, 9);
        localStorage.setItem('spt_device_id', deviceId);
    }
    return deviceId;
}

// Group member info storage
function setGroupMemberInfo(groupId, memberId, nickname) {
    const key = `spt_group_${groupId}`;
    localStorage.setItem(key, JSON.stringify({ memberId, nickname, joinedAt: new Date().toISOString() }));
}

function getGroupMemberInfo(groupId) {
    const key = `spt_group_${groupId}`;
    const data = localStorage.getItem(key);
    return data ? JSON.parse(data) : null;
}

// Scroll to bottom of chat
function scrollToBottom(element) {
    if (element) {
        element.scrollTop = element.scrollHeight;
    }
}

// Voice recognition
let recognition = null;
let recognitionResult = '';

function startVoiceRecognition() {
    if (!('webkitSpeechRecognition' in window) && !('SpeechRecognition' in window)) {
        alert('Váš prohlížeč nepodporuje hlasové rozpoznávání.');
        return;
    }

    const SpeechRecognition = window.SpeechRecognition || window.webkitSpeechRecognition;
    recognition = new SpeechRecognition();
    recognition.lang = 'cs-CZ';
    recognition.continuous = true;
    recognition.interimResults = true;

    recognitionResult = '';

    recognition.onresult = function(event) {
        let interimTranscript = '';
        let finalTranscript = '';

        for (let i = event.resultIndex; i < event.results.length; i++) {
            const transcript = event.results[i][0].transcript;
            if (event.results[i].isFinal) {
                finalTranscript += transcript;
            } else {
                interimTranscript += transcript;
            }
        }

        recognitionResult = finalTranscript || interimTranscript;
    };

    recognition.onerror = function(event) {
        console.error('Speech recognition error:', event.error);
    };

    recognition.start();
}

function stopVoiceRecognition() {
    if (recognition) {
        recognition.stop();
        recognition = null;
    }
    return recognitionResult;
}

// KaTeX rendering
function renderMath() {
    if (typeof renderMathInElement === 'function') {
        renderMathInElement(document.body, {
            delimiters: [
                { left: '$$', right: '$$', display: true },
                { left: '$', right: '$', display: false }
            ],
            throwOnError: false
        });
    }

    // Render individual KaTeX elements
    document.querySelectorAll('.katex-display, .katex-inline').forEach(el => {
        const math = el.getAttribute('data-math');
        if (math && typeof katex !== 'undefined') {
            try {
                katex.render(math, el, {
                    displayMode: el.classList.contains('katex-display'),
                    throwOnError: false
                });
            } catch (e) {
                console.error('KaTeX error:', e);
            }
        }
    });
}

// Prism.js code highlighting
function highlightCode() {
    if (typeof Prism !== 'undefined') {
        Prism.highlightAll();
    }
}

// Auto-render after Blazor updates
if (typeof Blazor !== 'undefined') {
    Blazor.addEventListener('enhancedload', function() {
        renderMath();
        highlightCode();
    });
}

// Mutation observer for dynamic content
const observer = new MutationObserver(function(mutations) {
    mutations.forEach(function(mutation) {
        if (mutation.addedNodes.length) {
            setTimeout(() => {
                renderMath();
                highlightCode();
            }, 100);
        }
    });
});

// Start observing when DOM is ready
document.addEventListener('DOMContentLoaded', function() {
    observer.observe(document.body, {
        childList: true,
        subtree: true
    });

    renderMath();
    highlightCode();
});

// Copy to clipboard
function copyToClipboard(text) {
    navigator.clipboard.writeText(text).then(function() {
        console.log('Copied to clipboard');
    }).catch(function(err) {
        console.error('Could not copy text:', err);
    });
}

// Notifications permission
function requestNotificationPermission() {
    if ('Notification' in window && Notification.permission === 'default') {
        Notification.requestPermission();
    }
}

// Show browser notification
function showNotification(title, body) {
    if ('Notification' in window && Notification.permission === 'granted') {
        new Notification(title, { body, icon: '/favicon.ico' });
    }
}

// Format relative time
function formatRelativeTime(date) {
    const now = new Date();
    const diff = now - new Date(date);
    const minutes = Math.floor(diff / 60000);
    const hours = Math.floor(diff / 3600000);

    if (minutes < 1) return 'právě teď';
    if (minutes < 60) return `před ${minutes} min`;
    if (hours < 24) return `před ${hours} hod`;
    return new Date(date).toLocaleDateString('cs-CZ');
}
