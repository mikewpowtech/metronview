import axios from "axios";

export interface UnitModel {
    id: number;
    code: string;
    name: string;
    description?: string | null;
}

const BASE_URL = import.meta.env.REACT_APP_API_URL as string;

// Fetch all unit models
export async function fetchUnitModels(token?: string): Promise<UnitModel[]> {
    const response = await axios.get<UnitModel[]>(`${BASE_URL}/api/unitmodel`, {
        headers: token ? { Authorization: `Bearer ${token}` } : undefined,
    });
    return response.data;
}

// Add a new unit model
export async function addUnitModel(
    unitModel: Omit<UnitModel, "id">,
    token?: string
): Promise<UnitModel> {
    const response = await axios.post<UnitModel>(
        `${BASE_URL}/api/unitmodel`,
        unitModel,
        {
            headers: {
                "Content-Type": "application/json",
                ...(token ? { Authorization: `Bearer ${token}` } : {}),
            },
        }
    );
    return response.data;
}

// Update an existing unit model
export async function updateUnitModel(
    id: number,
    unitModel: Omit<UnitModel, "id">,
    token?: string
): Promise<void> {
    await axios.put(
        `${BASE_URL}/api/unitmodel/${id}`,
        unitModel,
        {
            headers: {
                "Content-Type": "application/json",
                ...(token ? { Authorization: `Bearer ${token}` } : {}),
            },
        }
    );
}

// Delete a unit model
export async function deleteUnitModel(
    id: number,
    token?: string
): Promise<void> {
    await axios.delete(
        `${BASE_URL}/api/unitmodel/${id}`,
        {
            headers: token ? { Authorization: `Bearer ${token}` } : undefined,
        }
    );
}