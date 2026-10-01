import React, { useEffect, useState } from "react";
import { api } from "../../api";
import { useAuth } from "../../store";

export default function Plans() {
  const user = useAuth((s) => s.user);
  const canWrite =
    user.roles.includes("ADMIN") || user.permissions.includes("plans.write");
  const [plans, setPlans] = useState([]),
    [versions, setVersions] = useState([]);
  const [selected, setSelected] = useState(""),
    [error, setError] = useState(""),
    [busy, setBusy] = useState(false);
  const [form, setForm] = useState({ code: "", name: "", description: "" });
  const [version, setVersion] = useState({
    challengeCapital: 100000,
    registrationFee: 999,
    count: 2,
    rewardSharePercent: 80,
    taxWithholdingPercent: 0,
    otherDeductionPercent: 0,
    fundedDailyLossPercent: 5,
    fundedTotalLossPercent: 10,
    targets: [8, 5, 5],
    policySetId: "",
    minimumTradingDays: 5,
    maxDailyLossPercent: 5,
    maxTotalLossPercent: 10,
  });
  async function load() {
    setPlans(await api("/admin/plans/"));
  }
  useEffect(() => {
    load().catch((e) => setError(e.message));
  }, []);
  useEffect(() => {
    let active = true;
    setVersions([]);
    if (selected)
      api(`/admin/plans/${selected}/versions`)
        .then((data) => {
          if (active) setVersions(data);
        })
        .catch((e) => {
          if (active) setError(e.message);
        });
    return () => {
      active = false;
    };
  }, [selected]);
  async function action(operation) {
    setBusy(true);
    setError("");
    try {
      await operation();
      await load();
      if (selected) setVersions(await api(`/admin/plans/${selected}/versions`));
    } catch (e) {
      setError(e.message);
    } finally {
      setBusy(false);
    }
  }
  async function create(event) {
    event.preventDefault();
    await action(async () => {
      await api("/admin/plans/", {
        method: "POST",
        body: JSON.stringify(form),
      });
      setForm({ code: "", name: "", description: "" });
    });
  }
  async function createVersion(event) {
    event.preventDefault();
    const stages = Array.from(
      { length: Number(version.count) },
      (_, index) => ({
        name: `Stage ${index + 1}`,
        startingCapital: Number(version.challengeCapital),
        policySetId: version.policySetId,
        profitTargetPercent: Number(version.targets[index]),
        maxDailyLossPercent: Number(version.maxDailyLossPercent),
        maxTotalLossPercent: Number(version.maxTotalLossPercent),
        minimumTradingDays: Number(version.minimumTradingDays),
        maximumCalendarDays: null,
      }),
    );
    await action(() =>
      api(`/admin/plans/${selected}/versions`, {
        method: "POST",
        body: JSON.stringify({
          challengeCapital: Number(version.challengeCapital),
          registrationFee: Number(version.registrationFee),
          rewardSharePercent: Number(version.rewardSharePercent),
          taxWithholdingPercent: Number(version.taxWithholdingPercent),
          otherDeductionPercent: Number(version.otherDeductionPercent),
          fundedDailyLossPercent: Number(version.fundedDailyLossPercent),
          fundedTotalLossPercent: Number(version.fundedTotalLossPercent),
          stages,
        }),
      }),
    );
  }
  return (
    <>
      <h1>Plans and challenge paths</h1>
      {error && (
        <p className="error" role="alert">
          {error}
        </p>
      )}
      {canWrite && (
        <form className="card" onSubmit={create}>
          <h2>Create plan</h2>
          {Object.keys(form).map((field) => (
            <label key={field}>
              {field}
              <input
                className="field"
                required={field !== "description"}
                value={form[field]}
                onChange={(e) => setForm({ ...form, [field]: e.target.value })}
              />
            </label>
          ))}
          <button className="btn" disabled={busy}>
            Create draft plan
          </button>
        </form>
      )}
      <section className="card">
        <h2>Plan versions</h2>
        <label>
          Plan
          <select
            className="field"
            value={selected}
            onChange={(e) => setSelected(e.target.value)}
          >
            <option value="">Choose a plan</option>
            {plans.map((plan) => (
              <option key={plan.planId} value={plan.planId}>
                {plan.name}
              </option>
            ))}
          </select>
        </label>
        {selected && canWrite && (
          <form className="form-grid" onSubmit={createVersion}>
            <label>
              Path
              <select
                className="field"
                value={version.count}
                onChange={(e) =>
                  setVersion({ ...version, count: Number(e.target.value) })
                }
              >
                <option value={2}>Two-Step</option>
                <option value={3}>Three-Step</option>
              </select>
            </label>
            {[
              ["challengeCapital", "Capital (INR)"],
              ["registrationFee", "Assessment fee (INR)"],
              ["minimumTradingDays", "Minimum trading days per stage"],
              ["maxDailyLossPercent", "Daily loss limit (%)"],
              ["maxTotalLossPercent", "Total loss limit (%)"],
            ].map(([field, label]) => (
              <label key={field}>
                {label}
                <input
                  required
                  className="field"
                  type="number"
                  min="1"
                  step={field === "registrationFee" ? "0.01" : "1"}
                  value={version[field]}
                  onChange={(e) =>
                    setVersion({ ...version, [field]: e.target.value })
                  }
                />
              </label>
            ))}
            {Array.from({ length: Number(version.count) }, (_, index) => (
              <label key={index}>
                Step {index + 1} profit target (%)
                <input
                  className="field"
                  type="number"
                  min="0.01"
                  max="100"
                  step="0.01"
                  required
                  value={version.targets[index]}
                  onChange={(e) =>
                    setVersion({
                      ...version,
                      targets: version.targets.map((value, i) =>
                        i === index ? e.target.value : value,
                      ),
                    })
                  }
                />
              </label>
            ))}
            <label>
              Active policy public ID
              <input
                className="field"
                required
                value={version.policySetId}
                onChange={(e) =>
                  setVersion({ ...version, policySetId: e.target.value })
                }
              />
            </label>
            <p className="muted">
              Each stage uses the configured target and an unlimited trading
              period. Review before publication.
            </p>
            {[
              ["Tax withholding on trader share (%)", "taxWithholdingPercent"],
              ["Other deductions on trader share (%)", "otherDeductionPercent"],
              ["Reward share (%)", "rewardSharePercent"],
              ["Funded daily loss (%)", "fundedDailyLossPercent"],
              ["Funded total loss (%)", "fundedTotalLossPercent"],
            ].map(([label, field]) => (
              <label key={field}>
                {label}
                <input
                  className="field"
                  required
                  type="number"
                  min={
                    field === "taxWithholdingPercent" ||
                    field === "otherDeductionPercent"
                      ? "0"
                      : "0.01"
                  }
                  max="100"
                  step="0.01"
                  value={version[field]}
                  onChange={(e) =>
                    setVersion({ ...version, [field]: e.target.value })
                  }
                />
              </label>
            ))}
            <button className="btn" disabled={busy}>
              Create version
            </button>
          </form>
        )}
        {versions.length === 0 && <p>No versions to display.</p>}
        {versions.map((item) => (
          <article className="card" key={item.planVersionId}>
            <h3>
              Version {item.versionNumber} Â· {item.path} Â· {item.status}
            </h3>
            <p>
              Capital â‚¹{item.challengeCapital} Â· Fee â‚¹
              {item.registrationFee} Â· Reward share {item.rewardSharePercent}%
              Â· Tax withholding {item.taxWithholdingPercent}% Â· Other
              deductions {item.otherDeductionPercent}%
            </p>
            {canWrite && item.status === "Draft" && (
              <button
                className="btn"
                disabled={busy}
                onClick={() =>
                  action(() =>
                    api(`/admin/plans/versions/${item.planVersionId}/publish`, {
                      method: "POST",
                      body: JSON.stringify({ version: item.version }),
                    }),
                  )
                }
              >
                Publish validated version
              </button>
            )}
          </article>
        ))}
      </section>
    </>
  );
}
