import { Link } from "react-router-dom";
import React, { useEffect, useState } from "react";
import { api } from "../../api";
import { useAuth } from "../../store";

const money = (value) =>
  new Intl.NumberFormat("en-IN", {
    style: "currency",
    currency: "INR",
    maximumFractionDigits: 2,
  }).format(value);
const optionalNumber = (value) =>
  value === "" || value === null ? null : Number(value);
const initialStage = (index) => ({
  name: `Stage ${index + 1}`,
  profitTargetPercent: index === 0 ? 8 : 5,
  maxDailyLossPercent: 5,
  maxTotalLossPercent: 10,
  minimumTradingDays: 5,
  tradingPeriod: 0,
  maximumLeverage: "",
  policySetId: "",
});
function savedDraft() {
  try {
    return JSON.parse(sessionStorage.getItem("fundex.admin.planDraft") || "{}");
  } catch {
    return {};
  }
}
const initialTerms = {
  challengeCapital: 100000,
  registrationFee: 999,
  rewardSharePercent: 80,
  taxWithholdingPercent: 0,
  otherDeductionPercent: 0,
  fundedDailyLossPercent: 5,
  fundedTotalLossPercent: 10,
};
function NumberField({
  label,
  hint,
  value,
  onChange,
  min = 0,
  max,
  optional = false,
  step = "0.01",
}) {
  return (
    <label className="plan-field">
      <span>
        {label}
        {optional && <small className="optional-label">Optional</small>}
      </span>
      <input
        className="field"
        type="number"
        required={!optional}
        min={min}
        max={max}
        step={step}
        value={value ?? ""}
        onChange={(e) => onChange(e.target.value)}
      />
      <small className="field-help">{hint}</small>
    </label>
  );
}
export default function Plans() {
  const user = useAuth((s) => s.user);
  const canWrite =
    user.roles.includes("ADMIN") || user.permissions?.includes("plans.write");
  const [plans, setPlans] = useState([]),
    [versions, setVersions] = useState([]);
  const [options, setOptions] = useState({ tradingPeriods: [], policies: [] });
  const [selected, setSelected] = useState(() => savedDraft().selected || ""),
    [error, setError] = useState(""),
    [busy, setBusy] = useState(false),
    [loading, setLoading] = useState(true);
  const [form, setForm] = useState({ code: "", name: "", description: "" });
  const [terms, setTerms] = useState(() => savedDraft().terms || initialTerms),
    [stages, setStages] = useState(
      () => savedDraft().stages || [initialStage(0), initialStage(1)],
    );
  useEffect(() => {
    sessionStorage.setItem(
      "fundex.admin.planDraft",
      JSON.stringify({ selected, terms, stages }),
    );
  }, [selected, terms, stages]);
  const [versionLoading, setVersionLoading] = useState(false);
  useEffect(() => {
    let active = true;
    Promise.all([api("/admin/plans/"), api("/admin/plans/options")])
      .then(([list, data]) => {
        if (active) {
          setPlans(list);
          setOptions(data);
        }
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
    setVersions([]);
    if (selected) {
      setVersionLoading(true);
      api(`/admin/plans/${selected}/versions`)
        .then((data) => {
          if (active) setVersions(data);
        })
        .catch((e) => {
          if (active) setError(e.message);
        })
        .finally(() => {
          if (active) setVersionLoading(false);
        });
    }
    return () => {
      active = false;
    };
  }, [selected]);
  function updateStage(index, field, value) {
    setStages((old) =>
      old.map((stage, i) =>
        i === index ? { ...stage, [field]: value } : stage,
      ),
    );
  }
  async function action(operation) {
    if (busy) return;
    setBusy(true);
    setError("");
    try {
      await operation();
    } catch (e) {
      setError(e.message);
    } finally {
      setBusy(false);
    }
  }
  async function create(event) {
    event.preventDefault();
    await action(async () => {
      const result = await api("/admin/plans/", {
        method: "POST",
        successMessage: "Plan created. Add its assessment rules below.",
        body: JSON.stringify({
          ...form,
          description: form.description.trim() || null,
        }),
      });
      setPlans(await api("/admin/plans/"));
      setSelected(result.planId);
      setForm({ code: "", name: "", description: "" });
    });
  }
  async function createVersion(event) {
    event.preventDefault();
    await action(async () => {
      await api(`/admin/plans/${selected}/versions`, {
        method: "POST",
        successMessage:
          "Draft rules saved. Review all stages before publishing.",
        body: JSON.stringify({
          ...Object.fromEntries(
            Object.entries(terms).map(([key, value]) => [key, Number(value)]),
          ),
          stages: stages.map((stage) => ({
            ...stage,
            startingCapital: Number(terms.challengeCapital),
            profitTargetPercent: Number(stage.profitTargetPercent),
            maxDailyLossPercent: Number(stage.maxDailyLossPercent),
            maxTotalLossPercent: Number(stage.maxTotalLossPercent),
            minimumTradingDays: optionalNumber(stage.minimumTradingDays),
            tradingPeriod: Number(stage.tradingPeriod),
            maximumCalendarDays: null,
            maximumLeverage: optionalNumber(stage.maximumLeverage),
          })),
        }),
      });
      setVersions(await api(`/admin/plans/${selected}/versions`));
    });
  }
  function termField(field, label, hint, min, max) {
    return (
      <NumberField
        key={field}
        label={label}
        hint={hint}
        value={terms[field]}
        min={min}
        max={max}
        onChange={(value) => setTerms((old) => ({ ...old, [field]: value }))}
      />
    );
  }
  const periodLabel = (stage) =>
    stage.tradingPeriod == null
      ? stage.maximumCalendarDays == null
        ? "Unlimited (legacy)"
        : `${stage.maximumCalendarDays} calendar days`
      : options.tradingPeriods.find((x) => x.value === stage.tradingPeriod)
          ?.label || "Unknown period";
  return (
    <>
      <div className="top">
        <div>
          <h1>Assessment plans</h1>
          <p className="muted">
            Set the entry fee, assessment objectives, and funded-account reward
            rules.
          </p>
        </div>
        <span className="badge">INR · Equities</span>
      </div>
      <div className="plan-guide">
        <strong>1. Create a plan</strong>
        <span>2. Configure each stage</span>
        <span>3. Review and publish</span>
      </div>
      {error && (
        <p className="error form-alert" role="alert">
          {error}
        </p>
      )}
      {loading && <p role="status">Loading plans and available settings…</p>}
      {canWrite && (
        <form className="card plan-form" onSubmit={create}>
          <div className="section-heading">
            <span className="step-number">1</span>
            <div>
              <h2>Plan details</h2>
              <p>Give this assessment a name your applicants will recognize.</p>
            </div>
          </div>
          <fieldset disabled={busy}>
            <legend>Basic information</legend>
            <label className="plan-field">
              Plan name
              <input
                className="field"
                required
                maxLength={150}
                placeholder="e.g. Equity Assessment – 2 Stages"
                value={form.name}
                onChange={(e) => setForm({ ...form, name: e.target.value })}
              />
              <small className="field-help">
                Shown to applicants when they choose a plan.
              </small>
            </label>
            <label className="plan-field">
              Internal plan code
              <input
                className="field"
                required
                maxLength={50}
                placeholder="e.g. EQUITY-100K-2"
                value={form.code}
                onChange={(e) => setForm({ ...form, code: e.target.value })}
              />
              <small className="field-help">
                A unique reference for your team. It is saved in uppercase.
              </small>
            </label>
            <label className="plan-field full-width">
              Description <small className="optional-label">Optional</small>
              <textarea
                rows={3}
                placeholder="Explain who this plan is for and what the assessment covers."
                value={form.description}
                onChange={(e) =>
                  setForm({ ...form, description: e.target.value })
                }
              />
            </label>
          </fieldset>
          <div className="form-footer">
            <span className="muted">Creating a plan does not publish it.</span>
            <button className="btn" disabled={busy}>
              {busy ? "Saving…" : "Create plan"}
            </button>
          </div>
        </form>
      )}
      <section className="card">
        <div className="section-heading">
          <span className="step-number">2</span>
          <div>
            <h2>Assessment rules</h2>
            <Link to="/policies" state={{ from: "/plans" }}>
              Create or view risk policies
            </Link>
            <button
              type="button"
              className="btn secondary"
              onClick={() =>
                api("/admin/plans/options")
                  .then(setOptions)
                  .catch((e) => setError(e.message))
              }
            >
              Refresh available policies
            </button>
            <p>
              Select a plan, then configure a new version. Published versions
              stay unchanged.
            </p>
          </div>
        </div>
        <label>
          Choose a plan
          <select
            className="field"
            value={selected}
            disabled={busy || loading}
            onChange={(e) => {
              setSelected(e.target.value);
              setTerms(initialTerms);
              setStages([initialStage(0), initialStage(1)]);
            }}
          >
            <option value="">Select an assessment plan</option>
            {plans.map((plan) => (
              <option key={plan.planId} value={plan.planId}>
                {plan.name} ({plan.code})
              </option>
            ))}
          </select>
        </label>
        {selected && canWrite && (
          <form className="plan-form" onSubmit={createVersion}>
            <fieldset disabled={busy || !options.tradingPeriods.length}>
              <legend>Capital and entry fee</legend>
              {termField(
                "challengeCapital",
                "Assessment capital (INR)",
                "Simulated starting balance for each assessment stage.",
                1,
              )}
              {termField(
                "registrationFee",
                "Assessment fee (INR)",
                "The amount an applicant pays to begin an assessment.",
                1,
              )}
              <label className="plan-field">
                Assessment path
                <select
                  className="field"
                  value={stages.length}
                  onChange={(e) =>
                    setStages((old) =>
                      Array.from(
                        { length: Number(e.target.value) },
                        (_, i) => old[i] || initialStage(i),
                      ),
                    )
                  }
                >
                  <option value={2}>Two stages</option>
                  <option value={3}>Three stages</option>
                </select>
                <small className="field-help">
                  Applicants must pass every stage before contract review.
                </small>
              </label>
            </fieldset>
            {stages.map((stage, index) => (
              <fieldset key={index} disabled={busy} className="stage-fieldset">
                <legend>Stage {index + 1} · Assessment objectives</legend>
                <label className="plan-field">
                  Stage name
                  <input
                    className="field"
                    required
                    maxLength={100}
                    value={stage.name}
                    onChange={(e) => updateStage(index, "name", e.target.value)}
                  />
                </label>
                <label className="plan-field">
                  Risk policy
                  <select
                    className="field"
                    required
                    value={stage.policySetId}
                    onChange={(e) =>
                      updateStage(index, "policySetId", e.target.value)
                    }
                  >
                    <option value="">Select an active policy</option>
                    {options.policies.map((policy) => (
                      <option key={policy.policyId} value={policy.policyId}>
                        {policy.name} ({policy.code})
                      </option>
                    ))}
                  </select>
                  <small className="field-help">
                    Applies the selected trading restrictions to this stage.
                  </small>
                </label>
                {[
                  [
                    "profitTargetPercent",
                    "Profit target (%)",
                    "Profit required to pass, measured against starting capital.",
                  ],
                  [
                    "maxDailyLossPercent",
                    "Maximum daily loss (%)",
                    "The stage fails when this loss threshold is reached.",
                  ],
                  [
                    "maxTotalLossPercent",
                    "Maximum total loss (%)",
                    "Maximum loss measured against the stage’s starting capital.",
                  ],
                ].map(([field, label, hint]) => (
                  <NumberField
                    key={field}
                    label={label}
                    hint={hint}
                    value={stage[field]}
                    min={0.01}
                    max={100}
                    onChange={(value) => updateStage(index, field, value)}
                  />
                ))}
                <NumberField
                  label="Minimum trading days"
                  hint="Blank: not specified. Zero: explicitly no minimum. Trading days are days with trading activity."
                  optional
                  step="1"
                  value={stage.minimumTradingDays}
                  onChange={(value) =>
                    updateStage(index, "minimumTradingDays", value)
                  }
                />
                <label className="plan-field">
                  Trading period
                  <select
                    className="field"
                    required
                    value={stage.tradingPeriod}
                    onChange={(e) =>
                      updateStage(
                        index,
                        "tradingPeriod",
                        Number(e.target.value),
                      )
                    }
                  >
                    {options.tradingPeriods.map((period) => (
                      <option key={period.value} value={period.value}>
                        {period.label}
                      </option>
                    ))}
                  </select>
                  <small className="field-help">
                    Starts when this stage begins. Months and years follow the
                    calendar; unlimited has no expiry.
                  </small>
                </label>
                <NumberField
                  label="Maximum leverage (1 : N)"
                  hint="Enter 100 for 1:100. Blank: not specified. This is a ceiling; current cash-only execution and broker restrictions can be stricter. It does not increase buying power."
                  min={1}
                  max={100}
                  step="1"
                  optional
                  value={stage.maximumLeverage}
                  onChange={(value) =>
                    updateStage(index, "maximumLeverage", value)
                  }
                />
              </fieldset>
            ))}
            <fieldset disabled={busy}>
              <legend>Funded-account rewards and risk</legend>
              {termField(
                "rewardSharePercent",
                "Trader’s profit share (%)",
                "The trader’s gross share of eligible profit. The remaining share belongs to the platform.",
                0.01,
                100,
              )}
              {termField(
                "taxWithholdingPercent",
                "Tax withheld from trader share (%)",
                "Use 0 when no withholding applies. Set the rate appropriate to your payment arrangement.",
                0,
                100,
              )}
              {termField(
                "otherDeductionPercent",
                "Other deductions from trader share (%)",
                "Use 0 for no other deductions. Tax plus other deductions must be below 100%.",
                0,
                100,
              )}
              {termField(
                "fundedDailyLossPercent",
                "Funded maximum daily loss (%)",
                "Daily loss threshold after a funded account is approved.",
                0.01,
                100,
              )}
              {termField(
                "fundedTotalLossPercent",
                "Funded maximum total loss (%)",
                "Overall loss threshold after a funded account is approved.",
                0.01,
                100,
              )}
            </fieldset>
            <div className="plan-example">
              <strong>Example: INR 1,000 eligible profit</strong>
              <p>
                Trader’s gross share:{" "}
                {money((1000 * Number(terms.rewardSharePercent)) / 100)}.
                Platform share:{" "}
                {money((1000 * (100 - Number(terms.rewardSharePercent))) / 100)}
                . Tax and other deductions apply to the trader’s share.
              </p>
            </div>
            <div className="form-footer">
              <span className="muted">
                Required fields cannot be blank. Optional blanks are saved as
                null.
              </span>
              <button
                className="btn"
                disabled={
                  busy ||
                  !options.tradingPeriods.length ||
                  !options.policies.length
                }
              >
                {busy ? "Saving…" : "Save draft rules"}
              </button>
            </div>
            {!options.policies.length && (
              <p className="error">
                An active risk policy is required before you can save assessment
                rules.
              </p>
            )}
          </form>
        )}
      </section>
      {selected && (
        <section className="card">
          <div className="section-heading">
            <span className="step-number">3</span>
            <div>
              <h2>Review and publish</h2>
              <p>
                Check the fee, reward split, and every stage before making this
                plan available.
              </p>
            </div>
          </div>
          {versionLoading ? (
            <p role="status">Loading saved versions…</p>
          ) : versions.length === 0 ? (
            <p className="empty-state">
              No saved rules yet. Complete the form above to create the first
              version.
            </p>
          ) : (
            versions.map((item) => (
              <article className="plan-version" key={item.planVersionId}>
                <div className="top">
                  <h3>
                    Version {item.versionNumber} ·{" "}
                    {item.path === "TwoStep" ? "Two stages" : "Three stages"}
                  </h3>
                  <span className="badge">{item.status}</span>
                </div>
                <dl className="plan-summary">
                  {[
                    ["Assessment capital", money(item.challengeCapital)],
                    ["Assessment fee", money(item.registrationFee)],
                    ["Trader profit share", `${item.rewardSharePercent}%`],
                    ["Tax withholding", `${item.taxWithholdingPercent}%`],
                    ["Other deductions", `${item.otherDeductionPercent}%`],
                    [
                      "Funded daily / total loss",
                      `${item.fundedDailyLossPercent}% / ${item.fundedTotalLossPercent}%`,
                    ],
                  ].map(([label, value]) => (
                    <div key={label}>
                      <dt>{label}</dt>
                      <dd>{value}</dd>
                    </div>
                  ))}
                </dl>
                <div className="table-scroll">
                  <table>
                    <thead>
                      <tr>
                        <th>Stage</th>
                        <th>Target</th>
                        <th>Daily / total loss</th>
                        <th>Minimum days</th>
                        <th>Trading period</th>
                        <th>Leverage ceiling</th>
                      </tr>
                    </thead>
                    <tbody>
                      {item.stages?.map((stage) => (
                        <tr key={stage.stageNumber}>
                          <td>{stage.name}</td>
                          <td>{stage.profitTargetPercent}%</td>
                          <td>
                            {stage.maxDailyLossPercent}% /{" "}
                            {stage.maxTotalLossPercent}%
                          </td>
                          <td>{stage.minimumTradingDays ?? "Not specified"}</td>
                          <td>{periodLabel(stage)}</td>
                          <td>
                            {stage.maximumLeverage == null
                              ? "Not specified"
                              : `1:${stage.maximumLeverage}`}
                          </td>
                        </tr>
                      ))}
                    </tbody>
                  </table>
                </div>
                {canWrite && item.status === "Draft" && (
                  <div className="form-footer">
                    <span className="muted">
                      Publishing makes these rules available for new
                      assessments.
                    </span>
                    <button
                      className="btn"
                      disabled={busy}
                      onClick={() =>
                        action(async () => {
                          await api(
                            `/admin/plans/versions/${item.planVersionId}/publish`,
                            {
                              method: "POST",
                              successMessage:
                                "Plan version published successfully.",
                              body: JSON.stringify({ version: item.version }),
                            },
                          );
                          setVersions(
                            await api(`/admin/plans/${selected}/versions`),
                          );
                        })
                      }
                    >
                      Publish this version
                    </button>
                  </div>
                )}
              </article>
            ))
          )}
        </section>
      )}
    </>
  );
}
