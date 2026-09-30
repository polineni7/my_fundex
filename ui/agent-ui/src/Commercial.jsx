import React, { useState } from "react";
import { api } from "./api";
import { useAuth } from "./store";

export function Register({ onCancel }) {
  const [form, setForm] = useState({
    firstName: "",
    lastName: "",
    email: "",
    password: "",
  });
  const [busy, setBusy] = useState(false),
    [error, setError] = useState("");
  async function submit(event) {
    event.preventDefault();
    setBusy(true);
    setError("");
    try {
      await api("/auth/register", {
        method: "POST",
        body: JSON.stringify(form),
      });
      const result = await api("/auth/login", {
        method: "POST",
        body: JSON.stringify({ email: form.email, password: form.password }),
      });
      useAuth.getState().login(result.accessToken, result.user);
    } catch (error) {
      setError(error.message);
    } finally {
      setBusy(false);
    }
  }
  return (
    <div className="login">
      <form className="card" onSubmit={submit}>
        <h1>Create your account</h1>
        {Object.keys(form).map((key) => (
          <label key={key}>
            {
              {
                firstName: "First name",
                lastName: "Last name",
                email: "Email",
                password: "Password",
              }[key]
            }
            <input
              className="field"
              required
              type={
                key === "password"
                  ? "password"
                  : key === "email"
                    ? "email"
                    : "text"
              }
              minLength={key === "password" ? 12 : undefined}
              autoComplete={
                key === "password"
                  ? "new-password"
                  : key === "email"
                    ? "email"
                    : "name"
              }
              value={form[key]}
              onChange={(e) => setForm({ ...form, [key]: e.target.value })}
            />
          </label>
        ))}
        <p className="muted">Use a password of at least 12 characters.</p>
        {error && (
          <p role="alert" className="error">
            {error}
          </p>
        )}
        <button className="btn" disabled={busy}>
          {busy ? "Creating account…" : "Create account"}
        </button>
        <button className="btn secondary" type="button" onClick={onCancel}>
          Back to sign in
        </button>
      </form>
    </div>
  );
}
