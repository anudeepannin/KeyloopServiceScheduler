
import { useState } from "react";
import AppLayout from "./pages/AppLayout";
import Dashboard from "./pages/Dashboard";
import AppointmentsPage from "./pages/AppointmentsPage";
import NewBooking from "./pages/NewBooking";

import "./App.css";

type Page = "dashboard" | "booking" | "appointments";

function App() {
  const [activePage, setActivePage] = useState<Page>("dashboard");

  return (
    <AppLayout
      activePage={activePage}
      onNavigate={(page) => {
        if (
          page === "dashboard" ||
          page === "booking" ||
          page === "appointments"
        ) {
          setActivePage(page);
        }
      }}
    >
      {activePage === "dashboard" && (
        <Dashboard
          onNewBooking={() => setActivePage("booking")}
          onViewAppointments={() => setActivePage("appointments")}
        />
      )}

      {activePage === "booking" && <NewBooking />}

      {activePage === "appointments" && <AppointmentsPage />}
    </AppLayout>
  );
}

export default App;
