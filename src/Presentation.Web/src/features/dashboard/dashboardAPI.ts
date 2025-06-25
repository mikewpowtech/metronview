import axios from "axios";

const BASE_URL = import.meta.env.REACT_APP_API_URL as string;

// Adjust this interface to match the UnitSummary structure from the API
export interface UnitSummary {
  // Example properties, update as needed to match your actual UnitSummary
  id: number;
  unitType: string;
  lastComms?: string;
  carrier?: string | null;
  signal?: number | null;
  companyID?: number;
  company?: string;
  // Add other properties as needed
}

export async function fetchDashboardUnits(token?: string): Promise<UnitSummary[]> {
    const response = await axios.get<UnitSummary[]>(`${BASE_URL}/api/dashboard`, {
    headers: token ? { Authorization: `Bearer ${token}` } : undefined,
  });
  return response.data;
}