import { useQuery } from "@tanstack/react-query";

const API_URL = import.meta.env.VITE_API_URL;

async function fetchPageCount(size: number): Promise<number> {
  const result = await fetch(`${API_URL}/files/pagecount?pageSize=${size}`, {
    credentials: "include" as RequestCredentials,
  });

  if (!result.ok) {
    throw new Error();
  }

  return result.json();
}

export function usePageCount(size: number) {
  return useQuery({
    queryKey: ["page", { size }],
    queryFn: () => fetchPageCount(size),
  });
}
