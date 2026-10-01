import React, { useEffect, useState } from "react";
import { Link } from "react-router-dom";
import { api } from "../../api";
const money = (value) =>
  new Intl.NumberFormat("en-IN", {
    style: "currency",
    currency: "INR",
    maximumFractionDigits: 2,
  }).format(value);
export default function Challenges() {
  const [rows, setRows] = useState([]),
    [error, setError] = useState("");
  useEffect(() => {
    let active = true;
    async function refresh() {
      try {
        const data = await api("/challenges");
        if (active) {
          setRows(data);
          setError("");
        }
      } catch (e) {
        if (active) setError(e.message);
      }
    }
    refresh();
    const timer = setInterval(refresh, 10000);
    return () => {
      active = false;
      clearInterval(timer);
    };
  }, []);
  return (
    <section>
      <p className="eyebrow">YOUR FUNDED JOURNEY</p>
      <h1>Evaluation progress</h1>
      <p>
        Paper fills are simulated using market quotes. Each stage starts with a
        fresh allocation.
      </p>
      {error && <p role="alert">{error}</p>}
      {!rows.length && (
        <div className="card">
          <p>Your evaluation appears after a verified plan purchase.</p>
          <Link to="/plans">Explore plans</Link>
        </div>
      )}
      <div className="grid">
        {rows.map(({ attempt: a, progress: p }) => {
          const profit = p?.realizedProfit ?? 0,
            target = (a.startingCapital * a.profitTargetPercent) / 100;
          return (
            <article className="card" key={a.attemptId}>
              <span className="eyebrow">
                PAPER · STAGE {a.stageNumber} · {p?.status ?? a.status}
              </span>
              <h2>{a.planName}</h2>
              <label>
                Realized profit target{" "}
                <progress
                  max={target}
                  value={Math.max(0, Math.min(target, profit))}
                />
              </label>
              <p>
                {money(profit)} / {money(target)}
              </p>
              <dl className="metric-list">
                <dt>Equity</dt>
                <dd>{money(p?.equity ?? a.startingCapital)}</dd>
                <dt>Available funds</dt>
                <dd>{money(p?.cash ?? a.startingCapital)}</dd>
                <dt>Trading days</dt>
                <dd>
                  {p?.tradingDays ?? 0} / {a.minimumTradingDays}
                </dd>
                <dt>Daily loss limit</dt>
                <dd>{a.maxDailyLossPercent}%</dd>
                <dt>Total loss limit</dt>
                <dd>{a.maxTotalLossPercent}%</dd>
              </dl>
              {p?.valuedAt && (
                <small>
                  Last valuation: {new Date(p.valuedAt).toLocaleString()}
                </small>
              )}
              {(p?.status === "Failed" || a.status === "Failed") && (
                <p>
                  <Link to={`/plans?retry=${a.subscriptionId}`}>
                    Purchase a fresh attempt
                  </Link>
                </p>
              )}
              {a.status === "InProgress" && (
                <p>
                  <Link to="/trade">Open paper trading</Link>
                </p>
              )}
            </article>
          );
        })}
      </div>
    </section>
  );
}
