// site.js — Dopamine Detox Ledger

// Scroll-based header shadow
window.addEventListener('scroll', () => {
    const header = document.getElementById('app-header');
    if (!header) return;
    if (window.scrollY > 10) {
        header.style.boxShadow = '0 4px 30px rgba(0,0,0,0.5)';
    } else {
        header.style.boxShadow = 'none';
    }
}, { passive: true });
