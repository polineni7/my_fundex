export const API =
  import.meta.env.VITE_API_URL || "http://localhost:50901/api/v1";
export async function api(path, options = {}) {
  const token = localStorage.getItem("token");
  const headers = {
    "Content-Type": "application/json",
    ...(token ? { Authorization: `Bearer ${token}` } : {}),
    ...(options.headers || {}),
  };
  const response = await fetch(`${API}${path}`, { ...options, headers });
  if (response.status === 204) return null;
  const body = await response.json().catch(() => null);
  if (!response.ok) {
    const validation = body?.errors
      ? Object.values(body.errors).flat().join(" ")
      : null;
    throw new Error(
      validation ||
        body?.message ||
        body?.detail ||
        body?.title ||
        `Request failed (${response.status})`,
    );
  }
  return body;
}
