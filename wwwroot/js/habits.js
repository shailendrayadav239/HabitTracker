const habitList = document.getElementById("habitList");
const emptyState = document.getElementById("emptyState");
const loadingState = document.getElementById("loadingState");
const saveHabitBtn = document.getElementById("saveHabitBtn");
const habitIdInput = document.getElementById("habitId");
const habitNameInput = document.getElementById("habitName");
const habitFrequencyInput = document.getElementById("habitFrequency");
const habitNameError = document.getElementById("habitNameError");
const habitModalTitle = document.getElementById("habitModalTitle");
const openAddModalBtn = document.getElementById("openAddModalBtn");
const confirmDeleteBtn = document.getElementById("confirmDeleteBtn");
const deleteHabitName = document.getElementById("deleteHabitName");

const habitModal = new bootstrap.Modal(document.getElementById("habitModal"));
const confirmDeleteModal = new bootstrap.Modal(
  document.getElementById("confirmDeleteModal"),
);

let pendingDeleteId = null;
let habitsCache = [];

const inFlight = new Set();
function withGuard(key, fn) {
  return async (...args) => {
    if (inFlight.has(key)) return;
    inFlight.add(key);
    try {
      await fn(...args);
    } finally {
      inFlight.delete(key);
    }
  };
}

function showToast(message, type = "success") {
  const container = document.getElementById("toastContainer");
  const toast = document.createElement("div");
  toast.className = `app-toast ${type}`;
  toast.textContent = message;
  container.appendChild(toast);
  requestAnimationFrame(() => toast.classList.add("show"));
  setTimeout(() => {
    toast.classList.remove("show");
    setTimeout(() => toast.remove(), 250);
  }, 3000);
}

async function loadHabits() {
  loadingState.classList.remove("d-none");
  habitList.classList.add("d-none");
  emptyState.classList.add("d-none");

  const res = await fetch("/api/habits");
  if (res.status === 401) {
    window.location.href = "/Login";
    return;
  }
  habitsCache = await res.json();
  loadingState.classList.add("d-none");
  renderHabits(habitsCache);
}

function renderHabits(habits) {
  habitList.innerHTML = "";

  if (habits.length === 0) {
    emptyState.classList.remove("d-none");
    habitList.classList.add("d-none");
    return;
  }
  emptyState.classList.add("d-none");
  habitList.classList.remove("d-none");

  habits.forEach((habit) => {
    const card = document.createElement("div");
    card.className = "habit-card";
    card.dataset.id = habit.id;
    card.dataset.weekOffset = "0";
    card.innerHTML = buildCardHtml(habit);
    habitList.appendChild(card);
    attachCardListeners(card, habit);
  });
}

function buildCardHtml(habit) {
  return `
        <div class="habit-card-top">
            <div>
                <h4><a href="/Habit/${habit.id}" class="habit-name-link">${escapeHtml(habit.name)}</a></h4>
                <span class="habit-frequency">${habit.frequency}</span>
            </div>
            <div class="habit-card-icons">
                <button class="icon-btn edit-habit-btn" title="Edit habit">✎</button>
                <button class="icon-btn delete-habit-btn" title="Delete habit">&times;</button>
            </div>
        </div>

        <div class="streak-badge">
            <span class="streak-number">${habit.streak}</span>
            <span class="streak-label">day streak</span>
        </div>

        <div class="week-nav">
            <button class="week-nav-btn prev-week-btn" title="Previous week">&lsaquo;</button>
            <span class="week-range-label">This week</span>
            <button class="week-nav-btn next-week-btn" title="Next week" disabled>&rsaquo;</button>
        </div>

        <div class="week-strip">
            ${habit.last7Days
              .map(
                (d) => `
                <div class="day-dot ${d.completed ? "completed" : ""}" title="${d.date}">
                    <span>${d.dayLabel}</span>
                </div>
            `,
              )
              .join("")}
        </div>

        <button class="tick-btn ${habit.completedToday ? "ticked" : ""}">
            ${habit.completedToday ? "✓ Done today (tap to undo)" : "Mark today complete"}
        </button>
    `;
}

function attachCardListeners(card, habit) {
  const id = habit.id;

  const tickBtn = card.querySelector(".tick-btn");
  tickBtn.addEventListener(
    "click",
    withGuard(`tick-${id}`, async () => {
      tickBtn.disabled = true;
      const isTicked = tickBtn.classList.contains("ticked");
      try {
        const res = await fetch(`/api/habits/${id}/complete`, {
          method: isTicked ? "DELETE" : "POST",
        });
        if (res.ok) {
          const updated = await res.json();
          updateCardStreakAndTick(card, updated);
          showToast(
            isTicked ? "Marked as not done" : "Nice work! Streak updated.",
          );
        } else {
          const err = await res.json().catch(() => ({}));
          showToast(err.message || "Something went wrong.", "error");
        }
      } finally {
        tickBtn.disabled = false;
      }
    }),
  );

  const editBtn = card.querySelector(".edit-habit-btn");
  editBtn.addEventListener(
    "click",
    withGuard(`edit-open-${id}`, async () => {
      habitModalTitle.textContent = "Edit habit";
      habitIdInput.value = id;
      habitNameInput.value = habit.name;
      habitFrequencyInput.value = habit.frequency;
      habitNameError.textContent = "";
      habitModal.show();
    }),
  );

  const deleteBtn = card.querySelector(".delete-habit-btn");
  deleteBtn.addEventListener(
    "click",
    withGuard(`delete-open-${id}`, async () => {
      pendingDeleteId = id;
      deleteHabitName.textContent = habit.name;
      confirmDeleteModal.show();
    }),
  );

  const prevBtn = card.querySelector(".prev-week-btn");
  const nextBtn = card.querySelector(".next-week-btn");

  prevBtn.addEventListener(
    "click",
    withGuard(`week-${id}`, async () => {
      await navigateWeek(card, id, -1);
    }),
  );
  nextBtn.addEventListener(
    "click",
    withGuard(`week-${id}`, async () => {
      await navigateWeek(card, id, 1);
    }),
  );
}

async function navigateWeek(card, id, direction) {
  const currentOffset = parseInt(card.dataset.weekOffset, 10);
  const newOffset = currentOffset + direction;
  if (newOffset > 0) return;

  const prevBtn = card.querySelector(".prev-week-btn");
  const nextBtn = card.querySelector(".next-week-btn");
  prevBtn.disabled = true;
  nextBtn.disabled = true;

  try {
    const res = await fetch(`/api/habits/${id}/week?offset=${newOffset}`);
    if (!res.ok) {
      showToast("Could not load that week.", "error");
      return;
    }
    const weekData = await res.json();
    card.dataset.weekOffset = String(newOffset);

    const strip = card.querySelector(".week-strip");
    strip.innerHTML = weekData.days
      .map(
        (d) => `
            <div class="day-dot ${d.completed ? "completed" : ""}" title="${d.date}">
                <span>${d.dayLabel}</span>
            </div>
        `,
      )
      .join("");

    card.querySelector(".week-range-label").textContent =
      newOffset === 0 ? "This week" : weekData.rangeLabel;
  } finally {
    prevBtn.disabled = false;
    nextBtn.disabled = newOffset >= 0;
  }
}

function updateCardStreakAndTick(card, habitDto) {
  card.querySelector(".streak-number").textContent = habitDto.streak;

  const tickBtn = card.querySelector(".tick-btn");
  if (habitDto.completedToday) {
    tickBtn.classList.add("ticked");
    tickBtn.textContent = "✓ Done today (tap to undo)";
  } else {
    tickBtn.classList.remove("ticked");
    tickBtn.textContent = "Mark today complete";
  }

  if (card.dataset.weekOffset === "0") {
    const strip = card.querySelector(".week-strip");
    strip.innerHTML = habitDto.last7Days
      .map(
        (d) => `
            <div class="day-dot ${d.completed ? "completed" : ""}" title="${d.date}">
                <span>${d.dayLabel}</span>
            </div>
        `,
      )
      .join("");
  }

  const cached = habitsCache.find((h) => h.id === habitDto.id);
  if (cached) Object.assign(cached, habitDto);
}

openAddModalBtn.addEventListener("click", () => {
  habitModalTitle.textContent = "Add a new habit";
  habitIdInput.value = "";
  habitNameInput.value = "";
  habitFrequencyInput.value = "daily";
  habitNameError.textContent = "";
  habitModal.show();
});

saveHabitBtn.addEventListener(
  "click",
  withGuard("save-habit", async () => {
    habitNameError.textContent = "";
    const name = habitNameInput.value.trim();
    const frequency = habitFrequencyInput.value;
    const id = habitIdInput.value;

    if (!name) {
      habitNameError.textContent = "Habit name is required.";
      return;
    }

    saveHabitBtn.disabled = true;
    try {
      const isEdit = !!id;
      const res = await fetch(isEdit ? `/api/habits/${id}` : "/api/habits", {
        method: isEdit ? "PUT" : "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ name, frequency }),
      });

      if (res.ok) {
        habitModal.hide();
        showToast(isEdit ? "Habit updated." : "Habit added.");
        loadHabits();
      } else {
        habitNameError.textContent = "Something went wrong. Try again.";
      }
    } finally {
      saveHabitBtn.disabled = false;
    }
  }),
);

confirmDeleteBtn.addEventListener(
  "click",
  withGuard("confirm-delete", async () => {
    if (!pendingDeleteId) return;
    confirmDeleteBtn.disabled = true;
    try {
      const res = await fetch(`/api/habits/${pendingDeleteId}`, {
        method: "DELETE",
      });
      confirmDeleteModal.hide();
      if (res.ok) {
        showToast("Habit deleted.");
        loadHabits();
      } else {
        showToast("Could not delete habit.", "error");
      }
    } finally {
      confirmDeleteBtn.disabled = false;
      pendingDeleteId = null;
    }
  }),
);

function escapeHtml(str) {
  const div = document.createElement("div");
  div.textContent = str;
  return div.innerHTML;
}

loadHabits();
