import React, { useEffect, useState } from "react";
import { api, API } from "./api";
import { useAuth } from "./store";

export default function GoogleSignIn() {
  const [state, setState] = useState({
    loading: true,
    enabled: false,
    error: "",
  });
  useEffect(() => {
    let active = true;
    if (
      new URLSearchParams(window.location.search).get("google") === "complete"
    ) {
      api("/auth/google/exchange", { method: "POST", credentials: "include" })
        .then((result) => {
          useAuth.getState().login(result.accessToken, result.user);
          window.history.replaceState({}, "", "/");
        })
        .catch((error) => {
          if (active)
            setState({ loading: false, enabled: true, error: error.message });
        });
    } else {
      api("/auth/providers")
        .then((result) => {
          if (active)
            setState({ loading: false, enabled: result.google, error: "" });
        })
        .catch((error) => {
          if (active)
            setState({ loading: false, enabled: false, error: error.message });
        });
    }
    return () => {
      active = false;
    };
  }, []);
  return (
    <div>
      <button
        className="btn"
        type="button"
        disabled={state.loading || !state.enabled}
        onClick={() => window.location.assign(`${API}/auth/google`)}
      >
        {state.loading ? "Preparing sign-in…" : "Continue with Google"}
      </button>
      {!state.loading && !state.enabled && (
        <p className="muted">Google sign-in is awaiting configuration.</p>
      )}
      {state.error && (
        <p className="error" role="alert">
          {state.error}
        </p>
      )}
    </div>
  );
}
