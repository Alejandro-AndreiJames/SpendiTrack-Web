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

// Home calculators: budget estimator + regular keypad
(function () {
    const panel = document.getElementById("homeCalculatorPanel");
    if (!panel) return;

    const titleEl = document.getElementById("calculatorPanelTitle");
    const modeButtons = panel.querySelectorAll(".calculator-mode-switch__btn[data-calc-mode]");
    const budgetPane = panel.querySelector('[data-calc-pane="budget"]');
    const regularPane = panel.querySelector('[data-calc-pane="regular"]');
    const form = document.getElementById("budget-calculator-form");
    const resultPanel = document.getElementById("budget-calculator-results");

    function setMode(mode) {
        const next = mode === "regular" ? "regular" : "budget";
        panel.dataset.calcMode = next;

        modeButtons.forEach(function (btn) {
            const active = btn.getAttribute("data-calc-mode") === next;
            btn.classList.toggle("is-active", active);
            btn.setAttribute("aria-pressed", active ? "true" : "false");
        });

        [budgetPane, regularPane].forEach(function (pane) {
            if (!pane) return;
            const active = pane.getAttribute("data-calc-pane") === next;
            pane.classList.toggle("is-active", active);
            pane.setAttribute("aria-hidden", active ? "false" : "true");
            pane.querySelectorAll("button, input").forEach(function (el) {
                el.tabIndex = active ? 0 : -1;
                if (!active && el.tagName === "INPUT") el.blur();
            });
        });

        if (titleEl) {
            titleEl.textContent = next === "regular" ? "Calculator" : "Budget calculator";
        }
    }

    modeButtons.forEach(function (btn) {
        btn.addEventListener("click", function () {
            setMode(btn.getAttribute("data-calc-mode"));
        });
    });

    if (form && resultPanel) {
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
    }

    const calcRoot = document.getElementById("regularCalculator");
    const displayEl = document.getElementById("regularCalcDisplay");
    const expressionEl = document.getElementById("regularCalcExpression");
    if (!calcRoot || !displayEl) return;

    let current = "0";
    let previous = null;
    let operator = null;
    let resetNext = false;

    function formatDisplay(value) {
        if (!isFinite(value)) return "Error";
        const text = String(value);
        if (text.length > 14) {
            return Number(value).toPrecision(10).replace(/\.?0+$/, "");
        }
        return text;
    }

    function updateDisplay() {
        displayEl.textContent = formatDisplay(current);
        if (expressionEl) {
            expressionEl.textContent =
                previous !== null && operator
                    ? `${formatDisplay(previous)} ${operator}`
                    : "";
        }
    }

    function inputDigit(digit) {
        if (resetNext) {
            current = digit;
            resetNext = false;
            updateDisplay();
            return;
        }

        if (digit === "0" && current === "0") return;
        current = current === "0" ? digit : current + digit;
        updateDisplay();
    }

    function inputDot() {
        if (resetNext) {
            current = "0.";
            resetNext = false;
            updateDisplay();
            return;
        }
        if (!current.includes(".")) {
            current += ".";
            updateDisplay();
        }
    }

    function clearAll() {
        current = "0";
        previous = null;
        operator = null;
        resetNext = false;
        updateDisplay();
    }

    function toggleSign() {
        if (current === "0" || current === "Error") return;
        current = current.startsWith("-") ? current.slice(1) : "-" + current;
        updateDisplay();
    }

    function percent() {
        const value = parseFloat(current);
        if (!isFinite(value)) return;
        current = String(value / 100);
        resetNext = true;
        updateDisplay();
    }

    function compute(a, b, op) {
        switch (op) {
            case "+": return a + b;
            case "-": return a - b;
            case "*": return a * b;
            case "/": return b === 0 ? NaN : a / b;
            default: return b;
        }
    }

    function setOperator(nextOp) {
        const value = parseFloat(current);
        if (!isFinite(value)) {
            clearAll();
            return;
        }

        if (previous !== null && operator && !resetNext) {
            const result = compute(previous, value, operator);
            if (!isFinite(result)) {
                current = "Error";
                previous = null;
                operator = null;
                resetNext = true;
                updateDisplay();
                return;
            }
            previous = result;
            current = String(result);
        } else {
            previous = value;
        }

        operator = nextOp;
        resetNext = true;
        updateDisplay();
    }

    function equals() {
        if (previous === null || !operator) return;
        const value = parseFloat(current);
        const result = compute(previous, value, operator);
        current = isFinite(result) ? String(result) : "Error";
        previous = null;
        operator = null;
        resetNext = true;
        updateDisplay();
    }

    calcRoot.addEventListener("click", function (e) {
        const btn = e.target.closest("button");
        if (!btn || !calcRoot.contains(btn)) return;

        if (btn.dataset.calcDigit != null) {
            inputDigit(btn.dataset.calcDigit);
            return;
        }
        if (btn.dataset.calcOp != null) {
            setOperator(btn.dataset.calcOp);
            return;
        }

        switch (btn.dataset.calcAction) {
            case "clear": clearAll(); break;
            case "sign": toggleSign(); break;
            case "percent": percent(); break;
            case "dot": inputDot(); break;
            case "equals": equals(); break;
        }
    });

    function backspace() {
        if (resetNext || current === "Error") {
            current = "0";
            resetNext = false;
            updateDisplay();
            return;
        }
        if (current.length <= 1 || (current.length === 2 && current.startsWith("-"))) {
            current = "0";
        } else {
            current = current.slice(0, -1);
        }
        updateDisplay();
    }

    function supportsDesktopKeyboard() {
        return window.matchMedia("(pointer: fine) and (min-width: 768px)").matches;
    }

    document.addEventListener("keydown", function (e) {
        if (panel.dataset.calcMode !== "regular") return;
        if (!supportsDesktopKeyboard()) return;
        if (e.target.closest("input, textarea, select, [contenteditable='true']")) return;

        const key = e.key;
        let handled = true;

        if (key >= "0" && key <= "9") {
            inputDigit(key);
        } else if (key === "." || key === ",") {
            inputDot();
        } else if (key === "+" || key === "-" || key === "*" || key === "/") {
            setOperator(key);
        } else if (key === "Enter" || key === "=") {
            equals();
        } else if (key === "Escape" || key === "Delete") {
            clearAll();
        } else if (key === "Backspace") {
            backspace();
        } else if (key === "%") {
            percent();
        } else if (key === "F9") {
            toggleSign();
        } else {
            handled = false;
        }

        if (handled) e.preventDefault();
    });

    updateDisplay();
    setMode(panel.dataset.calcMode || "regular");
})();
