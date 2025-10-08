import { Navigate, Outlet } from "react-router-dom";

import { useAuth } from "@/hooks";

export default function ProtectedRoute() {
  const { data: user } = useAuth();

  if (!user) {
    return <Navigate to="/login" replace />;
  }

  return <Outlet />;
}
