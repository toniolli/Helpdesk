export function apiErrorMessage(error, fallback) {
  const data = error?.response?.data;

  if (typeof data === "string" && data.trim()) return data;

  if (data?.errors && typeof data.errors === "object") {
    const mensagens = Object.values(data.errors).flat().filter(Boolean);
    if (mensagens.length) return mensagens.join(" ");
  }

  return data?.error || data?.detail || data?.message || data?.title || fallback;
}
