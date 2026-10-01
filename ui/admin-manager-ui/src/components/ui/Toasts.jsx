import React, { useEffect } from "react";
import { create } from "zustand";
import { CheckCircle2, AlertCircle, X } from "lucide-react";
let nextId = 0;
const useToasts = create((set) => ({
  items: [],
  add: (item) => set((state) => ({ items: [...state.items.slice(-3), item] })),
  dismiss: (id) =>
    set((state) => ({ items: state.items.filter((item) => item.id !== id) })),
}));
export function notify(message, kind = "success") {
  useToasts.getState().add({ message, kind, id: ++nextId });
}
function Toast({ item }) {
  const dismiss = useToasts((state) => state.dismiss);
  useEffect(() => {
    const timer = setTimeout(() => dismiss(item.id), 8000);
    return () => clearTimeout(timer);
  }, [dismiss, item.id]);
  return (
    <div
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
        onClick={() => dismiss(item.id)}
      >
        <X size={16} />
      </button>
    </div>
  );
}
export default function Toasts() {
  const items = useToasts((state) => state.items);
  return (
    <div className="toast-stack">
      {items.map((item) => (
        <Toast key={item.id} item={item} />
      ))}
    </div>
  );
}
