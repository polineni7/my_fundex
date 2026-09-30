import React from "react";
import { useState, useEffect } from "react";
import { api } from "../../api";
import Card from "../../components/ui/Card";
import Table from "../../components/tables/Table";
export default function Dashboard() {
  const [a, setA] = useState([]);
  useEffect(() => {
    api("/admin/accounts").then(setA);
  }, []);
  return (
    <>
      <h1>Operations dashboard</h1>
      <div className="grid">
        <Card t="Funded accounts" v={a.length} />
        <Card t="Active" v={a.filter((x) => x.status === "Active").length} />
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
  );
}
