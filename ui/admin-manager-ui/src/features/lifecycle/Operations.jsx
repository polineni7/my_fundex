import { Link } from "react-router-dom";
import React, { useEffect, useRef, useState } from "react";
import { api } from "../../api";
import { useAuth } from "../../store";
export default function Operations() {
  const user = useAuth((s) => s.user);
  const [brokerConnections, setBrokerConnections] = useState([]);
  const [loadErrors, setLoadErrors] = useState([]);
  const [subscriptions, setSubscriptions] = useState([]),
    [withdrawals, setWithdrawals] = useState([]),
    [accounts, setAccounts] = useState([]),
    [orders, setOrders] = useState([]);
  const [message, setMessage] = useState(""),
    [busy, setBusy] = useState(false),
    [tab, setTab] = useState("Graduation");
  const [live, setLive] = useState({
    subscriptionId: "",
    provider: "Upstox",
    credentialKey: "",
    brokerUserId: "",
  });
  const [beneficiaries, setBeneficiaries] = useState({}),
    [close, setClose] = useState({ accountId: "", reason: "" });
  const [settlement, setSettlement] = useState({
    accountId: "",
    subscriptionId: "",
    verifiedFees: "0",
    brokerSettlementReference: "",
  });
  const [calendar, setCalendar] = useState({
    exchange: "NSE",
    date: "",
    isTradingDay: true,
    open: "09:15:00",
    close: "15:30:00",
  });
  const reference = useRef(null);
  async function refresh() {
    const requests = [
      ["Assessments", "/admin/lifecycle/subscriptions", setSubscriptions],
      ["Withdrawals", "/admin/lifecycle/withdrawals", setWithdrawals],
      ["Accounts", "/admin/accounts", setAccounts],
      ["Orders", "/admin/lifecycle/orders", setOrders],
      ["Broker settings", "/admin/broker-settings", setBrokerConnections],
    ];
    const results = await Promise.allSettled(
      requests.map(async ([, path, setter]) => {
        setter(await api(path));
      }),
    );
    setLoadErrors(
      results.flatMap((result, i) =>
        result.status === "rejected"
          ? [`${requests[i][0]}: ${result.reason.message}`]
          : [],
      ),
    );
  }
  useEffect(() => {
    if (user?.roles?.includes("ADMIN"))
      refresh().catch((e) => setMessage(e.message));
  }, [user]);
  async function perform(path, body, method = "POST") {
    setBusy(true);
    setMessage("");
    try {
      await api(`/admin/lifecycle${path}`, {
        method,
        body: JSON.stringify(body),
      });
      setMessage(
        "Request recorded successfully. Refresh to see processing status.",
      );
      await refresh();
    } catch (e) {
      setMessage(e.message);
    } finally {
      setBusy(false);
    }
  }
  if (!user?.roles?.includes("ADMIN"))
    return (
      <section>
        <h1>Financial operations</h1>
        <p>Administrator access is required.</p>
      </section>
    );
  const funded = accounts.filter((a) => a.tradingMode === "Funded");
  function accountSelect(value, change) {
    return (
      <select
        className="field"
        required
        value={value}
        onChange={(e) => change(e.target.value)}
      >
        <option value="">Select real account</option>
        {funded.map((a) => (
          <option value={a.accountId} key={a.accountId}>
            {a.accountNumber} · {a.status}
          </option>
        ))}
      </select>
    );
  }
  return (
    <section>
      <p className="eyebrow">FUNDED ACCOUNT OPERATIONS</p>
      <h1>Lifecycle management</h1>
      <div className="actions">
        {[
          "Graduation",
          "Withdrawals",
          "Settlement",
          "Orders",
          "Calendar",
          "Accounts",
        ].map((t) => (
          <button
            className={`btn ${tab === t ? "" : "secondary"}`}
            key={t}
            onClick={() => setTab(t)}
          >
            {t}
          </button>
        ))}
        <button
          className="btn secondary"
          onClick={() => refresh().catch((e) => setMessage(e.message))}
        >
          Refresh
        </button>
      </div>
      {loadErrors.map((error) => (
        <p className="error" role="alert" key={error}>
          {error}
        </p>
      ))}
      {message && <p role="status">{message}</p>}
      {!accounts.length && (
        <p className="muted">
          No funded accounts yet. Accounts appear after an assessment purchase
          is fulfilled.
        </p>
      )}
      {tab === "Withdrawals" && !withdrawals.length && (
        <p>No withdrawal requests to review.</p>
      )}
      {tab === "Orders" && !orders.length && <p>No orders to reconcile.</p>}
      {tab === "Accounts" && (
        <div className="grid">
          {accounts.map((a) => (
            <article className="card" key={a.accountId}>
              <span className="eyebrow">
                {a.tradingMode} · {a.status}
              </span>
              <h2>{a.accountNumber}</h2>
              <p>Available: INR {a.currentBuyingPower}</p>
              <label>
                Review reason
                <input
                  className="field"
                  value={beneficiaries[a.accountId] ?? ""}
                  onChange={(e) =>
                    setBeneficiaries({
                      ...beneficiaries,
                      [a.accountId]: e.target.value,
                    })
                  }
                />
              </label>
              {["Active", "Suspended"].includes(a.status) && (
                <button
                  className="btn"
                  disabled={busy || !beneficiaries[a.accountId]}
                  onClick={() =>
                    perform(`/accounts/${a.accountId}/suspension`, {
                      suspend: a.status === "Active",
                      version: a.version,
                      reason: beneficiaries[a.accountId],
                    })
                  }
                >
                  {a.status === "Active"
                    ? "Suspend new trading"
                    : "Resume trading"}
                </button>
              )}
            </article>
          ))}
        </div>
      )}
      {tab === "Graduation" && (
        <form
          className="card"
          onSubmit={(e) => {
            e.preventDefault();
            perform("/activate-live", live);
          }}
        >
          <h2>Activate a real account</h2>
          <p>
            Every evaluation stage must be passed. The broker identity is
            verified before allocation.
          </p>
          <label>
            Completed assessment
            <select
              className="field"
              required
              value={live.subscriptionId}
              onChange={(e) =>
                setLive({ ...live, subscriptionId: e.target.value })
              }
            >
              <option value="">Choose a completed evaluation</option>
              {subscriptions
                .filter((s) => s.status === "Completed")
                .map((s) => (
                  <option key={s.subscriptionId} value={s.subscriptionId}>
                    Trader {s.userInternalId} · {s.subscriptionId}
                  </option>
                ))}
            </select>
          </label>
          <label>
            Configured real-trading connection
            <select
              className="field"
              required
              value={live.credentialKey}
              onChange={(e) => {
                const item = brokerConnections.find(
                  (x) =>
                    x.reference === e.target.value &&
                    x.environment === "PRODUCTION",
                );
                setLive({
                  ...live,
                  credentialKey: item?.reference || "",
                  provider: item?.provider || "",
                  brokerUserId: item?.brokerUserId || "",
                });
              }}
            >
              <option value="">Select a configured broker connection</option>
              {brokerConnections
                .filter(
                  (x) =>
                    x.isActive &&
                    x.executionSupported &&
                    x.environment === "PRODUCTION",
                )
                .map((x) => (
                  <option key={x.brokerAccountId} value={x.reference}>
                    {x.displayName} ({x.provider})
                  </option>
                ))}
            </select>
          </label>
          <Link to="/settings" state={{ from: "/operations" }}>
            Manage broker setup
          </Link>
          {!subscriptions.some((x) => x.status === "Completed") && (
            <p className="muted">
              No completed assessments are eligible for live allocation yet.
            </p>
          )}
          <button className="btn" disabled={busy}>
            Verify and allocate
          </button>
        </form>
      )}
      {tab === "Withdrawals" && (
        <div className="grid">
          {withdrawals.map((w) => (
            <article className="card" key={w.withdrawalId}>
              <span className="eyebrow">{w.status}</span>
              <h2>INR {w.requestedAmount}</h2>
              <p>Trader {w.userInternalId}</p>
              <small>{w.withdrawalId}</small>
              {w.status === "Processing" && (
                <>
                  <label>
                    Existing provider payout ID
                    <input
                      className="field"
                      value={beneficiaries[w.withdrawalId] ?? ""}
                      onChange={(e) =>
                        setBeneficiaries({
                          ...beneficiaries,
                          [w.withdrawalId]: e.target.value,
                        })
                      }
                    />
                  </label>
                  <button
                    className="btn secondary"
                    disabled={busy || !beneficiaries[w.withdrawalId]}
                    onClick={() =>
                      perform(`/withdrawals/${w.withdrawalId}/reconcile`, {
                        providerPayoutId: beneficiaries[w.withdrawalId],
                      })
                    }
                  >
                    Verify existing payout
                  </button>
                </>
              )}
              {w.status === "Requested" && (
                <>
                  <label>
                    Verified beneficiary reference
                    <input
                      className="field"
                      placeholder="fa_..."
                      value={beneficiaries[w.withdrawalId] ?? ""}
                      onChange={(e) =>
                        setBeneficiaries({
                          ...beneficiaries,
                          [w.withdrawalId]: e.target.value,
                        })
                      }
                    />
                  </label>
                  <div className="actions">
                    <button
                      className="btn"
                      disabled={busy || !beneficiaries[w.withdrawalId]}
                      onClick={() =>
                        perform(`/withdrawals/${w.withdrawalId}/review`, {
                          approve: true,
                          beneficiaryReference: beneficiaries[w.withdrawalId],
                        })
                      }
                    >
                      Approve payout
                    </button>
                    <button
                      className="btn secondary"
                      disabled={busy}
                      onClick={() =>
                        perform(`/withdrawals/${w.withdrawalId}/review`, {
                          approve: false,
                        })
                      }
                    >
                      Reject and release
                    </button>
                  </div>
                </>
              )}
            </article>
          ))}
        </div>
      )}
      {tab === "Settlement" && (
        <form
          className="card"
          onSubmit={(e) => {
            e.preventDefault();
            const key = JSON.stringify(settlement);
            if (reference.current?.key !== key)
              reference.current = { key, id: crypto.randomUUID() };
            perform("/settlements", {
              ...settlement,
              verifiedFees: Number(settlement.verifiedFees),
              requestId: reference.current.id,
            });
          }}
        >
          <h2>Confirm broker settlement</h2>
          <p>
            Confirm broker funds have settled and all charges are included. Open
            positions and unresolved orders block reward distribution.
          </p>
          <label>
            Real account
            {accountSelect(settlement.accountId, (value) =>
              setSettlement({ ...settlement, accountId: value }),
            )}
          </label>
          <label>
            Completed assessment
            <select
              className="field"
              required
              value={settlement.subscriptionId}
              onChange={(e) =>
                setSettlement({ ...settlement, subscriptionId: e.target.value })
              }
            >
              <option value="">Select matching assessment</option>
              {subscriptions
                .filter((s) => s.status === "Completed")
                .map((s) => (
                  <option key={s.subscriptionId} value={s.subscriptionId}>
                    {s.subscriptionId}
                  </option>
                ))}
            </select>
          </label>
          <label>
            Verified broker charges (INR)
            <input
              className="field"
              required
              type="number"
              min="0"
              step="0.01"
              value={settlement.verifiedFees}
              onChange={(e) =>
                setSettlement({ ...settlement, verifiedFees: e.target.value })
              }
            />
          </label>
          <label>
            Broker settlement reference
            <input
              className="field"
              required
              value={settlement.brokerSettlementReference}
              onChange={(e) =>
                setSettlement({
                  ...settlement,
                  brokerSettlementReference: e.target.value,
                })
              }
            />
          </label>
          <button className="btn" disabled={busy}>
            Confirm and distribute reward
          </button>
        </form>
      )}
      {tab === "Orders" && (
        <>
          <form
            className="card"
            onSubmit={(e) => {
              e.preventDefault();
              perform(`/accounts/${close.accountId}/square-off`, {
                reason: close.reason,
              });
            }}
          >
            <h2>Close real-account exposure</h2>
            <p>
              This blocks new exposure, requests cancellation of pending orders,
              and submits closing limit orders. Broker fills determine
              completion.
            </p>
            <label>
              Account
              {accountSelect(close.accountId, (value) =>
                setClose({ ...close, accountId: value }),
              )}
            </label>
            <label>
              Reason
              <input
                className="field"
                required
                value={close.reason}
                onChange={(e) => setClose({ ...close, reason: e.target.value })}
              />
            </label>
            <button className="btn" disabled={busy}>
              Request square-off
            </button>
          </form>
          <div className="card table-scroll">
            <table className="table">
              <thead>
                <tr>
                  <th>Stock</th>
                  <th>Mode</th>
                  <th>Side</th>
                  <th>Filled</th>
                  <th>Status</th>
                  <th>Action</th>
                </tr>
              </thead>
              <tbody>
                {orders.map((o) => (
                  <tr key={o.orderId}>
                    <td>{o.symbol}</td>
                    <td>{o.brokerEnvironment}</td>
                    <td>{o.side}</td>
                    <td>
                      {o.filledQuantity}/{o.quantity}
                    </td>
                    <td>{o.status}</td>
                    <td>
                      {o.brokerEnvironment === "PRODUCTION" && (
                        <button
                          className="btn secondary"
                          disabled={busy}
                          onClick={() =>
                            perform(`/orders/${o.orderId}/reconcile`, {})
                          }
                        >
                          Reconcile
                        </button>
                      )}
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </>
      )}
      {tab === "Calendar" && (
        <form
          className="card"
          onSubmit={(e) => {
            e.preventDefault();
            perform("/calendar", calendar, "PUT");
          }}
        >
          <h2>Exchange session</h2>
          <p>
            Use verified exchange calendar dates. All times are India Standard
            Time.
          </p>
          <label>
            Exchange
            <select
              className="field"
              value={calendar.exchange}
              onChange={(e) =>
                setCalendar({ ...calendar, exchange: e.target.value })
              }
            >
              <option>NSE</option>
              <option>BSE</option>
            </select>
          </label>
          <label>
            Date
            <input
              className="field"
              required
              type="date"
              value={calendar.date}
              onChange={(e) =>
                setCalendar({ ...calendar, date: e.target.value })
              }
            />
          </label>
          <label>
            <input
              type="checkbox"
              checked={calendar.isTradingDay}
              onChange={(e) =>
                setCalendar({ ...calendar, isTradingDay: e.target.checked })
              }
            />
            Trading day
          </label>
          <label>
            Session opens
            <input
              className="field"
              type="time"
              step="1"
              value={calendar.open}
              onChange={(e) =>
                setCalendar({
                  ...calendar,
                  open:
                    e.target.value.length === 5
                      ? e.target.value + ":00"
                      : e.target.value,
                })
              }
            />
          </label>
          <label>
            Session closes
            <input
              className="field"
              type="time"
              step="1"
              value={calendar.close}
              onChange={(e) =>
                setCalendar({
                  ...calendar,
                  close:
                    e.target.value.length === 5
                      ? e.target.value + ":00"
                      : e.target.value,
                })
              }
            />
          </label>
          <button className="btn" disabled={busy}>
            Save session
          </button>
        </form>
      )}
    </section>
  );
}
