
const API_BASE_URL = "http://localhost:5274/api";

export interface Appointment {
  appointmentId: number;
  customerId: number;
  customerName: string;
  customerEmail: string;
  vehicleId: number;
  vehicleMake: string;
  vehicleModel: string;
  vehicleYear: number;
  vehicleVin: string;
  dealershipId: number;
  dealershipName: string;
  serviceTypeId: number;
  serviceName: string;
  technicianId: number;
  technicianName: string;
  serviceBayId: number;
  serviceBayName: string;
  startTimeUtc: string;
  endTimeUtc: string;
  status: string;
}

export async function getAppointments(): Promise<Appointment[]> {
  const response = await fetch(`${API_BASE_URL}/appointments`);

  if (!response.ok) {
    throw new Error(
      `Failed to load appointments: ${response.status}`
    );
  }

  return response.json();
}


export async function cancelAppointment(
  appointmentId: number
): Promise<void> {
  const response = await fetch(
    `${API_BASE_URL}/appointments/${appointmentId}/cancel`,
    {
      method: "PATCH",
      headers: {
        Accept: "application/json",
      },
    }
  );

  if (!response.ok) {
    const body = await response.text();
    throw new Error(body || `Cancellation failed: ${response.status}`);
  }
}
