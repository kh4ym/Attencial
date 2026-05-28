window.attencialParallax = {
    triggers: [],

    initStaggeredParallax: function (containerSelector) {
        var container = document.querySelector(containerSelector);
        if (!container) {
            console.warn('attencialParallax: container not found:', containerSelector);
            return;
        }

        // ── 1. Parallax Background Layers ──────────────────────────────
        var bgLayers = container.querySelectorAll('[data-parallax-bg]');
        bgLayers.forEach(function (layer) {
            var speed = parseFloat(layer.getAttribute('data-speed') || '0.4');
            var st = gsap.fromTo(layer,
                { y: function () { return -window.innerHeight * speed * 0.5; } },
                {
                    y: function () { return window.innerHeight * speed * 0.5; },
                    ease: 'none',
                    scrollTrigger: {
                        trigger: layer.parentElement,
                        start: 'top bottom',
                        end: 'bottom top',
                        scrub: 0.5,
                        invalidateOnRefresh: true
                    }
                }
            );
            this.triggers.push(st.scrollTrigger);
        });

        // ── 2. Sequential Card Grid Reveal ─────────────────────────────
        var cardGrids = container.querySelectorAll('[data-stagger-cards]');
        cardGrids.forEach(function (grid) {
            var cards = grid.querySelectorAll('[data-card]');
            if (cards.length === 0) return;

            var st = gsap.fromTo(cards,
                {
                    opacity: 0,
                    y: 60,
                    rotateX: -8,
                    scale: 0.92
                },
                {
                    opacity: 1,
                    y: 0,
                    rotateX: 0,
                    scale: 1,
                    duration: 0.8,
                    stagger: 0.2,
                    ease: 'power3.out',
                    scrollTrigger: {
                        trigger: grid,
                        start: 'top 85%',
                        end: 'bottom 20%',
                        toggleActions: 'play none none none',
                        once: true
                    }
                }
            );
            this.triggers.push(st.scrollTrigger);
        });

        // ── 3. Vertical Card Stack ─────────────────────────────────────
        var stacks = container.querySelectorAll('[data-card-stack]');
        stacks.forEach(function (stack) {
            var stackCards = stack.querySelectorAll('[data-stack-card]');
            if (stackCards.length === 0) return;

            stackCards.forEach(function (card, i) {
                var offset = (stackCards.length - 1 - i) * 8;
                gsap.set(card, {
                    y: offset,
                    zIndex: i,
                    scale: 1 - (stackCards.length - 1 - i) * 0.02
                });

                var st = gsap.to(card, {
                    y: i * -4,
                    scale: 1,
                    zIndex: stackCards.length + i,
                    duration: 0.6,
                    ease: 'power2.out',
                    scrollTrigger: {
                        trigger: stack,
                        start: 'top 75%',
                        end: 'bottom 30%',
                        scrub: 0.5,
                        toggleActions: 'play none none none'
                    }
                });
                this.triggers.push(st.scrollTrigger);
            });
        });

        // ── 4. Fade-up text elements ───────────────────────────────────
        var fadeUps = container.querySelectorAll('[data-fade-up]');
        fadeUps.forEach(function (el) {
            var delay = parseFloat(el.getAttribute('data-delay') || '0');
            var st = gsap.fromTo(el,
                { opacity: 0, y: 40 },
                {
                    opacity: 1,
                    y: 0,
                    duration: 0.7,
                    delay: delay,
                    ease: 'power2.out',
                    scrollTrigger: {
                        trigger: el,
                        start: 'top 90%',
                        toggleActions: 'play none none none',
                        once: true
                    }
                }
            );
            this.triggers.push(st.scrollTrigger);
        });

        // ── 5. Scale-in reveal ─────────────────────────────────────────
        var scaleIns = container.querySelectorAll('[data-scale-in]');
        scaleIns.forEach(function (el) {
            var st = gsap.fromTo(el,
                { opacity: 0, scale: 0.85 },
                {
                    opacity: 1,
                    scale: 1,
                    duration: 0.7,
                    ease: 'back.out(1.4)',
                    scrollTrigger: {
                        trigger: el,
                        start: 'top 92%',
                        toggleActions: 'play none none none',
                        once: true
                    }
                }
            );
            this.triggers.push(st.scrollTrigger);
        });

        console.log('attencialParallax: initialized on', containerSelector,
            '| backgrounds:', bgLayers.length,
            '| card grids:', cardGrids.length,
            '| card stacks:', stacks.length,
            '| fade-ups:', fadeUps.length,
            '| scale-ins:', scaleIns.length);
    },

    cleanupParallax: function () {
        this.triggers.forEach(function (t) {
            if (t && t.kill) t.kill(false);
        });
        this.triggers = [];
        console.log('attencialParallax: cleaned up');
    },

    initGeometricSpin: function () {
        var spinners = document.querySelectorAll('[data-spin]');
        spinners.forEach(function (el) {
            var direction = parseFloat(el.getAttribute('data-spin-dir') || '1');
            var speed = parseFloat(el.getAttribute('data-spin-speed') || '360');
            var st = gsap.fromTo(el,
                { rotate: 0 },
                {
                    rotate: direction * speed,
                    ease: 'none',
                    scrollTrigger: {
                        trigger: el.closest('section') || el.parentElement,
                        start: 'top bottom',
                        end: 'bottom top',
                        scrub: 0.5
                    }
                }
            );
            this.triggers.push(st.scrollTrigger);
        }.bind(this));

        // Reveal text: clip from top to bottom on scroll
        var reveals = document.querySelectorAll('[data-reveal-top]');
        reveals.forEach(function (el) {
            var st = gsap.fromTo(el,
                { opacity: 0, clipPath: 'inset(0 0 100% 0)', y: -30 },
                {
                    opacity: 1,
                    clipPath: 'inset(0 0 0% 0)',
                    y: 0,
                    ease: 'none',
                    scrollTrigger: {
                        trigger: el,
                        start: 'top 85%',
                        end: 'top 30%',
                        scrub: 0.6
                    }
                }
            );
            this.triggers.push(st.scrollTrigger);
        }.bind(this));

        console.log('attencialParallax: geometric spin on', spinners.length, '| reveals on', reveals.length);

        // Refresh after a frame to ensure DOM is painted (important for Blazor WASM)
        requestAnimationFrame(function () {
            ScrollTrigger.refresh();
        });
    }
};
