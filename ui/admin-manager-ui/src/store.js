import { create } from "zustand";
const tokenKey = "fundex.admin.token",
  userKey = "fundex.admin.user";
// Preserve existing administrator sessions once when moving to portal-specific keys.
try {
  const legacyUser = JSON.parse(localStorage.getItem("user") || "null");
  if (
    !localStorage.getItem(tokenKey) &&
    legacyUser?.roles?.some((role) => ["ADMIN", "MANAGER"].includes(role)) &&
    localStorage.getItem("token")
  ) {
    localStorage.setItem(tokenKey, localStorage.getItem("token"));
    localStorage.setItem(userKey, JSON.stringify(legacyUser));
    localStorage.removeItem("token");
    localStorage.removeItem("user");
  }
} catch {
  /* Invalid saved sessions require sign-in. */
}
function savedUser() {
  try {
    const user = JSON.parse(localStorage.getItem(userKey) || "null");
    return localStorage.getItem(tokenKey) ? user : null;
  } catch {
    return null;
  }
}
export const getAccessToken = () => localStorage.getItem(tokenKey);
export const useAuth = create((set) => ({
  user: savedUser(),
  login: (accessToken, user) => {
    localStorage.setItem(tokenKey, accessToken);
    localStorage.setItem(userKey, JSON.stringify(user));
    set({ user });
  },
  updateProfile: (profile) =>
    set((state) => {
      const user = { ...state.user, ...profile };
      localStorage.setItem(userKey, JSON.stringify(user));
      return { user };
    }),
  logout: () => {
    localStorage.removeItem(tokenKey);
    localStorage.removeItem(userKey);
    sessionStorage.removeItem("fundex.admin.planDraft");
    set({ user: null });
  },
}));
window.addEventListener("storage", (event) => {
  if (event.key === tokenKey || event.key === userKey)
    useAuth.setState({ user: savedUser() });
});
