import { useEffect, useState } from "react";
import { fetchCompanies, addCompany } from "../features/companies/companyAPI";
import { useAppSelector } from "../app/hooks";
import { selectAuth } from "../app/store";

export interface Company {
    id: string;
    name: string;
    parentCompanyId?: string | null;
};

export const CompanyList = () => {
    const auth = useAppSelector(selectAuth);
    const token = auth["accessToken"];
    const [companies, setCompanies] = useState<Company[]>([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);
    const [showForm, setShowForm] = useState(false);
    const [newCompanyName, setNewCompanyName] = useState("");
    const [newParentCompanyId, setNewParentCompanyId] = useState<string | null>(null);

    // Get the auth token from the Redux store

    useEffect(() => {
        fetchCompanies(token)
            .then((data) => {
                setCompanies(data);
                setError(null);
            })
            .catch((err) => {
                setError(err.message || "An error occurred while fetching companies.");
                setCompanies([]);
            })
            .finally(() => setLoading(false));
    }, [token]);

    const handleAddCompany = async () => {
        try {
            const addedCompany = await addCompany(newCompanyName, newParentCompanyId, token);
            setCompanies((prev) => [...prev, addedCompany]);
            setShowForm(false);
            setNewCompanyName("");
            setNewParentCompanyId(null);
        } catch (err: any) {
            setError(err.message || "An error occurred while adding the company.");
        }
    };

    if (loading) return <div>Loading companies...</div>;
    if (error) return <div style={{ color: "red" }}>Error: {error}</div>;

    return (
        <div>
            <h1>Company List</h1>
            <button onClick={() => setShowForm((s) => !s)}>
                {showForm ? "Cancel" : "Add Company"}
            </button>
            {showForm && (
                <form onSubmit={(e) => { e.preventDefault(); handleAddCompany(); }} style={{ margin: "1em 0" }}>
                    <input
                        type="text"
                        placeholder="Company Name"
                        value={newCompanyName}
                        onChange={(e) => setNewCompanyName(e.target.value)}
                        required
                    />
                    <input
                        type="text"
                        placeholder="Parent Company Id (optional)"
                        value={newParentCompanyId ?? ""}
                        onChange={(e) => setNewParentCompanyId(e.target.value || null)}
                    />
                    <button type="submit">Save</button>
                </form>
            )}
            {companies.length === 0 ? (
                <p>No companies found.</p>
            ) : (
                <table>
                    <thead>
                        <tr>
                            <th>Name</th>
                            <th>Id</th>
                            <th>Parent Company Id</th>
                        </tr>
                    </thead>
                    <tbody>
                        {companies.map((company) => (
                            <tr key={company.id}>
                                <td>{company.name}</td>
                                <td>{company.id}</td>
                                <td>{company.parentCompanyId ?? "-"}</td>
                            </tr>
                        ))}
                    </tbody>
                </table>
            )}
        </div>
    );
};