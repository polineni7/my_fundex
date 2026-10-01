import React from "react";
import { NavLink, Route, Routes } from "react-router-dom";
import { useAuth } from "../store";
import Dashboard from "../features/dashboard/Dashboard";
import Orders from "../features/trading/Orders";
import Trade from "../features/trading/Trade";
import Simple from "../components/tables/RecordsPage";
import Subscriptions from "../features/subscriptions/Subscriptions";
import Challenges from "../features/lifecycle/Challenges";
import Wallet from "../features/lifecycle/Wallet";
import Plans from "../features/plans/Plans";
export default function Shell() {
  const logout = useAuth((s) => s.logout);
  return (
    <div className="shell">
      <aside className="side">
        <div className="brand">MyFundex</div>
        <nav className="nav">
          <NavLink to="/">Dashboard</NavLink>
          <NavLink to="/plans">Plans</NavLink>
          <NavLink to="/subscriptions">Subscriptions</NavLink>
          <NavLink to="/payments">Payments</NavLink>
          <NavLink to="/challenges">Evaluation progress</NavLink>
          <NavLink to="/wallet">Wallet & withdrawals</NavLink>
          <NavLink to="/trade">Trade</NavLink>
          <NavLink to="/orders">Orders</NavLink>
          <NavLink to="/positions">Positions</NavLink>
          <NavLink to="/notifications">Notifications</NavLink>
        </nav>
        <button className="btn secondary" onClick={logout}>
          Sign out
        </button>
      </aside>
      <main className="main">
        <Routes>
          <Route path="/" element={<Dashboard />} />
          <Route path="/plans" element={<Plans />} />
          <Route path="/subscriptions" element={<Subscriptions />} />
          <Route
            path="/payments"
            element={
              <Simple
                path="/payments"
                title="Payments"
                cols={[
                  "paymentId",
                  "amount",
                  "currencyCode",
                  "status",
                  "paidAt",
                ]}
              />
            }
          />
          <Route path="/challenges" element={<Challenges />} />
          <Route path="/wallet" element={<Wallet />} />
          <Route path="/trade" element={<Trade />} />
          <Route path="/orders" element={<Orders />} />
          <Route
            path="/positions"
            element={
              <Simple
                path="/positions"
                title="Positions"
                cols={[
                  "symbol",
                  "quantity",
                  "averageCost",
                  "realizedPnl",
                  "unrealizedPnl",
                  "status",
                ]}
              />
            }
          />
          <Route
            path="/notifications"
            element={
              <Simple
                path="/notifications"
                title="Notifications"
                cols={["type", "title", "message", "isRead", "createdAt"]}
              />
            }
          />
        </Routes>
      </main>
    </div>
  );
}
