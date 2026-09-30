import React from "react";
import { useState, useEffect } from "react";
import { api } from "../../api";
export default function Trade() {
  const [accounts, setAccounts] = useState([]),
    [q, setQ] = useState(""),
    [items, setItems] = useState([]),
    [form, setForm] = useState({
      accountId: "",
      instrumentToken: "",
      symbol: "",
      side: "BUY",
      orderType: "MARKET",
      quantity: 1,
      price: null,
      lastPrice: null,
    }),
    [msg, setMsg] = useState("");
  useEffect(() => {
    api("/accounts").then((a) => {
      setAccounts(a);
      if (a[0]) setForm((f) => ({ ...f, accountId: a[0].accountId }));
    });
  }, []);
  async function search(v) {
    setQ(v);
    if (v.length > 1)
      setItems(await api("/market/instruments?q=" + encodeURIComponent(v)));
  }
  async function place() {
    try {
      const r = await api("/orders", {
        method: "POST",
        body: JSON.stringify({
          ...form,
          quantity: Number(form.quantity),
          price: form.orderType === "LIMIT" ? Number(form.price) : null,
          idempotencyKey: crypto.randomUUID(),
        }),
      });
      setMsg(`Order ${r.orderId} ${r.status}`);
    } catch (e) {
      setMsg(e.message);
    }
  }
  return (
    <div>
      <h1>Trade</h1>
      <div className="grid">
        <div className="card">
          <label>Account</label>
          <select
            className="field"
            value={form.accountId}
            onChange={(e) => setForm({ ...form, accountId: e.target.value })}
          >
            {accounts.map((a) => (
              <option key={a.accountId} value={a.accountId}>
                {a.accountNumber}
              </option>
            ))}
          </select>
          <label>Find NSE equity</label>
          <input
            className="field"
            value={q}
            onChange={(e) => search(e.target.value)}
            placeholder="INFY, TCS..."
          />
          {items.map((i) => (
            <button
              className="field"
              style={{ textAlign: "left" }}
              key={i.instrumentId}
              onClick={() => {
                api(
                  "/market/quote?instrumentToken=" +
                    encodeURIComponent(i.instrumentToken),
                ).then((qt) =>
                  setForm({
                    ...form,
                    instrumentToken: i.instrumentToken,
                    symbol: i.tradingSymbol,
                    lastPrice: qt.lastPrice,
                  }),
                );
                setQ(i.tradingSymbol);
                setItems([]);
              }}
            >
              {i.tradingSymbol} · {i.name}
            </button>
          ))}
        </div>
        <div className="card">
          <h3>{form.symbol || "Select stock"}</h3>
          <select
            className="field"
            value={form.side}
            onChange={(e) => setForm({ ...form, side: e.target.value })}
          >
            <option>BUY</option>
            <option>SELL</option>
          </select>
          <input
            className="field"
            type="number"
            min="1"
            value={form.quantity}
            onChange={(e) => setForm({ ...form, quantity: e.target.value })}
            placeholder="Quantity"
          />
          <div className="card" style={{ marginBottom: 14 }}>
            Live LTP:{" "}
            <strong>{form.lastPrice ? `₹${form.lastPrice}` : "—"}</strong>
          </div>
          <select
            className="field"
            value={form.orderType}
            onChange={(e) => setForm({ ...form, orderType: e.target.value })}
          >
            <option>MARKET</option>
            <option>LIMIT</option>
          </select>
          {form.orderType === "LIMIT" && (
            <input
              className="field"
              type="number"
              value={form.price || ""}
              onChange={(e) => setForm({ ...form, price: e.target.value })}
              placeholder="Limit price"
            />
          )}
          <button
            className="btn"
            disabled={!form.instrumentToken}
            onClick={place}
          >
            Place order
          </button>
          {msg && <p>{msg}</p>}
        </div>
      </div>
    </div>
  );
}
