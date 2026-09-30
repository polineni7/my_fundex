import React from "react";
import { useState } from "react";
import { useNavigate } from "react-router-dom";
import { api } from "../../api";
import { useAuth } from "../../store";
export default function Login() {
  const nav = useNavigate(),
    login = useAuth((s) => s.login);
  const [email, setEmail] = useState(""),
    [password, setPassword] = useState(""),
    [err, setErr] = useState("");
  async function submit(e) {
    e.preventDefault();
    try {
      const r = await api("/auth/login", {
        method: "POST",
        body: JSON.stringify({ email, password }),
      });
      if (!r.user.roles.some((role) => role === "ADMIN" || role === "MANAGER"))
        throw new Error("Administrator or manager access is required.");
      login(r.accessToken, r.user);
      nav("/");
    } catch (e) {
      setErr(e.message);
    }
  }
  return (
    <div className="login">
      <form className="card" onSubmit={submit}>
        <h1>MyFundex Admin</h1>
        <input
          className="field"
          value={email}
          onChange={(e) => setEmail(e.target.value)}
        />
        <input
          className="field"
          type="password"
          value={password}
          onChange={(e) => setPassword(e.target.value)}
        />
        {err && <p className="error">{err}</p>}
        <button className="btn">Sign in</button>
      </form>
    </div>
  );
}
