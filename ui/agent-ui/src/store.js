import { create } from 'zustand';
function savedUser() { try { return JSON.parse(localStorage.getItem('user') || 'null'); } catch { return null; } }
export const useAuth = create(set => ({
  user: savedUser(),
  login: (accessToken, user) => { localStorage.setItem('token', accessToken); localStorage.setItem('user', JSON.stringify(user)); set({ user }); },
  logout: () => { localStorage.removeItem('token'); localStorage.removeItem('user'); set({ user: null }); }
}));
