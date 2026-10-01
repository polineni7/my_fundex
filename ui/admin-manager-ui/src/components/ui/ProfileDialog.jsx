import React, { useEffect, useRef, useState } from "react";
import { api } from "../../api";
import { useAuth } from "../../store";
export default function ProfileDialog({ onClose }) {
  const dialog = useRef(null),
    updateProfile = useAuth((s) => s.updateProfile),
    logout = useAuth((s) => s.logout);
  const [profile, setProfile] = useState(null),
    [error, setError] = useState(""),
    [busy, setBusy] = useState(false),
    [change, setChange] = useState(false);
  const [passwords, setPasswords] = useState({
    currentPassword: "",
    newPassword: "",
    confirmPassword: "",
  });
  useEffect(() => {
    dialog.current.showModal();
    let active = true;
    api("/me")
      .then((data) => {
        if (active) setProfile(data);
      })
      .catch((e) => {
        if (active) setError(e.message);
      });
    return () => {
      active = false;
    };
  }, []);
  async function save(e) {
    e.preventDefault();
    setBusy(true);
    setError("");
    try {
      if (change) {
        if (passwords.newPassword !== passwords.confirmPassword)
          throw new Error("The new passwords do not match.");
        await api("/me/password", {
          method: "POST",
          body: JSON.stringify(passwords),
          successMessage:
            "Password changed. Sign in again with your new password.",
        });
        logout();
        onClose();
      } else {
        const result = await api("/me", {
          method: "PUT",
          body: JSON.stringify({
            firstName: profile.firstName,
            lastName: profile.lastName,
            version: profile.version,
          }),
          successMessage: "Profile updated.",
        });
        updateProfile({
          firstName: profile.firstName,
          lastName: profile.lastName,
          version: result.version,
        });
        onClose();
      }
    } catch (e) {
      setError(e.message);
    } finally {
      setBusy(false);
    }
  }
  return (
    <dialog
      className="profile-dialog"
      ref={dialog}
      onCancel={onClose}
      aria-labelledby="profile-title"
    >
      <div className="top">
        <div>
          <h2 id="profile-title">
            {change ? "Change password" : "My profile"}
          </h2>
          <span className="muted">{profile?.email}</span>
        </div>
        <button
          type="button"
          className="icon-button"
          aria-label="Close profile"
          onClick={onClose}
        >
          ×
        </button>
      </div>
      {error && (
        <p role="alert" className="error">
          {error}
        </p>
      )}
      {!profile ? (
        <p>Loading your profile…</p>
      ) : (
        <form onSubmit={save}>
          <fieldset disabled={busy}>
            {change ? (
              Object.keys(passwords).map((key) => (
                <label key={key}>
                  {
                    {
                      currentPassword: "Current password",
                      newPassword: "New password",
                      confirmPassword: "Confirm new password",
                    }[key]
                  }
                  <input
                    type="password"
                    required
                    minLength={key === "currentPassword" ? 1 : 12}
                    maxLength={72}
                    autoComplete={
                      key === "currentPassword"
                        ? "current-password"
                        : "new-password"
                    }
                    value={passwords[key]}
                    onChange={(e) =>
                      setPasswords({ ...passwords, [key]: e.target.value })
                    }
                  />
                </label>
              ))
            ) : (
              <>
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
                <label>
                  Email
                  <input readOnly value={profile.email} />
                </label>
                <label>
                  Role
                  <input readOnly value={profile.roles.join(", ")} />
                </label>
              </>
            )}
          </fieldset>
          <div className="actions">
            <button
              type="button"
              className="btn secondary"
              disabled={busy}
              onClick={() => {
                setChange(!change);
                setError("");
              }}
            >
              {change ? "Back to profile" : "Change password"}
            </button>
          </div>
          <div className="form-footer">
            <button
              type="button"
              className="btn secondary"
              disabled={busy}
              onClick={onClose}
            >
              Cancel
            </button>
            <button className="btn" disabled={busy}>
              {busy ? "Saving…" : "Save changes"}
            </button>
          </div>
        </form>
      )}
    </dialog>
  );
}
