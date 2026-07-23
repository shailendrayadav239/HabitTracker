async function loadDetail() {
  const res = await fetch(`/api/habits/${HABIT_ID}/details`);
  if (res.status === 401) {
    window.location.href = "/Login";
    return;
  }
  if (!res.ok) {
    document.getElementById("statsGrid").innerHTML =
      "<p>Could not load habit.</p>";
    return;
  }

  const data = await res.json();
  renderStats(data);
  renderHeatmap(data.completionDates, data.createdAt);
}

function renderStats(data) {
  const grid = document.getElementById("statsGrid");
  grid.innerHTML = `
        <div class="stat-box">
            <span class="stat-value">${data.currentStreak}</span>
            <span class="stat-label">Current streak</span>
        </div>
        <div class="stat-box">
            <span class="stat-value">${data.longestStreak}</span>
            <span class="stat-label">Longest streak</span>
        </div>
        <div class="stat-box">
            <span class="stat-value">${data.totalCompletions}</span>
            <span class="stat-label">Total completions</span>
        </div>
        <div class="stat-box">
            <span class="stat-value">${data.completionRate}%</span>
            <span class="stat-label">Completion rate</span>
        </div>
    `;
}

// Formats a Date as local yyyy-MM-dd (avoids UTC/timezone shifting the day)
function toLocalDateStr(date) {
  const y = date.getFullYear();
  const m = String(date.getMonth() + 1).padStart(2, "0");
  const d = String(date.getDate()).padStart(2, "0");
  return `${y}-${m}-${d}`;
}

function renderHeatmap(completionDates, createdAt) {
  const heatmap = document.getElementById("heatmap");
  if (!heatmap) return;

  heatmap.innerHTML = "";
  heatmap.style.display = "flex";
  heatmap.style.flexWrap = "wrap";
  heatmap.style.gap = "5px";
  heatmap.style.minHeight = "24px";

  const completedSet = new Set(completionDates || []);

  const today = new Date();
  today.setHours(0, 0, 0, 0);

  const [cy, cm, cd] = (createdAt || toLocalDateStr(today))
    .split("-")
    .map(Number);
  const created = new Date(cy, cm - 1, cd);
  created.setHours(0, 0, 0, 0);

  const totalDays = Math.max(1, Math.floor((today - created) / 86400000) + 1);

  let cursor = new Date(created);
  for (let i = 0; i < totalDays; i++) {
    const dateStr = toLocalDateStr(cursor);
    const isCompleted = completedSet.has(dateStr);

    const box = document.createElement("div");
    box.title = dateStr;
    box.style.width = "22px";
    box.style.height = "22px";
    box.style.borderRadius = "4px";
    box.style.flexShrink = "0";
    box.style.border = "1px solid #999";
    box.style.backgroundColor = isCompleted ? "#2B5540" : "#f0f0f0";

    heatmap.appendChild(box);
    cursor.setDate(cursor.getDate() + 1);
  }
}

loadDetail();
