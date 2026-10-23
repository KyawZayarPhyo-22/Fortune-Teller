// Web Audio API Sound Synthesizer & Canvas Starfield Background for Fortune Teller
window.fortuneAudio = {
    audioCtx: null,

    init: function () {
        if (!this.audioCtx) {
            const AudioContext = window.AudioContext || window.webkitAudioContext;
            if (AudioContext) {
                this.audioCtx = new AudioContext();
            }
        }
    },

    playChime: function () {
        try {
            this.init();
            if (!this.audioCtx) return;
            if (this.audioCtx.state === 'suspended') {
                this.audioCtx.resume();
            }

            const now = this.audioCtx.currentTime;
            const freqs = [528, 660, 792, 1056]; // Solfeggio Love & Miracles frequency

            freqs.forEach((freq, idx) => {
                const osc = this.audioCtx.createOscillator();
                const gain = this.audioCtx.createGain();

                osc.type = 'sine';
                osc.frequency.setValueAtTime(freq, now + idx * 0.08);

                gain.gain.setValueAtTime(0.001, now + idx * 0.08);
                gain.gain.exponentialRampToValueAtTime(0.12, now + idx * 0.08 + 0.05);
                gain.gain.exponentialRampToValueAtTime(0.0001, now + idx * 0.08 + 1.8);

                osc.connect(gain);
                gain.connect(this.audioCtx.destination);

                osc.start(now + idx * 0.08);
                osc.stop(now + idx * 0.08 + 1.9);
            });
        } catch (e) {
            console.debug('Audio not supported or blocked:', e);
        }
    },

    playCardFlip: function () {
        try {
            this.init();
            if (!this.audioCtx) return;
            if (this.audioCtx.state === 'suspended') {
                this.audioCtx.resume();
            }

            const now = this.audioCtx.currentTime;
            const osc = this.audioCtx.createOscillator();
            const gain = this.audioCtx.createGain();

            osc.type = 'triangle';
            osc.frequency.setValueAtTime(220, now);
            osc.frequency.exponentialRampToValueAtTime(440, now + 0.12);

            gain.gain.setValueAtTime(0.08, now);
            gain.gain.exponentialRampToValueAtTime(0.001, now + 0.15);

            osc.connect(gain);
            gain.connect(this.audioCtx.destination);

            osc.start(now);
            osc.stop(now + 0.16);
        } catch (e) {
            console.debug('Audio not supported:', e);
        }
    }
};

// Canvas Starfield Generator
window.initStarfield = function (canvasId) {
    const canvas = document.getElementById(canvasId);
    if (!canvas) return;
    const ctx = canvas.getContext('2d');
    if (!ctx) return;

    let width = canvas.width = window.innerWidth;
    let height = canvas.height = window.innerHeight;

    window.addEventListener('resize', () => {
        width = canvas.width = window.innerWidth;
        height = canvas.height = window.innerHeight;
    });

    const stars = [];
    const numStars = 120;

    for (let i = 0; i < numStars; i++) {
        stars.push({
            x: Math.random() * width,
            y: Math.random() * height,
            radius: Math.random() * 1.5 + 0.3,
            alpha: Math.random(),
            speed: Math.random() * 0.015 + 0.005,
            direction: Math.random() > 0.5 ? 1 : -1
        });
    }

    function animate() {
        ctx.clearRect(0, 0, width, height);

        for (let star of stars) {
            star.alpha += star.speed * star.direction;
            if (star.alpha > 0.95) {
                star.alpha = 0.95;
                star.direction = -1;
            } else if (star.alpha < 0.15) {
                star.alpha = 0.15;
                star.direction = 1;
            }

            ctx.beginPath();
            ctx.arc(star.x, star.y, star.radius, 0, Math.PI * 2);
            ctx.fillStyle = `rgba(255, 230, 160, ${star.alpha})`;
            ctx.shadowBlur = star.radius > 1.2 ? 6 : 0;
            ctx.shadowColor = '#ffd166';
            ctx.fill();
        }

        requestAnimationFrame(animate);
    }

    animate();
};
