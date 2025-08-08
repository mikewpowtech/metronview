import axios from "axios";

const BASE_URL = import.meta.env.REACT_APP_API_URL as string;

export interface Unit {
    id: number;
    unitTypeId: number;
    phoneNumber?: string | null;
    pin?: string | null;
    manufacturerCode: string;
    unitCode?: string | null;
    secret?: string | null;
    status: number;
    companyId?: number | null;
    daysBeforeNotReported?: number | null;
    customFieldValues?: string | null;
}

// Fetch all units
export async function fetchUnits(token?: string): Promise<Unit[]> {
    const response = await axios.get<Unit[]>(`${BASE_URL}/api/unit`, {
        headers: token ? { Authorization: `Bearer ${token}` } : undefined,
    });
    return response.data;
}

// Add a new unit
export async function addUnit(
    payload: Unit,
    token?: string
): Promise<Unit> {
    const response = await axios.post<Unit>(
        `${BASE_URL}/api/unit`,
        payload,
        {
            headers: {
                "Content-Type": "application/json",
                ...(token ? { Authorization: `Bearer ${token}` } : {}),
            },
        }
    );
    return response.data;
}

// Update an existing unit
export async function updateUnit(
    id: number,
    unit: Unit,//Omit<Unit, "id">,
    token?: string
): Promise<void> {
    await axios.put(
        `${BASE_URL}/api/unit/${id}`,
        unit,
        {
            headers: {
                "Content-Type": "application/json",
                ...(token ? { Authorization: `Bearer ${token}` } : {}),
            },
        }
    );
}

// Delete a unit
export async function deleteUnit(
    id: number|string,
    token?: string
): Promise<void> {
    await axios.delete(
        `${BASE_URL}/api/unit/${id}`,
        {
            headers: token ? { Authorization: `Bearer ${token}` } : undefined,
        }
    );
}