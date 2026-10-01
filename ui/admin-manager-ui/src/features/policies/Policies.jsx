import React, { useEffect, useState } from "react";
import { Link } from "react-router-dom";
import { api } from "../../api";
import { useAuth } from "../../store";
const initial = {
  code: "",
  name: "",
  maximumOrderValue: 100000,
  entryStartTime: "",
  entryEndTime: "",
};
export default function Policies() {
  const user = useAuth((s) => s.user);
  const canWrite =
    user.roles.includes("ADMIN") ||
    user.permissions?.includes("policies.write");
  const [form, setForm] = useState(initial),
    [policies, setPolicies] = useState([]),
    [details, setDetails] = useState(null),
    [selected, setSelected] = useState("");
  const [error, setError] = useState(""),
    [busy, setBusy] = useState(false),
    [loading, setLoading] = useState(true);
  useEffect(() => {
    let active = true;
    api("/admin/policies")
      .then((data) => {
        if (active) setPolicies(data);
      })
      .catch((e) => {
        if (active) setError(e.message);
      })
      .finally(() => {
        if (active) setLoading(false);
      });
    return () => {
      active = false;
    };
  }, []);
  useEffect(() => {
    let active = true;
    setDetails(null);
    if (selected)
      api(`/admin/policies/${selected}`)
        .then((data) => {
          if (active) setDetails(data);
        })
        .catch((e) => {
          if (active) setError(e.message);
        });
    return () => {
      active = false;
    };
  }, [selected]);
  function update(key, value) {
    setForm((old) => ({ ...old, [key]: value }));
  }
  async function create(e) {
    e.preventDefault();
    if (busy) return;
    setBusy(true);
    setError("");
    try {
      const result = await api("/admin/policies/", {
        method: "POST",
        successMessage:
          "Risk policy created. You can now select it in an assessment plan.",
        body: JSON.stringify({
          ...form,
          maximumOrderValue: Number(form.maximumOrderValue),
          entryStartTime: form.entryStartTime || null,
          entryEndTime: form.entryEndTime || null,
        }),
      });
      setPolicies(await api("/admin/policies"));
      setSelected(result.policyId);
      setForm(initial);
    } catch (e) {
      setError(e.message);
    } finally {
      setBusy(false);
    }
  }
  return (
    <>
      <div className="top">
        <div>
          <h1>Risk policies</h1>
          <p className="muted">
            A policy group contains the rules checked when a trader places an
            order.
          </p>
        </div>
        <Link className="btn secondary" to="/plans">
          Assessment plans
        </Link>
      </div>
      <div className="plan-guide">
        Create a policy group → Add its rules → Select it for each assessment
        stage
      </div>
      {error && (
        <p className="error form-alert" role="alert">
          {error}
        </p>
      )}
      {canWrite && (
        <form className="card plan-form" onSubmit={create}>
          <h2>Create policy group</h2>
          <fieldset disabled={busy}>
            <legend>Group details</legend>
            <label>
              Policy name
              <input
                className="field"
                required
                maxLength={150}
                placeholder="e.g. Equity assessment – standard risk"
                value={form.name}
                onChange={(e) => update("name", e.target.value)}
              />
            </label>
            <label>
              Internal code
              <input
                className="field"
                required
                maxLength={50}
                placeholder="e.g. EQUITY-STANDARD"
                value={form.code}
                onChange={(e) => update("code", e.target.value)}
              />
              <small className="field-help">
                A unique reference saved in uppercase.
              </small>
            </label>
          </fieldset>
          <fieldset disabled={busy}>
            <legend>Rules in this group</legend>
            <div>
              <strong>1. Equity instruments only</strong>
              <p className="field-help">
                Required for this equity-only application. Instrument
                eligibility is also checked by the trading service.
              </p>
              <span className="badge">Always enabled</span>
            </div>
            <label>
              2. Maximum value per order (INR)
              <input
                className="field"
                type="number"
                min="0.01"
                step="0.01"
                required
                value={form.maximumOrderValue}
                onChange={(e) => update("maximumOrderValue", e.target.value)}
              />
              <small className="field-help">
                Reject an order when quantity × estimated price exceeds this
                amount. Equality is allowed. This is not a daily turnover limit.
              </small>
            </label>
            <div className="full-width">
              <strong>
                3. Order entry window{" "}
                <small className="optional-label">Optional</small>
              </strong>
              <p className="field-help">
                Both times use India Standard Time. Leave both blank to use
                exchange hours only. New buy orders are allowed from the start
                time up to, but not including, the end time. Sell orders remain
                allowed for exits. Exchange holidays and market hours still
                apply.
              </p>
            </div>
            <label>
              Entry starts (IST)
              <input
                className="field"
                type="time"
                step="60"
                required={Boolean(form.entryEndTime)}
                value={form.entryStartTime}
                onChange={(e) => update("entryStartTime", e.target.value)}
              />
            </label>
            <label>
              Entry ends (IST)
              <input
                className="field"
                type="time"
                step="60"
                required={Boolean(form.entryStartTime)}
                value={form.entryEndTime}
                onChange={(e) => update("entryEndTime", e.target.value)}
              />
            </label>
          </fieldset>
          <div className="form-footer">
            <span className="muted">
              Creates an active policy for new account assignments. Existing
              account rules stay unchanged.
            </span>
            <button className="btn" disabled={busy}>
              {busy ? "Creating…" : "Create policy group"}
            </button>
          </div>
        </form>
      )}
      <section className="card">
        <h2>Policy groups and rules</h2>
        {loading ? (
          <p role="status">Loading policy groups…</p>
        ) : (
          <label>
            Choose a policy group
            <select
              className="field"
              value={selected}
              onChange={(e) => setSelected(e.target.value)}
            >
              <option value="">Select a group to view its rules</option>
              {policies.map((p) => (
                <option key={p.policyId} value={p.policyId}>
                  {p.name} ({p.code})
                </option>
              ))}
            </select>
          </label>
        )}
        {!loading && !policies.length && (
          <p>No policy groups yet. Create one above.</p>
        )}
        {selected && !details && !error && <p role="status">Loading rules…</p>}
        {details?.versions.map((version) => (
          <article className="plan-version" key={version.policyVersionId}>
            <div className="top">
              <h3>
                {details.name} · Version {version.versionNumber}
              </h3>
              <span className="badge">{version.status}</span>
            </div>
            <div className="table-scroll">
              <table>
                <thead>
                  <tr>
                    <th>Rule</th>
                    <th>Condition</th>
                    <th>Action</th>
                    <th>Status</th>
                  </tr>
                </thead>
                <tbody>
                  {version.rules.map((rule) => (
                    <tr key={rule.ruleId}>
                      <td>{rule.name}</td>
                      <td>
                        {rule.ruleCode === "MAX_ORDER_VALUE"
                          ? `Above ${new Intl.NumberFormat("en-IN", { style: "currency", currency: "INR" }).format(rule.decimalValue)}`
                          : rule.ruleCode === "ENTRY_TIME_WINDOW"
                            ? `${rule.stringValue?.replace("|", " to ")} IST (buy orders)`
                            : rule.ruleCode === "EQUITY_ONLY"
                              ? "Equities only"
                              : rule.ruleCode}
                      </td>
                      <td>
                        {rule.violationAction === "RejectOrder"
                          ? "Reject order"
                          : rule.violationAction}
                      </td>
                      <td>{rule.isEnabled ? "Enabled" : "Disabled"}</td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          </article>
        ))}
      </section>
      <p className="muted">
        Profit targets and daily/total loss thresholds are configured in the
        assessment plan. This page manages order-level rules. Entry windows do
        not cancel previously accepted orders.
      </p>
    </>
  );
}
