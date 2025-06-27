/* eslint-disable @typescript-eslint/no-explicit-any */
import { useEffect, useState } from "react";
import { Table, Button, Dropdown, Checkbox, Tooltip } from "antd";
import { DownOutlined, SettingOutlined } from "@ant-design/icons";
import SettingFilled from "@ant-design/icons/SettingFilled";
import { fetchDashboardUnits, type UnitSummary } from "../features/dashboard/dashboardAPI";
import { useAppSelector } from "../app/hooks";
import { selectAuth } from "../app/store";
import { useNavigate } from "react-router-dom";
import { GenericTable } from "../components/GenericTable";
import dayjs from "dayjs"; // Add this import for date formatting

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
    if (typeof window   !== "undefined") {
        localStorage.setItem(COLUMN_VISIBILITY_KEY, JSON.stringify(keys));
    }
}

// Update columns to match UnitSummary fields
const allColumnDefs = [
    { title: "ID", dataIndex: "id", key: "id", width: 100 },
    { title: "Unit Type", dataIndex: "unitType", key: "unitType", width: 120 },
    {
        title: "Last Comms(UTC)",
        dataIndex: "lastComms",
        key: "lastComms",
        width: 140,
        render: (value: string) =>
            value ? dayjs(value).format("YYYY-MM-DD HH:mm:ss") : "",
    },
    { title: "Carrier", dataIndex: "carrier", key: "carrier", width: 110 },
    { title: "Signal", dataIndex: "signal", key: "signal", width: 90 },
    { title: "Company", dataIndex: "company", key: "company", width: 150 },
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

    // Add the Actions column as the first column, with minimal width for the buttons
    const actionsColumn = {
        title: "",
        key: "actions",
        align: "center" as const,
        className: "actions-col",
        render: (_: any, record: UnitSummary) => (
            <span className="actions-col-inner">
                <Tooltip title="configuration">
                    <Button
                        icon={<SettingFilled />}
                        size="small"
                        style={{ padding: 0, minWidth: 0, width: 28, height: 28 }}
                        onClick={() => navigate(`/configurationuploads/${record.id}`)}
                    />
                </Tooltip>
                {/* Add more buttons here if needed */}
            </span>
        ),
    };

    // Place Actions column first
    const tableColumns = [
        actionsColumn,
        ...columns,
    ];

    if (loading) return <div>Loading dashboard...</div>;
    if (error) return <div style={{ color: "red" }}>Error: {error}</div>;

    return (
        <div style={{ padding: "12px 0 12px 30px" }} >
            <div style={{ display: "flex", alignItems: "center", justifyContent: "space-between", marginBottom: 12 }}>
                <h2 style={{ fontSize: 20, margin: "12px 0 12px 0" }}>Home - Units & Readings Overview</h2>
                <Dropdown menu={{ items: columnMenuItems }} trigger={["click"]}>
                    <Button size="middle">
                        Columns <SettingOutlined />
                    </Button>
                </Dropdown>
            </div>
            <GenericTable<UnitSummary>
                data={units}
                columns={tableColumns}
            />
        </div>
    );
};

export default Dashboard;