import React from "react";
import { useState, useEffect } from "react";
import { api } from "../../api";
import Card from "../../components/ui/Card";
import { Link } from "react-router-dom";
import { Layers } from "lucide-react";
import Table from "../../components/tables/Table";
export default function Dashboard() {
  const [a, setA] = useState([]);
  const [error, setError] = useState(""),
    [loading, setLoading] = useState(true);
  useEffect(() => {
    let active = true;
    api("/admin/accounts")
      .then((data) => {
        if (active) setA(data);
      })
      .catch((e) => {
        if (active) setError(e.message);
      })
      .finally(() => {
        if (active) setLoading(false);
      });
    return () => {
      active = false;
    };
  }, []);
  return (
    <>
      <div className="top">
        <div>
          <h1>Overview</h1>
          <span className="muted">Your operations at a glance</span>
        </div>
        <Link className="btn secondary" to="/plans">
          Manage assessment plans
        </Link>
      </div>
      <section className="dashboard-welcome">
        <div>
          <h2>Welcome to your workspace</h2>
          <p>
            Manage assessments, monitor accounts, and keep your team in sync.
          </p>
        </div>
        <Layers size={55} />
      </section>
      {error && (
        <p className="error" role="alert">
          {error}
        </p>
      )}
      {loading && <p role="status">Loading account overview…</p>}
      {!loading && !error && (
        <>
          <div className="grid">
            <Card t="Funded accounts" v={a.length} />
            <Card
              t="Active"
              v={a.filter((x) => x.status === "Active").length}
            />
            <Card
              t="Suspended"
              v={a.filter((x) => x.status === "Suspended").length}
            />
            <Card
              t="Capital monitored"
              v={
                "₹" +
                a
                  .reduce((s, x) => s + Number(x.fundedCapital || 0), 0)
                  .toLocaleString()
              }
            />
          </div>
          <div className="card" style={{ marginTop: 16 }}>
            <div className="top">
              <h2>Recent accounts</h2>
              <Link to="/accounts">View all accounts</Link>
            </div>
            <Table
              rows={a.slice(0, 20)}
              cols={[
                "accountNumber",
                "status",
                "fundedCapital",
                "currentBuyingPower",
                "version",
                "createdAt",
              ]}
            />
          </div>
        </>
      )}
    </>
  );
}
