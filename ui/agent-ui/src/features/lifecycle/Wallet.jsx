import React, { useEffect, useRef, useState } from "react";
import { api } from "../../api";
export default function Wallet() {
  const [wallets, setWallets] = useState([]),
    [requests, setRequests] = useState([]),
    [entries, setEntries] = useState([]);
  const [account, setAccount] = useState(""),
    [amount, setAmount] = useState(""),
    [busy, setBusy] = useState(false),
    [message, setMessage] = useState("");
  const reference = useRef(null);
  async function refresh() {
    const [w, r] = await Promise.all([api("/wallets"), api("/withdrawals")]);
    setWallets(w);
    setRequests(r);
    setAccount((current) => current || w[0]?.accountId || "");
  }
  useEffect(() => {
    refresh().catch((e) => setMessage(e.message));
  }, []);
  useEffect(() => {
    let active = true;
    if (account)
      api(`/wallets/${account}/ledger`)
        .then((data) => {
          if (active) setEntries(data);
        })
        .catch((e) => {
          if (active) setMessage(e.message);
        });
    return () => {
      active = false;
    };
  }, [account]);
  async function withdraw(e) {
    e.preventDefault();
    if (busy) return;
    setBusy(true);
    setMessage("");
    const fingerprint = `${account}:${amount}`;
    if (reference.current?.fingerprint !== fingerprint)
      reference.current = { fingerprint, id: crypto.randomUUID() };
    try {
      await api("/withdrawals", {
        method: "POST",
        body: JSON.stringify({
          accountId: account,
          amount: Number(amount),
          requestId: reference.current.id,
        }),
      });
      reference.current = null;
      setAmount("");
      setMessage("Funds reserved. Your withdrawal is awaiting review.");
      await refresh();
    } catch (error) {
      setMessage(error.message);
    } finally {
      setBusy(false);
    }
  }
  const wallet = wallets.find((x) => x.accountId === account);
  return (
    <section>
      <p className="eyebrow">REAL ACCOUNT REWARDS</p>
      <h1>Wallet & withdrawals</h1>
      <p>
        Only verified, settled trader rewards can be withdrawn. Paper profits
        are never withdrawable.
      </p>
      {message && <p role="status">{message}</p>}
      {!wallets.length ? (
        <div className="card">
          Your reward wallet becomes available with a real funded account.
        </div>
      ) : (
        <div className="grid">
          <form className="card" onSubmit={withdraw}>
            <label>
              Real account
              <select
                className="field"
                value={account}
                onChange={(e) => setAccount(e.target.value)}
              >
                {wallets.map((w) => (
                  <option key={w.accountId} value={w.accountId}>
                    {w.accountId}
                  </option>
                ))}
              </select>
            </label>
            <h2>
              INR{" "}
              {Number(wallet?.cachedWithdrawableBalance ?? 0).toLocaleString(
                "en-IN",
              )}
            </h2>
            <p>Available to withdraw</p>
            <label>
              Amount in INR
              <input
                className="field"
                required
                type="number"
                min="1"
                step="0.01"
                max={wallet?.cachedWithdrawableBalance ?? 0}
                value={amount}
                onChange={(e) => setAmount(e.target.value)}
              />
            </label>
            <button className="btn" disabled={busy}>
              {busy ? "Reserving..." : "Request withdrawal"}
            </button>
          </form>
          <div className="card">
            <h2>Account ledger</h2>
            <div className="table-scroll">
              <table className="table">
                <thead>
                  <tr>
                    <th>Type</th>
                    <th>Direction</th>
                    <th>INR</th>
                  </tr>
                </thead>
                <tbody>
                  {entries.map((e) => (
                    <tr key={e.entryId}>
                      <td>{e.transactionType}</td>
                      <td>{e.direction}</td>
                      <td>{e.amount}</td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          </div>
        </div>
      )}
      <div className="card">
        <h2>Withdrawal history</h2>
        <button
          className="btn secondary"
          onClick={() => refresh().catch((e) => setMessage(e.message))}
        >
          Refresh status
        </button>
        <table className="table">
          <thead>
            <tr>
              <th>Requested</th>
              <th>INR</th>
              <th>Status</th>
            </tr>
          </thead>
          <tbody>
            {requests.map((r) => (
              <tr key={r.withdrawalId}>
                <td>{new Date(r.requestedAt).toLocaleString()}</td>
                <td>{r.requestedAmount}</td>
                <td>{r.status}</td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </section>
  );
}
