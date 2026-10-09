
import MyAppointments from "../components/MyAppointments";

function AppointmentsPage() {
  return (
    <div className="appointments-page">
      <div className="page-header">
        <div>
          <h1>Appointments</h1>
          <p>
            View and manage your vehicle service appointments.
          </p>
        </div>
      </div>

      <MyAppointments />
    </div>
  );
}

export default AppointmentsPage;
