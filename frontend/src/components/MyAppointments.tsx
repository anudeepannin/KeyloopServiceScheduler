
import { useState } from "react";
import {
  getAppointments,
    cancelAppointment,
  type Appointment,
} from "../services/appointmentService";

function MyAppointments() {
  const [appointments, setAppointments] = useState<Appointment[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState("");
  const [loaded, setLoaded] = useState(false);

  async function loadAppointments() {
    setLoading(true);
    setError("");

    try {
      const data = await getAppointments();
      setAppointments(data);
      setLoaded(true);
    } catch (err: unknown) {
      setError(
        err instanceof Error
          ? err.message
          : "Failed to load appointments."
      );
    } finally {
      setLoading(false);
    }
  }
  
async function handleCancel(appointmentId: number) {
  const confirmed = window.confirm(
    `Are you sure you want to cancel appointment #${appointmentId}?`
  );

  if (!confirmed) {
    return;
  }

  setError("");

  try {
    await cancelAppointment(appointmentId);
    await loadAppointments();
  } catch (err: unknown) {
    setError(
      err instanceof Error
        ? err.message
        : "Failed to cancel appointment."
    );
  }
}


  return (
    <section style={{ marginTop: 40 }}>
      <h2>My Appointments</h2>

      <button onClick={loadAppointments} disabled={loading}>
        {loading ? "Loading..." : "View Appointments"}
      </button>

      {error && <p role="alert">{error}</p>}

      {loaded && appointments.length === 0 && !error && (
        <p>No appointments found.</p>
      )}

      {appointments.map((item) => (
        <article
          key={item.appointmentId}
          style={{
            border: "1px solid #ddd",
            borderRadius: 8,
            padding: 16,
            marginTop: 12,
            textAlign: "left",
          }}
        >
          <h3>Appointment #{item.appointmentId}</h3>
          <p>Status: {item.status}</p>
          <p>Customer: {item.customerName}</p>
          <p>Email: {item.customerEmail}</p>
          <p>
            Vehicle: {item.vehicleYear} {item.vehicleMake}{" "}
            {item.vehicleModel}
          </p>
          <p>VIN: {item.vehicleVin}</p>
          <p>Service: {item.serviceName}</p>
          <p>Dealership: {item.dealershipName}</p>
          <p>Technician: {item.technicianName}</p>
          <p>Service bay: {item.serviceBayName}</p>
          <p>
            Start: {new Date(item.startTimeUtc + "Z").toLocaleString()}
          </p>
          <p>
            End: {new Date(item.endTimeUtc + "Z").toLocaleString()}
          </p>
            {item.status !== "Cancelled" && (
                <button
                onClick={() => handleCancel(item.appointmentId)}
                >
                Cancel Appointment
                </button>
            )}
        </article>
      ))}
    </section>
  );
}

export default MyAppointments;
