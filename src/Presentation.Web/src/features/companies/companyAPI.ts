import axios from "axios";
import type { Company } from "../../pages/CompanyList";

const BASE_URL = import.meta.env.REACT_APP_API_URL as string;

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
    parentCompanyId?: string | null,
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