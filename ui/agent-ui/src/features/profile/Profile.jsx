import React, { useEffect, useState } from "react";
import { api } from "../../api";
import { useAuth } from "../../store";

export default function Profile() {
  const [profile, setProfile] = useState(null);
  const [message, setMessage] = useState("");
  const [busy, setBusy] = useState(false);
  const logout = useAuth((s) => s.logout);
  useEffect(() => {
    api("/me")
      .then(setProfile)
      .catch((e) => setMessage(e.message));
  }, []);
  async function save(event) {
    event.preventDefault();
    setBusy(true);
    setMessage("");
    try {
      await api("/me", {
        method: "PUT",
        body: JSON.stringify({
          firstName: profile.firstName,
          lastName: profile.lastName,
          version: profile.version,
        }),
      });
      setProfile(await api("/me"));
      setMessage("Profile updated.");
    } catch (e) {
      setMessage(e.message);
    } finally {
      setBusy(false);
    }
  }
  async function revoke() {
    setBusy(true);
    try {
      await api("/me/logout-all", { method: "POST" });
      logout();
    } catch (e) {
      setMessage(e.message);
      setBusy(false);
    }
  }
  return (
    <>
      <h1>My profile</h1>
      {message && <p role="status">{message}</p>}
      {profile && (
        <section className="card">
          <p>{profile.email}</p>
          <p>Sign-in: {profile.hasGoogleLogin ? "Google" : "Password"}</p>
          <form onSubmit={save}>
            <fieldset disabled={busy}>
              <legend>Personal details</legend>
              <label>
                First name
                <input
                  required
                  maxLength={100}
                  value={profile.firstName}
                  onChange={(e) =>
                    setProfile({ ...profile, firstName: e.target.value })
                  }
                />
              </label>
              <label>
                Last name
                <input
                  maxLength={100}
                  value={profile.lastName}
                  onChange={(e) =>
                    setProfile({ ...profile, lastName: e.target.value })
                  }
                />
              </label>
              <button className="btn">Save profile</button>
            </fieldset>
          </form>
          <p>
            Your profile is shared across your evaluation and funded accounts.
          </p>
          <button className="btn secondary" disabled={busy} onClick={revoke}>
            Sign out on all devices
          </button>
        </section>
      )}
    </>
  );
}
