import React, { useEffect, useState } from "react";
import { api } from "../../api";
import { useAuth } from "../../store";

export default function Users() {
  const current = useAuth((s) => s.user);
  const [create, setCreate] = useState({
    email: "",
    password: "",
    firstName: "",
    lastName: "",
    role: "TRADER",
  });
  const [users, setUsers] = useState([]);
  const [next, setNext] = useState(null);
  const [selected, setSelected] = useState(null);
  const [roles, setRoles] = useState([]);
  const [status, setStatus] = useState("Active");
  const [reason, setReason] = useState("");
  const [history, setHistory] = useState([]);
  const [message, setMessage] = useState("");
  const [busy, setBusy] = useState(false);
  async function createUser(event) {
    event.preventDefault();
    setBusy(true);
    setMessage("");
    try {
      await api("/admin/identity/users", {
        method: "POST",
        body: JSON.stringify({ ...create, roles: [create.role] }),
      });
      setCreate({
        email: "",
        password: "",
        firstName: "",
        lastName: "",
        role: "TRADER",
      });
      await load();
      setMessage(
        "User created. Trading still requires a paid plan and account eligibility.",
      );
    } catch (e) {
      setMessage(e.message);
    } finally {
      setBusy(false);
    }
  }
  async function load(after) {
    const data = await api(
      "/admin/identity/users" + (after ? `?after=${after}` : ""),
    );
    setUsers((old) => (after ? [...old, ...data.items] : data.items));
    setNext(data.next);
  }
  useEffect(() => {
    if (current?.roles?.includes("ADMIN"))
      load().catch((e) => setMessage(e.message));
  }, [current]);
  async function select(user) {
    setSelected(user);
    setRoles(user.roles);
    setStatus(user.status);
    setReason("");
    setHistory([]);
    setMessage("");
    try {
      setHistory(await api(`/admin/identity/users/${user.userId}/history`));
    } catch (e) {
      setMessage(e.message);
    }
  }
  async function save(event) {
    event.preventDefault();
    setBusy(true);
    setMessage("");
    try {
      await api(`/admin/identity/users/${selected.userId}/access`, {
        method: "PUT",
        body: JSON.stringify({
          version: selected.version,
          roles,
          status,
          reason,
        }),
      });
      setSelected(null);
      await load();
      setMessage("Access updated. Existing sessions have been revoked.");
    } catch (e) {
      setMessage(e.message);
    } finally {
      setBusy(false);
    }
  }
  if (!current?.roles?.includes("ADMIN"))
    return <section className="card">Administrator access required.</section>;
  return (
    <>
      <h1>Users and access</h1>
      <p>
        One identity can own paper and funded trading accounts. Roles control
        application access; account eligibility controls trading.
      </p>
      {message && <p role="status">{message}</p>}
      <section className="card">
        <h2>Create user</h2>
        <form onSubmit={createUser}>
          <fieldset disabled={busy}>
            <legend>Account details</legend>
            {["firstName", "lastName", "email", "password"].map((field) => (
              <label key={field}>
                {
                  {
                    firstName: "First name",
                    lastName: "Last name",
                    email: "Email",
                    password: "Initial password",
                  }[field]
                }
                <input
                  required
                  type={
                    field === "password"
                      ? "password"
                      : field === "email"
                        ? "email"
                        : "text"
                  }
                  autoComplete={field === "password" ? "new-password" : "off"}
                  minLength={field === "password" ? 12 : 1}
                  maxLength={
                    field === "email" ? 254 : field === "password" ? 72 : 100
                  }
                  value={create[field]}
                  onChange={(e) =>
                    setCreate({ ...create, [field]: e.target.value })
                  }
                />
              </label>
            ))}
            <label>
              Role
              <select
                value={create.role}
                onChange={(e) => setCreate({ ...create, role: e.target.value })}
              >
                <option>TRADER</option>
                <option>MANAGER</option>
                <option>ADMIN</option>
              </select>
            </label>
            <button className="btn">Create user</button>
          </fieldset>
        </form>
      </section>
      <section className="card">
        <table>
          <thead>
            <tr>
              <th>User</th>
              <th>Roles</th>
              <th>Status</th>
              <th>Sign-in</th>
              <th>Manage</th>
            </tr>
          </thead>
          <tbody>
            {users.map((user) => (
              <tr key={user.userId}>
                <td>
                  {user.firstName} {user.lastName}
                  <br />
                  {user.email}
                </td>
                <td>{user.roles.join(", ")}</td>
                <td>{user.status}</td>
                <td>{user.hasGoogleLogin ? "Google" : "Password"}</td>
                <td>
                  <button
                    className="btn secondary"
                    onClick={() => select(user)}
                  >
                    View access
                  </button>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
        {next && (
          <button
            className="btn secondary"
            onClick={() => load(next).catch((e) => setMessage(e.message))}
          >
            Load more
          </button>
        )}
        {!users.length && <p>No users found.</p>}
      </section>
      {selected && (
        <section className="card">
          <h2>{selected.email}</h2>
          <form onSubmit={save}>
            <fieldset disabled={busy || selected.userId === current?.userId}>
              <legend>Access</legend>
              {["TRADER", "MANAGER", "ADMIN"].map((role) => (
                <label key={role}>
                  <input
                    type="checkbox"
                    checked={roles.includes(role)}
                    onChange={(e) =>
                      setRoles(
                        e.target.checked
                          ? [...roles, role]
                          : roles.filter((x) => x !== role),
                      )
                    }
                  />
                  {role}
                </label>
              ))}
              <label>
                Status
                <select
                  value={status}
                  onChange={(e) => setStatus(e.target.value)}
                >
                  <option>Active</option>
                  <option>Suspended</option>
                </select>
              </label>
              <label>
                Reason
                <input
                  required
                  maxLength={300}
                  value={reason}
                  onChange={(e) => setReason(e.target.value)}
                />
              </label>
              <button className="btn" disabled={!roles.length}>
                Save access
              </button>
            </fieldset>
          </form>
          <h3>Identity history</h3>
          <ul>
            {history.map((entry) => (
              <li key={entry.eventId}>
                {new Date(entry.createdAt).toLocaleString()} — {entry.eventType}{" "}
                / {entry.method} — {entry.succeeded ? "Succeeded" : "Failed"}
                {entry.detail && `: ${entry.detail}`}
              </li>
            ))}
          </ul>
        </section>
      )}
    </>
  );
}
