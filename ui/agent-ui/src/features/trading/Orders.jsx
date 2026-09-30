import React, { useEffect, useState } from "react";
import { api } from "../../api";
export default function Orders() {
  const [orders, setOrders] = useState([]),
    [busy, setBusy] = useState(null),
    [error, setError] = useState("");
  async function refresh() {
    setOrders(await api("/orders"));
  }
  useEffect(() => {
    refresh().catch((e) => setError(e.message));
  }, []);
  async function cancel(order) {
    setBusy(order.orderId);
    setError("");
    try {
      await api(`/orders/${order.orderId}/cancel`, { method: "POST" });
      await refresh();
    } catch (e) {
      setError(e.message);
    } finally {
      setBusy(null);
    }
  }
  return (
    <section>
      <h1>Orders</h1>
      <p>
        Cancellation remains pending until the broker confirms the final fills
        and status.
      </p>
      <button
        className="btn secondary"
        onClick={() => refresh().catch((e) => setError(e.message))}
      >
        Refresh
      </button>
      {error && <p role="alert">{error}</p>}
      <div className="card table-scroll">
        <table className="table">
          <thead>
            <tr>
              <th>Stock</th>
              <th>Account type</th>
              <th>Side</th>
              <th>Quantity</th>
              <th>Status</th>
              <th>Action</th>
            </tr>
          </thead>
          <tbody>
            {orders.map((order) => (
              <tr key={order.orderId}>
                <td>{order.symbol}</td>
                <td>
                  {order.brokerEnvironment === "SANDBOX" ? "Paper" : "Real"}
                </td>
                <td>{order.side}</td>
                <td>{order.quantity}</td>
                <td>{order.status}</td>
                <td>
                  {["Submitted", "PartiallyFilled"].includes(order.status) && (
                    <button
                      className="btn secondary"
                      disabled={busy !== null}
                      onClick={() => cancel(order)}
                    >
                      {busy === order.orderId ? "Requesting..." : "Cancel"}
                    </button>
                  )}
                </td>
              </tr>
            ))}
          </tbody>
        </table>
        {!orders.length && <p>No orders yet.</p>}
      </div>
    </section>
  );
}
