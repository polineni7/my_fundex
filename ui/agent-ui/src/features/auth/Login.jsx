import React from "react";
import { useState } from "react";
import { useNavigate } from "react-router-dom";
import { api } from "../../api";
import { useAuth } from "../../store";
import { Register } from "../../Commercial";
import GoogleSignIn from "../../GoogleSignIn";
export default function Login() {
  const [register, setRegister] = useState(false);
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
      login(r.accessToken, r.user);
      nav("/");
    } catch (e) {
      setErr(e.message);
    }
  }
  if (register) return <Register onCancel={() => setRegister(false)} />;
  return (
    <div className="login">
      <form className="card" onSubmit={submit}>
        <h1>MyFundex</h1>
        <p className="muted">Funded equity trading</p>
        <GoogleSignIn />
        <p className="muted">Existing password account</p>
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
        <button
          className="btn secondary"
          type="button"
          onClick={() => setRegister(true)}
        >
          Create account
        </button>
      </form>
    </div>
  );
}
