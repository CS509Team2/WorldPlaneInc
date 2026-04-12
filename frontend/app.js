const API_BASE_URL = "http://localhost:5237";

async function login(username, password) {
  const res = await fetch(`${API_BASE_URL}/Login/api/login`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ username, password }),
  });
  return { ok: res.ok, status: res.status, data: await res.json() };
}

async function signup(username, password) {
  const res = await fetch(`${API_BASE_URL}/Login/api/signup`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ username, password }),
  });
  return { ok: res.ok, status: res.status, data: await res.json() };
}

async function guestSign() {
  var username = "guest";
  var password = "guestPassword"

  const res = await fetch(`${API_BASE_URL}/Login/api/guestsign`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ username, password }),
  });
  return { ok: res.ok, status: res.status, data: await res.json() };
}

function showMessage(el, text, type) {
  el.textContent = text;
  el.className = `message ${type}`;
}

function clearMessage(el) {
  el.textContent = "";
  el.className = "message";
}

document.addEventListener("DOMContentLoaded", () => {
  const guestLink = document.getElementById("guest-link");
  if (guestLink) {
    guestLink.addEventListener("click", async (e) => {
      e.preventDefault();
      const msg = document.getElementById("msg");
      clearMessage(msg);

      const username = "guest";
      const password = "guestPassword";

      try {
        const result = await guestSign();
        if (result.ok) {
          sessionStorage.setItem("username", username);
          showMessage(msg, "Signing in as guest...", "success");
          setTimeout(() => (window.location.href = "home.html"), 1000);
        } else {
          showMessage("An error occured signing in as guest.", "error");
        }
      } catch {
        showMessage("Could not connect to the server.", "error");
      }



      setTimeout(() => (window.location.href = "home.html"), 1000);
    });
  }

  const loginForm = document.getElementById("login-form");
  if (loginForm) {
    loginForm.addEventListener("submit", async (e) => {
      e.preventDefault();
      const msg = document.getElementById("msg");
      clearMessage(msg);

      const username = document.getElementById("username").value.trim();
      const password = document.getElementById("password").value;

      if (!username || !password) {
        showMessage(msg, "Please fill in all fields.", "error");
        return;
      }

      try {
        const result = await login(username, password);
        if (result.ok) {
          sessionStorage.setItem("username", username);
          showMessage(msg, "Login successful! Redirecting…", "success");
          setTimeout(() => (window.location.href = "home.html"), 1000);
        } else {
          showMessage(msg, result.data.message || "Invalid username or password.", "error");
        }
      } catch {
        showMessage(msg, "Could not connect to the server.", "error");
      }
    });
  }

  const signupForm = document.getElementById("signup-form");
  if (signupForm) {
    signupForm.addEventListener("submit", async (e) => {
      e.preventDefault();
      const msg = document.getElementById("msg");
      clearMessage(msg);

      const username = document.getElementById("username").value.trim();
      const password = document.getElementById("password").value;
      const confirm = document.getElementById("confirm-password").value;

      if (!username || !password || !confirm) {
        showMessage(msg, "Please fill in all fields.", "error");
        return;
      }

      if (password !== confirm) {
        showMessage(msg, "Passwords do not match.", "error");
        return;
      }

      try {
        const result = await signup(username, password);
        if (result.ok) {
          showMessage(msg, "Account created! Redirecting to login…", "success");
          setTimeout(() => (window.location.href = "login.html"), 1500);
        } else if (result.status === 409) {
          showMessage(msg, "Username is already taken.", "error");
        } else {
          showMessage(msg, result.data.message || "Signup failed.", "error");
        }
      } catch {
        showMessage(msg, "Could not connect to the server.", "error");
      }
    });
  }
});
