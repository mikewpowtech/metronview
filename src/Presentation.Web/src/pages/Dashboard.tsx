import { useEffect, useState } from "react";
import { Table, Button, Dropdown, Checkbox } from "antd";
import { DownOutlined, FileSearchOutlined } from "@ant-design/icons";
import { fetchDashboardUnits, type UnitSummary } from "../features/dashboard/dashboardAPI";
import { useAppSelector } from "../app/hooks";
import { selectAuth } from "../app/store";
import { useNavigate } from "react-router-dom";

// Persistent column visibility utilities
const COLUMN_VISIBILITY_KEY = "dashboard.visibleColumns";

function getPersistedVisibleKeys(defaultKeys: string[]) {
    if (typeof window === "undefined") return defaultKeys;
    const stored = localStorage.getItem(COLUMN_VISIBILITY_KEY);
    if (!stored) return defaultKeys;
    try {
        const parsed = JSON.parse(stored);
        if (Array.isArray(parsed)) return parsed;
        return defaultKeys;
    } catch {
        return defaultKeys;
    }
}
function setPersistedVisibleKeys(keys: string[]) {
    if (typeof window !== "undefined") {
        localStorage.setItem(COLUMN_VISIBILITY_KEY, JSON.stringify(keys));
    }
}

// Update columns to match UnitSummary fields
const allColumnDefs = [
    { title: "ID", dataIndex: "id", key: "id", width: 100 },
    { title: "Unit Type", dataIndex: "unitType", key: "unitType", width: 120 },
    { title: "Last Comms(UTC)", dataIndex: "lastComms", key: "lastComms", width: 140 },
    { title: "Carrier", dataIndex: "carrier", key: "carrier", width: 120 },
    { title: "Signal", dataIndex: "signal", key: "signal", width: 120 },
    { title: "Company", dataIndex: "company", key: "company", width: 180 },
];

const Dashboard: React.FC = () => {
    const auth = useAppSelector(selectAuth);
    const token = auth?.accessToken;
    const [units, setUnits] = useState<UnitSummary[]>([]);
    const [loading, setLoading] = useState<boolean>(true);
    const [error, setError] = useState<string | null>(null);
    const navigate = useNavigate();

    // Persistent column visibility state
    const defaultVisibleKeys = allColumnDefs.filter(col => col.key !== "id").map(col => col.key as string);
    const [visibleKeys, setVisibleKeys] = useState<string[]>(() => getPersistedVisibleKeys(defaultVisibleKeys));
    const [columns, setColumns] = useState(allColumnDefs);

    useEffect(() => {
        loadUnits();
        // eslint-disable-next-line
    }, [token]);

    useEffect(() => {
        setColumns(
            allColumnDefs.filter(col => visibleKeys.includes(col.key as string))
        );
        setPersistedVisibleKeys(visibleKeys);
    }, [visibleKeys]);

    const loadUnits = async () => {
        setLoading(true);
        try {
            const data = await fetchDashboardUnits(token);
            setUnits(data);
            setError(null);
        } catch (err: unknown) {
            if (err && typeof err === "object" && "message" in err) {
                setError((err as { message?: string }).message || "An error occurred while fetching dashboard data.");
            } else {
                setError("An error occurred while fetching dashboard data.");
            }
            setUnits([]);
        } finally {
            setLoading(false);
        }
    };

    //// Fetch configuration uploads for a unit (modal, still available if needed)
    //const showConfigUploads = async (unitId: number) => {
    //    setConfigModal({ open: true, unitId });
    //    setConfigLoading(true);
    //    try {
    //        const uploads = await fetchConfigurationUploadsByUnit(unitId, token);
    //        setConfigUploads(uploads);
    //    } catch {
    //        setConfigUploads([]);
    //    } finally {
    //        setConfigLoading(false);
    //    }
    //};

    // Dropdown menu items for columns
    const columnMenuItems = allColumnDefs.map(col => ({
        key: col.key,
        label: (
            <Checkbox
                checked={visibleKeys.includes(col.key as string)}
                onChange={e => {
                    const checked = e.target.checked;
                    setVisibleKeys(keys => {
                        const newKeys = checked
                            ? [...keys, col.key as string]
                            : keys.filter(k => k !== col.key);
                        setPersistedVisibleKeys(newKeys);
                        return newKeys;
                    });
                }}
                disabled={visibleKeys.length === 1 && visibleKeys.includes(col.key as string)}
                style={{ width: "100%", padding: "4px 12px" }}
            >
                {col.title}
            </Checkbox>
        ),
    }));

    // Add the Actions column
    const tableColumns = [
        ...columns,
        {
            title: "Actions",
            key: "actions",
            width: 160,
            render: (_: any, record: UnitSummary) => (
                <Button
                    icon={<FileSearchOutlined />}
                    size="small"
                    onClick={() => navigate(`/configurationuploads/${record.id}`)}
                >
                    Config Uploads
                </Button>
            ),
        },
    ];

    if (loading) return <div>Loading dashboard...</div>;
    if (error) return <div style={{ color: "red" }}>Error: {error}</div>;

    return (
        <div>
            <div style={{ marginBottom: 16 }}>
                <Dropdown menu={{ items: columnMenuItems }} trigger={["click"]}>
                    <Button>
                        Columns <DownOutlined />
                    </Button>
                </Dropdown>
            </div>
            <h2>Home - Units & Readings Overview</h2>
            <Table<UnitSummary>
                bordered
                dataSource={units}
                columns={tableColumns}
                rowKey="id"
                pagination={false}
                scroll={{ x: "max-content" }}
            />
            {/* The modal logic can be removed if you only want navigation */}
        </div>
    );
};

export default Dashboard;