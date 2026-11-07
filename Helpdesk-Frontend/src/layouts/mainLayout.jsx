import { Outlet } from "react-router-dom";
import Sidebar from "../components/Sidebar/sidebar";
import Topbar from "../components/Topbar/topbar";
import styles from "./mainLayout.module.css";

export default function MainLayout() {
  return (
    <div className={styles.layout}>
      <Sidebar />
      <div className={styles.content}>
        <Topbar />

        <main className={styles.main}>
          <Outlet />
        </main>
      </div>
    </div>
  );
}
