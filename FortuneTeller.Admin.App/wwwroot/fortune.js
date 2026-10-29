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
    },

    playShutter: function () {
        try {
            this.init();
            if (!this.audioCtx) return;
            if (this.audioCtx.state === 'suspended') {
                this.audioCtx.resume();
            }

            const now = this.audioCtx.currentTime;
            
            // Camera mechanical click simulation
            const osc = this.audioCtx.createOscillator();
            const gain = this.audioCtx.createGain();
            osc.type = 'triangle';
            osc.frequency.setValueAtTime(680, now);
            osc.frequency.exponentialRampToValueAtTime(140, now + 0.07);

            gain.gain.setValueAtTime(0.2, now);
            gain.gain.exponentialRampToValueAtTime(0.001, now + 0.08);

            osc.connect(gain);
            gain.connect(this.audioCtx.destination);

            osc.start(now);
            osc.stop(now + 0.09);

            // Resonant celestial harmonic chime
            setTimeout(() => {
                if (window.fortuneAudio && window.fortuneAudio.playChime) {
                    window.fortuneAudio.playChime();
                }
            }, 50);
        } catch (e) {
            console.debug('Shutter audio error:', e);
        }
    }
};

// Live Camera Service for Palmistry Ingestion
window.fortuneCamera = {
    stream: null,
    videoElement: null,
    currentFacingMode: 'user',

    isSupported: function () {
        return !!(navigator.mediaDevices && navigator.mediaDevices.getUserMedia);
    },

    startCamera: async function (videoElementId, facingMode) {
        try {
            if (!this.isSupported()) {
                return {
                    success: false,
                    error: 'Camera is not supported in this browser environment or requires HTTPS/localhost.'
                };
            }

            // Stop existing tracks if any
            this.stopCamera(videoElementId);

            this.currentFacingMode = facingMode || 'user';
            const video = document.getElementById(videoElementId);
            if (!video) {
                return { success: false, error: 'Video viewport element was not found in DOM.' };
            }

            this.videoElement = video;
            video.style.display = 'block';

            // Check available video devices if permissions allow
            let videoInputs = [];
            try {
                if (navigator.mediaDevices.enumerateDevices) {
                    const devices = await navigator.mediaDevices.enumerateDevices();
                    videoInputs = devices.filter(d => d.kind === 'videoinput');
                    // If labels or devices list is populated and has 0 cameras
                    if (devices.length > 0 && videoInputs.length === 0) {
                        return {
                            success: false,
                            noDevicesFound: true,
                            error: 'No video camera detected on your system. Switched to Virtual Demo Camera.'
                        };
                    }
                }
            } catch (e) {
                // Ignore enumerateDevices error
            }

            // Attempt stream with fallback constraints
            let stream = null;
            let lastErr = null;
            const attempts = [
                { audio: false, video: { facingMode: { ideal: this.currentFacingMode } } },
                { audio: false, video: true }
            ];

            for (const constraint of attempts) {
                try {
                    const streamPromise = navigator.mediaDevices.getUserMedia(constraint);
                    const timeoutPromise = new Promise((_, reject) => {
                        setTimeout(() => {
                            const err = new Error('Camera permission request timed out. Please check your browser address bar and click Allow.');
                            err.name = 'TimeoutError';
                            reject(err);
                        }, 8000);
                    });

                    stream = await Promise.race([streamPromise, timeoutPromise]);
                    if (stream) break;
                } catch (e) {
                    lastErr = e;
                    if (e.name === 'NotAllowedError' || e.name === 'PermissionDeniedError' || e.name === 'TimeoutError') {
                        break;
                    }
                }
            }

            if (!stream) {
                throw lastErr || new Error('Could not acquire video stream.');
            }

            this.stream = stream;
            video.srcObject = stream;
            video.setAttribute('playsinline', 'true');
            video.setAttribute('autoplay', 'true');
            video.muted = true;

            // Trigger play without blocking if promise is pending paint
            video.play().catch(playErr => {
                console.warn('Video play warning:', playErr);
            });

            // Mirror preview when using front-facing camera
            if (this.currentFacingMode === 'user') {
                video.style.transform = 'scaleX(-1)';
            } else {
                video.style.transform = 'scaleX(1)';
            }

            return {
                success: true,
                facingMode: this.currentFacingMode,
                hasMultipleCameras: videoInputs.length > 1
            };
        } catch (err) {
            console.error('Camera stream error:', err);
            let message = 'ကင်မရာကို ဖွင့်၍မရပါ။';
            let noDevices = false;
            if (err.name === 'NotAllowedError' || err.name === 'PermissionDeniedError') {
                message = 'ကင်မရာ အသုံးပြုခွင့်ကို ငြင်းပယ်ထားပါသည်။ Browser လိပ်စာဘားရှိ သော့ခလောက်ပုံစံကို နှိပ်၍ Camera ကို "Allow" ပေးပါ။';
            } else if (err.name === 'NotFoundError' || err.name === 'DevicesNotFoundError') {
                message = 'စက်ပစ္စည်းတွင် ကင်မရာ မတွေ့ရှိပါ။';
                noDevices = true;
            } else if (err.name === 'NotReadableError' || err.name === 'TrackStartError') {
                message = 'ကင်မရာကို အခြား application တစ်ခုခုမှ အသုံးပြုနေပါသည်။';
            } else if (err.name === 'TimeoutError') {
                message = err.message;
            } else {
                message = err.message || message;
            }
            return { success: false, noDevicesFound: noDevices, error: message };
        }
    },

    switchCamera: async function (videoElementId) {
        const nextMode = this.currentFacingMode === 'user' ? 'environment' : 'user';
        return await this.startCamera(videoElementId, nextMode);
    },

    startSimulatedCamera: function (videoElementId) {
        try {
            this.stopCamera(videoElementId);
            const video = document.getElementById(videoElementId);
            if (!video) return { success: false, error: 'Video element not found.' };

            this.currentFacingMode = 'simulated';
            this.videoElement = video;
            video.style.display = 'block';
            video.style.transform = 'scaleX(1)';

            const canvas = document.createElement('canvas');
            canvas.width = 640;
            canvas.height = 480;
            const ctx = canvas.getContext('2d');

            const img = new Image();
            img.src = '/images/palms/water-palm.jpg';

            let t = 0;
            const interval = setInterval(() => {
                if (!video.srcObject) {
                    clearInterval(interval);
                    return;
                }
                t += 0.05;
                ctx.fillStyle = '#060a12';
                ctx.fillRect(0, 0, 640, 480);

                if (img.complete && img.naturalWidth > 0) {
                    const scale = 1 + Math.sin(t) * 0.02;
                    const w = 420 * scale;
                    const h = 420 * scale;
                    ctx.drawImage(img, (640 - w) / 2, (480 - h) / 2, w, h);
                }

                // Digital simulation badge
                ctx.fillStyle = 'rgba(20, 184, 166, 0.85)';
                ctx.font = '12px monospace';
                ctx.fillText('VIRTUAL CAMERA ACTIVE · 30 FPS', 20, 30);
            }, 33);

            let stream;
            if (canvas.captureStream) {
                stream = canvas.captureStream(30);
            } else {
                throw new Error('Canvas captureStream not supported in this browser.');
            }

            this.stream = stream;
            video.srcObject = stream;
            video.setAttribute('playsinline', 'true');
            video.setAttribute('autoplay', 'true');
            video.muted = true;
            video.play().catch(e => console.warn(e));

            return {
                success: true,
                facingMode: 'simulated',
                hasMultipleCameras: false
            };
        } catch (e) {
            console.error('Simulated camera error:', e);
            return { success: false, error: e.message };
        }
    },

    stopCamera: function (videoElementId) {
        try {
            if (this.stream) {
                this.stream.getTracks().forEach(track => {
                    try {
                        track.stop();
                    } catch (e) {}
                });
                this.stream = null;
            }
            this.stopHandDetection();
            const video = document.getElementById(videoElementId) || this.videoElement;
            if (video) {
                video.srcObject = null;
            }
            return true;
        } catch (e) {
            console.warn('Error closing camera:', e);
            return false;
        }
    },

    cachedCover: null,
    cachedGuide: null,

    preloadAssets: function () {
        if (!this.cachedCover) {
            this.cachedCover = new Image();
            this.cachedCover.src = '/images/palms/hand-portal-cover.png';
        }
        if (!this.cachedGuide) {
            this.cachedGuide = new Image();
            this.cachedGuide.src = '/images/palms/hand-border-guide.png';
        }
    },

    captureFrame: function (videoElementId) {
        try {
            const video = document.getElementById(videoElementId) || this.videoElement;
            if (!video || !video.videoWidth || !video.videoHeight) {
                return { success: false, error: 'Camera frame is not ready yet. Please wait a second and try again.' };
            }

            const canvas = document.createElement('canvas');
            let w = video.videoWidth;
            let h = video.videoHeight;

            // Constrain maximum dimension for optimal memory and WebSocket payload size
            const maxDim = 800;
            if (w > maxDim || h > maxDim) {
                if (w > h) {
                    h = Math.round((h * maxDim) / w);
                    w = maxDim;
                } else {
                    w = Math.round((w * maxDim) / h);
                    h = maxDim;
                }
            }

            canvas.width = w;
            canvas.height = h;
            const ctx = canvas.getContext('2d');

            // 1. Draw video feed (mirrored for front/selfie camera so user gets what they saw)
            ctx.save();
            if (this.currentFacingMode === 'user') {
                ctx.translate(w, 0);
                ctx.scale(-1, 1);
            }
            ctx.drawImage(video, 0, 0, w, h);
            ctx.restore();

            // 2. Draw portal cover mask to hide room, body and face outside the hand shape
            const coverImg = document.querySelector('.palm-portal-cover') || this.cachedCover;
            if (coverImg && (coverImg.complete || coverImg.naturalWidth > 0)) {
                ctx.drawImage(coverImg, 0, 0, w, h);
            }

            // 3. Draw cyan hand guide lines faintly for high-tech aesthetic
            const guideImg = document.querySelector('#palm-border-guide-img') || this.cachedGuide;
            if (guideImg && (guideImg.complete || guideImg.naturalWidth > 0)) {
                ctx.globalAlpha = 0.75;
                ctx.drawImage(guideImg, 0, 0, w, h);
                ctx.globalAlpha = 1.0;
            }

            const base64Data = canvas.toDataURL('image/jpeg', 0.88);

            try {
                sessionStorage.setItem('fortune_captured_palm', base64Data);
            } catch (storageErr) {
                console.debug('sessionStorage palm cache warning:', storageErr);
            }

            // Shutter feedback sound
            if (window.fortuneAudio && window.fortuneAudio.playShutter) {
                window.fortuneAudio.playShutter();
            }

            return {
                success: true,
                dataUrl: base64Data,
                width: w,
                height: h
            };
        } catch (err) {
            console.error('Frame capture failure:', err);
            return { success: false, error: 'Failed to capture frame: ' + err.message };
        }
    },

    clearPalmCache: function () {
        try {
            sessionStorage.removeItem('fortune_captured_palm');
        } catch (e) {
            console.debug('Failed to clear palm cache:', e);
        }
    },

    version: 10,
    autoDetecting: false,
    framePumpTimer: null,
    consecutiveMatches: 0,
    requiredConsecutive: 1, // Instant capture once fingers enter mold

    initMediaPipe: function () {
        this.preloadAssets();
        if (this.mpInitialized) return;
        if (typeof window.Hands === 'function') {
            try {
                this.mpHands = new window.Hands({
                    locateFile: (file) => `/lib/mediapipe/${file}`
                });
                this.mpHands.setOptions({
                    maxNumHands: 1,
                    modelComplexity: 0,
                    minDetectionConfidence: 0.35,
                    minTrackingConfidence: 0.35
                });
                this.mpHands.onResults((results) => {
                    this.handleHandResults(results);
                });
                this.mpInitialized = true;
                console.log('MediaPipe Hands initialized locally from /lib/mediapipe');
            } catch (e) {
                console.warn('MediaPipe initialization warning:', e);
            }
        }
    },

    handleHandResults: function (results) {
        if (!this.autoDetecting) return;

        let isHandAligned = false;
        let statusHint = null;

        if (results.multiHandLandmarks && results.multiHandLandmarks.length > 0) {
            const lm = results.multiHandLandmarks[0];
            const isSelfie = (this.currentFacingMode === 'user');
            const toScreenX = (x) => isSelfie ? (1.0 - x) : x;

            const wrist = { x: toScreenX(lm[0].x), y: lm[0].y, z: lm[0].z };
            const middleMcp = { x: toScreenX(lm[9].x), y: lm[9].y, z: lm[9].z };
            const middleTip = { x: toScreenX(lm[12].x), y: lm[12].y, z: lm[12].z };

            // 1. Handedness Check (Physical Left Hand preferred)
            let isLeftHand = true;
            if (results.multiHandedness && results.multiHandedness.length > 0) {
                const mpLabel = results.multiHandedness[0].label;
                // On mirrored selfie camera, physical left hand is labeled 'Right'
                isLeftHand = (mpLabel === 'Right' || mpLabel === 'Left');
            }

            // 2. Upright hand check (fingers pointing up above wrist)
            const isUpright = (middleTip.y < wrist.y);

            // 3. Mold Alignment & Span Check (လက်ချောင်းများ ပုံစံခွက်အတွင်း ရောက်ရှိမှု)
            const handSpan = Math.hypot(wrist.x - middleTip.x, wrist.y - middleTip.y);
            const fingersInMold = (middleTip.y <= 0.65 && middleTip.y >= 0.05);
            const isCentered = (middleMcp.x >= 0.15 && middleMcp.x <= 0.85);

            if (!isUpright) {
                statusHint = "လက်ချောင်းများကို အပေါ်သို့ တည့်တည့်ထောင်ပြပါ";
                isHandAligned = false;
            } else if (handSpan < 0.20) {
                statusHint = "လက်ဝါးကို ပုံစံခွက်အပြည့် ကပ်ပြပေးပါ";
                isHandAligned = false;
            } else if (!fingersInMold || !isCentered) {
                statusHint = "လက်ဝါးကို ပုံစံခွက်အတွင်း အံဝင်အောင် ချိန်ပါ";
                isHandAligned = false;
            } else {
                // Fingers are inside the mold: Capture immediately!
                isHandAligned = true;
                statusHint = null;
            }
        } else {
            statusHint = "ဘယ်လက် လက်ဖဝါးကို လမ်းညွှန်ပုံစံခွက်အတွင်း ချိန်ထားပါ";
            isHandAligned = false;
        }

        this.updateAlignmentState(isHandAligned, statusHint);
    },

    updateAlignmentState: function (isAligned, statusHint) {
        if (!this.autoDetecting) return;

        if (isAligned) {
            this.consecutiveMatches++;
            const progress = Math.min(100, Math.round((this.consecutiveMatches / this.requiredConsecutive) * 100));

            if (this.dotNetRef && (progress !== this.lastReportedProgress || this.lastReportedHint !== '')) {
                this.lastReportedProgress = progress;
                this.lastReportedHint = '';
                this.dotNetRef.invokeMethodAsync('OnHandAlignmentProgress', progress, true, null);
            }

            if (this.consecutiveMatches >= this.requiredConsecutive) {
                this.stopHandDetection();
                if (this.dotNetRef) {
                    this.dotNetRef.invokeMethodAsync('OnAutoCaptureTriggered');
                }
            }
        } else {
            this.consecutiveMatches = Math.max(0, this.consecutiveMatches - 2);
            if (this.consecutiveMatches === 0) {
                const hint = statusHint || null;
                if (this.dotNetRef && (this.lastReportedProgress !== 0 || this.lastReportedHint !== (hint || ''))) {
                    this.lastReportedProgress = 0;
                    this.lastReportedHint = hint || '';
                    this.dotNetRef.invokeMethodAsync('OnHandAlignmentProgress', 0, false, hint);
                }
            }
        }
    },

    startHandDetection: function (videoElementId, dotNetRef) {
        this.stopHandDetection();
        this.dotNetRef = dotNetRef;
        this.autoDetecting = true;
        this.consecutiveMatches = 0;
        this.lastReportedProgress = -1;
        this.lastReportedHint = '';
        this.mpProcessing = false;

        this.initMediaPipe();

        const video = document.getElementById(videoElementId) || this.videoElement;
        if (!video) return;

        // If simulated demo camera:
        if (this.currentFacingMode === 'simulated') {
            const simInterval = setInterval(() => {
                if (!this.autoDetecting) {
                    clearInterval(simInterval);
                    return;
                }
                this.updateAlignmentState(true, null);
                if (this.consecutiveMatches >= this.requiredConsecutive) {
                    clearInterval(simInterval);
                }
            }, 80);
            return;
        }

        // Pump video frames to MediaPipe at high responsiveness (~12fps / 80ms)
        const pumpFrame = async () => {
            if (!this.autoDetecting) return;
            const currentVideo = document.getElementById(videoElementId) || this.videoElement;
            if (currentVideo && !currentVideo.paused && !currentVideo.ended && currentVideo.videoWidth > 0) {
                if (this.mpHands && this.mpInitialized && !this.mpProcessing) {
                    this.mpProcessing = true;
                    try {
                        await this.mpHands.send({ image: currentVideo });
                    } catch (e) {
                        console.debug('MediaPipe inference warning:', e);
                    } finally {
                        this.mpProcessing = false;
                    }
                }
            }
            if (this.autoDetecting) {
                this.framePumpTimer = setTimeout(pumpFrame, 80);
            }
        };

        pumpFrame();
    },

    stopHandDetection: function () {
        this.autoDetecting = false;
        if (this.framePumpTimer) {
            clearTimeout(this.framePumpTimer);
            this.framePumpTimer = null;
        }
        this.mpProcessing = false;
        this.consecutiveMatches = 0;
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
