import React, { useEffect, useState } from "react";
import { api } from "../../api";
import { useAuth } from "../../store";
const empty = {
  displayName: "",
  provider: "Upstox",
  environment: "SANDBOX",
  reference: "",
  brokerUserId: "",
  isActive: false,
  isDefault: false,
  useForMarketData: false,
  sessionExpiresAt: "",
  apiKey: "",
  apiSecret: "",
  accessToken: "",
  refreshToken: "",
  version: 0,
};
export default function Settings() {
  const user = useAuth((s) => s.user),
    canWrite = user.roles.includes("ADMIN");
  const [connections, setConnections] = useState([]),
    [settings, setSettings] = useState([]),
    [form, setForm] = useState(empty),
    [editing, setEditing] = useState(null),
    [error, setError] = useState(""),
    [busy, setBusy] = useState(false),
    [tab, setTab] = useState("brokers"),
    [loading, setLoading] = useState(true);
  const [setting, setSetting] = useState({
    settingKey: "Trading.LiveEnabled",
    value: "false",
    environment: "GLOBAL",
    category: "Trading",
  });
  async function load() {
    const [brokers, runtime] = await Promise.all([
      api("/admin/broker-settings"),
      api("/admin/settings"),
    ]);
    setConnections(brokers);
    setSettings(runtime);
  }
  useEffect(() => {
    if (canWrite)
      load()
        .catch((e) => setError(e.message))
        .finally(() => setLoading(false));
  }, [canWrite]);
  async function act(operation) {
    if (busy) return;
    setBusy(true);
    setError("");
    try {
      await operation();
      await load();
    } catch (e) {
      setError(e.message);
    } finally {
      setBusy(false);
    }
  }
  function edit(item) {
    setEditing(item.brokerAccountId);
    setForm({
      ...empty,
      ...item,
      sessionExpiresAt: item.sessionExpiresAt
        ? new Date(
            new Date(item.sessionExpiresAt).getTime() -
              new Date().getTimezoneOffset() * 60000,
          )
            .toISOString()
            .slice(0, 16)
        : "",
    });
  }
  if (!canWrite)
    return (
      <section className="card">
        Administrator access is required to manage broker credentials.
      </section>
    );
  return (
    <>
      <h1>Settings</h1>
      <p className="muted">
        Manage broker connections and application settings from this workspace.
      </p>
      <div className="actions">
        <button
          className={`btn ${tab === "brokers" ? "" : "secondary"}`}
          onClick={() => setTab("brokers")}
        >
          Broker setup
        </button>
        <button
          className={`btn ${tab === "runtime" ? "" : "secondary"}`}
          onClick={() => setTab("runtime")}
        >
          Runtime settings
        </button>
        <button
          className="btn secondary"
          disabled={busy}
          onClick={() => act(async () => {})}
        >
          Refresh
        </button>
      </div>
      {error && (
        <p role="alert" className="error form-alert">
          {error}
        </p>
      )}
      {loading && <p role="status">Loading settings…</p>}
      {tab === "brokers" ? (
        <>
          <div className="plan-guide">
            Assessment sandbox and real trading have separate credentials.
            Saving a connection does not place trades.
          </div>
          <div className="connection-grid">
            {connections.map((item) => (
              <article className="card" key={item.brokerAccountId}>
                <span className="badge">
                  {item.environment === "SANDBOX"
                    ? "Assessment sandbox"
                    : "Real trading"}
                </span>
                <h2>{item.displayName || item.reference}</h2>
                <p>
                  {item.provider} · {item.isActive ? "Enabled" : "Disabled"}
                </p>
                <p className="muted">
                  {item.executionSupported
                    ? "Trading adapter available"
                    : "Configuration only — execution adapter pending"}
                </p>
                <dl className="metric-list">
                  <dt>Reference</dt>
                  <dd>{item.reference}</dd>
                  <dt>Credentials</dt>
                  <dd>
                    {item.hasCredentials ? "Stored securely" : "Not configured"}
                  </dd>
                  <dt>Session expires</dt>
                  <dd>
                    {item.sessionExpiresAt
                      ? new Date(item.sessionExpiresAt).toLocaleString()
                      : "Not specified"}
                  </dd>
                </dl>
                <div className="actions">
                  <button
                    className="btn secondary"
                    disabled={busy}
                    onClick={() => edit(item)}
                  >
                    Edit connection
                  </button>
                  {item.isActive &&
                    item.provider === "Upstox" &&
                    item.environment === "PRODUCTION" && (
                      <button
                        className="btn"
                        disabled={busy}
                        onClick={() =>
                          act(() =>
                            api(
                              `/admin/broker-settings/${item.brokerAccountId}/verify`,
                              {
                                method: "POST",
                                successMessage:
                                  "Broker identity verified. No order was placed.",
                              },
                            ),
                          )
                        }
                      >
                        Verify identity
                      </button>
                    )}
                </div>
              </article>
            ))}
          </div>
          <form
            className="card plan-form"
            onSubmit={(e) => {
              e.preventDefault();
              act(async () => {
                await api(
                  editing
                    ? `/admin/broker-settings/${editing}`
                    : "/admin/broker-settings",
                  {
                    method: editing ? "PUT" : "POST",
                    successMessage: "Broker configuration saved securely.",
                    body: JSON.stringify({
                      ...form,
                      sessionExpiresAt: form.sessionExpiresAt
                        ? new Date(form.sessionExpiresAt).toISOString()
                        : null,
                    }),
                  },
                );
                setForm(empty);
                setEditing(null);
              });
            }}
          >
            <h2>
              {editing ? "Edit broker connection" : "Add broker connection"}
            </h2>
            <fieldset disabled={busy}>
              <legend>Connection details</legend>
              <label>
                Connection name
                <input
                  required
                  maxLength={100}
                  value={form.displayName}
                  onChange={(e) =>
                    setForm({ ...form, displayName: e.target.value })
                  }
                />
              </label>
              <label>
                Broker
                <select
                  disabled={Boolean(editing)}
                  value={form.provider}
                  onChange={(e) =>
                    setForm({
                      ...form,
                      provider: e.target.value,
                      isActive: false,
                    })
                  }
                >
                  <option value="Upstox">Upstox</option>
                  <option value="AngelOne">Angel One (setup only)</option>
                  <option value="Groww">Groww (setup only)</option>
                </select>
              </label>
              <label>
                Trading environment
                <select
                  disabled={Boolean(editing)}
                  value={form.environment}
                  onChange={(e) =>
                    setForm({
                      ...form,
                      environment: e.target.value,
                      useForMarketData: false,
                    })
                  }
                >
                  <option value="SANDBOX">Assessment / sandbox</option>
                  <option value="PRODUCTION">Real trading / production</option>
                </select>
              </label>
              <label>
                Credential reference
                <input
                  required
                  disabled={Boolean(editing)}
                  maxLength={100}
                  pattern="[A-Za-z0-9_-]+"
                  placeholder="e.g. corporate-live"
                  value={form.reference}
                  onChange={(e) =>
                    setForm({ ...form, reference: e.target.value })
                  }
                />
                <small className="field-help">
                  Used to link funded accounts to this connection. Cannot be
                  changed later.
                </small>
              </label>
              <label>
                Broker user / client ID
                <input
                  maxLength={100}
                  value={form.brokerUserId}
                  onChange={(e) =>
                    setForm({ ...form, brokerUserId: e.target.value })
                  }
                />
              </label>
              <label>
                Session expiry (your local time)
                <input
                  type="datetime-local"
                  value={form.sessionExpiresAt}
                  onChange={(e) =>
                    setForm({ ...form, sessionExpiresAt: e.target.value })
                  }
                />
              </label>
            </fieldset>
            <fieldset disabled={busy}>
              <legend>Encrypted credentials</legend>
              {[
                ["apiKey", "API key"],
                ["apiSecret", "API secret"],
                ["accessToken", "Access / session token"],
                ["refreshToken", "Refresh token"],
              ].map(([key, label]) => (
                <label key={key}>
                  {label}
                  <input
                    type="password"
                    autoComplete="new-password"
                    maxLength={8000}
                    value={form[key]}
                    placeholder={
                      editing
                        ? "Leave blank to keep stored value"
                        : "Enter credential"
                    }
                    onChange={(e) =>
                      setForm({ ...form, [key]: e.target.value })
                    }
                  />
                </label>
              ))}
            </fieldset>
            <fieldset disabled={busy}>
              <legend>Connection use</legend>
              <label>
                <input
                  type="checkbox"
                  disabled={form.provider !== "Upstox"}
                  checked={form.isActive}
                  onChange={(e) =>
                    setForm({ ...form, isActive: e.target.checked })
                  }
                />
                Enable connection
              </label>
              <label>
                <input
                  type="checkbox"
                  checked={form.isDefault}
                  onChange={(e) =>
                    setForm({ ...form, isDefault: e.target.checked })
                  }
                />
                Default for this trading environment
              </label>
              <label>
                <input
                  type="checkbox"
                  disabled={form.environment !== "PRODUCTION"}
                  checked={form.useForMarketData}
                  onChange={(e) =>
                    setForm({ ...form, useForMarketData: e.target.checked })
                  }
                />
                Use for market quotes (including paper assessments)
              </label>
            </fieldset>
            <p className="muted">
              Paper assessments use the internal simulation engine. Sandbox
              credentials are isolated from real-order credentials. Angel One
              and Groww configurations can be saved but remain disabled until
              their execution and reconciliation adapters are implemented.
            </p>
            <div className="form-footer">
              <button
                type="button"
                className="btn secondary"
                disabled={busy}
                onClick={() => {
                  setForm(empty);
                  setEditing(null);
                }}
              >
                Cancel / clear
              </button>
              <button className="btn" disabled={busy}>
                {busy ? "Saving…" : "Save connection"}
              </button>
            </div>
          </form>
        </>
      ) : (
        <>
          <form
            className="card plan-form"
            onSubmit={(e) => {
              e.preventDefault();
              act(() =>
                api(
                  `/admin/settings/${encodeURIComponent(setting.settingKey)}`,
                  { method: "PUT", body: JSON.stringify(setting) },
                ),
              );
            }}
          >
            <h2>Edit runtime setting</h2>
            <fieldset disabled={busy}>
              <legend>Application configuration</legend>
              {["settingKey", "value", "environment", "category"].map((key) => (
                <label key={key}>
                  {
                    {
                      settingKey: "Setting key",
                      value: "Value",
                      environment: "Scope",
                      category: "Category",
                    }[key]
                  }
                  <input
                    required
                    value={setting[key]}
                    onChange={(e) =>
                      setSetting({ ...setting, [key]: e.target.value })
                    }
                  />
                </label>
              ))}
            </fieldset>
            <p className="muted">
              Use GLOBAL for trading settings. Broker secrets belong in Broker
              setup. Infrastructure keys and authentication secrets remain
              deployment configuration.
            </p>
            <button className="btn" disabled={busy}>
              Save setting
            </button>
          </form>
          <section className="card table-scroll">
            <table>
              <thead>
                <tr>
                  <th>Setting</th>
                  <th>Value</th>
                  <th>Scope</th>
                  <th>Action</th>
                </tr>
              </thead>
              <tbody>
                {settings.map((item) => (
                  <tr key={item.settingId}>
                    <td>{item.settingKey}</td>
                    <td>{item.value}</td>
                    <td>{item.environment}</td>
                    <td>
                      <button
                        className="btn secondary"
                        disabled={item.value === "********"}
                        onClick={() => setSetting({ ...item })}
                      >
                        Edit
                      </button>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </section>
        </>
      )}
    </>
  );
}
