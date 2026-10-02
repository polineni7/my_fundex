import React, { useEffect, useState } from "react";
import { api } from "../../api";
const money = (value) =>
  new Intl.NumberFormat("en-IN", { style: "currency", currency: "INR" }).format(
    value,
  );
const csvCell = (value) =>
  `"${String(value ?? "")
    .replace(/^[=+@\-\t\r]/, "'$&")
    .replaceAll('"', '""')}"`;
export default function BusinessReport() {
  const [data, setData] = useState(null),
    [error, setError] = useState(""),
    [busy, setBusy] = useState(false),
    [from, setFrom] = useState(""),
    [to, setTo] = useState(""),
    [status, setStatus] = useState("All");
  async function load(start = "", end = "") {
    setBusy(true);
    setError("");
    try {
      if (start && end && start > end)
        throw new Error("Start date must be before end date.");
      const query = new URLSearchParams();
      if (start) query.set("from", `${start}T00:00:00+05:30`);
      if (end) {
        const next = new Date(`${end}T00:00:00+05:30`);
        next.setUTCDate(next.getUTCDate() + 1);
        query.set("to", next.toISOString());
      }
      setData(await api(`/admin/business-report?${query}`));
    } catch (e) {
      setError(e.message);
      setData(null);
    } finally {
      setBusy(false);
    }
  }
  useEffect(() => {
    load();
  }, []);
  const rows = (data?.traders || []).filter(
    (t) =>
      status === "All" ||
      (status === "Not enrolled"
        ? !t.assessments.length
        : t.assessments.some((a) => a.status === status)),
  );
  function download() {
    const lines = [
      [
        "User ID",
        "Email",
        "Registered",
        "Account status",
        "Assessment plan",
        "Assessment status",
      ],
    ];
    rows.forEach((t) => {
      (t.assessments.length ? t.assessments : [{}]).forEach((a) =>
        lines.push([
          t.userId,
          t.email,
          t.registeredAt,
          t.status,
          a.plan,
          a.status || "Not enrolled",
        ]),
      );
    });
    const url = URL.createObjectURL(
      new Blob(
        ["\ufeff" + lines.map((r) => r.map(csvCell).join(",")).join("\r\n")],
        { type: "text/csv;charset=utf-8" },
      ),
    );
    const a = document.createElement("a");
    a.href = url;
    a.download = "fundex-traders.csv";
    a.click();
    setTimeout(() => URL.revokeObjectURL(url), 1000);
  }
  return (
    <>
      <h1>Business reports</h1>
      <p className="muted">
        Registrations, assessment progress and collected fees. Figures reflect
        recorded activity.
      </p>
      <form
        className="card"
        onSubmit={(e) => {
          e.preventDefault();
          load(from, to);
        }}
      >
        <div className="actions">
          <label>
            From (India time)
            <input
              type="date"
              value={from}
              onChange={(e) => setFrom(e.target.value)}
            />
          </label>
          <label>
            Through
            <input
              type="date"
              value={to}
              onChange={(e) => setTo(e.target.value)}
            />
          </label>
          <button className="btn" disabled={busy}>
            Apply dates
          </button>
        </div>
      </form>
      {error && (
        <p role="alert" className="error">
          {error}
        </p>
      )}
      {busy && <p role="status">Loading report…</p>}
      {data && (
        <>
          <div className="connection-grid">
            <section className="card">
              <h2>Registered traders</h2>
              <strong>{data.registrations}</strong>
            </section>
            <section className="card">
              <h2>Collected fees</h2>
              <strong>{money(data.collectedFees)}</strong>
              <p className="muted">
                Paid INR transactions in this period; not net profit or bank
                balance.
              </p>
            </section>
          </div>
          <section className="card">
            <h2>Assessment enrollments</h2>
            <div className="actions">
              {data.statusCounts.map((s) => (
                <span className="badge" key={s.status}>
                  {s.status}: {s.count}
                </span>
              ))}
              {!data.statusCounts.length && (
                <p>No enrollments in this period.</p>
              )}
            </div>
          </section>
          <section className="card">
            <h2>Current allocated capital</h2>
            <p className="muted">
              Active account allocations across all dates. Virtual capital is
              not company cash.
            </p>
            {data.capital.map((c) => (
              <p key={c.mode}>
                {c.mode}: {money(c.amount)} across {c.accounts} accounts
              </p>
            ))}
            {!data.capital.length && <p>No active allocations.</p>}
          </section>
          <section className="card">
            <div className="top">
              <h2>Trader directory</h2>
              <button
                className="btn secondary"
                onClick={download}
                disabled={!rows.length || busy}
              >
                Export displayed traders
              </button>
            </div>
            <label>
              Assessment status
              <select
                value={status}
                onChange={(e) => setStatus(e.target.value)}
              >
                {[
                  "All",
                  "Not enrolled",
                  "PendingPayment",
                  "Evaluation",
                  "Completed",
                  "Failed",
                  "Expired",
                ].map((s) => (
                  <option key={s}>{s}</option>
                ))}
              </select>
            </label>
            <p className="muted">
              Latest {data.limit} registrations in the selected period.
              Assessment history includes all dates. Export includes only
              displayed traders.
            </p>
            <div className="table-scroll">
              <table>
                <thead>
                  <tr>
                    <th>Trader</th>
                    <th>Registered</th>
                    <th>Status</th>
                    <th>Plan and assessment history</th>
                  </tr>
                </thead>
                <tbody>
                  {rows.map((t) => (
                    <tr key={t.userId}>
                      <td>{t.email}</td>
                      <td>
                        {new Date(t.registeredAt).toLocaleDateString("en-IN")}
                      </td>
                      <td>{t.status}</td>
                      <td>
                        {t.assessments.map((a) => (
                          <p key={a.subscriptionId}>
                            {a.plan} · {a.status}
                          </p>
                        ))}
                        {!t.assessments.length && "Not enrolled"}
                      </td>
                    </tr>
                  ))}
                  {!rows.length && (
                    <tr>
                      <td colSpan={4}>No traders match these filters.</td>
                    </tr>
                  )}
                </tbody>
              </table>
            </div>
          </section>
        </>
      )}
    </>
  );
}
