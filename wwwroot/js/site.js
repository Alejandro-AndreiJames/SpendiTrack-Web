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
