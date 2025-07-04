import axios from "axios";

const BASE_URL = import.meta.env.REACT_APP_API_URL as string;

export interface Alarm {
    id: number;
    companyId: number;
    name: string;
    recipientSetId: number;
    isActive: boolean;
    // Add other properties as needed
}

// Interface for creating/updating alarms (without navigation properties)
export interface AlarmRequest {
    companyId: number;
    name: string;
    recipientSetId: number;
    isActive: boolean;
}

// Fetch all alarms
export async function fetchAlarms(token?: string): Promise<Alarm[]> {
    const response = await axios.get<Alarm[]>(`${BASE_URL}/api/alarm`, {
        headers: token ? { Authorization: `Bearer ${token}` } : undefined,
    });
    return response.data;
}

// Add a new alarm
export async function addAlarm(
    alarm: AlarmRequest,
    token?: string
): Promise<Alarm> {
    const response = await axios.post<Alarm>(
        `${BASE_URL}/api/alarm`,
        alarm,
        {
            headers: {
                "Content-Type": "application/json",
                ...(token ? { Authorization: `Bearer ${token}` } : {}),
            },
        }
    );
    return response.data;
}

// Update an existing alarm
export async function updateAlarm(
    id: number,
    alarm: AlarmRequest & { id: number },
    token?: string
): Promise<void> {
    await axios.put(
        `${BASE_URL}/api/alarm/${id}`,
        alarm,
        {
            headers: {
                "Content-Type": "application/json",
                ...(token ? { Authorization: `Bearer ${token}` } : {}),
            },
        }
    );
}

// Delete an alarm
export async function deleteAlarm(
    id: number,
    token?: string
): Promise<void> {
    await axios.delete(
        `${BASE_URL}/api/alarm/${id}`,
        {
            headers: token ? { Authorization: `Bearer ${token}` } : undefined,
        }
    );
}