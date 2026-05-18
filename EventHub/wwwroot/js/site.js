// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
function toggleDarkMode() {
    document.body.classList.toggle("dark");
    localStorage.setItem(
        "theme",
        document.body.classList.contains("dark") ? "dark" : "light"
    );
}

window.onload = () => {
    const theme = localStorage.getItem("theme");
    if (theme === "dark") {
        document.body.classList.add("dark");
    }
};

(function () {
    const storageKey = "eventhub.currency";
    const rateCache = {};

    function getSelectedCurrency() {
        const selector = document.querySelector("[data-currency-selector]");
        return (localStorage.getItem(storageKey) || selector?.value || "EUR").toUpperCase();
    }

    function setCheckoutCurrency(currency) {
        document.querySelectorAll('input[name="checkoutCurrency"]').forEach(input => {
            input.value = currency;
        });
    }

    function formatAmount(amount, currency) {
        return `${Number(amount).toFixed(2)} ${currency}`;
    }

    async function getRate(currency) {
        currency = (currency || "EUR").toUpperCase();
        if (rateCache[currency]) return rateCache[currency];

        const response = await fetch(`/api/currency/rate?from=EUR&to=${encodeURIComponent(currency)}`);
        if (!response.ok) throw new Error("Unable to fetch currency rate.");

        const data = await response.json();
        rateCache[currency] = {
            currency: (data.currency || "EUR").toUpperCase(),
            rate: Number(data.rate || 1)
        };
        return rateCache[currency];
    }

    async function applyCurrency(currency) {
        const requested = (currency || getSelectedCurrency()).toUpperCase();
        let rateInfo;

        try {
            rateInfo = await getRate(requested);
        } catch {
            rateInfo = { currency: "EUR", rate: 1 };
        }

        localStorage.setItem(storageKey, rateInfo.currency);
        document.querySelectorAll("[data-currency-selector]").forEach(selector => {
            selector.value = rateInfo.currency;
        });
        setCheckoutCurrency(rateInfo.currency);

        document.querySelectorAll("[data-currency-amount]").forEach(element => {
            const sourceCurrency = (element.dataset.currencySource || "EUR").toUpperCase();
            const amount = Number(element.dataset.currencyAmount || 0);
            const converted = sourceCurrency === rateInfo.currency
                ? amount
                : amount * rateInfo.rate;
            element.textContent = formatAmount(converted, rateInfo.currency);
        });

        document.dispatchEvent(new CustomEvent("eventhub:currency-changed", {
            detail: rateInfo
        }));
    }

    window.EventHubCurrency = {
        apply: applyCurrency,
        refresh: () => applyCurrency(getSelectedCurrency()),
        convert: amount => {
            const selected = getSelectedCurrency();
            const cached = rateCache[selected] || { currency: selected, rate: selected === "EUR" ? 1 : 1 };
            return {
                amount: Number(amount || 0) * cached.rate,
                currency: cached.currency
            };
        },
        selected: getSelectedCurrency
    };

    document.addEventListener("DOMContentLoaded", () => {
        document.querySelectorAll("[data-currency-selector]").forEach(selector => {
            selector.addEventListener("change", () => applyCurrency(selector.value));
        });

        applyCurrency(getSelectedCurrency());
    });
})();
