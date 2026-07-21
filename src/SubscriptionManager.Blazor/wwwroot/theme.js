window.subscriptionManagerTheme = {
    get: function (cookieName) {
        const storedTheme = window.localStorage.getItem(cookieName);

        if (storedTheme) {
            return storedTheme;
        }

        const encodedName = `${encodeURIComponent(cookieName)}=`;
        const cookie = document.cookie
            .split(";")
            .map(value => value.trim())
            .find(value => value.startsWith(encodedName));

        return cookie
            ? decodeURIComponent(cookie.substring(encodedName.length))
            : null;
    },

    set: function (theme, cookieName) {
        const maxAge = 60 * 60 * 24 * 365;
        const secure = window.location.protocol === "https:" ? "; Secure" : "";

        window.localStorage.setItem(cookieName, theme);

        document.cookie =
            `${encodeURIComponent(cookieName)}=${encodeURIComponent(theme)}` +
            `; Path=/; Max-Age=${maxAge}; SameSite=Lax${secure}`;
    }
};
