import axios from "axios";

export interface Sensor {
  id: string;
  name?: string | null;
  channel: number;
  channelType?: number | null;
  lowValue?: number | null;
  highValue?: number | null;
  engineeringUnits?: string | null;
  unitId?: string;
  companyID?: string | null;
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