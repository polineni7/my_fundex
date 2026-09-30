import React, { useRef, useState } from "react";
import { api } from "../../api";
import { useRecords } from "../../useRecords";
const money = (value) =>
  new Intl.NumberFormat("en-IN", {
    style: "currency",
    currency: "INR",
    maximumFractionDigits: 0,
  }).format(value);
export default function Plans() {
  const { rows, loading, error } = useRecords("/plan-catalogue");
  const [path, setPath] = useState("TwoStep");
  const purchaseRequests = useRef(new Map());
  const [busy, setBusy] = useState(false);
  const [message, setMessage] = useState("");
  async function subscribe(plan) {
    setBusy(true);
    setMessage("");
    try {
      if (!purchaseRequests.current.has(plan.planVersionId))
        purchaseRequests.current.set(plan.planVersionId, crypto.randomUUID());
      await api("/subscriptions", {
        method: "POST",
        body: JSON.stringify({
          planVersionId: plan.planVersionId,
          idempotencyKey: purchaseRequests.current.get(plan.planVersionId),
        }),
      });
      setMessage(
        "Your subscription is awaiting payment. See Subscriptions for its status.",
      );
    } catch (error) {
      setMessage(error.message);
    } finally {
      setBusy(false);
    }
  }
  const plans = rows.filter((plan) => plan.path === path);
  return (
    <section className="plan-page">
      <p className="eyebrow">YOUR PATH TO A FUNDED ACCOUNT</p>
      <h1>Choose your plan path</h1>
      <p>
        Build a verified track record in evaluation. Every stage has clear
        objectives and risk limits.
      </p>
      <div className="plan-tabs" role="group" aria-label="Evaluation path">
        {["TwoStep", "ThreeStep"].map((value) => (
          <button
            className={path === value ? "selected" : ""}
            aria-pressed={path === value}
            key={value}
            onClick={() => setPath(value)}
          >
            {value === "TwoStep" ? "Two-Step" : "Three-Step"}
          </button>
        ))}
      </div>
      {loading && <p role="status">Loading plans…</p>}
      {error && <p role="alert">{error}</p>}
      {message && <p role="status">{message}</p>}
      {!loading && !error && plans.length === 0 && (
        <div className="card">
          No published {path === "TwoStep" ? "two-step" : "three-step"} plans
          are available yet.
        </div>
      )}
      {plans.map((plan) => (
        <article className="plan-offer" key={plan.planVersionId}>
          <header>
            <div>
              <span>TRADING CAPITAL</span>
              <h2>{money(plan.challengeCapital)}</h2>
              <p>{plan.name}</p>
            </div>
            <div>
              <span>ENTRY FEE</span>
              <h3>{money(plan.registrationFee)}</h3>
            </div>
          </header>
          <div className="plan-rules">
            {plan.stages.map((stage) => (
              <div key={stage.stageNumber}>
                <span>STEP {stage.stageNumber} TARGET</span>
                <strong>{stage.profitTargetPercent}%</strong>
              </div>
            ))}
          </div>
          <div className="table-scroll">
            <table className="table">
              <caption>Trading rules by stage</caption>
              <thead>
                <tr>
                  <th>Objective</th>
                  {plan.stages.map((stage) => (
                    <th key={stage.stageNumber}>Stage {stage.stageNumber}</th>
                  ))}
                </tr>
              </thead>
              <tbody>
                {[
                  ["Daily loss limit", "maxDailyLossPercent", "%"],
                  ["Total loss limit", "maxTotalLossPercent", "%"],
                  ["Minimum trading days", "minimumTradingDays", " days"],
                  ["Trading period", "maximumCalendarDays", " days"],
                ].map(([label, field, suffix]) => (
                  <tr key={field}>
                    <th>{label}</th>
                    {plan.stages.map((stage) => (
                      <td key={stage.stageNumber}>
                        {stage[field] === null
                          ? "Unlimited"
                          : `${stage[field]}${suffix}`}
                      </td>
                    ))}
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
          <footer>
            <div>
              <span>FUNDED REWARD SHARE</span>
              <strong>{plan.rewardSharePercent}%</strong>
            </div>
            <button
              className="btn"
              disabled={busy}
              onClick={() => subscribe(plan)}
            >
              {busy ? "Submitting…" : "Select evaluation"}
            </button>
          </footer>
        </article>
      ))}
    </section>
  );
}
