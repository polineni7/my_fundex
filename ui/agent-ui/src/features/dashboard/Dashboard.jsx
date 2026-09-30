import React from "react";
import { useState, useEffect } from "react";
import { api } from "../../api";
import Card from "../../components/ui/Card";
import Table from "../../components/tables/Table";
export default function Dashboard() {
  const [a, setA] = useState([]),
    [o, setO] = useState([]),
    [p, setP] = useState([]);
  useEffect(() => {
    Promise.all([api("/accounts"), api("/orders"), api("/positions")]).then(
      ([a, o, p]) => {
        setA(a);
        setO(o);
        setP(p);
      },
    );
  }, []);
  const account = a[0];
  return (
    <>
      <h1>Trading dashboard</h1>
      <div className="grid">
        <Card
          t="Buying power"
          v={
            account
              ? `₹${Number(account.currentBuyingPower).toLocaleString()}`
              : "—"
          }
        />
        <Card
          t="Funded capital"
          v={
            account ? `₹${Number(account.fundedCapital).toLocaleString()}` : "—"
          }
        />
        <Card
          t="Open positions"
          v={p.filter((x) => x.status === "Open").length}
        />
        <Card t="Recent orders" v={o.length} />
      </div>
      <section className="card" style={{ marginTop: 16 }}>
        <h3>Recent orders</h3>
        <Table
          rows={o.slice(0, 10)}
          cols={["symbol", "side", "quantity", "status", "createdAt"]}
        />
      </section>
    </>
  );
}
