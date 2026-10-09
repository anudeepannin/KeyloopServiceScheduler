
import { useEffect, useState } from "react";
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

  useEffect(() => {
    void loadAppointments();
  }, []);

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
    <section className="my-appointments">
      {loading && !loaded && <p>Loading appointments...</p>}

      {error && <p role="alert" className="error-message">{error}</p>}

      {loaded && appointments.length === 0 && !error && (
        <p>No appointments found.</p>
      )}

      {appointments.map((item) => (
        <article
          key={item.appointmentId}
          className="appointment-card"
        >
          <div className="appointment-card-header">
            <h3>Appointment #{item.appointmentId}</h3>
            <span
              className={`appointment-status ${
                item.status.toLowerCase() === "cancelled"
                  ? "cancelled"
                  : "confirmed"
              }`}
            >
              {item.status}
            </span>
          </div>

          <div className="appointment-details">
            <p><strong>Customer:</strong> {item.customerName}</p>
            <p><strong>Email:</strong> {item.customerEmail}</p>
            <p>
              <strong>Vehicle:</strong> {item.vehicleYear}{" "}
              {item.vehicleMake} {item.vehicleModel}
            </p>
            <p><strong>VIN:</strong> {item.vehicleVin}</p>
            <p><strong>Service:</strong> {item.serviceName}</p>
            <p><strong>Dealership:</strong> {item.dealershipName}</p>
            <p><strong>Technician:</strong> {item.technicianName}</p>
            <p><strong>Service bay:</strong> {item.serviceBayName}</p>
            <p>
              <strong>Start:</strong>{" "}
              {new Date(item.startTimeUtc).toLocaleString()}
            </p>
            <p>
              <strong>End:</strong>{" "}
              {new Date(item.endTimeUtc).toLocaleString()}
            </p>
          </div>

          {item.status.toLowerCase() !== "cancelled" && (
            <button
              className="cancel-appointment-button"
              onClick={() => handleCancel(item.appointmentId)}
              disabled={loading}
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
