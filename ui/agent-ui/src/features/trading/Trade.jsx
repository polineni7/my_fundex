import React from "react";
import { useState, useEffect, useRef } from "react";
import { api } from "../../api";
export default function Trade() {
  const submitting = useRef(false);
  const request = useRef(null);
  const [busy, setBusy] = useState(false);
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
    api("/accounts")
      .then((a) => {
        a = a.filter((account) => account.status === "Active");
        setAccounts(a);
        if (a[0]) setForm((f) => ({ ...f, accountId: a[0].accountId }));
      })
      .catch((error) => setMsg(error.message));
  }, []);
  useEffect(() => {
    if (q.length < 2) {
      setItems([]);
      return;
    }
    const controller = new AbortController();
    const timer = setTimeout(() => {
      api("/market/instruments?q=" + encodeURIComponent(q), {
        signal: controller.signal,
      })
        .then(setItems)
        .catch((error) => {
          if (error.name !== "AbortError") setMsg(error.message);
        });
    }, 250);
    return () => {
      clearTimeout(timer);
      controller.abort();
    };
  }, [q]);
  async function place() {
    if (submitting.current) return;
    submitting.current = true;
    setBusy(true);
    const payload = {
      accountId: form.accountId,
      instrumentToken: form.instrumentToken,
      symbol: form.symbol,
      side: form.side,
      orderType: form.orderType,
      quantity: Number(form.quantity),
      price: form.orderType === "LIMIT" ? Number(form.price) : null,
    };
    const fingerprint = JSON.stringify(payload);
    if (request.current?.fingerprint !== fingerprint)
      request.current = { fingerprint, id: crypto.randomUUID() };
    try {
      const r = await api("/orders", {
        method: "POST",
        body: JSON.stringify({
          ...payload,
          idempotencyKey: request.current.id,
        }),
      });
      setMsg(`Order ${r.orderId} ${r.status}`);
      request.current = null;
    } catch (e) {
      setMsg(e.message);
    } finally {
      submitting.current = false;
      setBusy(false);
    }
  }
  return (
    <div>
      <h1>Trade</h1>
      <p>
        Paper orders simulate fills at market quotes; limit orders wait for their price. Real trading
        uses limit orders and broker-confirmed fills.
      </p>
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
                {a.tradingMode === "Evaluation" ? "Paper" : "Real"} ·{" "}
                {a.accountNumber}
              </option>
            ))}
          </select>
          <label>Find NSE equity</label>
          <input
            className="field"
            value={q}
            onChange={(e) => setQ(e.target.value)}
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
            disabled={busy || !form.instrumentToken || !form.accountId}
            onClick={place}
          >
            {busy ? "Submitting..." : "Place order"}
          </button>
          {msg && <p>{msg}</p>}
        </div>
      </div>
    </div>
  );
}
