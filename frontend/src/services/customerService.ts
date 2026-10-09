
const API_BASE_URL = "http://localhost:5274/api";

export interface Customer {
  customerId: number;
  name: string;
  email: string;
  phone: string;
}

export interface Vehicle {
  vehicleId: number;
  customerId: number;
  make: string;
  model: string;
  year: number;
  vin: string;
}

export async function getCustomers(): Promise<Customer[]> {
  const response = await fetch(`${API_BASE_URL}/customers`);

  if (!response.ok) {
    throw new Error(`Failed to load customers: ${response.status}`);
  }

  return response.json();
}

export async function getCustomerVehicles(
  customerId: number
): Promise<Vehicle[]> {
  const response = await fetch(
    `${API_BASE_URL}/customers/${customerId}/vehicles`
  );

  if (!response.ok) {
    throw new Error(`Failed to load vehicles: ${response.status}`);
  }

  return response.json();
}
