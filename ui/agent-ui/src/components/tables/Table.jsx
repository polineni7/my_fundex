import React from "react";
export default function Table({ rows, cols }) {
  return (
    <div style={{ overflow: "auto" }}>
      <table className="table">
        <thead>
          <tr>
            {cols.map((c) => (
              <th key={c}>{c}</th>
            ))}
          </tr>
        </thead>
        <tbody>
          {rows.map((r, i) => (
            <tr key={i}>
              {cols.map((c) => (
                <td key={c}>
                  {typeof r[c] === "object"
                    ? JSON.stringify(r[c])
                    : String(r[c] ?? "")}
                </td>
              ))}
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}
