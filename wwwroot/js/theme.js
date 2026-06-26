(function () {
    const THEMES = [
        { id: 'classic', label: 'Classic', swatch: '#2563eb' },
        { id: 'light', label: 'Light', swatch: '#c89b3c' },
        { id: 'midnight', label: 'Midnight', swatch: '#60a5fa' },
        { id: 'luxury', label: 'Luxury Gold', swatch: '#d4af37' },
        { id: 'sky', label: 'Sky', swatch: '#38bdf8' },
        { id: 'pastel', label: 'Pastel', swatch: '#c08457' },
        { id: 'ocean', label: 'Ocean', swatch: '#38bdf8' },
        { id: 'lavender', label: 'Lavender', swatch: '#a855f7' },
        { id: 'rose', label: 'Rose', swatch: '#f472b6' },
        { id: 'growth', label: 'Growth', swatch: '#34d399' }
    ];

    const DEFAULT_THEME = 'classic';
    const VALID_THEME_IDS = THEMES.map(function (t) { return t.id; });

    function storageKey() {
        const userKey = document.documentElement.dataset.userThemeKey
            || document.body?.dataset.userThemeKey
            || 'guest';
        return 'spenditrack.theme.' + userKey;
    }

    function getSavedTheme() {
        try {
            const saved = localStorage.getItem(storageKey());
            return VALID_THEME_IDS.indexOf(saved) !== -1 ? saved : DEFAULT_THEME;
        } catch {
            return DEFAULT_THEME;
        }
    }

    function applyTheme(themeId) {
        const theme = VALID_THEME_IDS.indexOf(themeId) !== -1 ? themeId : DEFAULT_THEME;
        document.documentElement.setAttribute('data-theme', theme);

        try {
            localStorage.setItem(storageKey(), theme);
        } catch {
            /* ignore */
        }

        document.querySelectorAll('[data-theme-option]').forEach(function (btn) {
            const isActive = btn.dataset.themeOption === theme;
            btn.classList.toggle('active', isActive);
            btn.setAttribute('aria-checked', isActive ? 'true' : 'false');
        });

        document.querySelectorAll('[data-theme-label]').forEach(function (el) {
            const match = THEMES.find(function (t) { return t.id === theme; });
            if (match) el.textContent = match.label;
        });
    }

    function buildThemeMenu(menu) {
        if (!menu || menu.dataset.themeMenuBuilt === 'true') return;
        menu.dataset.themeMenuBuilt = 'true';
        menu.innerHTML = '';

        THEMES.forEach(function (theme) {
            const li = document.createElement('li');
            li.setAttribute('role', 'none');

            const btn = document.createElement('button');
            btn.type = 'button';
            btn.className = 'dropdown-item theme-picker__option';
            btn.setAttribute('role', 'menuitemradio');
            btn.dataset.themeOption = theme.id;
            btn.innerHTML =
                '<span class="theme-picker__swatch" style="background:' + theme.swatch + '"></span>' +
                '<span class="theme-picker__name">' + theme.label + '</span>';

            btn.addEventListener('click', function () {
                applyTheme(theme.id);
            });

            li.appendChild(btn);
            menu.appendChild(li);
        });
    }

    function initThemePicker() {
        const menu = document.getElementById('themePickerMenu');
        buildThemeMenu(menu);
        applyTheme(getSavedTheme());
    }

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', initThemePicker);
    } else {
        initThemePicker();
    }

    window.SpendiTrackTheme = {
        apply: applyTheme,
        get: getSavedTheme,
        list: THEMES,
        storageKey: storageKey
    };
})();
