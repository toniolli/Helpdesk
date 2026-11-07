import { Route } from "react-router-dom";
import Login from "../pages/Login/login";

const authRoutes = (
  <>
    <Route path="/login" element={<Login />} />
  </>
);

export default authRoutes;
