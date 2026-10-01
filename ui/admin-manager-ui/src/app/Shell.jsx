import ProfileDialog from "../components/ui/ProfileDialog";
import Policies from "../features/policies/Policies";
import Users from "../features/identity/Users";
import Operations from "../features/lifecycle/Operations";
import Plans from "../features/plans/Plans";
import React, { useState } from "react";
import {
  Layers,
  LayoutDashboard,
  ClipboardList,
  Activity,
  Users as UsersIcon,
  Wallet,
  ShieldCheck,
  Settings as SettingsIcon,
  FileClock,
  LogOut,
  Menu,
  X,
} from "lucide-react";
import ThemeToggle from "../components/ui/ThemeToggle";
import {
  Link,
  NavLink,
  Route,
  Routes,
  useLocation,
  useNavigate,
} from "react-router-dom";
import { useAuth } from "../store";
import TablePage from "../components/tables/RecordsPage";
import Settings from "../features/settings/Settings";
import Dashboard from "../features/dashboard/Dashboard";
export default function Shell() {
  const [profileOpen, setProfileOpen] = useState(false);
  const location = useLocation(),
    navigate = useNavigate();
  const pageNames = {
    "/": "Overview",
    "/plans": "Assessment plans",
    "/policies": "Risk policies",
    "/operations": "Lifecycle management",
    "/users": "Users and access",
    "/accounts": "Funded accounts",
    "/settings": "Settings",
    "/audit": "Audit history",
  };
  const [menuOpen, setMenuOpen] = useState(false);
  const user = useAuth((s) => s.user);
  const logout = useAuth((s) => s.logout);
  return (
    <div className="shell">
      {menuOpen && (
        <button
          className="sidebar-backdrop"
          aria-label="Close navigation"
          onClick={() => setMenuOpen(false)}
        />
      )}
      <aside className={`side ${menuOpen ? "open" : ""}`}>
        <div className="brand">
          <span className="brand-icon">
            <Layers size={23} />
          </span>
          MyFundex
          <button
            className="icon-button mobile-only"
            aria-label="Close navigation"
            onClick={() => setMenuOpen(false)}
          >
            <X size={20} />
          </button>
        </div>
        <div className="sidebar-caption">ADMIN WORKSPACE</div>
        <nav className="nav" aria-label="Main navigation">
          {[
            ["/", "Overview", LayoutDashboard],
            ["/plans", "Assessment plans", ClipboardList],
            ["/operations", "Trading operations", Activity],
            ["/users", "Users & access", UsersIcon],
            ["/accounts", "Funded accounts", Wallet],
            ["/policies", "Risk policies", ShieldCheck],
            ["/settings", "Settings", SettingsIcon],
            ["/audit", "Audit history", FileClock],
          ]
            .filter(
              ([path]) => path !== "/users" || user?.roles?.includes("ADMIN"),
            )
            .map(([path, title, Icon]) => (
              <NavLink
                key={path}
                to={path}
                end={path === "/"}
                onClick={() => setMenuOpen(false)}
              >
                <Icon size={19} />
                {title}
              </NavLink>
            ))}
        </nav>
        <div className="sidebar-bottom">
          <div className="workspace-note">
            <ShieldCheck size={19} />
            <span>
              MyFundex operations
              <br />
              <small>Role-based workspace</small>
            </span>
          </div>
          <button className="btn secondary signout" onClick={logout}>
            <LogOut size={17} />
            Sign out
          </button>
        </div>
      </aside>
      <div className="workspace">
        <header className="app-header">
          <div className="row">
            <button
              className="icon-button mobile-only"
              aria-label="Open navigation"
              aria-expanded={menuOpen}
              onClick={() => setMenuOpen(!menuOpen)}
            >
              <Menu size={21} />
            </button>
            <nav className="header-breadcrumb" aria-label="Breadcrumb">
              <Link
                to="/"
                aria-current={location.pathname === "/" ? "page" : undefined}
              >
                Home
              </Link>
              {location.pathname !== "/" && (
                <>
                  <span aria-hidden="true"> / </span>
                  <span aria-current="page">
                    {pageNames[location.pathname] || "Page not found"}
                  </span>
                </>
              )}
            </nav>
          </div>
          <div className="row">
            <ThemeToggle />
            <span className="header-divider" />
            <button
              className="profile-trigger"
              aria-label="Open my profile"
              onClick={() => setProfileOpen(true)}
            >
              <span className="avatar">
                {(user?.firstName || user?.email || "A")
                  .slice(0, 1)
                  .toUpperCase()}
              </span>
              <div className="profile-label">
                <strong>{user?.firstName || "Team member"}</strong>
                <small>{user?.roles?.join(" · ")}</small>
              </div>
            </button>
          </div>
        </header>
        {profileOpen && <ProfileDialog onClose={() => setProfileOpen(false)} />}
        <main className="main">
          {location.pathname !== "/" && (
            <div className="page-navigation">
              {location.pathname !== "/" && (
                <button
                  className="btn secondary"
                  onClick={() =>
                    location.state?.from
                      ? navigate(location.state.from)
                      : navigate("/")
                  }
                >
                  {location.state?.from === "/plans"
                    ? "Back to assessment plan"
                    : "Back to overview"}
                </button>
              )}
            </div>
          )}
          <Routes>
            <Route path="/users" element={<Users />} />
            <Route path="/operations" element={<Operations />} />
            <Route path="/plans" element={<Plans />} />
            <Route path="/" element={<Dashboard />} />
            <Route
              path="/accounts"
              element={
                <TablePage
                  path="/admin/accounts"
                  title="Funded accounts"
                  cols={[
                    "accountNumber",
                    "tradingMode",
                    "brokerProvider",
                    "status",
                    "fundedCapital",
                    "currentBuyingPower",
                    "version",
                    "createdAt",
                  ]}
                />
              }
            />
            <Route path="/policies" element={<Policies />} />
            <Route path="/settings" element={<Settings />} />
            <Route
              path="/audit"
              element={
                <TablePage
                  path="/admin/audit"
                  title="Audit trail"
                  cols={[
                    "module",
                    "entityType",
                    "entityId",
                    "action",
                    "occurredAt",
                    "correlationId",
                  ]}
                />
              }
            />
            <Route
              path="*"
              element={
                <section className="card">
                  <h1>Page not found</h1>
                  <Link to="/">Return to overview</Link>
                </section>
              }
            />
          </Routes>
        </main>
      </div>
    </div>
  );
}
