import axios from "axios";

const BASE_URL = import.meta.env.REACT_APP_API_URL as string;

// Add these interfaces to the existing triggerAPI.ts file

export interface TriggerType {
    id: number;
    code: string;
    name: string;
    order: number;
}

export interface CommunicationMode {
    id: number;
    code: string;
    name: string;
    order: number;
}

// Fetch all trigger types
export async function fetchTriggerTypes(token?: string): Promise<TriggerType[]> {
    const response = await axios.get<TriggerType[]>(`${BASE_URL}/api/triggertype`, {
        headers: token ? { Authorization: `Bearer ${token}` } : undefined,
    });
    return response.data;
}

// Fetch trigger type by ID
export async function fetchTriggerTypeById(
    id: number,
    token?: string
): Promise<TriggerType> {
    const response = await axios.get<TriggerType>(`${BASE_URL}/api/triggertype/${id}`, {
        headers: token ? { Authorization: `Bearer ${token}` } : undefined,
    });
    return response.data;
}

// Fetch all communication modes
export async function fetchCommunicationModes(token?: string): Promise<CommunicationMode[]> {
    const response = await axios.get<CommunicationMode[]>(`${BASE_URL}/api/communicationmode`, {
        headers: token ? { Authorization: `Bearer ${token}` } : undefined,
    });
    return response.data;
}

// Fetch communication mode by ID
export async function fetchCommunicationModeById(
    id: number,
    token?: string
): Promise<CommunicationMode> {
    const response = await axios.get<CommunicationMode>(`${BASE_URL}/api/communicationmode/${id}`, {
        headers: token ? { Authorization: `Bearer ${token}` } : undefined,
    });
    return response.data;
}


export interface Trigger {
    id: number;
    alarmId: number;
    triggerTypeId: number;
    triggerValue: number;
    communicationModeId: number;
    subject: string;
    body: string;
    minimumSendIntervalMinutes: number;
    isEnabled: boolean;
    // Optional navigation properties
    alarm?: {
        id: number;
        companyId: number;
        name: string;
        recipientSetId: number;
        isActive: boolean;
    };
    triggerType?: {
        id: number;
        code: string;
        name: string;
        order: number;
    };
    communicationMode?: {
        id: number;
        code: string;
        name: string;
        order: number;
    };
}

// Fetch all triggers
export async function fetchTriggers(token?: string): Promise<Trigger[]> {
    const response = await axios.get<Trigger[]>(`${BASE_URL}/api/trigger`, {
        headers: token ? { Authorization: `Bearer ${token}` } : undefined,
    });
    return response.data;
}

// Fetch trigger by ID
export async function fetchTriggerById(
    id: number,
    token?: string
): Promise<Trigger> {
    const response = await axios.get<Trigger>(`${BASE_URL}/api/trigger/${id}`, {
        headers: token ? { Authorization: `Bearer ${token}` } : undefined,
    });
    return response.data;
}

// Fetch triggers by alarm ID
export async function fetchTriggersByAlarmId(
    alarmId: number,
    token?: string
): Promise<Trigger[]> {
    const response = await axios.get<Trigger[]>(
        `${BASE_URL}/api/trigger/by-alarm/${alarmId}`,
        {
            headers: token ? { Authorization: `Bearer ${token}` } : undefined,
        }
    );
    return response.data;
}

// Fetch triggers by trigger type ID
export async function fetchTriggersByTriggerTypeId(
    triggerTypeId: number,
    token?: string
): Promise<Trigger[]> {
    const response = await axios.get<Trigger[]>(
        `${BASE_URL}/api/trigger/by-trigger-type/${triggerTypeId}`,
        {
            headers: token ? { Authorization: `Bearer ${token}` } : undefined,
        }
    );
    return response.data;
}

// Fetch triggers by communication mode ID
export async function fetchTriggersByCommunicationModeId(
    communicationModeId: number,
    token?: string
): Promise<Trigger[]> {
    const response = await axios.get<Trigger[]>(
        `${BASE_URL}/api/trigger/by-communication-mode/${communicationModeId}`,
        {
            headers: token ? { Authorization: `Bearer ${token}` } : undefined,
        }
    );
    return response.data;
}

// Fetch enabled triggers only
export async function fetchEnabledTriggers(token?: string): Promise<Trigger[]> {
    const response = await axios.get<Trigger[]>(`${BASE_URL}/api/trigger/enabled`, {
        headers: token ? { Authorization: `Bearer ${token}` } : undefined,
    });
    return response.data;
}

// Interface for creating/updating triggers (without navigation properties)
export interface TriggerRequest {
    alarmId: number;
    triggerTypeId: number;
    triggerValue: number;
    communicationModeId: number;
    subject: string;
    body: string;
    minimumSendIntervalMinutes: number;
    isEnabled: boolean;
}

// Add a new trigger
export async function addTrigger(
    trigger: TriggerRequest,
    token?: string
): Promise<Trigger> {
    const response = await axios.post<Trigger>(
        `${BASE_URL}/api/trigger`,
        trigger,
        {
            headers: {
                "Content-Type": "application/json",
                ...(token ? { Authorization: `Bearer ${token}` } : {}),
            },
        }
    );
    return response.data;
}

// Update an existing trigger
export async function updateTrigger(
    id: number,
    trigger: TriggerRequest & { id: number },
    token?: string
): Promise<void> {
    await axios.put(
        `${BASE_URL}/api/trigger/${id}`,
        trigger,
        {
            headers: {
                "Content-Type": "application/json",
                ...(token ? { Authorization: `Bearer ${token}` } : {}),
            },
        }
    );
}

// Delete a trigger
export async function deleteTrigger(
    id: number,
    token?: string
): Promise<void> {
    await axios.delete(
        `${BASE_URL}/api/trigger/${id}`,
        {
            headers: token ? { Authorization: `Bearer ${token}` } : undefined,
        }
    );
}
