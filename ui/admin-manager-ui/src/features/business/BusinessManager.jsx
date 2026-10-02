import React, { useEffect, useState } from "react";
import { api } from "../../api";
const fields = {
  coupons: [
    ["code", "Coupon code"],
    ["name", "Name"],
    ["discountType", "Discount type", "select", ["Percentage", "Fixed"]],
    ["value", "Discount value", "number"],
    ["minimumFee", "Minimum assessment fee (INR)", "number"],
    ["planId", "Eligible plan", "plan"],
    ["userId", "Eligible user", "user"],
    ["startsAt", "Valid from", "datetime-local"],
    ["endsAt", "Valid until", "datetime-local"],
    ["maximumUses", "Maximum total uses", "number"],
    ["usesPerUser", "Uses per user", "number"],
    ["isActive", "Enabled", "checkbox"],
  ],
  audiences: [
    ["name", "Audience name"],
    ["description", "Description", "textarea"],
  ],
  campaigns: [
    ["name", "Campaign name"],
    ["audienceId", "Audience", "audience"],
    ["subject", "Email subject"],
    ["body", "Message (plain text)", "textarea"],
    ["scheduledAt", "Send no earlier than", "datetime-local"],
  ],
  content: [
    ["kind", "Content type", "select", ["Page", "Banner", "FAQ", "Event"]],
    ["slug", "URL slug"],
    ["title", "Title"],
    ["body", "Content (plain text)", "textarea"],
    ["status", "Publication", "select", ["Draft", "Published"]],
    ["startsAt", "Visible from", "datetime-local"],
    ["endsAt", "Visible until", "datetime-local"],
  ],
};
const defaults = {
  coupons: {
    code: "",
    name: "",
    discountType: "Percentage",
    value: 10,
    minimumFee: 0,
    planId: "",
    userId: "",
    startsAt: "",
    endsAt: "",
    maximumUses: 100,
    usesPerUser: 1,
    isActive: false,
  },
  audiences: { name: "", description: "" },
  campaigns: {
    name: "",
    audienceId: "",
    subject: "",
    body: "",
    scheduledAt: "",
  },
  content: {
    kind: "Page",
    slug: "",
    title: "",
    body: "",
    status: "Draft",
    startsAt: "",
    endsAt: "",
  },
};
const titles = {
  coupons: "Coupons and offers",
  audiences: "Audience lists",
  campaigns: "Email campaigns",
  content: "Content and events",
};
const identities = {
  coupons: "couponId",
  audiences: "audienceId",
  campaigns: "campaignId",
  content: "contentId",
};
const localDate = (v) =>
  v
    ? new Date(new Date(v).getTime() - new Date(v).getTimezoneOffset() * 60000)
        .toISOString()
        .slice(0, 16)
    : "";
export default function BusinessManager({ kind }) {
  const [memberEdit, setMemberEdit] = useState(null);
  const [dateRange, setDateRange] = useState({ from: "", to: "" });
  const [rows, setRows] = useState([]),
    [form, setForm] = useState(null),
    [error, setError] = useState(""),
    [busy, setBusy] = useState(false),
    [audiences, setAudiences] = useState([]),
    [plans, setPlans] = useState([]),
    [users, setUsers] = useState([]),
    [details, setDetails] = useState(null),
    [member, setMember] = useState({
      email: "",
      name: "",
      marketingConsent: false,
      consentSource: "",
    }),
    [confirm, setConfirm] = useState(null);
  const path =
    kind === "coupons" ? "/admin/coupons" : `/admin/engagement/${kind}`;
  async function load() {
    setRows(await api(path));
    if (kind === "coupons") {
      const [p, u] = await Promise.all([
        api("/admin/plans"),
        api("/admin/identity/users"),
      ]);
      setPlans(p);
      setUsers(u.items);
    }
    if (kind === "campaigns")
      setAudiences(await api("/admin/engagement/audiences"));
  }
  useEffect(() => {
    setRows([]);
    setForm(null);
    setDetails(null);
    setError("");
    load().catch((e) => setError(e.message));
  }, [kind]);
  async function act(fn) {
    if (busy) return;
    setBusy(true);
    setError("");
    try {
      await fn();
      await load();
    } catch (e) {
      setError(e.message);
    } finally {
      setBusy(false);
    }
  }
  function edit(row) {
    const value = { ...row };
    fields[kind].forEach(([key, , type]) => {
      if (type === "datetime-local") value[key] = localDate(value[key]);
    });
    setForm(value);
    setDetails(null);
  }
  async function save(e) {
    e.preventDefault();
    await act(async () => {
      const body = { ...form };
      fields[kind].forEach(([key, , type]) => {
        if (type === "datetime-local")
          body[key] = body[key] ? new Date(body[key]).toISOString() : null;
        if (type === "number") body[key] = Number(body[key]);
      });
      ["planId", "userId"].forEach((k) => {
        if (k in body) body[k] = body[k] || null;
      });
      const id = form[identities[kind]];
      await api(id ? `${path}/${id}` : path, {
        method: id ? "PUT" : "POST",
        body: JSON.stringify(body),
      });
      setForm(null);
    });
  }
  async function view(row) {
    const suffix = kind === "audiences" ? "members" : "deliveries";
    const data = await api(`${path}/${row[identities[kind]]}/${suffix}`);
    setDetails({ row, data });
    setForm(null);
  }
  return (
    <>
      <div className="top">
        <div>
          <h1>{titles[kind]}</h1>
          <p className="muted">
            Manage saved records. Changes are validated on the server.
          </p>
        </div>
        <button
          className="btn"
          onClick={() => {
            setForm({ ...defaults[kind], version: 0 });
            setDetails(null);
          }}
        >
          + Create
        </button>
      </div>
      {kind === "coupons" && (
        <p className="plan-guide">
          Discounts apply to new checkouts. Reservations consume redemption
          limits, including uncertain payment attempts. Minimum payable fee is
          ₹1. A reserved coupon's price and eligibility cannot change.
        </p>
      )}
      {kind === "campaigns" && (
        <p className="plan-guide">
          Save a draft, review the audience and explicitly queue it. Only
          opted-in members receive offers. SMTP must be configured and sending
          enabled. Uncertain deliveries require review rather than automatic
          resending.
        </p>
      )}
      {error && (
        <p role="alert" className="error">
          {error}
        </p>
      )}
      {form && (
        <form className="card plan-form" onSubmit={save}>
          <h2>
            {form[identities[kind]] ? "Edit" : "Create"}{" "}
            {kind === "content" ? "content" : kind.slice(0, -1)}
          </h2>
          <fieldset disabled={busy}>
            <legend>Details</legend>
            {fields[kind].map(([key, label, type = "text", options]) => (
              <label key={key}>
                {label}
                {["select", "audience", "plan", "user"].includes(type) ? (
                  <select
                    value={form[key] || ""}
                    required={type !== "plan" && type !== "user"}
                    onChange={(e) =>
                      setForm({ ...form, [key]: e.target.value })
                    }
                  >
                    {type === "plan" ? (
                      <>
                        <option value="">All plans</option>
                        {plans.map((p) => (
                          <option value={p.planId} key={p.planId}>
                            {p.name}
                          </option>
                        ))}
                      </>
                    ) : type === "user" ? (
                      <>
                        <option value="">All users</option>
                        {users.map((u) => (
                          <option value={u.userId} key={u.userId}>
                            {u.email}
                          </option>
                        ))}
                      </>
                    ) : type === "audience" ? (
                      <>
                        <option value="">Choose audience</option>
                        {audiences.map((a) => (
                          <option key={a.audienceId} value={a.audienceId}>
                            {a.name}
                          </option>
                        ))}
                      </>
                    ) : (
                      options.map((o) => <option key={o}>{o}</option>)
                    )}
                  </select>
                ) : type === "textarea" ? (
                  <textarea
                    rows={6}
                    maxLength={key === "description" ? 1000 : 20000}
                    value={form[key] || ""}
                    onChange={(e) =>
                      setForm({ ...form, [key]: e.target.value })
                    }
                  />
                ) : (
                  <input
                    type={type}
                    step={type === "number" ? "0.01" : undefined}
                    min={type === "number" ? 0 : undefined}
                    checked={
                      type === "checkbox" ? Boolean(form[key]) : undefined
                    }
                    value={type === "checkbox" ? undefined : (form[key] ?? "")}
                    onChange={(e) =>
                      setForm({
                        ...form,
                        [key]:
                          type === "checkbox"
                            ? e.target.checked
                            : e.target.value,
                      })
                    }
                  />
                )}
              </label>
            ))}
          </fieldset>
          <div className="actions">
            <button className="btn" disabled={busy}>
              Save
            </button>
            <button
              type="button"
              className="btn secondary"
              onClick={() => setForm(null)}
            >
              Cancel
            </button>
          </div>
        </form>
      )}
      <section className="card table-scroll">
        <table>
          <thead>
            <tr>
              <th>Name</th>
              <th>Status / type</th>
              <th>Details</th>
              <th>Actions</th>
            </tr>
          </thead>
          <tbody>
            {rows.map((row) => (
              <tr key={row[identities[kind]]}>
                <td>{row.name || row.title}</td>
                <td>
                  {row.status ||
                    row.kind ||
                    (kind === "coupons"
                      ? row.isActive
                        ? "Enabled"
                        : "Disabled"
                      : "Saved")}
                </td>
                <td>
                  {kind === "coupons"
                    ? `${row.code} · ${row.reservedUses}/${row.maximumUses} reserved`
                    : row.slug || row.subject || row.description}
                </td>
                <td>
                  <div className="actions">
                    <button
                      className="btn secondary"
                      disabled={
                        busy || (kind === "campaigns" && row.status !== "Draft")
                      }
                      onClick={() => edit(row)}
                    >
                      Edit
                    </button>
                    {["audiences", "campaigns"].includes(kind) && (
                      <button
                        className="btn secondary"
                        onClick={() => act(() => view(row))}
                      >
                        {kind === "audiences" ? "Members" : "Delivery history"}
                      </button>
                    )}
                    {kind === "campaigns" && row.status === "Draft" && (
                      <button
                        className="btn"
                        onClick={() => setConfirm({ row, action: "queue" })}
                      >
                        Queue email
                      </button>
                    )}
                    {kind === "campaigns" && row.status === "Queued" && (
                      <button
                        className="btn secondary"
                        onClick={() => setConfirm({ row, action: "cancel" })}
                      >
                        Cancel sending
                      </button>
                    )}
                    {(kind !== "campaigns" || row.status === "Draft") && (
                      <button
                        className="btn secondary"
                        onClick={() => setConfirm({ row, action: "delete" })}
                      >
                        Archive
                      </button>
                    )}
                  </div>
                </td>
              </tr>
            ))}
            {!rows.length && (
              <tr>
                <td colSpan={4}>
                  No records yet. Create your first record to get started.
                </td>
              </tr>
            )}
          </tbody>
        </table>
        <p className="muted">Showing up to 500 records.</p>
      </section>
      {confirm && (
        <section
          className="card"
          role="alertdialog"
          aria-label="Confirm action"
        >
          <h2>Confirm {confirm.action}</h2>
          <p>
            {confirm.action === "queue"
              ? "This schedules real emails to opted-in audience members when sending is enabled."
              : "Apply this action to the selected record?"}
          </p>
          <div className="actions">
            <button
              className="btn"
              disabled={busy}
              onClick={() =>
                act(async () => {
                  const { row, action } = confirm;
                  await api(
                    `${path}/${row[identities[kind]]}${action === "delete" ? `?version=${row.version}` : `/${action}`}`,
                    {
                      method: action === "delete" ? "DELETE" : "POST",
                      ...(action === "delete"
                        ? {}
                        : { body: JSON.stringify({ version: row.version }) }),
                    },
                  );
                  setConfirm(null);
                })
              }
            >
              Confirm
            </button>
            <button className="btn secondary" onClick={() => setConfirm(null)}>
              Keep unchanged
            </button>
          </div>
        </section>
      )}
      {memberEdit && (
        <form
          className="card"
          onSubmit={(e) => {
            e.preventDefault();
            act(async () => {
              await api(`/admin/engagement/members/${memberEdit.memberId}`, {
                method: "PUT",
                body: JSON.stringify(memberEdit),
              });
              setMemberEdit(null);
              await view(details.row);
            });
          }}
        >
          <h2>Consent: {memberEdit.email}</h2>
          <label>
            Evidence/source
            <input
              required
              maxLength={250}
              value={memberEdit.consentSource}
              onChange={(e) =>
                setMemberEdit({ ...memberEdit, consentSource: e.target.value })
              }
            />
          </label>
          <label>
            <input
              type="checkbox"
              checked={memberEdit.marketingConsent}
              onChange={(e) =>
                setMemberEdit({
                  ...memberEdit,
                  marketingConsent: e.target.checked,
                })
              }
            />
            Marketing permission recorded
          </label>
          <button className="btn" disabled={busy}>
            Save consent
          </button>
          <button
            className="btn secondary"
            type="button"
            onClick={() => setMemberEdit(null)}
          >
            Cancel
          </button>
        </form>
      )}
      {details && (
        <section className="card">
          <h2>
            {details.row.name}:{" "}
            {kind === "audiences" ? "members" : "delivery history"}
          </h2>
          {kind === "audiences" && (
            <form
              onSubmit={(e) => {
                e.preventDefault();
                act(async () => {
                  await api(
                    `${path}/${details.row.audienceId}/import-traders`,
                    {
                      method: "POST",
                      body: JSON.stringify({
                        from: new Date(
                          `${dateRange.from}T00:00:00+05:30`,
                        ).toISOString(),
                        to: new Date(
                          `${dateRange.to}T00:00:00+05:30`,
                        ).toISOString(),
                      }),
                    },
                  );
                  await view(details.row);
                });
              }}
            >
              <h3>Import registered traders</h3>
              <p className="muted">
                Start inclusive, end exclusive (India time). Imported members
                are excluded from marketing until consent is recorded.
              </p>
              <div className="actions">
                <input
                  type="date"
                  aria-label="Registration from"
                  required
                  value={dateRange.from}
                  onChange={(e) =>
                    setDateRange({ ...dateRange, from: e.target.value })
                  }
                />
                <input
                  type="date"
                  aria-label="Registration before"
                  required
                  value={dateRange.to}
                  onChange={(e) =>
                    setDateRange({ ...dateRange, to: e.target.value })
                  }
                />
                <button className="btn secondary" disabled={busy}>
                  Import date range
                </button>
              </div>
            </form>
          )}
          {kind === "audiences" && (
            <form
              onSubmit={(e) => {
                e.preventDefault();
                act(async () => {
                  await api(`${path}/${details.row.audienceId}/members`, {
                    method: "POST",
                    body: JSON.stringify(member),
                  });
                  await view(details.row);
                  setMember({
                    email: "",
                    name: "",
                    marketingConsent: false,
                    consentSource: "",
                  });
                });
              }}
            >
              <fieldset disabled={busy}>
                <legend>Add member</legend>
                <label>
                  Email
                  <input
                    type="email"
                    required
                    value={member.email}
                    onChange={(e) =>
                      setMember({ ...member, email: e.target.value })
                    }
                  />
                </label>
                <label>
                  Name
                  <input
                    value={member.name}
                    onChange={(e) =>
                      setMember({ ...member, name: e.target.value })
                    }
                  />
                </label>
                <label>
                  Consent evidence / source
                  <input
                    value={member.consentSource}
                    onChange={(e) =>
                      setMember({ ...member, consentSource: e.target.value })
                    }
                  />
                </label>
                <label>
                  <input
                    type="checkbox"
                    checked={member.marketingConsent}
                    onChange={(e) =>
                      setMember({
                        ...member,
                        marketingConsent: e.target.checked,
                      })
                    }
                  />
                  Recorded permission for marketing emails
                </label>
              </fieldset>
              <button className="btn" disabled={busy}>
                Add member
              </button>
            </form>
          )}
          <div className="table-scroll">
            <table>
              <thead>
                <tr>
                  <th>Email</th>
                  <th>Status</th>
                  <th>Details</th>
                </tr>
              </thead>
              <tbody>
                {details.data.map((d, i) => (
                  <tr key={d.memberId || d.deliveryId || i}>
                    <td>{d.email}</td>
                    <td>
                      {d.status ||
                        (d.marketingConsent && !d.unsubscribedAt
                          ? "Opted in"
                          : "Excluded")}
                    </td>
                    <td>
                      {d.error || d.consentSource || d.sentAt || "—"}
                      {kind === "audiences" && (
                        <button
                          className="btn secondary"
                          onClick={() => setMemberEdit({ ...d })}
                        >
                          Edit consent
                        </button>
                      )}
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </section>
      )}
    </>
  );
}
