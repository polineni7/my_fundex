import React from "react";
import { useState, useEffect } from "react";
import { api } from "../../api";
export default function Settings() {
  const [r, setR] = useState([]);
  useEffect(() => {
    api("/admin/settings").then(setR);
  }, []);
  async function edit(x) {
    const value = prompt(
      `Value for ${x.settingKey}`,
      x.value === "********" ? "" : x.value,
    );
    if (value == null) return;
    await api("/admin/settings/" + encodeURIComponent(x.settingKey), {
      method: "PUT",
      body: JSON.stringify({
        value,
        environment: x.environment,
        category: x.category,
      }),
    });
    setR(await api("/admin/settings"));
  }
  return (
    <>
      <h1>Runtime settings</h1>
      <div className="card">
        <table className="table">
          <thead>
            <tr>
              <th>Key</th>
              <th>Value</th>
              <th>Environment</th>
              <th>Category</th>
              <th></th>
            </tr>
          </thead>
          <tbody>
            {r.map((x) => (
              <tr key={x.settingId}>
                <td>{x.settingKey}</td>
                <td>{x.value}</td>
                <td>{x.environment}</td>
                <td>{x.category}</td>
                <td>
                  <button className="btn secondary" onClick={() => edit(x)}>
                    Edit
                  </button>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </>
  );
}
