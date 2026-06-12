/* Hospital Management System — Premium UI Interactions
   Scroll animations, counters, ripple effects & transitions */

(function () {
    'use strict';

    // ─── Intersection Observer: Scroll Reveal Animations ───
    function initScrollAnimations() {
        var observerOptions = {
            root: null,
            rootMargin: '0px 0px -60px 0px',
            threshold: 0.1
        };

        var observer = new IntersectionObserver(function (entries) {
            entries.forEach(function (entry) {
                if (entry.isIntersecting) {
                    entry.target.classList.add('is-visible');
                    observer.unobserve(entry.target);
                }
            });
        }, observerOptions);

        // Observe elements with scroll-reveal class
        document.querySelectorAll('.scroll-reveal').forEach(function (el) {
            observer.observe(el);
        });

        // Auto-apply to common elements
        document.querySelectorAll('.card, .list-group-item, .table tbody tr').forEach(function (el) {
            if (!el.closest('.modal') && !el.classList.contains('no-animate')) {
                el.style.opacity = '1';
            }
        });
    }

    // ─── Animated Counter (for dashboard stats) ───
    function animateCounters() {
        var counters = document.querySelectorAll('[data-count-to]');
        if (!counters.length) return;

        var counterObserver = new IntersectionObserver(function (entries) {
            entries.forEach(function (entry) {
                if (entry.isIntersecting) {
                    var el = entry.target;
                    var target = parseInt(el.getAttribute('data-count-to'), 10);
                    var duration = parseInt(el.getAttribute('data-count-duration') || '1000', 10);
                    var start = 0;
                    var startTime = null;

                    function easeOutQuart(t) {
                        return 1 - Math.pow(1 - t, 4);
                    }

                    function updateCount(timestamp) {
                        if (!startTime) startTime = timestamp;
                        var progress = Math.min((timestamp - startTime) / duration, 1);
                        var easedProgress = easeOutQuart(progress);
                        var current = Math.floor(easedProgress * target);
                        el.textContent = current.toLocaleString();
                        if (progress < 1) {
                            requestAnimationFrame(updateCount);
                        } else {
                            el.textContent = target.toLocaleString();
                        }
                    }

                    requestAnimationFrame(updateCount);
                    counterObserver.unobserve(el);
                }
            });
        }, { threshold: 0.3 });

        counters.forEach(function (counter) {
            counterObserver.observe(counter);
        });
    }

    // ─── Navbar scroll effect ───
    function initNavbarScroll() {
        var navbar = document.querySelector('.navbar');
        if (!navbar) return;

        var lastScroll = 0;
        window.addEventListener('scroll', function () {
            var currentScroll = window.pageYOffset;
            if (currentScroll > 20) {
                navbar.classList.add('scrolled');
            } else {
                navbar.classList.remove('scrolled');
            }
            lastScroll = currentScroll;
        }, { passive: true });
    }

    // ─── Button ripple effect ───
    function initRippleEffect() {
        document.addEventListener('click', function (e) {
            var btn = e.target.closest('.btn');
            if (!btn) return;

            var ripple = document.createElement('span');
            var rect = btn.getBoundingClientRect();
            var size = Math.max(rect.width, rect.height);
            var x = e.clientX - rect.left - size / 2;
            var y = e.clientY - rect.top - size / 2;

            ripple.style.cssText =
                'position:absolute;border-radius:50%;background:rgba(255,255,255,0.2);' +
                'width:' + size + 'px;height:' + size + 'px;left:' + x + 'px;top:' + y + 'px;' +
                'transform:scale(0);animation:ripple-effect 0.6s ease-out;pointer-events:none;';

            btn.style.position = 'relative';
            btn.style.overflow = 'hidden';
            btn.appendChild(ripple);

            setTimeout(function () {
                ripple.remove();
            }, 600);
        });

        // Inject ripple keyframes
        if (!document.getElementById('ripple-style')) {
            var style = document.createElement('style');
            style.id = 'ripple-style';
            style.textContent = '@keyframes ripple-effect{to{transform:scale(4);opacity:0;}}';
            document.head.appendChild(style);
        }
    }

    // ─── Smooth card entrance ───
    function initCardEntrance() {
        var cards = document.querySelectorAll('.dashboard-card, .feature-card, .hover-lift');
        cards.forEach(function (card, index) {
            card.style.animationDelay = (index * 0.08) + 's';
        });
    }

    // ─── Initialize everything on DOM ready ───

    function initDarkModeToggle() {
        const toggle = document.getElementById('dark-mode-toggle');
        if (!toggle) return;
        // Apply saved theme on load
        if (localStorage.getItem('theme') === 'dark') {
            document.body.classList.add('dark-mode');
        }
        toggle.addEventListener('click', function (e) {
            e.preventDefault();
            document.body.classList.toggle('dark-mode');
            const isDark = document.body.classList.contains('dark-mode');
            localStorage.setItem('theme', isDark ? 'dark' : 'light');
        });
    }
    document.addEventListener('DOMContentLoaded', function () {
        initScrollAnimations();
        animateCounters();
        initNavbarScroll();
        initRippleEffect();
        initCardEntrance();
        initDarkModeToggle();

        // Add scroll-reveal visible class style
        if (!document.getElementById('scroll-reveal-style')) {
            var style = document.createElement('style');
            style.id = 'scroll-reveal-style';
            style.textContent =
                '.scroll-reveal{opacity:0;transform:translateY(20px);transition:opacity 0.6s cubic-bezier(0.16,1,0.3,1),transform 0.6s cubic-bezier(0.16,1,0.3,1);}' + '.scroll-reveal.is-visible{opacity:1;transform:translateY(0);}';
            document.head.appendChild(style);
        }
    });
})();