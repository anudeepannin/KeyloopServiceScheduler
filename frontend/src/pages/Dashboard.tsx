
interface DashboardProps {
  onNewBooking: () => void;
  onViewAppointments: () => void;
}

function Dashboard({
  onNewBooking,
  onViewAppointments,
}: DashboardProps) {
  return (
    <div className="dashboard-page">
      <div className="page-header">
        <div>
          <h1>Dashboard</h1>
          <p>Manage your vehicle service appointments.</p>
        </div>
      </div>

      <div className="dashboard-grid">
        <div className="dashboard-card">
          <div className="dashboard-card-icon">📅</div>
          <h3>Service Booking</h3>
          <p>Schedule a service for a customer vehicle.</p>
          <button onClick={onNewBooking}>
            Create New Booking
          </button>
        </div>

        <div className="dashboard-card">
          <div className="dashboard-card-icon">🔧</div>
          <h3>Appointments</h3>
          <p>View existing appointments and manage cancellations.</p>
          <button onClick={onViewAppointments}>
            View Appointments
          </button>
        </div>
      </div>

      <div className="dashboard-info-card">
        <h2>Service Scheduler</h2>
        <p>
          Select a page from the sidebar to book a service or
          manage existing appointments.
        </p>
      </div>
    </div>
  );
}

export default Dashboard;
