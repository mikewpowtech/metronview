import axios from "axios";

const BASE_URL = import.meta.env.REACT_APP_API_URL as string;

export interface ConfigurationUpload {
  id: number;
  unitId: number;
  fileName?: string;
  uploadedAtUtc: string;
  uploadedBy?: string;
  // Add other fields as needed
}

// Fetch all configuration uploads
export async function fetchConfigurationUploads(token?: string): Promise<ConfigurationUpload[]> {
  const response = await axios.get<ConfigurationUpload[]>(`${BASE_URL}/api/configurationuploads`, {
    headers: token ? { Authorization: `Bearer ${token}` } : undefined,
  });
  return response.data;
}

// Fetch configuration uploads by unitId
export async function fetchConfigurationUploadsByUnit(
  unitId: number,
  token?: string
): Promise<ConfigurationUpload[]> {
  const response = await axios.get<ConfigurationUpload[]>(
    `${BASE_URL}/api/configurationuploads/unit/${unitId}`,
    {
      headers: token ? { Authorization: `Bearer ${token}` } : undefined,
    }
  );
  return response.data;
}

// Add a new configuration upload
export async function addConfigurationUpload(
  upload: Omit<ConfigurationUpload, "id">,
  token?: string
): Promise<ConfigurationUpload> {
  const response = await axios.post<ConfigurationUpload>(
    `${BASE_URL}/api/configurationuploads`,
    upload,
    {
      headers: {
        "Content-Type": "application/json",
        ...(token ? { Authorization: `Bearer ${token}` } : {}),
      },
    }
  );
  return response.data;
}

// Update an existing configuration upload
export async function updateConfigurationUpload(
  id: number,
  upload: ConfigurationUpload,
  token?: string
): Promise<void> {
  await axios.put(
    `${BASE_URL}/api/configurationuploads/${id}`,
    upload,
    {
      headers: {
        "Content-Type": "application/json",
        ...(token ? { Authorization: `Bearer ${token}` } : {}),
      },
    }
  );
}

// Delete a configuration upload
export async function deleteConfigurationUpload(
  id: number,
  token?: string
): Promise<void> {
  await axios.delete(
    `${BASE_URL}/api/configurationuploads/${id}`,
    {
      headers: token ? { Authorization: `Bearer ${token}` } : undefined,
    }
  );
}