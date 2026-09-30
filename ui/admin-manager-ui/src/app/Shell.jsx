import Plans from "../features/plans/Plans";
import React from "react";
import { NavLink, Route, Routes } from "react-router-dom";
import { useAuth } from "../store";
import TablePage from "../components/tables/RecordsPage";
import Settings from "../features/settings/Settings";
import Dashboard from "../features/dashboard/Dashboard";
export default function Shell() {
  const logout = useAuth((s) => s.logout);
  return (
    <div className="shell">
      <aside className="side">
        <div className="brand">MyFundex Ops</div>
        <nav className="nav">
          <NavLink to="/">Dashboard</NavLink>
          <NavLink to="/plans">Plans</NavLink>
          <NavLink to="/accounts">Accounts</NavLink>
          <NavLink to="/policies">Policies</NavLink>
          <NavLink to="/settings">Settings</NavLink>
          <NavLink to="/audit">Audit</NavLink>
        </nav>
        <button className="btn secondary" onClick={logout}>
          Sign out
        </button>
      </aside>
      <main className="main">
        <Routes>
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
          <Route
            path="/policies"
            element={
              <TablePage
                path="/admin/policies"
                title="Policies"
                cols={["code", "name", "policyType"]}
              />
            }
          />
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
        </Routes>
      </main>
    </div>
  );
}
