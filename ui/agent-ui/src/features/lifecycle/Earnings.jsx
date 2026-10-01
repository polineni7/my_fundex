import React, { useEffect, useState } from "react";
import { api } from "../../api";
const money = (value) =>
  value == null
    ? "Unavailable"
    : new Intl.NumberFormat("en-IN", {
        style: "currency",
        currency: "INR",
      }).format(value);
export default function Earnings() {
  const [accounts, setAccounts] = useState([]),
    [account, setAccount] = useState(""),
    [data, setData] = useState(null),
    [error, setError] = useState("");
  useEffect(() => {
    api("/accounts")
      .then((rows) => {
        const live = rows.filter((x) => x.tradingMode === "Funded");
        setAccounts(live);
        setAccount(live[0]?.accountId || "");
      })
      .catch((e) => setError(e.message));
  }, []);
  useEffect(() => {
    let active = true;
    setData(null);
    if (account)
      api(`/accounts/${account}/earnings`)
        .then((x) => {
          if (active) setData(x);
        })
        .catch((e) => {
          if (active) setError(e.message);
        });
    return () => {
      active = false;
    };
  }, [account]);
  return (
    <>
      <h1>Trade earnings</h1>
      {error && <p role="alert">{error}</p>}
      <label>
        Funded account
        <select value={account} onChange={(e) => setAccount(e.target.value)}>
          {accounts.map((x) => (
            <option key={x.accountId} value={x.accountId}>
              {x.accountNumber}
            </option>
          ))}
        </select>
      </label>
      {!accounts.length && (
        <p>
          Earnings become available after evaluation completion and funded
          activation.
        </p>
      )}
      {data && (
        <>
          <p>{data.note}</p>
          <section className="card">
            <h2>Realized sells — provisional split</h2>
            <table>
              <thead>
                <tr>
                  <th>Stock</th>
                  <th>Profit / loss</th>
                  <th>Trader gross</th>
                  <th>Platform</th>
                  <th>Tax withheld</th>
                  <th>Other</th>
                  <th>Net estimate</th>
                </tr>
              </thead>
              <tbody>
                {data.trades.map((x) => (
                  <tr key={x.trade.executionId}>
                    <td>{x.trade.symbol}</td>
                    <td>{money(x.trade.realizedProfit)}</td>
                    <td>{money(x.estimate?.traderGross)}</td>
                    <td>{money(x.estimate?.platform)}</td>
                    <td>{money(x.estimate?.tax)}</td>
                    <td>{money(x.estimate?.other)}</td>
                    <td>{money(x.estimate?.net)}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </section>
          <section className="card">
            <h2>Verified settlements</h2>
            <table>
              <thead>
                <tr>
                  <th>Gross profit</th>
                  <th>Broker charges</th>
                  <th>Platform</th>
                  <th>Tax withheld</th>
                  <th>Other</th>
                  <th>Trader net</th>
                  <th>Wallet</th>
                </tr>
              </thead>
              <tbody>
                {data.settlements.map((x) => (
                  <tr key={x.distributionId}>
                    <td>{money(x.grossProfit)}</td>
                    <td>{money(x.fees)}</td>
                    <td>{money(x.platformReward)}</td>
                    <td>{money(x.taxWithheld)}</td>
                    <td>{money(x.otherDeductions)}</td>
                    <td>{money(x.traderReward)}</td>
                    <td>{x.deliveredAt ? "Credited" : "Pending"}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </section>
        </>
      )}
    </>
  );
}
