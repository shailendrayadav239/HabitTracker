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

function renderHeatmap(completionDates, createdAt) {
  const completedSet = new Set(completionDates);
  const today = new Date();
  today.setHours(0, 0, 0, 0);

  const created = new Date(createdAt + "T00:00:00");
  const daysSinceCreation = Math.floor((today - created) / 86400000) + 1;
  const totalWeeks = Math.min(52, Math.ceil(daysSinceCreation / 7) + 1);

  const endDate = new Date(today);
  const startDate = new Date(today);
  startDate.setDate(startDate.getDate() - (totalWeeks * 7 - 1));
  startDate.setDate(startDate.getDate() - startDate.getDay());

  const heatmap = document.getElementById("heatmap");
  heatmap.innerHTML = "";
  heatmap.style.gridTemplateColumns = `repeat(${totalWeeks}, 1fr)`;

  const cells = [];
  let cursor = new Date(startDate);
  while (cursor <= endDate) {
    const dateStr = cursor.toISOString().split("T")[0];
    cells.push({
      dateStr,
      completed: completedSet.has(dateStr),
      isFuture: cursor > today,
      isBeforeCreation: cursor < created,
    });
    cursor.setDate(cursor.getDate() + 1);
  }

  for (let w = 0; w < totalWeeks; w++) {
    const col = document.createElement("div");
    col.className = "heatmap-col";
    for (let d = 0; d < 7; d++) {
      const cell = cells[w * 7 + d];
      const box = document.createElement("div");
      if (!cell || cell.isFuture || cell.isBeforeCreation) {
        box.className = "heatmap-cell empty-cell";
      } else {
        box.className = `heatmap-cell ${cell.completed ? "completed" : ""}`;
        box.title = cell.dateStr;
      }
      col.appendChild(box);
    }
    heatmap.appendChild(col);
  }
}

loadDetail();
