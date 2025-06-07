import axios from "axios";
import type { Unit } from "../../pages/UnitList";

const BASE_URL = import.meta.env.REACT_APP_API_URL as string;

// Fetch all units
export async function fetchUnits(token?: string): Promise<Unit[]> {
    const response = await axios.get<Unit[]>(`${BASE_URL}/api/unit`, {
        headers: token ? { Authorization: `Bearer ${token}` } : undefined,
    });
    return response.data;
}

// Add a new unit
export async function addUnit(
    unit: Omit<Unit, "id">,
    token?: string
): Promise<Unit> {
    const response = await axios.post<Unit>(
        `${BASE_URL}/api/unit`,
        unit,
        {
            headers: {
                "Content-Type": "application/json",
                ...(token ? { Authorization: `Bearer ${token}` } : {}),
            },
        }
    );
    return response.data;
}