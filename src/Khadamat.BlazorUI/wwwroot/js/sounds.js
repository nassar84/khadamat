
window.KhadamatSounds = {
    audioContext: null,
    sounds: {},

    init: function() {
        if (!this.audioContext) {
            this.audioContext = new (window.AudioContext || window.webkitAudioContext)();
        }
        console.log("Khadamat Sounds Initialized");
    },

    playStoredSound: function(name) {
        if (!name) return;
        let filename = name;
        if (!filename.includes('.')) filename += '.mp3';
        const audio = new Audio(`/sounds/${filename}`);
        audio.play().catch(e => {
            console.warn("Could not play sound file, using synth fallback:", filename);
            if (name.includes('bic_ring1') || name.includes('startup')) this.playSynthBeep(440, 0.4);
            else if (name.includes('message')) this.playSynthBeep(880, 0.1);
            else this.playSynthBeep(660, 0.2);
        });
    },

    playStartup: function() {
        this.playStoredSound('bic_ring1');
    },

    playNotification: function() {
        this.playStoredSound('notification');
    },

    playMessage: function() {
        this.playStoredSound('message');
        this.showVisualEffect('message');
    },

    showVisualEffect: function(type) {
        const div = document.createElement('div');
        div.className = `visual-effect-${type}`;
        div.style.position = 'fixed';
        div.style.top = '50%';
        div.style.left = '50%';
        div.style.transform = 'translate(-50%, -50%)';
        div.style.pointerEvents = 'none';
        div.style.zIndex = '9999';
        
        if (type === 'message') {
            div.innerHTML = '💬';
            div.style.fontSize = '2rem';
        } else {
            div.innerHTML = '✨';
            div.style.fontSize = '2.5rem';
        }

        document.body.appendChild(div);

        // Simple animation
        div.animate([
            { opacity: 0, scale: 0.5, transform: 'translate(-50%, -50%) translateY(20px)' },
            { opacity: 1, scale: 1.2, transform: 'translate(-50%, -50%) translateY(-10px)' },
            { opacity: 0, scale: 0.8, transform: 'translate(-50%, -50%) translateY(-30px)' }
        ], {
            duration: 1000,
            easing: 'ease-out'
        }).onfinish = () => div.remove();
    },

    // A simple synthesized beep if files are missing
    playSynthBeep: function(freq, duration) {
        try {
            if (!this.audioContext) this.init();
            const osc = this.audioContext.createOscillator();
            const gain = this.audioContext.createGain();
            
            osc.type = 'sine';
            osc.frequency.setValueAtTime(freq || 440, this.audioContext.currentTime);
            
            gain.gain.setValueAtTime(0.1, this.audioContext.currentTime);
            gain.gain.exponentialRampToValueAtTime(0.01, this.audioContext.currentTime + (duration || 0.2));
            
            osc.connect(gain);
            gain.connect(this.audioContext.destination);
            
            osc.start();
            osc.stop(this.audioContext.currentTime + (duration || 0.2));
        } catch (e) {
            console.error("Synth beep failed", e);
        }
    }
};
