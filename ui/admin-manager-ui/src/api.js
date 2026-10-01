import { notify } from "./components/ui/Toasts";
import { useAuth, getAccessToken } from "./store";
const API = import.meta.env.VITE_API_URL || "http://localhost:50901/api/v1";
export async function api(path, options = {}) {
  const { successMessage, ...requestOptions } = options;
  const token = getAccessToken();
  const headers = {
    "Content-Type": "application/json",
    ...(token ? { Authorization: `Bearer ${token}` } : {}),
    ...(options.headers || {}),
  };
  let response;
  try {
    response = await fetch(`${API}${path}`, { ...requestOptions, headers });
  } catch {
    const message = "Unable to connect. Check your connection and try again.";
    notify(message, "error");
    throw new Error(message);
  }
  if (response.status === 401) useAuth.getState().logout();
  if (response.status === 204) {
    notify(successMessage || "Changes saved successfully.");
    return null;
  }
  const body = await response.json().catch(() => null);
  if (!response.ok) {
    const validation = body?.errors
      ? Object.values(body.errors).flat().join(" ")
      : null;
    const message =
      validation ||
      body?.message ||
      body?.detail ||
      body?.title ||
      (response.status === 404
        ? "This API endpoint or record is unavailable. Restart the backend with the latest build, then refresh this page."
        : response.status === 401
          ? "Your session has expired. Please sign in again."
          : `Request failed (${response.status})`);
    notify(message, "error");
    throw new Error(message);
  }
  if (options.method && options.method !== "GET" && path !== "/auth/login")
    notify(successMessage || "Changes saved successfully.");
  return body;
}
