
const API_BASE_URL = "http://localhost:5274/api";

export interface Dealership {
  dealershipId: number;
  name: string;
  address: string;
  timeZoneId: string;
}

export async function getDealerships(): Promise<Dealership[]> {
  const response = await fetch(`${API_BASE_URL}/dealerships`);

  if (!response.ok) {
    throw new Error(`Failed to load dealerships: ${response.status}`);
  }

  return response.json();
}

export interface ServiceType {
  serviceTypeId: number;
  name: string;
  durationMinutes: number;
  requiredSkill: string;
}

export async function getServiceTypes(
  dealershipId: number
): Promise<ServiceType[]> {
  const response = await fetch(
    `${API_BASE_URL}/dealerships/${dealershipId}/service-types`
  );

  if (!response.ok) {
    throw new Error(`Failed to load service types: ${response.status}`);
  }

  return response.json();
}

export interface Availability {
  technicianId: number;
  technicianName: string;
  serviceBayId: number;
  bayName: string;
  startTimeUtc: string;
  endTimeUtc: string;
}

export async function getAvailability(
  dealershipId: number,
  serviceTypeId: number,
  startTimeUtc: string
): Promise<Availability[]> {
  const params = new URLSearchParams({
    serviceTypeId: String(serviceTypeId),
    startTimeUtc,
  });

  const response = await fetch(
    `${API_BASE_URL}/dealerships/${dealershipId}/availability?${params}`
  );

  if (!response.ok) {
    const body = await response.text();
    throw new Error(body || `Availability request failed: ${response.status}`);
  }

  return response.json();
}

export interface CreateAppointmentRequest {
  customerId: number;
  vehicleId: number;
  dealershipId: number;
  serviceTypeId: number;
  technicianId: number;
  serviceBayId: number;
  startTimeUtc: string;
}

export interface AppointmentResponse {
  appointmentId: number;
  customerId: number;
  vehicleId: number;
  dealershipId: number;
  serviceTypeId: number;
  technicianId: number;
  serviceBayId: number;
  startTimeUtc: string;
  endTimeUtc: string;
  status: string;
}

export async function createAppointment(
  request: CreateAppointmentRequest
): Promise<AppointmentResponse> {
  const response = await fetch(`${API_BASE_URL}/appointments`, {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
    },
    body: JSON.stringify(request),
  });

  if (!response.ok) {
    const body = await response.text();
    throw new Error(body || `Booking failed: ${response.status}`);
  }

  return response.json();
}
