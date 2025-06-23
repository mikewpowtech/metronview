import axios from "axios";

const BASE_URL = import.meta.env.REACT_APP_API_URL as string;

export interface Company {
    id: number;
    name: string;
    parentCompanyId?: number | null;
}

// Fetch all companies
export async function fetchCompanies(token?: string): Promise<Company[]> {
    const response = await axios.get<Company[]>(`${BASE_URL}/api/company`, {
        headers: token ? { Authorization: `Bearer ${token}` } : undefined,
    });
    return response.data;
}

// Add a new company
export async function addCompany(
    name: string,
    parentCompanyId?: number | null,
    token?: string
): Promise<Company> {
    const response = await axios.post<Company>(
        `${BASE_URL}/api/company`,
        { name, parentCompanyId: parentCompanyId || null },
        {
            headers: {
                "Content-Type": "application/json",
                ...(token ? { Authorization: `Bearer ${token}` } : {}),
            },
        }
    );
    return response.data;
}

// Update an existing company
export async function updateCompany(
    id: number,
    company: { id: number; name: string; parentCompanyId?: number | null },
    token?: string
): Promise<void> {
    await axios.put(
        `${BASE_URL}/api/company/${id}`,
        company,
        {
            headers: {
                "Content-Type": "application/json",
                ...(token ? { Authorization: `Bearer ${token}` } : {}),
            },
        }
    );
}

// Delete a company
export async function deleteCompany(
    id: number,
    token?: string
): Promise<void> {
    await axios.delete(
        `${BASE_URL}/api/company/${id}`,
        {
            headers: token ? { Authorization: `Bearer ${token}` } : undefined,
        }
    );
}