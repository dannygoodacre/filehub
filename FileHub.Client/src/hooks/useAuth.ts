import { useQuery } from "@tanstack/react-query";

import { UserInfo } from "@/types";

const API_URL = import.meta.env.VITE_API_URL;

export default function useAuth() {
  return useQuery({
    queryKey: ["currentUser"],
    queryFn: async (): Promise<UserInfo> => {
      const userInfoResponse = await fetch(`${API_URL}/account/info`, {
        credentials: "include",
      });

      if (!userInfoResponse.ok) {
        throw new Error("User info failed");
      }

      return await userInfoResponse.json();
    },
  });
}
