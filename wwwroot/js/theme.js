function setTheme(theme) {
  document.documentElement.setAttribute("data-theme", theme);
  localStorage.setItem("theme", theme);
  updateToggleUI(theme);
}

function updateToggleUI(theme) {
  const btn = document.getElementById("themeToggleBtn");
  if (btn) {
    btn.textContent = theme === "dark" ? "☀️ Light mode" : "🌙 Dark mode";
  }
}

document.addEventListener("DOMContentLoaded", function () {
  const current = localStorage.getItem("theme") || "light";
  updateToggleUI(current);

  const btn = document.getElementById("themeToggleBtn");
  if (btn) {
    btn.addEventListener("click", function () {
      const newTheme =
        document.documentElement.getAttribute("data-theme") === "dark"
          ? "light"
          : "dark";
      setTheme(newTheme);
    });
  }
});
