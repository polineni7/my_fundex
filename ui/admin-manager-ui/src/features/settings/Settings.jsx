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
    [loading, setLoading] = useState(true),
    [purpose, setPurpose] = useState("SANDBOX"),
    [showForm, setShowForm] = useState(false),
    [archive, setArchive] = useState(null);
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
    setShowForm(true);
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
          Trading controls
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
      {archive && (
        <section
          className="card"
          role="alertdialog"
          aria-label="Archive broker"
        >
          <h2>Archive {archive.displayName}?</h2>
          <p>
            This removes its saved credentials. Connections referenced by
            trading accounts cannot be archived.
          </p>
          <button
            className="btn"
            disabled={busy}
            onClick={() =>
              act(async () => {
                await api(
                  `/admin/broker-settings/${archive.brokerAccountId}?version=${archive.version}`,
                  { method: "DELETE" },
                );
                setArchive(null);
              })
            }
          >
            Archive connection
          </button>
          <button className="btn secondary" onClick={() => setArchive(null)}>
            Cancel
          </button>
        </section>
      )}

      {tab === "brokers" ? (
        <>
          <div className="plan-guide">
            Assessment sandbox and real trading have separate credentials.
            Saving a connection does not place trades.
          </div>
          <section className="card">
            <div className="top">
              <div>
                <h2>Broker connections</h2>
                <p className="muted">
                  Choose a purpose, then add or manage its connections.
                </p>
              </div>
              <button
                className="btn"
                onClick={() => {
                  setEditing(null);
                  setForm({ ...empty, environment: purpose });
                  setShowForm(true);
                }}
              >
                + Add connection
              </button>
            </div>
            <label>
              Trading purpose
              <select
                value={purpose}
                onChange={(e) => {
                  setPurpose(e.target.value);
                  setShowForm(false);
                }}
              >
                <option value="SANDBOX">Assessment — practice trading</option>
                <option value="PRODUCTION">Funded — real trading</option>
              </select>
            </label>
            <p className="muted">
              {purpose === "SANDBOX"
                ? "Assessment orders use the paper engine. Live prices come from the real-trading market-data connection."
                : "Real orders require a passed assessment, approved account and an enabled broker connection."}
            </p>
            <div className="table-scroll">
              <table>
                <thead>
                  <tr>
                    <th>Name</th>
                    <th>Broker</th>
                    <th>Status</th>
                    <th>Default</th>
                    <th>Session expires</th>
                    <th>Actions</th>
                  </tr>
                </thead>
                <tbody>
                  {connections
                    .filter((x) => x.environment === purpose)
                    .map((item) => (
                      <tr key={item.brokerAccountId}>
                        <td>{item.displayName || item.reference}</td>
                        <td>{item.provider}</td>
                        <td>{item.isActive ? "Enabled" : "Disabled"}</td>
                        <td>{item.isDefault ? "Yes" : "No"}</td>
                        <td>
                          {item.sessionExpiresAt
                            ? new Date(item.sessionExpiresAt).toLocaleString()
                            : "Not set"}
                        </td>
                        <td>
                          <button
                            className="btn secondary"
                            disabled={busy}
                            onClick={() => edit(item)}
                          >
                            View / edit
                          </button>
                          {!item.isActive && (
                            <button
                              className="btn secondary"
                              disabled={busy}
                              onClick={() => setArchive(item)}
                            >
                              Archive
                            </button>
                          )}
                          {item.isActive &&
                            item.provider === "Upstox" &&
                            purpose === "PRODUCTION" && (
                              <button
                                className="btn secondary"
                                disabled={busy}
                                onClick={() =>
                                  act(() =>
                                    api(
                                      `/admin/broker-settings/${item.brokerAccountId}/verify`,
                                      {
                                        method: "POST",
                                        successMessage:
                                          "Broker identity verified. No order placed.",
                                      },
                                    ),
                                  )
                                }
                              >
                                Verify
                              </button>
                            )}
                        </td>
                      </tr>
                    ))}
                  {!connections.some((x) => x.environment === purpose) && (
                    <tr>
                      <td colSpan={6}>
                        No connections for this purpose. Select Add connection
                        to configure one.
                      </td>
                    </tr>
                  )}
                </tbody>
              </table>
            </div>
          </section>
          {showForm && (
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
                  setShowForm(false);
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
                    <option value="PRODUCTION">
                      Real trading / production
                    </option>
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
                    setShowForm(false);
                  }}
                >
                  Cancel / clear
                </button>
                <button className="btn" disabled={busy}>
                  {busy ? "Saving…" : "Save connection"}
                </button>
              </div>
            </form>
          )}
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
              <label>
                Control
                <select
                  value={setting.settingKey}
                  onChange={() =>
                    setSetting({
                      settingKey: "Trading.LiveEnabled",
                      value: "false",
                      environment: "GLOBAL",
                      category: "Trading",
                    })
                  }
                >
                  <option value="Trading.LiveEnabled">
                    Allow real trading
                  </option>
                </select>
              </label>
              <label>
                State
                <select
                  value={setting.value}
                  onChange={(e) =>
                    setSetting({ ...setting, value: e.target.value })
                  }
                >
                  <option value="false">Disabled</option>
                  <option value="true">Enabled</option>
                </select>
              </label>
              <label>
                Applies to
                <select value="GLOBAL" disabled>
                  <option value="GLOBAL">All eligible funded accounts</option>
                </select>
              </label>
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
                        disabled={
                          item.settingKey !== "Trading.LiveEnabled" ||
                          item.environment !== "GLOBAL"
                        }
                        onClick={() =>
                          setSetting({
                            ...item,
                            value: item.value === "true" ? "true" : "false",
                          })
                        }
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
