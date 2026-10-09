
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

import {
  getCustomers,
  getCustomerVehicles,
  type Customer,
  type Vehicle,
} from "./services/customerService";

import "./App.css";

function App() {
  const [dealerships, setDealerships] = useState<Dealership[]>([]);
  const [serviceTypes, setServiceTypes] = useState<ServiceType[]>([]);
  const [customers, setCustomers] = useState<Customer[]>([]);
  const [vehicles, setVehicles] = useState<Vehicle[]>([]);

  const [selectedDealershipId, setSelectedDealershipId] =
    useState<number | null>(null);
  const [selectedServiceTypeId, setSelectedServiceTypeId] =
    useState<number | null>(null);
  const [selectedCustomerId, setSelectedCustomerId] =
    useState<number | null>(null);
  const [selectedVehicleId, setSelectedVehicleId] =
    useState<number | null>(null);

  const [serviceDate, setServiceDate] = useState("");
  const [serviceTime, setServiceTime] = useState("09:00");

  const [availability, setAvailability] = useState<Availability[]>([]);
  const [selectedSlot, setSelectedSlot] =
    useState<Availability | null>(null);

  const [appointment, setAppointment] =
    useState<AppointmentResponse | null>(null);

  const [loading, setLoading] = useState(true);
  const [loadingCustomers, setLoadingCustomers] = useState(true);
  const [loadingServices, setLoadingServices] = useState(false);
  const [loadingVehicles, setLoadingVehicles] = useState(false);
  const [searching, setSearching] = useState(false);
  const [booking, setBooking] = useState(false);

  const [error, setError] = useState("");
  const [message, setMessage] = useState("");

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

  // Load customers
  useEffect(() => {
    let cancelled = false;

    async function loadCustomers() {
      try {
        const data = await getCustomers();

        if (!cancelled) {
          setCustomers(data);

          if (data.length > 0) {
            setSelectedCustomerId(data[0].customerId);
          }
        }
      } catch (err: unknown) {
        if (!cancelled) {
          setError(
            err instanceof Error
              ? err.message
              : "Failed to load customers."
          );
        }
      } finally {
        if (!cancelled) {
          setLoadingCustomers(false);
        }
      }
    }

    void loadCustomers();

    return () => {
      cancelled = true;
    };
  }, []);

  // Load services when dealership changes
  useEffect(() => {
    let cancelled = false;

    async function loadServices() {
      setServiceTypes([]);
      setSelectedServiceTypeId(null);
      setAvailability([]);
      setSelectedSlot(null);
      setAppointment(null);
      setMessage("");

      if (selectedDealershipId === null) {
        setLoadingServices(false);
        return;
      }

      setLoadingServices(true);

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
              : "Failed to load services."
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

  // Load vehicles when customer changes
  useEffect(() => {
    let cancelled = false;

    async function loadVehicles() {
      setVehicles([]);
      setSelectedVehicleId(null);
      setAvailability([]);
      setSelectedSlot(null);
      setAppointment(null);
      setMessage("");

      if (selectedCustomerId === null) {
        setLoadingVehicles(false);
        return;
      }

      setLoadingVehicles(true);

      try {
        const data = await getCustomerVehicles(selectedCustomerId);

        if (!cancelled) {
          setVehicles(data);

          if (data.length > 0) {
            setSelectedVehicleId(data[0].vehicleId);
          }
        }
      } catch (err: unknown) {
        if (!cancelled) {
          setError(
            err instanceof Error
              ? err.message
              : "Failed to load customer vehicles."
          );
        }
      } finally {
        if (!cancelled) {
          setLoadingVehicles(false);
        }
      }
    }

    void loadVehicles();

    return () => {
      cancelled = true;
    };
  }, [selectedCustomerId]);

  // Search available technicians and service bays
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
      const results = await getAvailability(
        selectedDealershipId,
        selectedServiceTypeId,
        selectedDateTime.toISOString()
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
      selectedSlot === null ||
      selectedDealershipId === null ||
      selectedServiceTypeId === null ||
      selectedCustomerId === null ||
      selectedVehicleId === null
    ) {
      setError("Please complete all required selections.");
      return;
    }

    setBooking(true);
    setError("");
    setMessage("");
    setAppointment(null);

    try {
      const result = await createAppointment({
        customerId: selectedCustomerId,
        vehicleId: selectedVehicleId,
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
        err instanceof Error
          ? err.message
          : "Appointment booking failed."
      );
    } finally {
      setBooking(false);
    }
  }

  const selectedDealership = dealerships.find(
    (d) => d.dealershipId === selectedDealershipId
  );

  const selectedCustomer = customers.find(
    (c) => c.customerId === selectedCustomerId
  );

  const selectedVehicle = vehicles.find(
    (v) => v.vehicleId === selectedVehicleId
  );

  const selectedService = serviceTypes.find(
    (s) => s.serviceTypeId === selectedServiceTypeId
  );

  return (
    <main style={{ maxWidth: 900, margin: "40px auto", padding: 24 }}>
      <h1>Keyloop Service Scheduler</h1>
      <p>Book your vehicle service appointment.</p>

      {error && (
        <p role="alert" style={{ color: "#ff7777" }}>
          {error}
        </p>
      )}

      {loading ? (
        <p>Loading dealerships...</p>
      ) : (
        <>
          <section>
            <h2>Select Dealership</h2>

            <select
              value={selectedDealershipId ?? ""}
              onChange={(event) =>
                setSelectedDealershipId(
                  event.target.value ? Number(event.target.value) : null
                )
              }
            >
              <option value="">Select a dealership</option>
              {dealerships.map((d) => (
                <option key={d.dealershipId} value={d.dealershipId}>
                  {d.name}
                </option>
              ))}
            </select>

            {selectedDealership && (
              <>
                <p>{selectedDealership.address}</p>
                <p>Time zone: {selectedDealership.timeZoneId}</p>
              </>
            )}
          </section>

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
                  checked={selectedServiceTypeId === service.serviceTypeId}
                  onChange={() => {
                    setSelectedServiceTypeId(service.serviceTypeId);
                    setAvailability([]);
                    setSelectedSlot(null);
                    setAppointment(null);
                    setMessage("");
                    setError("");
                  }}
                />
                <strong style={{ marginLeft: 8 }}>{service.name}</strong>
                <p>Duration: {service.durationMinutes} minutes</p>
                <p>Required skill: {service.requiredSkill}</p>
              </label>
            ))}
          </section>

          <section>
            <h2>Choose Date and Time</h2>

            <label>
              Service date:{" "}
              <input
                type="date"
                min={new Date().toLocaleDateString("en-CA")}
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

          {message && !appointment && <p role="status">{message}</p>}

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
                      Start: {new Date(slot.startTimeUtc).toLocaleString()}
                    </p>
                    <p>
                      End: {new Date(slot.endTimeUtc).toLocaleString()}
                    </p>
                  </label>
                );
              })}
            </section>
          )}

          {selectedSlot && !appointment && (
            <section>
              <h2>Customer Details</h2>

              {loadingCustomers ? (
                <p>Loading customers...</p>
              ) : (
                <>
                  <label>
                    Customer:
                    <br />
                    <select
                      value={selectedCustomerId ?? ""}
                      onChange={(event) => {
                        setSelectedCustomerId(
                          event.target.value ? Number(event.target.value) : null
                        );
                        setSelectedSlot(null);
                        setAppointment(null);
                        setError("");
                      }}
                    >
                      <option value="">Select a customer</option>
                      {customers.map((customer) => (
                        <option
                          key={customer.customerId}
                          value={customer.customerId}
                        >
                          {customer.name} (ID: {customer.customerId})
                        </option>
                      ))}
                    </select>
                  </label>

                  {selectedCustomer && (
                    <div>
                      <p>Email: {selectedCustomer.email}</p>
                      <p>Phone: {selectedCustomer.phone}</p>
                    </div>
                  )}
                </>
              )}

              <h2>Vehicle Details</h2>

              {loadingVehicles ? (
                <p>Loading vehicles...</p>
              ) : (
                <>
                  <label>
                    Vehicle:
                    <br />
                    <select
                      value={selectedVehicleId ?? ""}
                      onChange={(event) => {
                        setSelectedVehicleId(
                          event.target.value ? Number(event.target.value) : null
                        );
                        setAppointment(null);
                        setError("");
                      }}
                      disabled={
                        selectedCustomerId === null || vehicles.length === 0
                      }
                    >
                      <option value="">Select a vehicle</option>
                      {vehicles.map((vehicle) => (
                        <option
                          key={vehicle.vehicleId}
                          value={vehicle.vehicleId}
                        >
                          {vehicle.year} {vehicle.make} {vehicle.model} — VIN:{" "}
                          {vehicle.vin}
                        </option>
                      ))}
                    </select>
                  </label>

                  {selectedVehicle && (
                    <div>
                      <p>
                        Vehicle: {selectedVehicle.year}{" "}
                        {selectedVehicle.make} {selectedVehicle.model}
                      </p>
                      <p>VIN: {selectedVehicle.vin}</p>
                    </div>
                  )}

                  {!loadingVehicles && vehicles.length === 0 && (
                    <p>No vehicles found for this customer.</p>
                  )}
                </>
              )}

              <h2>Booking Summary</h2>
              <p>Dealership: {selectedDealership?.name}</p>
              <p>Service: {selectedService?.name}</p>
              <p>Technician: {selectedSlot.technicianName}</p>
              <p>Service bay: {selectedSlot.bayName}</p>

              <button
                onClick={bookAppointment}
                disabled={
                  booking ||
                  loadingCustomers ||
                  loadingVehicles ||
                  selectedCustomerId === null ||
                  selectedVehicleId === null
                }
              >
                {booking ? "Booking..." : "Book Appointment"}
              </button>
            </section>
          )}

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
              <p>Appointment ID: {appointment.appointmentId}</p>
              <p>Status: {appointment.status}</p>
              <p>Customer: {selectedCustomer?.name}</p>
              <p>
                Vehicle: {selectedVehicle?.year} {selectedVehicle?.make}{" "}
                {selectedVehicle?.model}
              </p>
              <p>
                Start: {new Date(appointment.startTimeUtc).toLocaleString()}
              </p>
              <p>
                End: {new Date(appointment.endTimeUtc).toLocaleString()}
              </p>
            </section>
          )}
        </>
      )}
    </main>
  );
}

export default App;
