import axios from "axios";

const BASE_URL = import.meta.env.REACT_APP_API_URL as string;

export interface RecipientSet {
    id: number;
    companyId: number;
    name: string;
    // Optionally include recipients if needed:
    // recipients?: Recipient[];
}

// Fetch all recipient sets
export async function fetchRecipientSets(token?: string): Promise<RecipientSet[]> {
    const response = await axios.get<RecipientSet[]>(`${BASE_URL}/api/recipientset`, {
        headers: token ? { Authorization: `Bearer ${token}` } : undefined,
    });
    return response.data;
}

// Add a new recipient set
export async function addRecipientSet(
    recipientSet: Omit<RecipientSet, "id">,
    token?: string
): Promise<RecipientSet> {
    const response = await axios.post<RecipientSet>(
        `${BASE_URL}/api/recipientset`,
        recipientSet,
        {
            headers: {
                "Content-Type": "application/json",
                ...(token ? { Authorization: `Bearer ${token}` } : {}),
            },
        }
    );
    return response.data;
}

// Update an existing recipient set
export async function updateRecipientSet(
    id: number,
    recipientSet: RecipientSet,
    token?: string
): Promise<void> {
    await axios.put(
        `${BASE_URL}/api/recipientset/${id}`,
        recipientSet,
        {
            headers: {
                "Content-Type": "application/json",
                ...(token ? { Authorization: `Bearer ${token}` } : {}),
            },
        }
    );
}

// Delete a recipient set
export async function deleteRecipientSet(
    id: number,
    token?: string
): Promise<void> {
    await axios.delete(
        `${BASE_URL}/api/recipientset/${id}`,
        {
            headers: token ? { Authorization: `Bearer ${token}` } : undefined,
        }
    );
}