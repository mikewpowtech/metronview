import axios from "axios";

const BASE_URL = import.meta.env.REACT_APP_API_URL as string;

export interface Recipient {
    id: number;
    companyId: number;
    unitId?: number | null;
    name: string;
    email: string;
    sms: string;
    webServiceRoot: string;
    isEnabled: boolean;
}

// Fetch all recipients
export async function fetchRecipients(token?: string): Promise<Recipient[]> {
    const response = await axios.get<Recipient[]>(`${BASE_URL}/api/recipient`, {
        headers: token ? { Authorization: `Bearer ${token}` } : undefined,
    });
    return response.data;
}

// Add a new recipient
export async function addRecipient(
    recipient: Omit<Recipient, "id">,
    token?: string
): Promise<Recipient> {
    const payload = {
        ...recipient,
        unitId: !recipient.unitId ? null : recipient.unitId,
    };
    const response = await axios.post<Recipient>(
        `${BASE_URL}/api/recipient`,
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

// Update an existing recipient
export async function updateRecipient(
    id: number,
    recipient: Recipient,
    token?: string
): Promise<void> {
    await axios.put(
        `${BASE_URL}/api/recipient/${id}`,
        recipient,
        {
            headers: {
                "Content-Type": "application/json",
                ...(token ? { Authorization: `Bearer ${token}` } : {}),
            },
        }
    );
}

// Delete a recipient
export async function deleteRecipient(
    id: number,
    token?: string
): Promise<void> {
    await axios.delete(
        `${BASE_URL}/api/recipient/${id}`,
        {
            headers: token ? { Authorization: `Bearer ${token}` } : undefined,
        }
    );
}