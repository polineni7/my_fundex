import React, { useEffect, useState } from "react";
import { Link } from "react-router-dom";
import { api } from "../../api";
let checkoutScript;
function loadCheckout() {
  if (window.Razorpay) return Promise.resolve();
  if (!checkoutScript)
    checkoutScript = new Promise((resolve, reject) => {
      const script = document.createElement("script");
      script.src = "https://checkout.razorpay.com/v1/checkout.js";
      script.onload = resolve;
      script.onerror = () => {
        checkoutScript = undefined;
        script.remove();
        reject(new Error("Payment checkout could not load. Try again."));
      };
      document.head.appendChild(script);
    });
  return checkoutScript;
}
export default function Subscriptions() {
  const [couponCode, setCouponCode] = useState("");
  const [rows, setRows] = useState([]),
    [busy, setBusy] = useState(null),
    [message, setMessage] = useState("");
  async function refresh() {
    setRows(await api("/subscriptions"));
  }
  useEffect(() => {
    refresh().catch((error) => setMessage(error.message));
  }, []);
  async function pay(subscription) {
    setBusy(subscription.subscriptionId);
    setMessage("");
    try {
      await loadCheckout();
      const checkout = await api("/payments/checkout", {
        method: "POST",
        body: JSON.stringify({
          subscriptionId: subscription.subscriptionId,
          couponCode: couponCode.trim() || null,
          idempotencyKey: crypto.randomUUID(),
        }),
      });
      const widget = new window.Razorpay({
        key: checkout.keyId,
        amount: checkout.amount,
        currency: checkout.currency,
        name: "MyFundex",
        description: subscription.name,
        order_id: checkout.orderId,
        handler: async (response) => {
          try {
            await api(`/payments/${checkout.paymentId}/verify`, {
              method: "POST",
              body: JSON.stringify({
                paymentId: response.razorpay_payment_id,
                signature: response.razorpay_signature,
              }),
            });
            setMessage(
              "Payment verified. Your paper account is being prepared. Refresh shortly to see its status.",
            );
            await refresh();
          } catch (error) {
            setMessage(error.message);
          } finally {
            setBusy(null);
          }
        },
        modal: { ondismiss: () => setBusy(null) },
      });
      widget.on("payment.failed", () =>
        setMessage(
          "Payment was not completed. You can retry in checkout or resume it later.",
        ),
      );
      widget.open();
    } catch (error) {
      setMessage(error.message);
      setBusy(null);
    }
  }
  return (
    <section>
      <h1>Your evaluations</h1>
      <label>Coupon code (optional)<input value={couponCode} maxLength={40} onChange={e=>setCouponCode(e.target.value.toUpperCase())} /></label>
      <p>Coupons apply when a new checkout is created. Existing checkout amounts remain unchanged.</p>
      <p>
        Each purchase starts a separate paper account. Previous attempts stay in
        your history.
      </p>
      <button
        className="btn secondary"
        onClick={() => refresh().catch((error) => setMessage(error.message))}
      >
        Refresh status
      </button>
      {message && <p role="status">{message}</p>}
      {!rows.length && (
        <div className="card">
          <p>No assessment enrolments yet.</p>
          <Link to="/plans">Choose a plan</Link>
        </div>
      )}
      <div className="grid">
        {rows.map((row) => (
          <article className="card" key={row.subscriptionId}>
            <span className="eyebrow">{row.status}</span>
            <h2>{row.name}</h2>
            <p>Plan version {row.versionNumber}</p>
            <p>
              Assessment fee: INR{" "}
              {Number(row.registrationFee).toLocaleString("en-IN")}
            </p>
            {row.status === "PendingPayment" && (
              <button
                className="btn"
                disabled={busy !== null}
                onClick={() => pay(row)}
              >
                {busy === row.subscriptionId
                  ? "Processing..."
                  : "Continue to payment"}
              </button>
            )}
            {row.status === "Evaluation" && (
              <Link to="/trade">Open paper trading</Link>
            )}
            {row.status === "Failed" && (
              <Link to={`/plans?retry=${row.subscriptionId}`}>
                Choose a plan for a fresh attempt
              </Link>
            )}
          </article>
        ))}
      </div>
    </section>
  );
}
