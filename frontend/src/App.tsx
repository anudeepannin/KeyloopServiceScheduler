
import { useEffect, useState } from "react";
import {
  getDealerships,
  getServiceTypes,
  getAvailability,
  createAppointment,
  type Dealership,
  type ServiceType,
  type Availability,
  type AppointmentResponse,
} from "./services/api";
import "./App.css";

function App() {
  const [dealerships, setDealerships] = useState<Dealership[]>([]);
  const [serviceTypes, setServiceTypes] = useState<ServiceType[]>([]);
  const [selectedDealershipId, setSelectedDealershipId] = useState<number | null>(null);
  const [selectedServiceTypeId, setSelectedServiceTypeId] = useState<number | null>(null);

  const [serviceDate, setServiceDate] = useState("");
  const [serviceTime, setServiceTime] = useState("09:00");

  const [availability, setAvailability] = useState<Availability[]>([]);
  const [selectedSlot, setSelectedSlot] = useState<Availability | null>(null);

  const [customerId, setCustomerId] = useState("1");
  const [vehicleId, setVehicleId] = useState("1");

  const [loading, setLoading] = useState(true);
  const [loadingServices, setLoadingServices] = useState(false);
  const [searching, setSearching] = useState(false);
  const [booking, setBooking] = useState(false);

  const [error, setError] = useState("");
  const [message, setMessage] = useState("");
  const [appointment, setAppointment] =
    useState<AppointmentResponse | null>(null);

  // Load dealerships
  useEffect(() => {
    let cancelled = false;

    async function loadDealerships() {
      try {
        const data = await getDealerships();

        if (!cancelled) {
          setDealerships(data);

          if (data.length > 0) {
            setSelectedDealershipId(data[0].dealershipId);
          }
        }
      } catch (err: unknown) {
        if (!cancelled) {
          setError(
            err instanceof Error
              ? err.message
              : "Failed to load dealerships."
          );
        }
      } finally {
        if (!cancelled) {
          setLoading(false);
        }
      }
    }

    void loadDealerships();

    return () => {
      cancelled = true;
    };
  }, []);

  // Load services whenever the dealership changes
  useEffect(() => {
    let cancelled = false;

    async function loadServices() {
      if (selectedDealershipId === null) {
        setServiceTypes([]);
        setSelectedServiceTypeId(null);
        return;
      }

      setLoadingServices(true);
      setServiceTypes([]);
      setSelectedServiceTypeId(null);
      setAvailability([]);
      setSelectedSlot(null);
      setAppointment(null);
      setMessage("");
      setError("");

      try {
        const data = await getServiceTypes(selectedDealershipId);

        if (!cancelled) {
          setServiceTypes(data);
        }
      } catch (err: unknown) {
        if (!cancelled) {
          setError(
            err instanceof Error
              ? err.message
              : "Failed to load service types."
          );
        }
      } finally {
        if (!cancelled) {
          setLoadingServices(false);
        }
      }
    }

    void loadServices();

    return () => {
      cancelled = true;
    };
  }, [selectedDealershipId]);

  // Search for available technicians and bays
  async function searchAvailability() {
    if (
      selectedDealershipId === null ||
      selectedServiceTypeId === null ||
      !serviceDate ||
      !serviceTime
    ) {
      setError("Please select a dealership, service, date, and time.");
      return;
    }

    const selectedDateTime = new Date(
      `${serviceDate}T${serviceTime}:00`
    );

    if (Number.isNaN(selectedDateTime.getTime())) {
      setError("Please enter a valid date and time.");
      return;
    }

    if (selectedDateTime.getTime() <= Date.now()) {
      setError("Please select a future date and time.");
      return;
    }

    setSearching(true);
    setError("");
    setMessage("");
    setAvailability([]);
    setSelectedSlot(null);
    setAppointment(null);

    try {
      // Convert the browser-local date/time to UTC for the API.
      const startTimeUtc = selectedDateTime.toISOString();

      const results = await getAvailability(
        selectedDealershipId,
        selectedServiceTypeId,
        startTimeUtc
      );

      setAvailability(results);

      if (results.length === 0) {
        setMessage("No availability found. Try another date or time.");
      }
    } catch (err: unknown) {
      setError(
        err instanceof Error
          ? err.message
          : "Failed to search availability."
      );
    } finally {
      setSearching(false);
    }
  }

  // Book the selected slot
  async function bookAppointment() {
    if (
      !selectedSlot ||
      selectedDealershipId === null ||
      selectedServiceTypeId === null
    ) {
      setError("Please select an available slot.");
      return;
    }

    const parsedCustomerId = Number(customerId);
    const parsedVehicleId = Number(vehicleId);

    if (
      !Number.isInteger(parsedCustomerId) ||
      parsedCustomerId <= 0 ||
      !Number.isInteger(parsedVehicleId) ||
      parsedVehicleId <= 0
    ) {
      setError("Enter valid positive customer and vehicle IDs.");
      return;
    }

    setBooking(true);
    setError("");
    setMessage("");
    setAppointment(null);

    try {
      const result = await createAppointment({
        customerId: parsedCustomerId,
        vehicleId: parsedVehicleId,
        dealershipId: selectedDealershipId,
        serviceTypeId: selectedServiceTypeId,
        technicianId: selectedSlot.technicianId,
        serviceBayId: selectedSlot.serviceBayId,
        startTimeUtc: selectedSlot.startTimeUtc,
      });

      setAppointment(result);
      setMessage("Your appointment was booked successfully.");
    } catch (err: unknown) {
      setError(
        err instanceof Error ? err.message : "Appointment booking failed."
      );
    } finally {
      setBooking(false);
    }
  }

  const selectedDealership = dealerships.find(
    (d) => d.dealershipId === selectedDealershipId
  );

  const selectedService = serviceTypes.find(
    (s) => s.serviceTypeId === selectedServiceTypeId
  );

  return (
    <main
      style={{
        maxWidth: 900,
        margin: "40px auto",
        padding: 24,
      }}
    >
      <h1>Keyloop Service Scheduler</h1>
      <p>Book your vehicle service appointment.</p>

      {loading && <p>Loading dealerships...</p>}

      {error && (
        <p role="alert" style={{ color: "#ff7777" }}>
          {error}
        </p>
      )}

      {!loading && (
        <>
          {/* Dealership selection */}
          <section>
            <h2>Select Dealership</h2>

            <select
              value={selectedDealershipId ?? ""}
              onChange={(event) =>
                setSelectedDealershipId(
                  event.target.value
                    ? Number(event.target.value)
                    : null
                )
              }
            >
              <option value="">Select a dealership</option>

              {dealerships.map((dealership) => (
                <option
                  key={dealership.dealershipId}
                  value={dealership.dealershipId}
                >
                  {dealership.name}
                </option>
              ))}
            </select>

            {selectedDealership && (
              <div>
                <p>{selectedDealership.address}</p>
                <p>Time zone: {selectedDealership.timeZoneId}</p>
              </div>
            )}
          </section>

          {/* Service selection */}
          <section>
            <h2>Select Service</h2>

            {loadingServices && <p>Loading services...</p>}

            {!loadingServices && serviceTypes.length === 0 && (
              <p>No services available for this dealership.</p>
            )}

            {serviceTypes.map((service) => (
              <label
                key={service.serviceTypeId}
                style={{
                  display: "block",
                  border: "1px solid #ddd",
                  borderRadius: 8,
                  padding: 16,
                  marginBottom: 12,
                  cursor: "pointer",
                }}
              >
                <input
                  type="radio"
                  name="serviceType"
                  value={service.serviceTypeId}
                  checked={
                    selectedServiceTypeId === service.serviceTypeId
                  }
                  onChange={() => {
                    setSelectedServiceTypeId(service.serviceTypeId);
                    setAvailability([]);
                    setSelectedSlot(null);
                    setAppointment(null);
                    setMessage("");
                    setError("");
                  }}
                />

                <strong style={{ marginLeft: 8 }}>
                  {service.name}
                </strong>

                <p>Duration: {service.durationMinutes} minutes</p>
                <p>Required skill: {service.requiredSkill}</p>
              </label>
            ))}
          </section>

          {/* Date and time selection */}
          <section>
            <h2>Choose Date and Time</h2>

            <label>
              Service date:{" "}
              <input
                type="date"
                value={serviceDate}
                onChange={(event) => {
                  setServiceDate(event.target.value);
                  setAvailability([]);
                  setSelectedSlot(null);
                  setAppointment(null);
                  setMessage("");
                }}
              />
            </label>

            <br />
            <br />

            <label>
              Start time:{" "}
              <input
                type="time"
                value={serviceTime}
                onChange={(event) => {
                  setServiceTime(event.target.value);
                  setAvailability([]);
                  setSelectedSlot(null);
                  setAppointment(null);
                  setMessage("");
                }}
              />
            </label>

            <br />
            <br />

            <button
              onClick={searchAvailability}
              disabled={searching || loadingServices}
            >
              {searching ? "Searching..." : "Check Availability"}
            </button>
          </section>

          {/* Availability results */}
          {message && !appointment && (
            <p role="status">{message}</p>
          )}

          {availability.length > 0 && (
            <section>
              <h2>Available Slots</h2>

              {availability.map((slot) => {
                const isSelected =
                  selectedSlot?.technicianId === slot.technicianId &&
                  selectedSlot?.serviceBayId === slot.serviceBayId &&
                  selectedSlot?.startTimeUtc === slot.startTimeUtc;

                return (
                  <label
                    key={`${slot.technicianId}-${slot.serviceBayId}-${slot.startTimeUtc}`}
                    style={{
                      display: "block",
                      border: isSelected
                        ? "2px solid #61a5fa"
                        : "1px solid #ddd",
                      borderRadius: 8,
                      padding: 16,
                      marginBottom: 12,
                      cursor: "pointer",
                    }}
                  >
                    <input
                      type="radio"
                      name="availableSlot"
                      checked={isSelected}
                      onChange={() => {
                        setSelectedSlot(slot);
                        setAppointment(null);
                        setMessage("");
                        setError("");
                      }}
                    />

                    <strong style={{ marginLeft: 8 }}>
                      Select this slot
                    </strong>

                    <p>Technician: {slot.technicianName}</p>
                    <p>Service bay: {slot.bayName}</p>
                    <p>
                      Start:{" "}
                      {new Date(slot.startTimeUtc).toLocaleString()}
                    </p>
                    <p>
                      End:{" "}
                      {new Date(slot.endTimeUtc).toLocaleString()}
                    </p>
                  </label>
                );
              })}
            </section>
          )}

          {/* Booking form */}
          {selectedSlot && !appointment && (
            <section>
              <h2>Customer and Vehicle Details</h2>

              <p>
                Service: {selectedService?.name}
              </p>
              <p>
                Technician: {selectedSlot.technicianName}
              </p>
              <p>
                Service bay: {selectedSlot.bayName}
              </p>

              <div style={{ marginBottom: 16 }}>
                <label>
                  Customer ID:
                  <br />
                  <input
                    type="number"
                    min="1"
                    value={customerId}
                    onChange={(event) =>
                      setCustomerId(event.target.value)
                    }
                  />
                </label>
              </div>

              <div style={{ marginBottom: 16 }}>
                <label>
                  Vehicle ID:
                  <br />
                  <input
                    type="number"
                    min="1"
                    value={vehicleId}
                    onChange={(event) =>
                      setVehicleId(event.target.value)
                    }
                  />
                </label>
              </div>

              <button
                onClick={bookAppointment}
                disabled={booking}
              >
                {booking ? "Booking..." : "Book Appointment"}
              </button>
            </section>
          )}

          {/* Booking confirmation */}
          {appointment && (
            <section
              role="status"
              style={{
                border: "1px solid #4caf50",
                borderRadius: 8,
                padding: 20,
                marginTop: 24,
              }}
            >
              <h2>Appointment Confirmed!</h2>
              <p>{message}</p>
              <p>
                Appointment ID: {appointment.appointmentId}
              </p>
              <p>Status: {appointment.status}</p>
              <p>
                Start:{" "}
                {new Date(appointment.startTimeUtc).toLocaleString()}
              </p>
              <p>
                End:{" "}
                {new Date(appointment.endTimeUtc).toLocaleString()}
              </p>
            </section>
          )}
        </>
      )}
    </main>
  );
}

export default App;
