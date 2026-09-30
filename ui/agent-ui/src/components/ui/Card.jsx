import React from "react";
export default function Card({ t, v }) {
  return (
    <div className="card">
      <div className="muted">{t}</div>
      <div className="metric">{v}</div>
    </div>
  );
}
