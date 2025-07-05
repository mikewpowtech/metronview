import axios from "axios";
import type { Sensor } from "../sensors/sensorAPI";

const BASE_URL = import.meta.env.REACT_APP_API_URL as string;

export interface Reading {
  dateReceivedUtc: string;
  dateRecordedUtc: string;
  sensor: Sensor;
  sensorId: number; // Add sensorId for convenience and backend compatibility
  value?: number | null;
}

// Fetch all readings
export async function fetchReadings(token?: string): Promise<Reading[]> {
  const response = await axios.get<Reading[]>(`${BASE_URL}/api/reading`, {
    headers: token ? { Authorization: `Bearer ${token}` } : undefined,
  });
  return response.data;
}

// Fetch readings by sensor ID
export async function fetchReadingsBySensorId(sensorId: number, token?: string): Promise<Reading[]> {
  const response = await axios.get<Reading[]>(`${BASE_URL}/api/reading/by-sensor/${sensorId}`, {
    headers: token ? { Authorization: `Bearer ${token}` } : undefined,
  });
  return response.data;
}

// Fetch readings by unit ID
export async function fetchReadingsByUnitId(unitId: number, token?: string): Promise<Reading[]> {
  const response = await axios.get<Reading[]>(`${BASE_URL}/api/reading/by-unit/${unitId}`, {
    headers: token ? { Authorization: `Bearer ${token}` } : undefined,
  });
  return response.data;
}

// Add a new reading
export async function addReading(
    reading: {
        dateReceivedUtc: string;
        dateRecordedUtc: string;
        sensorId: number;
        sensor: Sensor | null;
        value?: number | null;
    },
  token?: string
): Promise<Reading> {
    reading.sensor = null;
    const response = await axios.post<Reading>(
        `${BASE_URL}/api/reading`,
        reading,
        {
            headers: {
                "Content-Type": "application/json",
                ...(token ? { Authorization: `Bearer ${token}` } : {}),
            },
        }
    );
    return response.data;
}

// Update an existing reading
export async function updateReading(
    dateRecordedUtc: string,
    sensorId: number,
    reading: {
        dateReceivedUtc: string;
        dateRecordedUtc: string;
        sensorId: number;
        value?: number | null;
    },
    token?: string
): Promise<void> {
    await axios.put(
        `${BASE_URL}/api/reading/${encodeURIComponent(dateRecordedUtc)}/${sensorId}`,
        reading,
        {
            headers: {
                "Content-Type": "application/json",
                ...(token ? { Authorization: `Bearer ${token}` } : {}),
            },
        }
    );
}

// Delete a reading
export async function deleteReading(
  record: Reading,
  token?: string
): Promise<void> {
  await axios.delete(
    `${BASE_URL}/api/reading/${encodeURIComponent(record.dateRecordedUtc)}/${record.sensor.id}`,
    {
      headers: token ? { Authorization: `Bearer ${token}` } : undefined,
    }
  );
}