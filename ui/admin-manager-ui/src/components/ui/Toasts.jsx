import React, { useEffect, useState } from "react";
import { CheckCircle2, AlertCircle, X } from "lucide-react";
export function notify(message, kind = "success") {
  window.dispatchEvent(
    new CustomEvent("fundex-toast", {
      detail: { message, kind, id: crypto.randomUUID() },
    }),
  );
}
export default function Toasts() {
  const [items, setItems] = useState([]);
  useEffect(() => {
    const timers = new Set();
    const receive = ({ detail }) => {
      setItems((old) => [...old.slice(-3), detail]);
      const timer = setTimeout(() => {
        setItems((old) => old.filter((x) => x.id !== detail.id));
        timers.delete(timer);
      }, 7000);
      timers.add(timer);
    };
    window.addEventListener("fundex-toast", receive);
    return () => {
      window.removeEventListener("fundex-toast", receive);
      timers.forEach(clearTimeout);
    };
  }, []);
  return (
    <div className="toast-stack" aria-live="polite">
      {items.map((item) => (
        <div
          key={item.id}
          className={`toast ${item.kind}`}
          role={item.kind === "error" ? "alert" : "status"}
        >
          {item.kind === "error" ? (
            <AlertCircle size={21} />
          ) : (
            <CheckCircle2 size={21} />
          )}
          <span>{item.message}</span>
          <button
            className="icon-button"
            aria-label="Dismiss notification"
            onClick={() =>
              setItems((old) => old.filter((x) => x.id !== item.id))
            }
          >
            <X size={16} />
          </button>
        </div>
      ))}
    </div>
  );
}
