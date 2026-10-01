import React, { lazy, Suspense } from "react";
import { BrowserRouter } from "react-router-dom";
import { useAuth } from "./store";
import Toasts from "./components/ui/Toasts";
const Shell = lazy(() => import("./app/Shell"));
const Login = lazy(() => import("./features/auth/Login"));
export default function App() {
  const user = useAuth((state) => state.user);
  return (
    <BrowserRouter>
      <Toasts />
      <Suspense fallback={<p role="status">Loading MyFundex…</p>}>
        {user ? <Shell /> : <Login />}
      </Suspense>
    </BrowserRouter>
  );
}
