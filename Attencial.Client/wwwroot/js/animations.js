// ============================================================================
// Attencial Animation Engine
// ============================================================================

window.attencialAnimations = {

    // =========================================================================
    // PARTICLE SYSTEM
    // =========================================================================
    particles: [],
    particleCanvas: null,
    particleCtx: null,
    particleAnimationId: null,

    startParticles: function (canvasId) {
        const canvas = document.getElementById(canvasId);
        if (!canvas) return;
        this.particleCanvas = canvas;
        this.particleCtx = canvas.getContext('2d');

        const resize = () => {
            canvas.width = canvas.offsetWidth * window.devicePixelRatio;
            canvas.height = canvas.offsetHeight * window.devicePixelRatio;
            this.particleCtx.scale(window.devicePixelRatio, window.devicePixelRatio);
        };
        resize();
        window.addEventListener('resize', resize);

        const colors = ['#e60023', '#ff4d6a', '#ffb3c1', '#ffffff', '#ffd1d9'];
        const particleCount = 45;

        for (let i = 0; i < particleCount; i++) {
            this.particles.push({
                x: Math.random() * canvas.offsetWidth,
                y: Math.random() * canvas.offsetHeight,
                radius: Math.random() * 3 + 1,
                color: colors[Math.floor(Math.random() * colors.length)],
                vx: (Math.random() - 0.5) * 0.6,
                vy: (Math.random() - 0.5) * 0.6,
                opacity: Math.random() * 0.5 + 0.1,
                pulseSpeed: Math.random() * 0.02 + 0.005,
                pulseOffset: Math.random() * Math.PI * 2
            });
        }

        const animate = () => {
            const w = canvas.offsetWidth;
            const h = canvas.offsetHeight;
            this.particleCtx.clearRect(0, 0, w, h);

            for (const p of this.particles) {
                p.x += p.vx;
                p.y += p.vy;

                if (p.x < -50) p.x = w + 50;
                if (p.x > w + 50) p.x = -50;
                if (p.y < -50) p.y = h + 50;
                if (p.y > h + 50) p.y = -50;

                const alpha = p.opacity + Math.sin(Date.now() * p.pulseSpeed + p.pulseOffset) * 0.15;

                this.particleCtx.beginPath();
                this.particleCtx.arc(p.x, p.y, p.radius, 0, Math.PI * 2);
                this.particleCtx.fillStyle = p.color;
                this.particleCtx.globalAlpha = Math.max(0.05, alpha);
                this.particleCtx.fill();

                // Glow
                this.particleCtx.beginPath();
                this.particleCtx.arc(p.x, p.y, p.radius * 2.5, 0, Math.PI * 2);
                this.particleCtx.fillStyle = p.color;
                this.particleCtx.globalAlpha = Math.max(0.01, alpha * 0.25);
                this.particleCtx.fill();
            }

            // Draw connections between nearby particles
            for (let i = 0; i < this.particles.length; i++) {
                for (let j = i + 1; j < this.particles.length; j++) {
                    const dx = this.particles[i].x - this.particles[j].x;
                    const dy = this.particles[i].y - this.particles[j].y;
                    const dist = Math.sqrt(dx * dx + dy * dy);
                    if (dist < 120) {
                        this.particleCtx.beginPath();
                        this.particleCtx.moveTo(this.particles[i].x, this.particles[i].y);
                        this.particleCtx.lineTo(this.particles[j].x, this.particles[j].y);
                        this.particleCtx.strokeStyle = '#e60023';
                        this.particleCtx.globalAlpha = 0.06 * (1 - dist / 120);
                        this.particleCtx.lineWidth = 0.5;
                        this.particleCtx.stroke();
                    }
                }
            }

            this.particleCtx.globalAlpha = 1;
            this.particleAnimationId = requestAnimationFrame(animate);
        };

        animate();
    },

    stopParticles: function () {
        if (this.particleAnimationId) {
            cancelAnimationFrame(this.particleAnimationId);
            this.particleAnimationId = null;
        }
        this.particles = [];
    },

    // =========================================================================
    // SCROLL REVEAL (Intersection Observer)
    // =========================================================================
    scrollObserver: null,

    initScrollReveal: function () {
        if (this.scrollObserver) return;

        this.scrollObserver = new IntersectionObserver((entries) => {
            entries.forEach(entry => {
                if (entry.isIntersecting) {
                    entry.target.classList.add('revealed');
                    // Stagger children
                    const children = entry.target.querySelectorAll('.stagger-child');
                    children.forEach((child, i) => {
                        child.style.transitionDelay = `${i * 0.1}s`;
                        child.classList.add('revealed');
                    });
                    this.scrollObserver.unobserve(entry.target);
                }
            });
        }, { threshold: 0.15, rootMargin: '0px 0px -40px 0px' });

        document.querySelectorAll('.reveal-on-scroll').forEach(el => {
            this.scrollObserver.observe(el);
        });
    },

    // =========================================================================
    // CONFETTI BURST
    // =========================================================================
    confettiBurst: function () {
        const colors = ['#e60023', '#ff4d6a', '#ffb3c1', '#ffffff', '#ffd1d9', '#ff6b81', '#ffd93d', '#6bcb77'];
        const confettiCount = 120;
        const container = document.body;

        for (let i = 0; i < confettiCount; i++) {
            const confetti = document.createElement('div');
            confetti.className = 'confetti-piece';
            confetti.style.cssText = `
                position: fixed;
                left: ${Math.random() * 100}vw;
                top: -10px;
                width: ${Math.random() * 10 + 6}px;
                height: ${Math.random() * 14 + 8}px;
                background: ${colors[Math.floor(Math.random() * colors.length)]};
                z-index: 99999;
                pointer-events: none;
                border-radius: ${Math.random() > 0.5 ? '50%' : '2px'};
                animation: confettiFall ${Math.random() * 2 + 2.5}s cubic-bezier(0.22, 0.61, 0.36, 1) forwards;
                animation-delay: ${Math.random() * 0.5}s;
                opacity: 0;
            `;
            container.appendChild(confetti);

            // Add slight horizontal drift via custom property
            const drift = (Math.random() - 0.5) * 200;
            confetti.style.setProperty('--drift', `${drift}px`);

            // Cleanup after animation
            setTimeout(() => confetti.remove(), 3500);
        }
    },

    // =========================================================================
    // CURSOR GLOW TRAIL
    // =========================================================================
    cursorGlowEl: null,
    cursorTrail: [],
    cursorTrailMax: 12,
    cursorActive: false,

    initCursorGlow: function () {
        if (this.cursorGlowEl) return;

        // Main glow
        this.cursorGlowEl = document.createElement('div');
        this.cursorGlowEl.className = 'cursor-glow';
        document.body.appendChild(this.cursorGlowEl);

        // Trail dots
        for (let i = 0; i < this.cursorTrailMax; i++) {
            const dot = document.createElement('div');
            dot.className = 'cursor-trail-dot';
            dot.style.opacity = (1 - i / this.cursorTrailMax) * 0.5;
            dot.style.transform = `scale(${1 - i / this.cursorTrailMax})`;
            document.body.appendChild(dot);
            this.cursorTrail.push({ el: dot, x: 0, y: 0 });
        }

        let mouseX = -100, mouseY = -100;
        let rafId = null;

        document.addEventListener('mousemove', (e) => {
            mouseX = e.clientX;
            mouseY = e.clientY;
            if (!this.cursorActive) {
                this.cursorActive = true;
                this.cursorGlowEl.style.opacity = '1';
                this.cursorTrail.forEach(t => t.el.style.opacity = '0.5');
            }

            if (!rafId) {
                rafId = requestAnimationFrame(() => {
                    // Main glow follows exactly
                    this.cursorGlowEl.style.transform = `translate(${mouseX}px, ${mouseY}px) translate(-50%, -50%)`;

                    // Trail follows with delay
                    let tx = mouseX, ty = mouseY;
                    for (const dot of this.cursorTrail) {
                        dot.x += (tx - dot.x) * 0.35;
                        dot.y += (ty - dot.y) * 0.35;
                        dot.el.style.transform = `translate(${dot.x}px, ${dot.y}px) scale(${1 - this.cursorTrail.indexOf(dot) / this.cursorTrailMax})`;
                        tx = dot.x;
                        ty = dot.y;
                    }
                    rafId = null;
                });
            }
        });

        document.addEventListener('mouseleave', () => {
            this.cursorGlowEl.style.opacity = '0';
            this.cursorTrail.forEach(t => t.el.style.opacity = '0');
            this.cursorActive = false;
        });
    },

    // =========================================================================
    // 3D TILT CARD
    // =========================================================================
    initTiltCards: function () {
        document.querySelectorAll('.tilt-card').forEach(card => {
            card.addEventListener('mousemove', (e) => {
                const rect = card.getBoundingClientRect();
                const x = e.clientX - rect.left;
                const y = e.clientY - rect.top;
                const centerX = rect.width / 2;
                const centerY = rect.height / 2;
                const rotateX = (y - centerY) / centerY * -8;
                const rotateY = (x - centerX) / centerX * 8;

                card.style.transform = `perspective(1000px) rotateX(${rotateX}deg) rotateY(${rotateY}deg) scale3d(1.02, 1.02, 1.02)`;

                // Move the glare
                const glare = card.querySelector('.card-glare');
                if (glare) {
                    glare.style.background = `radial-gradient(circle at ${x}px ${y}px, rgba(255,255,255,0.15) 0%, transparent 60%)`;
                }
            });

            card.addEventListener('mouseleave', () => {
                card.style.transform = 'perspective(1000px) rotateX(0) rotateY(0) scale3d(1, 1, 1)';
                const glare = card.querySelector('.card-glare');
                if (glare) {
                    glare.style.background = 'transparent';
                }
            });
        });
    },

    // =========================================================================
    // ANIMATED COUNTER
    // =========================================================================
    animateCounter: function (elementId, target, duration) {
        const el = document.getElementById(elementId);
        if (!el) return;

        const start = 0;
        const startTime = performance.now();

        const easeOutExpo = (t) => t === 1 ? 1 : 1 - Math.pow(2, -10 * t);

        const update = (currentTime) => {
            const elapsed = currentTime - startTime;
            const progress = Math.min(elapsed / duration, 1);
            const easedProgress = easeOutExpo(progress);
            const current = Math.floor(easedProgress * target);

            el.textContent = current.toLocaleString();

            if (progress < 1) {
                requestAnimationFrame(update);
            } else {
                el.textContent = target.toLocaleString();
            }
        };

        requestAnimationFrame(update);
    },

    // =========================================================================
    // TEXT SCRAMBLE EFFECT
    // =========================================================================
    scrambleText: function (elementId, finalText, duration) {
        const el = document.getElementById(elementId);
        if (!el) return;

        const chars = '!@#$%^&*()_+-=[]{}|;:,.<>?/~`ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789';
        const startTime = performance.now();
        let interval;

        const scramble = () => {
            const elapsed = performance.now() - startTime;
            const progress = Math.min(elapsed / duration, 1);

            let result = '';
            for (let i = 0; i < finalText.length; i++) {
                if (i < Math.floor(progress * finalText.length)) {
                    result += finalText[i];
                } else if (finalText[i] === ' ') {
                    result += ' ';
                } else {
                    result += chars[Math.floor(Math.random() * chars.length)];
                }
            }

            el.textContent = result;

            if (progress >= 1) {
                el.textContent = finalText;
                clearInterval(interval);
            }
        };

        interval = setInterval(scramble, 40);
    },

    // =========================================================================
    // MORPHING BLOB BACKGROUND
    // =========================================================================
    initMorphingBlobs: function () {
        if (!document.querySelector('.morphing-blob')) return;

        const blobs = document.querySelectorAll('.morphing-blob');
        blobs.forEach((blob, index) => {
            const speed = 8 + index * 3;
            const amplitude = 15 + index * 10;
            let time = index * 2;

            const animate = () => {
                time += 0.016;
                const x = Math.sin(time / speed) * amplitude;
                const y = Math.cos(time / (speed * 1.3)) * amplitude;
                const scale = 1 + Math.sin(time / (speed * 2)) * 0.15;
                blob.style.transform = `translate(${x}px, ${y}px) scale(${scale})`;
                requestAnimationFrame(animate);
            };

            animate();
        });
    },

    // =========================================================================
    // INIT ALL (call from Blazor)
    // =========================================================================
    initAll: function () {
        this.initScrollReveal();
        this.initTiltCards();
        this.initMorphingBlobs();
    }
};

// ============================================================================
// QR Code Helper  (uses qrcodejs library loaded in index.html)
// ============================================================================
window.attencialQr = {
    _instance: null,

    generate: function (containerId, text) {
        const container = document.getElementById(containerId);
        if (!container) return;

        // Clear previous QR
        container.innerHTML = '';
        this._instance = null;

        if (typeof QRCode === 'undefined') return;

        this._instance = new QRCode(container, {
            text:          text,
            width:         200,
            height:        200,
            colorDark:     '#000000',
            colorLight:    '#ffffff',
            correctLevel:  QRCode.CorrectLevel.M
        });
    }
};

// CSS injection for dynamic elements
(function () {
    const style = document.createElement('style');
    style.textContent = `
        @keyframes confettiFall {
            0% { opacity: 1; transform: translate(0, 0) rotate(0deg) scale(1); }
            100% { opacity: 0; transform: translate(var(--drift, 50px), 105vh) rotate(${Math.random() * 720 + 360}deg) scale(0.3); }
        }

        .cursor-glow {
            position: fixed;
            pointer-events: none;
            z-index: 99998;
            width: 300px;
            height: 300px;
            border-radius: 50%;
            background: radial-gradient(circle, rgba(230,0,35,0.08) 0%, rgba(230,0,35,0.03) 35%, transparent 65%);
            transition: opacity 0.3s;
            opacity: 0;
            will-change: transform;
        }

        .cursor-trail-dot {
            position: fixed;
            pointer-events: none;
            z-index: 99997;
            width: 8px;
            height: 8px;
            border-radius: 50%;
            background: rgba(230,0,35,0.35);
            box-shadow: 0 0 12px rgba(230,0,35,0.3), 0 0 4px rgba(230,0,35,0.5);
            transition: opacity 0.5s;
            opacity: 0;
            will-change: transform;
        }

        .reveal-on-scroll {
            opacity: 0;
            transform: translateY(30px);
            transition: opacity 0.7s cubic-bezier(0.16, 1, 0.3, 1),
                        transform 0.7s cubic-bezier(0.16, 1, 0.3, 1);
        }

        .reveal-on-scroll.revealed {
            opacity: 1;
            transform: translateY(0);
        }

        .stagger-child {
            opacity: 0;
            transform: translateY(20px);
            transition: opacity 0.5s cubic-bezier(0.16, 1, 0.3, 1),
                        transform 0.5s cubic-bezier(0.16, 1, 0.3, 1);
        }

        .stagger-child.revealed {
            opacity: 1;
            transform: translateY(0);
        }

        .tilt-card {
            transition: transform 0.1s ease-out;
            transform-style: preserve-3d;
            position: relative;
            overflow: hidden;
        }

        .card-glare {
            position: absolute;
            inset: 0;
            pointer-events: none;
            z-index: 10;
            border-radius: inherit;
            transition: background 0.2s;
        }
    `;
    document.head.appendChild(style);
})();
