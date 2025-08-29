import axios from "axios";

const BASE_URL = import.meta.env.REACT_APP_API_URL as string;

export interface Recipient {
    id: number;
    companyId: number;
    name: string;
    email: string;
    sms: string;
    webServiceRoot: string;
    isEnabled: boolean;
}

export class RecipientNotFoundError extends Error {
    constructor(message: string = "Recipient not found") {
        super(message);
        this.name = "RecipientNotFoundError";
    }
}

// Fetch all recipients
export async function fetchRecipients(token?: string): Promise<Recipient[]> {
    const response = await axios.get<Recipient[]>(`${BASE_URL}/api/recipient`, {
        headers: token ? { Authorization: `Bearer ${token}` } : undefined,
    });
    return response.data;
}

export const fetchRecipientsByRecipientSet = async (recipientSetId: number, token: string | undefined): Promise<Recipient[]> => {
    const response = await fetch(`/api/recipientset/${recipientSetId}/recipients`, {
        headers: {
            'Authorization': `Bearer ${token}`,
            'Content-Type': 'application/json',
        },
    });

    if (response.status === 404) {
        throw new RecipientNotFoundError(`Recipients not found for recipient set ${recipientSetId}`);
    }

    if (!response.ok) {
        throw new Error(`Failed to fetch recipients for recipient set: ${response.statusText}`);
    }

    return response.json();
};

// Add a new recipient
export async function addRecipient(
    recipient: Omit<Recipient, "id">,
    token?: string
): Promise<Recipient> {
    const payload = {
        ...recipient
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