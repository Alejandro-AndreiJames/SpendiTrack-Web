// Logout confirm → progress animation → submit
(function () {
    const form = document.getElementById("logoutConfirmForm");
    const modal = document.getElementById("logoutConfirmModal");
    if (!form || !modal) return;

    form.addEventListener("submit", function (e) {
        if (form.dataset.logoutReady === "1") return;

        e.preventDefault();

        const confirmPane = modal.querySelector(".logout-modal__confirm");
        const progressPane = modal.querySelector(".logout-modal__progress");
        const closeBtn = modal.querySelector(".btn-close");
        const stayBtn = modal.querySelector('[data-bs-dismiss="modal"]');

        if (confirmPane) confirmPane.classList.add("d-none");
        if (progressPane) progressPane.classList.remove("d-none");
        if (closeBtn) closeBtn.disabled = true;
        if (stayBtn) stayBtn.disabled = true;

        modal.classList.add("logout-modal--busy");
        form.dataset.logoutReady = "1";

        window.setTimeout(function () {
            form.submit();
        }, 1100);
    });
})();

// Budget calculator on the home page
(function () {
    const form = document.getElementById("budget-calculator-form");
    if (!form) return;

    const resultPanel = document.getElementById("budget-calculator-results");

    form.addEventListener("submit", function (e) {
        e.preventDefault();

        const income = parseFloat(document.getElementById("monthly-income").value) || 0;
        const savingsPercent = parseFloat(document.getElementById("savings-percent").value) || 0;
        const fixedCosts = parseFloat(document.getElementById("fixed-costs").value) || 0;

        const savingsAmount = income * (savingsPercent / 100);
        const spendable = Math.max(0, income - savingsAmount - fixedCosts);
        const daily = spendable / 30;
        const weekly = spendable / 4.345;

        const fmt = (n) =>
            n.toLocaleString(undefined, { style: "currency", currency: "USD" });

        resultPanel.innerHTML = `
            <div class="result-row"><span>Savings (${savingsPercent}%)</span><span>${fmt(savingsAmount)}</span></div>
            <div class="result-row"><span>After fixed costs</span><span>${fmt(spendable)}</span></div>
            <div class="result-row"><span>Weekly budget</span><span>${fmt(weekly)}</span></div>
            <div class="result-row"><span>Daily budget</span><span>${fmt(daily)}</span></div>
        `;
        resultPanel.style.display = "block";
    });
})();
