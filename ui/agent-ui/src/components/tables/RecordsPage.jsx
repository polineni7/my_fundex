import React from "react";
import { useRecords } from "../../useRecords";
import Table from "./Table";
export default function Simple({ path, title, cols }) {
  const { rows, loading, error } = useRecords(path);
  return (
    <>
      <h1>{title}</h1>
      <div className="card">
        {loading ? (
          <p role="status">Loading…</p>
        ) : error ? (
          <p role="alert" className="error">
            {error}
          </p>
        ) : rows.length === 0 ? (
          <p>No records yet.</p>
        ) : (
          <Table rows={rows} cols={cols} />
        )}
      </div>
    </>
  );
}
