import React, { useState } from "react";
import { useNavigate } from "react-router-dom";
import {
  ArrowRight,
  Eye,
  EyeOff,
  ShieldCheck,
  Layers,
  LockKeyhole,
  ChartNoAxesCombined,
} from "lucide-react";
import { api } from "../../api";
import { useAuth } from "../../store";
import ThemeToggle from "../../components/ui/ThemeToggle";
export default function Login() {
  const nav = useNavigate(),
    login = useAuth((s) => s.login);
  const [email, setEmail] = useState(""),
    [password, setPassword] = useState(""),
    [err, setErr] = useState(""),
    [busy, setBusy] = useState(false),
    [visible, setVisible] = useState(false);
  async function submit(e) {
    e.preventDefault();
    if (busy) return;
    setBusy(true);
    setErr("");
    try {
      const r = await api("/auth/login", {
        method: "POST",
        body: JSON.stringify({ email: email.trim(), password }),
      });
      if (!r.user.roles.some((role) => role === "ADMIN" || role === "MANAGER"))
        throw new Error("Administrator or manager access is required.");
      login(r.accessToken, r.user);
      nav("/");
    } catch (e) {
      setErr(e.message);
    } finally {
      setBusy(false);
    }
  }
  return (
    <div className="login-page">
      <header className="login-header">
        <a className="brand" href="/">
          <span className="brand-icon">
            <Layers size={23} />
          </span>
          MyFundex<span className="portal-tag">ADMIN</span>
        </a>
        <ThemeToggle />
      </header>
      <main className="login-layout">
        <section className="login-story">
          <span className="eyebrow">MYFUNDEX OPERATIONS</span>
          <h1>
            A clear view.
            <br />
            Better control.
          </h1>
          <p>
            One workspace for your people, assessment plans, and trading
            operations.
          </p>
          <div className="workspace-art" aria-hidden="true">
            <div className="art-toolbar">
              <span className="art-dot" />
              <span className="art-dot" />
              <span className="art-dot" />
              <span>Operations workspace</span>
            </div>
            <div className="art-body">
              <div className="art-icon">
                <ChartNoAxesCombined size={32} />
              </div>
              <div>
                <strong>Every stage. One place.</strong>
                <p>Assess. Review. Manage.</p>
              </div>
            </div>
            <div className="stage-track">
              <span>Assessment</span>
              <ArrowRight size={16} />
              <span>Review</span>
              <ArrowRight size={16} />
              <span>Funded</span>
            </div>
            <div className="art-bars">
              <i />
              <i />
              <i />
              <i />
              <i />
              <i />
              <i />
              <i />
              <i />
            </div>
            <div className="art-footer">
              <ShieldCheck size={17} /> Access built around your role
            </div>
          </div>
          <div className="story-caption">
            <ShieldCheck size={20} />
            <span>Purpose-built for your operations team</span>
          </div>
        </section>
        <section className="login-panel">
          <div className="login-lock">
            <LockKeyhole size={25} />
          </div>
          <span className="eyebrow">ADMIN & MANAGER ACCESS</span>
          <h2>Welcome back</h2>
          <p className="muted">Sign in to your MyFundex workspace.</p>
          <form onSubmit={submit} aria-busy={busy}>
            <label htmlFor="email">Work email</label>
            <input
              id="email"
              className="field"
              type="email"
              autoComplete="username"
              placeholder="you@company.com"
              required
              value={email}
              onChange={(e) => setEmail(e.target.value)}
              disabled={busy}
            />
            <label htmlFor="password">Password</label>
            <div className="password-field">
              <input
                id="password"
                className="field"
                type={visible ? "text" : "password"}
                autoComplete="current-password"
                placeholder="Enter your password"
                required
                value={password}
                onChange={(e) => setPassword(e.target.value)}
                disabled={busy}
              />
              <button
                type="button"
                className="icon-button"
                aria-label={visible ? "Hide password" : "Show password"}
                aria-pressed={visible}
                onClick={() => setVisible(!visible)}
              >
                {visible ? <EyeOff size={19} /> : <Eye size={19} />}
              </button>
            </div>
            {err && (
              <p className="error form-alert" role="alert">
                {err}
              </p>
            )}
            <button className="btn login-submit" disabled={busy}>
              {busy ? "Signing in…" : "Sign in"}
              <ArrowRight size={18} />
            </button>
          </form>
          <div className="login-help">
            <strong>Need access to the workspace?</strong>
            <p>Contact your administrator to create or restore your account.</p>
          </div>
          <div className="login-secure">
            <ShieldCheck size={15} /> Authorized team members only
          </div>
        </section>
      </main>
      <footer className="login-footer">
        © {new Date().getFullYear()} MyFundex · Administration workspace
      </footer>
    </div>
  );
}
