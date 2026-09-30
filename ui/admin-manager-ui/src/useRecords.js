import { useEffect, useState } from "react";
import { api } from "./api";
export function useRecords(path) {
  const [rows, setRows] = useState([]),
    [loading, setLoading] = useState(true),
    [error, setError] = useState("");
  useEffect(() => {
    let active = true;
    setLoading(true);
    setError("");
    setRows([]);
    api(path)
      .then((data) => {
        if (active) setRows(data);
      })
      .catch((error) => {
        if (active) setError(error.message);
      })
      .finally(() => {
        if (active) setLoading(false);
      });
    return () => {
      active = false;
    };
  }, [path]);
  return { rows, loading, error };
}
