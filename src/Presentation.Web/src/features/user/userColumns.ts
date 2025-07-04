import type { ColumnsType, ColumnType } from "antd/es/table";

export interface UserColumnProps {
    columnMapper: (col: any) => ColumnType<ApplicationUser>;
}

// ...existing ApplicationUser interface...
export interface ApplicationUser {
    id: string;
    userName: string;
    email: string;
    emailConfirmed: boolean;
    phoneNumber: string | null;
    phoneNumberConfirmed: boolean;
    lockoutEnabled: boolean;
    accessFailedCount: number;
}

const columnDefinitions: ColumnsType<ApplicationUser>  = [
    { title: "ID", dataIndex: "id", key: "id", width: 100 },
    { title: "UserName", dataIndex: "userName", key: "userName", width: 100 },
    { title: "Email", dataIndex: "email", key: "email", width: 300 },
];


//add custom rendering if required
export const getUserColumns = ({ columnMapper}: UserColumnProps): ColumnsType<ApplicationUser> => {
    return columnDefinitions.map(columnMapper);
};