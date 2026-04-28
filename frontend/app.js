const API_BASE_URL = "http://localhost:5237";

// ── API helpers ──

async function apiLogin(username, password) {
  const res = await fetch(`${API_BASE_URL}/Login/api/login`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ username, password }),
  });
  return { ok: res.ok, status: res.status, data: await res.json() };
}

async function apiSignup(username, password) {
  const res = await fetch(`${API_BASE_URL}/Login/api/signup`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ username, password }),
  });
  return { ok: res.ok, status: res.status, data: await res.json() };
}

async function apiGuestLogin() {
  const res = await fetch(`${API_BASE_URL}/Login/api/guest`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
  });
  return { ok: res.ok, status: res.status, data: await res.json() };
}

async function apiUpdateSettings(currentUsername, newUsername, currentPassword, newPassword) {
  const res = await fetch(`${API_BASE_URL}/Login/api/settings/update`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({
      currentUsername,
      newUsername,
      currentPassword,
      newPassword,
    }),
  });

  let data = null;
  const contentType = res.headers.get("content-type") || "";
  if (contentType.includes("application/json")) {
    try {
      data = await res.json();
    } catch {
      data = null;
    }
  } else {
    const text = await res.text();
    data = text ? { message: text } : null;
  }

  return { ok: res.ok, status: res.status, data };
}

async function apiSearchFlights(params) {
  const res = await fetch(`${API_BASE_URL}/Flights/search`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(params),
  });
  return { ok: res.ok, status: res.status, data: await res.json() };
}

async function apiGetSeats(flightId, airline) {
  const res = await fetch(
    `${API_BASE_URL}/Seats?flightId=${encodeURIComponent(flightId)}&airline=${encodeURIComponent(airline)}`
  );
  return { ok: res.ok, status: res.status, data: await res.json() };
}

async function apiBookSeat(flightId, airline, seatNumber, username) {
  const res = await fetch(`${API_BASE_URL}/Seats/book`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ flightId, airline, seatNumber, username }),
  });
  return { ok: res.ok, status: res.status, data: await res.json() };
}

async function apiGetUserReservations(username) {
  const res = await fetch(
    `${API_BASE_URL}/Seats/reservations?username=${encodeURIComponent(username)}`
  );
  return { ok: res.ok, status: res.status, data: await res.json() };
}

// ── Message helpers (Bootstrap alert style) ──

function showAlert(el, text, type) {
  el.textContent = text;
  el.className = `alert alert-${type === "error" ? "danger" : "success"} mt-3`;
  el.classList.remove("d-none");
}

function hideAlert(el) {
  el.textContent = "";
  el.className = "alert mt-3 d-none";
}

// ── Session guard for authenticated pages ──

function requireAuth() {
  const username = sessionStorage.getItem("username");
  if (!username) {
    window.location.href = "login.html";
    return null;
  }
  const navUser = document.getElementById("nav-username");
  if (navUser) {
    navUser.textContent = username;
    if (navUser.tagName === "A") {
      navUser.setAttribute("href", username === "guest" ? "#" : "settings.html");
    }
  }
  const logoutLink = document.getElementById("logout-link");
  if (logoutLink) {
    logoutLink.addEventListener("click", function (e) {
      e.preventDefault();
      sessionStorage.clear();
      window.location.href = "login.html";
    });
  }
  return username;
}

// ── Formatting helpers ──

function formatDuration(minutes) {
  const h = Math.floor(minutes / 60);
  const m = Math.round(minutes % 60);
  return h > 0 ? `${h}h ${m}m` : `${m}m`;
}

function formatTime(dtString) {
  return new Date(dtString).toLocaleTimeString([], { hour: "2-digit", minute: "2-digit" });
}

function formatDate(dtString) {
  return new Date(dtString).toLocaleDateString([], { month: "short", day: "numeric" });
}

function formatAirportLocalDateTime(utcDateTimeString, timeZoneId) {
  const date = new Date(utcDateTimeString);

  return new Intl.DateTimeFormat("en-US", {
    hour: "2-digit",
    minute: "2-digit",
    hour12: true,
    timeZone: timeZoneId
  }).format(date);
}

function formatAirportLocalDate(utcDateTimeString, timeZoneId) {
  const date = new Date(utcDateTimeString);

  return new Intl.DateTimeFormat("en-US", {
    month: "short",
    day: "numeric",
    timeZone: timeZoneId
  }).format(date);
}

function extractCode(airport) {
  const match = airport.match(/\(([A-Z]{3})\)/);
  return match ? match[1] : airport.trim().toUpperCase();
}

// ── DOMContentLoaded: Wire up all pages ──

document.addEventListener("DOMContentLoaded", () => {
  initLoginPage();
  initSignupPage();
  initSearchPage();
  initSeatsPage();
<<<<<<< HEAD
  initSettingsPage();
=======
  initHomePage();
>>>>>>> e2b5983 (added basic frontend to view reservations)
});

// -- Home Page --
function initHomePage() {
  const button = document.getElementById("reservationButton");
  let resShown = false;

  if (sessionStorage.getItem("username") === "guest") {
    const resDiv = document.getElementById("reservationDiv");
    resDiv.style.display = "none";
  }

  button.addEventListener("click", async (e) => {
    e.preventDefault();

    const resTable = document.getElementById("reservationTable");
    if (!resShown) {
      resShown = true;
      button.innerText = "Hide Reservations";

      const reservations = await apiGetUserReservations(sessionStorage.getItem("username"));

      if (JSON.stringify(reservations.data) === "[]") {
        resTable.innerHTML = "You do not have any reservations.";
      }
      else {

        let html = "";
        html += `<style>
                    table {
                      border-collapse: collapse;
                      width: 100%;
                    }
                    th, td {
                      text-align: left;
                      padding: 8px;
                    }
                    tr:nth-child(even) {
                      background-color: #D3D3D3;
                    }
                    tr {
                      border: 1px solid #ddd;
                    }
                    th {
                      border: 1px solid #000;
                    }
                  </style>
                  <tr>
                    <th>Departure Airport</th>
                    <th>Departure Date and Time</th>
                    <th>Arrival Airport</th>
                    <th>Arrival Date and Time</th>
                    <th>Airline</th>
                    <th>Flight Number</th>
                    <th>Seat</th>
                  </tr>`;

        for (var eachItem in reservations) {
          var dataObj = reservations[eachItem];
          for (var eachValue in dataObj) {
            html += "<tr>";
            html += `<td>${JSON.stringify(dataObj[eachValue].departureAirport).replace(/"/g, '')}</td>`;
            html += `<td>${formatAirportLocalDate(JSON.stringify(dataObj[eachValue].departureDateTime).replace(/"/g, ''), JSON.stringify(dataObj[eachValue].departTimeZoneId).replace(/"/g, '')) + " " +
              formatAirportLocalDateTime(JSON.stringify(dataObj[eachValue].departureDateTime).replace(/"/g, ''), JSON.stringify(dataObj[eachValue].departTimeZoneId).replace(/"/g, ''))}</td>`;

            html += `<td>${JSON.stringify(dataObj[eachValue].arrivalAirport).replace(/"/g, '')}</td>`;
            html += `<td>${formatAirportLocalDate(JSON.stringify(dataObj[eachValue].arrivalDateTime).replace(/"/g, ''), JSON.stringify(dataObj[eachValue].arriveTimeZoneId).replace(/"/g, '')) + " " +
              formatAirportLocalDateTime(JSON.stringify(dataObj[eachValue].arrivalDateTime).replace(/"/g, ''), JSON.stringify(dataObj[eachValue].arriveTimeZoneId).replace(/"/g, ''))}</td>`;
            html += `<td>${JSON.stringify(dataObj[eachValue].airline).replace(/"/g, '')}</td>`;
            html += `<td>${JSON.stringify(dataObj[eachValue].flightNumber).replace(/"/g, '')}</td>`;
            html += `<td>${JSON.stringify(dataObj[eachValue].seat).replace(/"/g, '')}</td>`;
            html += "</tr>";
          }
        }

        resTable.innerHTML = html;
      }
    }
    else {
      button.innerText = "Load Reservations";
      resTable.innerHTML = "";
      resShown = false;
    }
  });

}

// ── Login page ──

function initLoginPage() {
  const form = document.getElementById("login-form");
  if (!form) return;

  form.addEventListener("submit", async (e) => {
    e.preventDefault();
    const msg = document.getElementById("msg");
    hideAlert(msg);

    const username = document.getElementById("username").value.trim();
    const password = document.getElementById("password").value;

    if (!username || !password) {
      showAlert(msg, "Please fill in all fields.", "error");
      return;
    }

    try {
      const result = await apiLogin(username, password);
      if (result.ok) {
        sessionStorage.setItem("username", username);
        showAlert(msg, "Login successful! Redirecting...", "success");
        setTimeout(() => (window.location.href = "home.html"), 800);
      } else {
        showAlert(msg, result.data.message || "Invalid username or password.", "error");
      }
    } catch {
      showAlert(msg, "Could not connect to the server.", "error");
    }
  });

  const guestLink = document.getElementById("guest-link");
  if (guestLink) {
    guestLink.addEventListener("click", async (e) => {
      e.preventDefault();
      const msg = document.getElementById("msg");
      hideAlert(msg);

      try {
        const result = await apiGuestLogin();
        if (result.ok) {
          sessionStorage.setItem("username", "guest");
          showAlert(msg, "Signing in as guest...", "success");
          setTimeout(() => (window.location.href = "home.html"), 800);
        } else {
          showAlert(msg, result.data.message || "Guest login failed.", "error");
        }
      } catch {
        showAlert(msg, "Could not connect to the server.", "error");
      }
    });
  }
}

// ── Signup page ──

function initSignupPage() {
  const form = document.getElementById("signup-form");
  if (!form) return;

  form.addEventListener("submit", async (e) => {
    e.preventDefault();
    const msg = document.getElementById("msg");
    hideAlert(msg);

    const username = document.getElementById("username").value.trim();
    const password = document.getElementById("password").value;
    const confirm = document.getElementById("confirm-password").value;

    if (!username || !password || !confirm) {
      showAlert(msg, "Please fill in all fields.", "error");
      return;
    }
    if (password !== confirm) {
      showAlert(msg, "Passwords do not match.", "error");
      return;
    }

    try {
      const result = await apiSignup(username, password);
      if (result.ok) {
        showAlert(msg, "Account created! Redirecting to login...", "success");
        setTimeout(() => (window.location.href = "login.html"), 1200);
      } else if (result.status === 409) {
        showAlert(msg, "Username is already taken.", "error");
      } else {
        showAlert(msg, result.data.message || "Signup failed.", "error");
      }
    } catch {
      showAlert(msg, "Could not connect to the server.", "error");
    }
  });
}

// ── Search page ──

let searchResults = null;
let currentSort = "duration";

function initSearchPage() {
  const form = document.getElementById("search-form");
  if (!form) return;

  requireAuth();

  const roundTripToggle = document.getElementById("round-trip-toggle");
  const returnDateInput = document.getElementById("return-date");
  roundTripToggle.addEventListener("change", () => {
    returnDateInput.disabled = !roundTripToggle.checked;
    if (!roundTripToggle.checked) returnDateInput.value = "";
  });
  returnDateInput.disabled = true;

  form.addEventListener("submit", async (e) => {
    e.preventDefault();
    await performSearch();
  });

  document.querySelectorAll("[data-sort]").forEach((btn) => {
    btn.addEventListener("click", () => {
      document.querySelectorAll("[data-sort]").forEach((b) => b.classList.remove("active"));
      btn.classList.add("active");
      currentSort = btn.dataset.sort;
      renderResults();
    });
  });

  document.getElementById("filter-stops").addEventListener("change", renderResults);
  document.getElementById("filter-delta").addEventListener("change", renderResults);
  document.getElementById("filter-southwest").addEventListener("change", renderResults);
}

async function performSearch() {
  const errorEl = document.getElementById("search-error");
  const resultsSection = document.getElementById("results-section");
  const noResults = document.getElementById("no-results");
  const spinner = document.getElementById("search-spinner");
  const btnText = document.getElementById("search-btn-text");

  errorEl.classList.add("d-none");
  resultsSection.classList.add("d-none");
  noResults.classList.add("d-none");
  spinner.classList.remove("d-none");
  btnText.textContent = "Searching...";

  const params = {
    departureAirport: document.getElementById("departure-airport").value.trim(),
    arrivalAirport: document.getElementById("arrival-airport").value.trim(),
    departureDate: document.getElementById("departure-date").value,
  };

  const returnDate = document.getElementById("return-date").value;
  if (returnDate) params.returnDate = returnDate;

  const depStart = document.getElementById("dep-time-start").value;
  const depEnd = document.getElementById("dep-time-end").value;
  const arrStart = document.getElementById("arr-time-start").value;
  const arrEnd = document.getElementById("arr-time-end").value;

  if (depStart) params.departureTimeStart = depStart;
  if (depEnd) params.departureTimeEnd = depEnd;
  if (arrStart) params.arrivalTimeStart = arrStart;
  if (arrEnd) params.arrivalTimeEnd = arrEnd;

  try {
    const result = await apiSearchFlights(params);
    spinner.classList.add("d-none");
    btnText.textContent = "Search Flights";

    if (!result.ok) {
      errorEl.textContent = typeof result.data === "string" ? result.data : result.data.message || "Search failed.";
      errorEl.classList.remove("d-none");
      return;
    }

    searchResults = result.data;

    const totalOutbound = (searchResults.outboundItineraries || []).length;
    const totalReturn = (searchResults.returnItineraries || []).length;

    if (totalOutbound === 0 && totalReturn === 0) {
      noResults.classList.remove("d-none");
      return;
    }

    resultsSection.classList.remove("d-none");
    renderResults();
  } catch {
    spinner.classList.add("d-none");
    btnText.textContent = "Search Flights";
    errorEl.textContent = "Could not connect to the server.";
    errorEl.classList.remove("d-none");
  }
}

function filterAndSort(itineraries) {
  if (!itineraries) return [];

  const maxStops = document.getElementById("filter-stops").value;
  const showDelta = document.getElementById("filter-delta").checked;
  const showSouthwest = document.getElementById("filter-southwest").checked;

  let filtered = itineraries.filter((it) => {
    if (maxStops !== "all" && it.stops > parseInt(maxStops)) return false;
    const airlines = new Set(it.segments.map((s) => s.airline));
    if (!showDelta && airlines.has("Delta")) return false;
    if (!showSouthwest && airlines.has("Southwest")) return false;
    return true;
  });

  filtered.sort((a, b) => {
    if (currentSort === "duration") return a.totalDurationMinutes - b.totalDurationMinutes;
    if (currentSort === "stops") return a.stops - b.stops || a.totalDurationMinutes - b.totalDurationMinutes;
    if (currentSort === "depart") {
      return new Date(a.segments[0].departDateTime) - new Date(b.segments[0].departDateTime);
    }
    return 0;
  });

  return filtered;
}

function renderResults() {
  if (!searchResults) return;

  const outbound = filterAndSort(searchResults.outboundItineraries);
  const outboundEl = document.getElementById("outbound-results");
  outboundEl.innerHTML = renderItineraryList(outbound, 20);

  const returnSection = document.getElementById("return-section");
  if (searchResults.returnItineraries && searchResults.returnItineraries.length > 0) {
    returnSection.classList.remove("d-none");
    const returnFiltered = filterAndSort(searchResults.returnItineraries);
    document.getElementById("return-results").innerHTML = renderItineraryList(returnFiltered, 20);
  } else {
    returnSection.classList.add("d-none");
  }
}

function renderItineraryList(itineraries, limit) {
  if (itineraries.length === 0) {
    return '<p class="text-muted">No matching flights after filtering.</p>';
  }

  const shown = itineraries.slice(0, limit);
  let html = "";

  for (const it of shown) {
    const firstSeg = it.segments[0];
    const lastSeg = it.segments[it.segments.length - 1];
    const airlines = [...new Set(it.segments.map((s) => s.airline))].join(", ");

    html += `
      <div class="card itinerary-card mb-3">
        <div class="card-body">
          <div class="row align-items-center">
            <div class="col-md-3">
              <div class="fw-bold fs-5">${formatAirportLocalDateTime(firstSeg.departDateTime, firstSeg.departTimeZoneId)}</div>
              <div class="small text-muted">${extractCode(firstSeg.departAirport)}</div>
              <div class="small text-muted">${formatAirportLocalDate(firstSeg.departDateTime, firstSeg.departTimeZoneId)}</div>
            </div>
            <div class="col-md-3 text-center">
              <div class="small text-muted">${formatDuration(it.totalDurationMinutes)}</div>
              <div class="segment-arrow">&#8594;</div>
              <div class="small text-muted">${it.stops === 0 ? "Direct" : it.stops + " stop" + (it.stops > 1 ? "s" : "")}</div>
            </div>
            <div class="col-md-3">
              <div class="fw-bold fs-5">${formatAirportLocalDateTime(lastSeg.arriveDateTime, lastSeg.arriveTimeZoneId)}</div>
              <div class="small text-muted">${extractCode(lastSeg.arriveAirport)}</div>
              <div class="small text-muted">${formatAirportLocalDate(lastSeg.arriveDateTime, lastSeg.arriveTimeZoneId)}</div>
            </div>
            <div class="col-md-3 text-end">
              <span class="badge bg-secondary mb-2">${airlines}</span><br />
              <button class="btn btn-sm btn-wpi select-flight-btn"
                data-flight='${JSON.stringify({ flightId: firstSeg.id, flightNumber: firstSeg.flightNumber, airline: firstSeg.airline })}'>
                Select Seat
              </button>
            </div>
          </div>
          ${it.segments.length > 1 ? renderSegmentDetails(it.segments) : ""}
        </div>
      </div>`;
  }

  if (itineraries.length > limit) {
    html += `<p class="text-muted text-center">Showing ${limit} of ${itineraries.length} results.</p>`;
  }

  setTimeout(() => {
    document.querySelectorAll(".select-flight-btn").forEach((btn) => {
      btn.addEventListener("click", () => {
        const flight = JSON.parse(btn.dataset.flight);
        sessionStorage.setItem("selectedFlight", JSON.stringify(flight));
        window.location.href = "seats.html";
      });
    });
  }, 0);

  return html;
}

function renderSegmentDetails(segments) {
  let html = '<div class="mt-2 pt-2 border-top"><small class="text-muted">Segments:</small><div class="d-flex flex-wrap gap-2 mt-1">';
  for (const s of segments) {
    html += `<span class="badge bg-light text-dark border">
      ${s.flightNumber} (${s.airline}): ${extractCode(s.departAirport)} ${formatAirportLocalDateTime(s.departDateTime, s.departTimeZoneId)}
      &#8594; ${extractCode(s.arriveAirport)} ${formatAirportLocalDateTime(s.arriveDateTime, s.arriveTimeZoneId)}
    </span>`;
  }
  html += "</div></div>";
  return html;
}

// ── Settings page ──

function initSettingsPage() {
  const usernameInput = document.getElementById("settings-username");
  if (!usernameInput) return;

  const username = requireAuth();
  if (!username) return;

  const msg = document.getElementById("settings-msg");
  const resetBtn = document.getElementById("settings-reset-btn");
  const saveBtn = document.getElementById("settings-save-btn");
  const openPasswordModalBtn = document.getElementById("open-password-modal-btn");
  const passwordButtonWrap = document.getElementById("password-button-wrap");
  const passwordInputWrap = document.getElementById("password-input-wrap");
  const passwordInput = document.getElementById("settings-password");
  const verifyPasswordInput = document.getElementById("verify-password-input");
  const verifyPasswordEnterBtn = document.getElementById("verify-password-enter-btn");
  const verifyPasswordError = document.getElementById("verify-password-error");
  const verifyPasswordModalEl = document.getElementById("verifyPasswordModal");
  const settingsSidebar = document.getElementById("settings-sidebar");
  const generalSection = document.getElementById("settings-general-section");

  let originalUsername = username;
  let verifiedCurrentPassword = null;

  usernameInput.value = originalUsername;

  if (username === "guest") {
    if (settingsSidebar) {
      settingsSidebar.querySelectorAll("button").forEach((btn) => {
        btn.disabled = true;
      });
    }

    usernameInput.disabled = true;
    openPasswordModalBtn.disabled = true;
    resetBtn.disabled = true;
    saveBtn.disabled = true;

    msg.textContent = "Guest accounts cannot edit account settings. Please sign in with a registered account.";
    msg.className = "alert alert-warning";

    if (generalSection) {
      generalSection.classList.add("opacity-75");
    }

    return;
  }

  const verifyPasswordModal = bootstrap.Modal.getOrCreateInstance(verifyPasswordModalEl);

  verifyPasswordEnterBtn.addEventListener("click", async () => {
    const enteredPassword = verifyPasswordInput.value;

    verifyPasswordError.classList.add("d-none");
    verifyPasswordError.textContent = "Incorrect password.";

    if (!enteredPassword) {
      verifyPasswordError.textContent = "Please enter your password.";
      verifyPasswordError.classList.remove("d-none");
      return;
    }

    verifyPasswordEnterBtn.disabled = true;

    try {
      const result = await apiLogin(originalUsername, enteredPassword);
      if (result.ok) {
        verifiedCurrentPassword = enteredPassword;
        passwordInput.value = enteredPassword;
        passwordButtonWrap.classList.add("d-none");
        passwordInputWrap.classList.remove("d-none");
        verifyPasswordInput.value = "";
        verifyPasswordError.classList.add("d-none");
        verifyPasswordModal.hide();
      } else {
        verifyPasswordError.textContent = "Incorrect password.";
        verifyPasswordError.classList.remove("d-none");
      }
    } catch {
      verifyPasswordError.textContent = "Could not verify password.";
      verifyPasswordError.classList.remove("d-none");
    } finally {
      verifyPasswordEnterBtn.disabled = false;
    }
  });

  verifyPasswordModalEl.addEventListener("hidden.bs.modal", () => {
    verifyPasswordInput.value = "";
    verifyPasswordError.classList.add("d-none");
    verifyPasswordError.textContent = "Incorrect password.";
  });

  openPasswordModalBtn.addEventListener("click", () => {
    verifyPasswordError.classList.add("d-none");
    verifyPasswordInput.value = "";
  });

  resetBtn.addEventListener("click", () => {
    usernameInput.value = originalUsername;
    passwordInput.value = "";
    verifiedCurrentPassword = null;
    passwordInputWrap.classList.add("d-none");
    passwordButtonWrap.classList.remove("d-none");
    msg.className = "alert d-none";
    msg.textContent = "";
  });

  saveBtn.addEventListener("click", async () => {
    const newUsername = usernameInput.value.trim();

    if (!newUsername) {
      msg.textContent = "Username cannot be empty.";
      msg.className = "alert alert-danger";
      return;
    }

    let currentPassword = null;
    let newPassword = null;

    if (!passwordInputWrap.classList.contains("d-none")) {
      newPassword = passwordInput.value;
      currentPassword = verifiedCurrentPassword;

      if (!newPassword) {
        msg.textContent = "Password cannot be empty once password edit is enabled.";
        msg.className = "alert alert-danger";
        return;
      }
    }

    saveBtn.disabled = true;
    resetBtn.disabled = true;

    try {
      const result = await apiUpdateSettings(originalUsername, newUsername, currentPassword, newPassword);

      if (result.ok) {
        originalUsername = newUsername;
        sessionStorage.setItem("username", newUsername);

        if (newPassword) {
          verifiedCurrentPassword = newPassword;
        }

        const navUser = document.getElementById("nav-username");
        if (navUser) navUser.textContent = newUsername;

        msg.textContent = "Settings saved successfully.";
        msg.className = "alert alert-success";
      } else {
        msg.textContent = result.data?.message || "Could not save settings.";
        msg.className = "alert alert-danger";
      }
    } catch {
      msg.textContent = "Could not connect to the server.";
      msg.className = "alert alert-danger";
    } finally {
      saveBtn.disabled = false;
      resetBtn.disabled = false;
    }
  });
}

// ── Seats page ──

let selectedSeat = null;
let seatsData = [];

function initSeatsPage() {
  const seatGrid = document.getElementById("seat-grid");
  if (!seatGrid) return;

  const username = requireAuth();
  if (!username) return;

  const flight = JSON.parse(sessionStorage.getItem("selectedFlight") || "null");
  if (!flight) {
    window.location.href = "search.html";
    return;
  }

  document.getElementById("flight-info").textContent =
    `Flight ${flight.flightNumber} - ${flight.airline}`;
  document.getElementById("summary-flight").textContent = flight.flightNumber;
  document.getElementById("summary-airline").textContent = flight.airline;

  loadSeatMap(flight.flightId, flight.airline);

  document.getElementById("confirm-seat-btn").addEventListener("click", async () => {
    if (!selectedSeat) return;
    await bookSelectedSeat(flight.flightId, flight.flightNumber, flight.airline, username);
  });
}

async function loadSeatMap(flightId, airline) {
  const loading = document.getElementById("seat-loading");
  const errorEl = document.getElementById("seat-error");
  const container = document.getElementById("seat-map-container");

  try {
    const result = await apiGetSeats(flightId, airline);
    loading.classList.add("d-none");

    if (!result.ok) {
      errorEl.textContent = "Failed to load seats.";
      errorEl.classList.remove("d-none");
      return;
    }

    seatsData = result.data;
    container.classList.remove("d-none");
    renderSeatGrid();
  } catch {
    loading.classList.add("d-none");
    errorEl.textContent = "Could not connect to the server.";
    errorEl.classList.remove("d-none");
  }
}

function renderSeatGrid() {
  const grid = document.getElementById("seat-grid");
  const columns = ["A", "B", "C", "", "D", "E", "F"];
  const maxRow = 30;

  let html = '<div class="d-flex justify-content-center gap-1 mb-2 small fw-bold">';
  for (const col of columns) {
    if (col === "") {
      html += '<div style="width:20px"></div>';
    } else {
      html += `<div style="width:38px;text-align:center">${col}</div>`;
    }
  }
  html += "</div>";

  for (let row = 1; row <= maxRow; row++) {
    html += '<div class="d-flex justify-content-center gap-1 mb-0">';
    for (const col of columns) {
      if (col === "") {
        html += `<div style="width:20px;display:flex;align-items:center;justify-content:center" class="small text-muted fw-bold">${row}</div>`;
        continue;
      }

      const seatNum = `${row}${col}`;
      const seat = seatsData.find((s) => s.seatNumber === seatNum);

      if (!seat) {
        html += '<div class="seat-cell" style="visibility:hidden"></div>';
        continue;
      }

      let cls = "seat-available";
      if (!seat.isAvailable) cls = "seat-taken";
      else if (selectedSeat === seatNum) cls = "seat-selected";
      else if (seat.seatClass === "First") cls = "seat-first";
      else if (seat.seatClass === "Business") cls = "seat-business";

      html += `<div class="seat-cell ${cls}" data-seat="${seatNum}" data-class="${seat.seatClass}" data-price="${seat.price}"
                    title="${seatNum} - ${seat.seatClass} - $${seat.price.toFixed(2)}${seat.isAvailable ? "" : " (Taken)"}">${seatNum}</div>`;
    }
    html += "</div>";
  }

  grid.innerHTML = html;

  grid.querySelectorAll(".seat-available, .seat-business, .seat-first").forEach((el) => {
    el.addEventListener("click", () => {
      const seatNum = el.dataset.seat;
      if (selectedSeat === seatNum) {
        selectedSeat = null;
      } else {
        selectedSeat = seatNum;
      }
      renderSeatGrid();
      updateSeatSummary();
    });
  });

  if (selectedSeat) {
    const selEl = grid.querySelector(`[data-seat="${selectedSeat}"]`);
    if (selEl && !selEl.classList.contains("seat-taken")) {
      selEl.classList.remove("seat-available", "seat-business", "seat-first");
      selEl.classList.add("seat-selected");
    }
  }
}

function updateSeatSummary() {
  const seatEl = document.getElementById("summary-seat");
  const classEl = document.getElementById("summary-class");
  const priceEl = document.getElementById("summary-price");
  const confirmBtn = document.getElementById("confirm-seat-btn");

  if (!selectedSeat) {
    seatEl.textContent = "None selected";
    seatEl.className = "text-muted";
    classEl.textContent = "-";
    classEl.className = "text-muted";
    priceEl.textContent = "-";
    priceEl.className = "text-muted";
    confirmBtn.disabled = true;
    return;
  }

  const seat = seatsData.find((s) => s.seatNumber === selectedSeat);
  if (seat) {
    seatEl.textContent = seat.seatNumber;
    seatEl.className = "fw-bold";
    classEl.textContent = seat.seatClass;
    classEl.className = "";
    priceEl.textContent = `$${seat.price.toFixed(2)}`;
    priceEl.className = "fw-bold text-success";
    confirmBtn.disabled = false;
  }
}

async function bookSelectedSeat(flightId, flightNumber, airline, username) {
  const confirmBtn = document.getElementById("confirm-seat-btn");
  const spinner = document.getElementById("booking-spinner");
  const errorEl = document.getElementById("booking-error");

  confirmBtn.disabled = true;
  spinner.classList.remove("d-none");
  errorEl.classList.add("d-none");

  try {
    const result = await apiBookSeat(flightId, airline, selectedSeat, username);
    spinner.classList.add("d-none");

    if (result.ok) {
      const seat = seatsData.find((s) => s.seatNumber === selectedSeat);
      sessionStorage.setItem(
        "lastBooking",
        JSON.stringify({
          flightNumber,
          airline,
          seatNumber: selectedSeat,
          seatClass: seat ? seat.seatClass : "Economy",
        })
      );
      window.location.href = "confirmation.html";
    } else {
      errorEl.textContent = result.data.message || "Booking failed.";
      errorEl.classList.remove("d-none");
      confirmBtn.disabled = false;
    }
  } catch {
    spinner.classList.add("d-none");
    errorEl.textContent = "Could not connect to the server.";
    errorEl.classList.remove("d-none");
    confirmBtn.disabled = false;
  }
}
