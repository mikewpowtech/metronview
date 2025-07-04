import type { ColumnsType } from "antd/es/table";
import type { Unit } from "./unitsAPI";
import type { Company } from "../companies/companyAPI";
import type { UnitModel } from "../unitmodels/unitModelAPI";

export interface UnitListColumnProps {
    companies: Company[];
    unitModels: UnitModel[];
}

export const unitStatusOptions = [
    { value: 0, label: "Active" },
    { value: 1, label: "Inactive" },
    { value: 2, label: "Maintenance" },
    { value: 3, label: "Decommissioned" },
    // Add more statuses as defined in your backend enum
];

const columnDefinitions: ColumnsType<Unit> = [
    { title: "ID", dataIndex: "id", key: "id", width: 100 },
    { title: "Unit Type", dataIndex: "unitTypeId", key: "unitTypeId", width: 120 },
    { title: "Phone Number", dataIndex: "phoneNumber", key: "phoneNumber", width: 140 },
    { title: "PIN", dataIndex: "pin", key: "pin", width: 80 },
    { title: "Manufacturer Code", dataIndex: "manufacturerCode", key: "manufacturerCode", width: 150 },
    { title: "Unit Code", dataIndex: "unitCode", key: "unitCode", width: 120 },
    { title: "Secret", dataIndex: "secret", key: "secret", width: 120 },
    { title: "Status", dataIndex: "status", key: "status", width: 120 },
    { title: "Company", dataIndex: "companyID", key: "companyID", width: 180 },
    { title: "Days Before Not Reported", dataIndex: "daysBeforeNotReported", key: "daysBeforeNotReported", width: 180 },
    { title: "Custom Field Values", dataIndex: "customFieldValues", key: "customFieldValues", width: 180 },
];

//add custom rendering if required
export const getUnitListColumns = ({ companies, unitModels }: UnitListColumnProps): ColumnsType<Unit> => {
    const columnMapper = (col: any) => {
        if (col.key === "companyID") {
            return {
                ...col,
                render: (_: never, record: Unit) => {
                    if (!record.companyID) return "";
                    const company = companies.find(c => c.id === record.companyID);
                    return company?.name ?? "";
                }
            };
        }
        if (col.key === "unitTypeId") {
            return {
                ...col,
                render: (_: never, record: Unit) => {
                    const model = unitModels.find(m => m.id === record.unitTypeId);
                    return model?.name ?? "not specified";
                }
            };
        }
        if (col.key === "status") {
            return {
                ...col,
                render: (_: never, record: Unit) => {
                    const status = unitStatusOptions.find(opt => opt.value === record.status);
                    return status?.label ?? "Unknown";
                }
            };
        }
        return col;
    };

    return columnDefinitions.map(columnMapper);
};
