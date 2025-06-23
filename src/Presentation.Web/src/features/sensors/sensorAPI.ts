import axios from "axios";

export interface Sensor {
  id: number;
  name?: string | null;
  channel: number;
  channelType?: number | null;
  lowValue?: number | null;
  highValue?: number | null;
  engineeringUnits?: string | null;
  unitId?: string;
  companyID?: number | null;
  // Add other properties as needed
}

const BASE_URL = import.meta.env.REACT_APP_API_URL as string;

// Fetch all sensors
export async function fetchSensors(token?: string): Promise<Sensor[]> {
  const response = await axios.get<Sensor[]>(`${BASE_URL}/api/sensor`, {
    headers: token ? { Authorization: `Bearer ${token}` } : undefined,
  });
  return response.data;
}

// Fetch sensors by unit ID
export async function fetchSensorsByUnit(unitId: number, token?: string): Promise<Sensor[]> {
  const response = await axios.get<Sensor[]>(
    `${BASE_URL}/api/sensor/by-unit/${unitId}`,
    {
      headers: token ? { Authorization: `Bearer ${token}` } : undefined,
    }
  );
  return response.data;
}

// Add a new sensor
export async function addSensor(
  sensor: Omit<Sensor, "id">,
  token?: string
): Promise<Sensor> {
  const response = await axios.post<Sensor>(
    `${BASE_URL}/api/sensor`,
    sensor,
    {
      headers: {
        "Content-Type": "application/json",
        ...(token ? { Authorization: `Bearer ${token}` } : {}),
      },
    }
  );
  return response.data;
}

// Update an existing sensor
export async function updateSensor(
  id: number,
  sensor: Sensor,//Omit<Sensor, "id">,
  token?: string
): Promise<void> {
  await axios.put(
    `${BASE_URL}/api/sensor/${id}`,
    sensor,
    {
      headers: {
        "Content-Type": "application/json",
        ...(token ? { Authorization: `Bearer ${token}` } : {}),
      },
    }
  );
}

// Delete a sensor
export async function deleteSensor(
  id: number,
  token?: string
): Promise<void> {
  await axios.delete(
    `${BASE_URL}/api/sensor/${id}`,
    {
      headers: token ? { Authorization: `Bearer ${token}` } : undefined,
    }
  );
}